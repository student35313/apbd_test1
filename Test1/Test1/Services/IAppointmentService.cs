using Microsoft.Data.SqlClient;
using Test1.Models.DTOs;

namespace Test1.Services;

public interface IAppointmentService
{
    Task<bool> AppointmentExist(int? appointmentId, SqlConnection connection);
    Task<AppointmentResponceDTO> GetAppointment(int appointmentId);
    Task<bool> PatientExist(int? patientId, SqlConnection connection);
    Task<int> GetDoctorId(string pwz, SqlConnection connection);
    Task<int> GetServiceIdByName(string serviceName, SqlConnection connection);
    Task CreateAppointment(AppointmentCreationDTO appointment);

}