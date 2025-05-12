using Microsoft.Data.SqlClient;
using Test1.Models.DTOs;
using Tutorial9.Exceptions;

namespace Test1.Services;

public class AppointmentService : IAppointmentService
{
    private readonly string? _connectionString;

    public AppointmentService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<bool> AppointmentExist(int? appointmentId, SqlConnection connection)
    {

        const string command = @"
            SELECT 1 FROM Appointment
            WHERE appointment_id = @IdAppointment";

        await using var cmd = new SqlCommand(command, connection);
        cmd.Parameters.AddWithValue("@IdAppointment", appointmentId);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }
    
    public async Task<bool> PatientExist(int? patientId , SqlConnection connection)
    {
        
        const string command = @"
            SELECT 1 FROM Patient
            WHERE patient_id = @IdPatient";

        await using var cmd = new SqlCommand(command, connection);
        cmd.Parameters.AddWithValue("@IdPatient", patientId);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    public async Task<int> GetDoctorId(string pwz, SqlConnection connection)
    {
        const string command = @"
            SELECT doctor_id FROM Doctor
            WHERE PWZ = @PWZ";
        
        await using var cmd = new SqlCommand(command, connection);
        cmd.Parameters.AddWithValue("@PWZ", pwz);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return reader.GetInt32(0);
        
        throw new NotFoundException("Doctor not found");
    }
    
    public async Task<int> GetServiceIdByName(string serviceName, SqlConnection connection)
    {
        const string command = @"
            SELECT TOP 1 s.service_id FROM Service s
            where s.name = @ServiceName
            ";
        await using var cmd = new SqlCommand(command, connection);
        cmd.Parameters.AddWithValue("@ServiceName", serviceName);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return reader.GetInt32(0);
        throw new NotFoundException("Service not found");
    }

    public async Task<AppointmentResponceDTO> GetAppointment(int appointmentId)
    {
        var appointment = new AppointmentResponceDTO();
        
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        if (!await AppointmentExist(appointmentId, conn))
            throw new NotFoundException("Appointment not found");

        const string command1 = @"
            SELECT a.date, p.first_name, p.last_name, p.date_of_birth, d.doctor_id, 
            d.PWZ
            FROM Appointment a
            JOIN Patient p ON a.patient_id = p.patient_id
            JOIN Doctor d ON a.doctor_id = d.doctor_id
            WHERE appointment_id = @IdAppointment
            ";
        await using var cmd = new SqlCommand(command1, conn);
        cmd.Parameters.AddWithValue("@IdAppointment", appointmentId);
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                appointment.Date = reader.GetDateTime(0);
                appointment.Patient = new PatientDTO
                {
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    DateOfBirth = reader.GetDateTime(3)
                };
                appointment.Doctor = new DoctorDTO
                {
                    DoctorId = reader.GetInt32(4),
                    PWZ = reader.GetString(5)
                };
            }
                
        }
        
        appointment.Services = new List<AppointmentServiceDTO>();

        const string command2 = @"
            SELECT s.name, sa.service_fee
            From Appointment_Service sa 
            JOIN [Service] s ON sa.service_id = s.service_id
            WHERE appointment_id = @IdAppointment;
            ";
        await using var cmd2 = new SqlCommand(command2, conn);
        cmd2.Parameters.AddWithValue("@IdAppointment", appointmentId);
        await using (var reader2 = await cmd2.ExecuteReaderAsync())
        {
            while (await reader2.ReadAsync())
            {
                appointment.Services.Add(new AppointmentServiceDTO
                {
                    Name = reader2.GetString(0),
                    ServiceFee = reader2.GetDecimal(1)
                });
            }
        }

        return appointment;
        
    }

    public async Task CreateAppointment(AppointmentCreationDTO appointment)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        if (await AppointmentExist(appointment.AppointmentId, conn))
            throw new ConflictException("Appointment Already Exist");
        
        if (!await PatientExist(appointment.PatientId, conn))
            throw new NotFoundException("Patient not found");
        var doctorId = await GetDoctorId(appointment.PWZ, conn);
        var serviceIDs = new List<int>();
        foreach (var service in appointment.Services)
        {
            serviceIDs.Add(await GetServiceIdByName(service.Name, conn));
        }
        
        var transaction = conn.BeginTransaction();
        try
        {
            const string command1 = @"
            Insert into Appointment (appointment_id, patient_id, doctor_id, date)
            values (@IdAppointment, @IdPatient, @IdDoctor, @Date);
            ";
            await using var cmd1 = new SqlCommand(command1, conn, transaction);
            cmd1.Parameters.AddWithValue("@IdAppointment", appointment.AppointmentId);
            cmd1.Parameters.AddWithValue("@IdPatient", appointment.PatientId);
            cmd1.Parameters.AddWithValue("@IdDoctor", doctorId);
            cmd1.Parameters.AddWithValue("@Date", DateTime.Now);
            await cmd1.ExecuteNonQueryAsync();

            const string command2 = @"
                Insert into Appointment_Service (appointment_id, service_id, service_fee)
                values (@IdAppointment, @IdService, @ServiceFee);
                ";
            await using var cmd2 = new SqlCommand(command2, conn, transaction);
            cmd2.Parameters.AddWithValue("@IdAppointment", appointment.AppointmentId);
            for (var i = 0; i < serviceIDs.Count; i++)
            {
                cmd2.Parameters.Clear();
                cmd2.Parameters.AddWithValue("@IdService", serviceIDs[i]);
                cmd2.Parameters.AddWithValue("@ServiceFee", appointment.Services[i].ServiceFee);
                await cmd2.ExecuteNonQueryAsync();
            }
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            transaction.Commit();
            conn.Close();
        }
        
    }
}