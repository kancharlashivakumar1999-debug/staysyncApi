using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuestController : ControllerBase
    {
        private readonly IGuestService _guest;

        public GuestController(IGuestService guest)
        {
            _guest = guest;
        }

        [HttpGet("owner/{ownerId:guid}")]
        public async Task<ActionResult> GetGuests(Guid ownerId, [FromQuery] Guid? branchId)
        {
            return Ok(await _guest.GetGuestsAsync(ownerId, branchId));
        }

        [HttpPost]
        public async Task<ActionResult> CheckInGuest([FromBody] GuestDto dto)
        {
            try
            {
                return Ok(await _guest.CheckInGuestAsync(dto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{guestId:guid}/vacate")]
        public async Task<ActionResult> VacateGuest(Guid guestId, [FromBody] VacateSettlementDto dto)
        {
            var ok = await _guest.VacateGuestAsync(guestId, dto);
            return ok ? Ok(new { message = "Guest checked out & bed liberated." }) : NotFound();
        }
    }
}
