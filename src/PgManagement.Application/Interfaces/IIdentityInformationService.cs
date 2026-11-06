namespace PgManagement.Application.Interfaces
{
    public interface IIdentityInformationService
    {
        Guid? UserId { get; }
        string? OwnerCode { get; }
        string? Email { get; }
        string? FullName { get; }
        string? Role { get; }
        IEnumerable<Guid> BranchIds { get; }
        bool IsAuthenticated { get; }
    }
}
