using Microsoft.Data.SqlClient;
using Test1.Models.DTOs;

namespace Test1.Services;

public interface IAppointmentService
{
    Task<bool> AppointmentExist(int? appointmentId, SqlConnection connection);
    Task<AppointmentResponceDTO> GetAppointment(int appointmentId);
}