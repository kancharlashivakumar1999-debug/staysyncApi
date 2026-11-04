using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FloorController : ControllerBase
    {
        private readonly IFloorService _floor;

        public FloorController(IFloorService floor)
        {
            _floor = floor;
        }

        [HttpGet("branch/{branchId:guid}")]
        public async Task<ActionResult> GetFloors(Guid branchId)
        {
            return Ok(await _floor.GetFloorsByBranchIdAsync(branchId));
        }

        [HttpPost]
        public async Task<ActionResult> CreateFloor([FromBody] CreateFloorDto dto)
        {
            return Ok(await _floor.CreateFloorAsync(dto));
        }

        [HttpDelete("{floorId:guid}")]
        public async Task<ActionResult> DeleteFloor(Guid floorId)
        {
            return Ok(await _floor.DeleteFloorAsync(floorId));
        }
    }
}
