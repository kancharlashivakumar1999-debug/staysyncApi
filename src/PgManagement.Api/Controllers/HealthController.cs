using Microsoft.AspNetCore.Mvc;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Check()
        {
            return Ok(new
            {
                status = "Healthy",
                service = "PG & Hostel Management API",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
