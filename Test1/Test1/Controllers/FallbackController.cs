using Microsoft.AspNetCore.Mvc;

namespace Test1.Controllers;

[ApiController]
[Route("api/{*any}")]
public class FallbackController : ControllerBase
{
    [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch]
    public IActionResult NotImplemented() =>
        StatusCode(StatusCodes.Status501NotImplemented);
}