using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitorController : ControllerBase
    {
        private readonly IVisitorService _visitor;

        public VisitorController(IVisitorService visitor)
        {
            _visitor = visitor;
        }

        [HttpGet("owner/{ownerId:guid}")]
        public async Task<ActionResult> GetVisitors(
            Guid ownerId,
            [FromQuery] Guid? branchId,
            [FromQuery] Guid? guestId,
            [FromQuery] DateTime? date)
        {
            return Ok(await _visitor.GetVisitorsAsync(ownerId, branchId, guestId, date));
        }

        [HttpPost]
        public async Task<ActionResult> LogVisitor([FromBody] LogVisitorDto dto)
        {
            return Ok(await _visitor.LogVisitorAsync(dto));
        }

        [HttpPost("{visitorId:guid}/checkout")]
        public async Task<ActionResult> CheckOut(Guid visitorId)
        {
            var ok = await _visitor.CheckOutVisitorAsync(visitorId);
            return ok ? Ok(new { message = "Visitor checked out." }) : NotFound();
        }
    }
}
