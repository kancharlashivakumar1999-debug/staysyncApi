using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IOwnerRepository _ownerRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IConfiguration _config;

        public AuthService(IOwnerRepository ownerRepo, IBranchRepository branchRepo, IConfiguration config)
        {
            _ownerRepo = ownerRepo;
            _branchRepo = branchRepo;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterOwnerDto dto)
        {
            var existing = await _ownerRepo.GetByEmailAsync(dto.Email);
            if (existing != null) throw new InvalidOperationException("Email is already registered.");

            var owner = new Owner
            {
                FullName = dto.FullName,
                Email = dto.Email.Trim().ToLower(),
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                ExpectedBranches = dto.ExpectedBranches > 0 ? dto.ExpectedBranches : 1
            };

            var id = await _ownerRepo.CreateAsync(owner);
            owner.OwnerId = id;
            var created = await _ownerRepo.GetByIdAsync(id);
            return await GenerateAuthResponse(created ?? owner);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var owner = await _ownerRepo.GetByEmailAsync(dto.Email);
            if (owner == null) throw new UnauthorizedAccessException("Invalid credentials.");

            bool ok = false;
            try { ok = BCrypt.Net.BCrypt.Verify(dto.Password, owner.PasswordHash); }
            catch { ok = owner.PasswordHash == dto.Password; }

            if (!ok) throw new UnauthorizedAccessException("Invalid credentials.");

            return await GenerateAuthResponse(owner);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var rt = await _ownerRepo.GetRefreshTokenAsync(dto.RefreshToken);
            if (rt == null || rt.ExpiresAt <= DateTime.UtcNow) throw new SecurityTokenException("Invalid or expired token.");

            var owner = await _ownerRepo.GetByIdAsync(rt.OwnerId);
            if (owner == null) throw new SecurityTokenException("Owner not found.");

            await _ownerRepo.RevokeRefreshTokenAsync(dto.RefreshToken);
            return await GenerateAuthResponse(owner);
        }

        public async Task RevokeTokenAsync(string token)
        {
            await _ownerRepo.RevokeRefreshTokenAsync(token);
        }

        public async Task<AuthResponseDto> UpdateOwnerProfileAsync(Guid ownerId, UpdateOwnerProfileDto dto)
        {
            var owner = await _ownerRepo.GetByIdAsync(ownerId);
            if (owner == null) throw new InvalidOperationException("Owner not found.");

            await _ownerRepo.UpdateProfileAsync(ownerId, dto.FullName, dto.PhoneNumber, dto.ExpectedBranches);
            var updated = await _ownerRepo.GetByIdAsync(ownerId);
            return await GenerateAuthResponse(updated ?? owner);
        }

        private async Task<AuthResponseDto> GenerateAuthResponse(Owner owner)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SecretKeyForHostelPGManagementSystem2026SecureJwtSecretKey");

            // Fetch owner's associated branches
            var branches = (await _branchRepo.GetByOwnerIdAsync(owner.OwnerId)).ToList();
            var branchIdList = branches.Select(b => b.BranchId).ToList();

            var claims = new List<Claim>
            {
                new Claim("ownerId", owner.OwnerId.ToString()),
                new Claim("ownerCode", owner.OwnerCode),
                new Claim(ClaimTypes.NameIdentifier, owner.OwnerId.ToString()),
                new Claim(ClaimTypes.Email, owner.Email),
                new Claim(ClaimTypes.Name, owner.FullName),
                new Claim("phoneNumber", owner.PhoneNumber ?? string.Empty),
                new Claim(ClaimTypes.Role, "Owner"),
                new Claim("role", "Owner")
            };

            foreach (var bId in branchIdList)
            {
                claims.Add(new Claim("branchIds", bId.ToString()));
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60), // 60 minutes expiry
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"] ?? "PgManagementApi",
                Audience = _config["Jwt:Audience"] ?? "PgManagementApp"
            };

            var token = handler.WriteToken(handler.CreateToken(descriptor));
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            await _ownerRepo.SaveRefreshTokenAsync(owner.OwnerId, refreshToken, DateTime.UtcNow.AddDays(7)); // 7 days refresh token

            return new AuthResponseDto
            {
                OwnerId = owner.OwnerId,
                OwnerCode = owner.OwnerCode,
                FullName = owner.FullName,
                Email = owner.Email,
                PhoneNumber = owner.PhoneNumber,
                ExpectedBranches = owner.ExpectedBranches,
                Role = "Owner",
                BranchIds = branchIdList,
                Token = token,
                RefreshToken = refreshToken
            };
        }
    }
}
