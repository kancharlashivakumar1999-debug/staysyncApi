using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staff;

        public StaffController(IStaffService staff)
        {
            _staff = staff;
        }

        [HttpGet("owner/{ownerId:guid}")]
        public async Task<ActionResult> GetStaff(Guid ownerId, [FromQuery] Guid? branchId)
        {
            return Ok(await _staff.GetStaffAsync(ownerId, branchId));
        }

        [HttpPost]
        public async Task<ActionResult> CreateStaff([FromBody] CreateStaffDto dto)
        {
            return Ok(await _staff.CreateStaffAsync(dto));
        }

        [HttpPatch("{staffId:guid}/status")]
        public async Task<ActionResult> ToggleStatus(Guid staffId, [FromBody] bool isActive)
        {
            var ok = await _staff.ToggleStaffStatusAsync(staffId, isActive);
            return ok ? Ok(new { message = "Staff status updated." }) : NotFound();
        }
    }
}
