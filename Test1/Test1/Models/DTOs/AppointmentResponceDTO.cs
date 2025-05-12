namespace Test1.Models.DTOs;

public class AppointmentResponceDTO
{
    public DateTime Date { get; set; }
    public PatientDTO Patient { get; set; }
    public DoctorDTO Doctor { get; set; }
    public List<Appointment_Service> Services { get; set; }
}