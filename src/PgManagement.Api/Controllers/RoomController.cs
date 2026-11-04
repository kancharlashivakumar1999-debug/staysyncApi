using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _room;

        public RoomController(IRoomService room)
        {
            _room = room;
        }

        [HttpGet("floor/{floorId:guid}")]
        public async Task<ActionResult> GetRoomsByFloor(Guid floorId)
        {
            return Ok(await _room.GetRoomsByFloorIdAsync(floorId));
        }

        [HttpPost]
        public async Task<ActionResult> CreateRoom([FromBody] CreateRoomDto dto)
        {
            return Ok(await _room.CreateRoomAsync(dto));
        }

        [HttpPut("{roomId:guid}")]
        public async Task<ActionResult> UpdateRoom(Guid roomId, [FromBody] UpdateRoomDto dto)
        {
            try
            {
                return Ok(await _room.UpdateRoomAsync(roomId, dto));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("beds")]
        public async Task<ActionResult> AddBed([FromBody] CreateBedDto dto)
        {
            return Ok(await _room.AddBedAsync(dto));
        }

        [HttpGet("{roomId:guid}/beds")]
        public async Task<ActionResult> GetBeds(Guid roomId)
        {
            return Ok(await _room.GetBedsByRoomIdAsync(roomId));
        }

        [HttpDelete("{roomId:guid}")]
        public async Task<ActionResult> DeleteRoom(Guid roomId)
        {
            return Ok(await _room.DeleteRoomAsync(roomId));
        }
    }
}
