using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaint;

        public ComplaintController(IComplaintService complaint)
        {
            _complaint = complaint;
        }

        [HttpGet("owner/{ownerId:guid}")]
        public async Task<ActionResult> GetComplaints(
            Guid ownerId,
            [FromQuery] Guid? branchId,
            [FromQuery] string? status)
        {
            return Ok(await _complaint.GetComplaintsAsync(ownerId, branchId, status));
        }

        [HttpPost]
        public async Task<ActionResult> CreateComplaint([FromBody] CreateComplaintDto dto)
        {
            return Ok(await _complaint.CreateComplaintAsync(dto));
        }

        [HttpPatch("{complaintId:guid}/status")]
        public async Task<ActionResult> UpdateStatus(Guid complaintId, [FromBody] UpdateComplaintStatusDto dto)
        {
            var ok = await _complaint.UpdateStatusAsync(complaintId, dto);
            return ok ? Ok(new { message = "Complaint status updated." }) : NotFound();
        }
    }
}
