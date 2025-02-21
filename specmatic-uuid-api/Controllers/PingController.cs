using Microsoft.AspNetCore.Mvc;

namespace specmatic_uuid_api.Controllers;

[ApiController]
public class PingController:ControllerBase
{
    [HttpGet]
    [Route("ping")]
    public IActionResult Get()
    {
        return Ok("Ping");
    }
}