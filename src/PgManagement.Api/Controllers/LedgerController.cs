using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LedgerController : ControllerBase
    {
        private readonly ILedgerService _ledger;

        public LedgerController(ILedgerService ledger)
        {
            _ledger = ledger;
        }

        [HttpPost("charge")]
        public async Task<ActionResult> AddCharge([FromBody] CreateChargeDto dto)
        {
            return Ok(await _ledger.AddChargeAsync(dto));
        }

        [HttpPost("payment")]
        public async Task<ActionResult> RecordPayment([FromBody] RecordPaymentDto dto)
        {
            return Ok(await _ledger.RecordPaymentAsync(dto));
        }

        [HttpGet("guest/{guestId:guid}")]
        public async Task<ActionResult> GetGuestLedger(Guid guestId)
        {
            return Ok(await _ledger.GetGuestLedgerAsync(guestId));
        }

        [HttpGet("bed/{bedId:guid}")]
        public async Task<ActionResult> GetBedLedger(Guid bedId)
        {
            var summary = await _ledger.GetBedLedgerAsync(bedId);
            if (summary == null)
            {
                return NotFound(new { message = "No active resident checked-in on this bed." });
            }
            return Ok(summary);
        }

        [HttpGet("dues/{ownerId:guid}")]
        public async Task<ActionResult> GetPendingDues(Guid ownerId, [FromQuery] Guid? branchId)
        {
            return Ok(await _ledger.GetPendingDuesAsync(ownerId, branchId));
        }

        [HttpGet("payments/{ownerId:guid}")]
        public async Task<ActionResult> GetRecentPayments(Guid ownerId, [FromQuery] Guid? branchId)
        {
            return Ok(await _ledger.GetRecentPaymentsAsync(ownerId, branchId));
        }
    }
}
