using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PgManagement.Application.Interfaces;

namespace PgManagement.Infrastructure.Services
{
    public class IdentityInformationService : IIdentityInformationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityInformationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public Guid? UserId
        {
            get
            {
                var claim = User?.FindFirst("ownerId") 
                         ?? User?.FindFirst(ClaimTypes.NameIdentifier)
                         ?? User?.FindFirst("sub")
                         ?? User?.FindFirst("userId");

                return Guid.TryParse(claim?.Value, out var guid) ? guid : null;
            }
        }

        public string? OwnerCode => User?.FindFirst("ownerCode")?.Value;

        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value 
                             ?? User?.FindFirst("email")?.Value;

        public string? FullName => User?.FindFirst(ClaimTypes.Name)?.Value 
                                ?? User?.FindFirst("name")?.Value;

        public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value 
                            ?? User?.FindFirst("role")?.Value;

        public IEnumerable<Guid> BranchIds
        {
            get
            {
                var claims = User?.FindAll("branchIds");
                if (claims == null) yield break;

                foreach (var claim in claims)
                {
                    if (Guid.TryParse(claim.Value, out var bId))
                    {
                        yield return bId;
                    }
                }
            }
        }
    }
}
