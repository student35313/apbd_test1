using Microsoft.AspNetCore.Mvc;
using Test1.Models.DTOs;
using Test1.Services;
using Tutorial9.Exceptions;

namespace Test1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    
    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    } 
    
    [HttpGet("{appointmentId}")] 
    public async Task<IActionResult> GetAppointment(int appointmentId)
    {
        try
        {
            var result = await _appointmentService.GetAppointment(appointmentId);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }
}