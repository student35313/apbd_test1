using System.ComponentModel.DataAnnotations;

namespace Test1.Models.DTOs;

public class AppointmentCreationDTO
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The field AppointmentId must be greater than 0.")]
    public int? AppointmentId { get; set; }
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The field PatientId must be greater than 0.")]
    public int? PatientId { get; set; }
    [Required , MinLength(7), MaxLength(7)]
    public string? PWZ { get; set; }
    [Required, MinLength(1)]
    public List<AppointmentServiceDTO>? Services { get; set; }
}