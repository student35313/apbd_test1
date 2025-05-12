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
        
        appointment.Services = new List<Appointment_Service>();

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
                appointment.Services.Add(new Appointment_Service
                {
                    Name = reader2.GetString(0),
                    ServiceFee = reader2.GetDecimal(1)
                });
            }
        }

        return appointment;
        
    }
    
    
}