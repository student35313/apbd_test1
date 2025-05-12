using System.ComponentModel.DataAnnotations;

namespace Test1.Models.DTOs;

public class AppointmentServiceDTO
{
    [Required]
    public string? Name { get; set; }
    [Required]
    [Range(0.0, Double.MaxValue, ErrorMessage = "The field {0} must be greater than {1}.")]
    public decimal? ServiceFee { get; set; }
}