using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterOwnerDto dto)
        {
            try
            {
                return Ok(await _auth.RegisterAsync(dto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                return Ok(await _auth.LoginAsync(dto));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                return Ok(await _auth.RefreshTokenAsync(dto));
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto?.RefreshToken))
            {
                await _auth.RevokeTokenAsync(dto.RefreshToken);
            }
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPut("profile/{ownerId:guid}")]
        public async Task<ActionResult<AuthResponseDto>> UpdateProfile(Guid ownerId, [FromBody] UpdateOwnerProfileDto dto)
        {
            try
            {
                return Ok(await _auth.UpdateOwnerProfileAsync(ownerId, dto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
