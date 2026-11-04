using PgManagement.Application.DTOs;

namespace PgManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterOwnerDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task RevokeTokenAsync(string token);
        Task<AuthResponseDto> UpdateOwnerProfileAsync(Guid ownerId, UpdateOwnerProfileDto dto);
    }
}
