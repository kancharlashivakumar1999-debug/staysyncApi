using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branch;

        public BranchController(IBranchService branch)
        {
            _branch = branch;
        }

        [HttpGet("owner/{ownerId:guid}")]
        public async Task<ActionResult> GetBranches(Guid ownerId)
        {
            return Ok(await _branch.GetBranchesHierarchyAsync(ownerId));
        }

        [HttpGet("{branchId:guid}")]
        public async Task<ActionResult> GetBranch(Guid branchId)
        {
            var branch = await _branch.GetBranchByIdAsync(branchId);
            return branch == null ? NotFound() : Ok(branch);
        }

        [HttpPost]
        public async Task<ActionResult> CreateBranch([FromBody] CreateBranchDto dto)
        {
            var branch = await _branch.CreateBranchAsync(dto);
            return CreatedAtAction(nameof(GetBranch), new { branchId = branch.BranchId }, branch);
        }

        [HttpDelete("{branchId:guid}")]
        public async Task<ActionResult> DeleteBranch(Guid branchId)
        {
            return Ok(await _branch.DeleteBranchAsync(branchId));
        }
    }
}
