using Dapper;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Common;
using PgManagement.Domain.Entities;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Repositories
{
    public class OwnerRepository : IOwnerRepository
    {
        private readonly IDbConnectionFactory _factory;

        public OwnerRepository(IDbConnectionFactory factory) => _factory = factory;

        private const string OwnerProjectionSql = """
            SELECT o.owner_id AS OwnerId,
                   o.owner_code AS OwnerCode,
                   o.full_name AS FullName,
                   o.email AS Email,
                   o.phone_number AS PhoneNumber,
                   o.password_hash AS PasswordHash,
                   o.expected_branches AS ExpectedBranches,
                   o.created_at AS CreatedAt,
                   o.updated_at AS UpdatedAt
              FROM owners o
            """;

        private const string RefreshTokenProjectionSql = """
            SELECT rt.token_id AS TokenId,
                   rt.owner_id AS OwnerId,
                   rt.token AS Token,
                   rt.expires_at AS ExpiresAt,
                   rt.is_revoked AS IsRevoked,
                   rt.created_at AS CreatedAt,
                   rt.revoked_at AS RevokedAt,
                   rt.replaced_by_token AS ReplacedByToken
              FROM refresh_tokens rt
            """;

        public async Task<Owner?> GetByEmailAsync(string email)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{OwnerProjectionSql} WHERE LOWER(o.email) = LOWER(@Email) LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<Owner>(sql, new { Email = email.Trim() });
        }

        public async Task<Owner?> GetByIdAsync(Guid ownerId)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{OwnerProjectionSql} WHERE o.owner_id = @OwnerId LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<Owner>(sql, new { OwnerId = ownerId });
        }

        public async Task<Guid> CreateAsync(Owner owner)
        {
            using var conn = _factory.CreateConnection();
            owner.OwnerId = Uuid7.NewGuid();
            const string sql = @"
                INSERT INTO owners (owner_id, owner_code, full_name, email, phone_number, password_hash, expected_branches, created_at, updated_at)
                VALUES (@OwnerId, fn_next_owner_code(), @FullName, @Email, @PhoneNumber, @PasswordHash, @ExpectedBranches, @CreatedAt, @UpdatedAt)
                RETURNING owner_id;";
            return await conn.ExecuteScalarAsync<Guid>(sql, owner);
        }

        public async Task<bool> UpdateProfileAsync(Guid ownerId, string fullName, string phoneNumber, int expectedBranches)
        {
            using var conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE owners
                SET full_name = @FullName,
                    phone_number = @PhoneNumber,
                    expected_branches = @ExpectedBranches,
                    updated_at = CURRENT_TIMESTAMP
                WHERE owner_id = @OwnerId;";
            return await conn.ExecuteAsync(sql, new { OwnerId = ownerId, FullName = fullName, PhoneNumber = phoneNumber, ExpectedBranches = expectedBranches }) > 0;
        }

        public async Task SaveRefreshTokenAsync(Guid ownerId, string token, DateTime expiresAt)
        {
            using var conn = _factory.CreateConnection();
            const string sql = @"
                INSERT INTO refresh_tokens (token_id, owner_id, token, expires_at, is_revoked, created_at)
                VALUES (fn_generate_uuid_v7(), @OwnerId, @Token, @ExpiresAt, false, CURRENT_TIMESTAMP);";
            await conn.ExecuteAsync(sql, new { OwnerId = ownerId, Token = token, ExpiresAt = expiresAt });
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            using var conn = _factory.CreateConnection();
            string sql = $"{RefreshTokenProjectionSql} WHERE rt.token = @Token AND rt.is_revoked = false LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "UPDATE refresh_tokens SET is_revoked = true, revoked_at = CURRENT_TIMESTAMP WHERE token = @Token;";
            await conn.ExecuteAsync(sql, new { Token = token });
        }
    }
}
