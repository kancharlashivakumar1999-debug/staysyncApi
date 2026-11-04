using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IComplaintRepository
    {
        Task<Guid> CreateAsync(Complaint complaint);
        Task<IEnumerable<ComplaintDto>> GetComplaintsAsync(Guid ownerId, Guid? branchId = null, string? status = null);
        Task<ComplaintDto?> GetByIdAsync(Guid complaintId);
        Task<bool> UpdateStatusAsync(Guid complaintId, string status, string? assignedTo, string? resolutionNotes, DateTime? resolvedAt);
    }

    public interface IComplaintService
    {
        Task<ComplaintDto> CreateComplaintAsync(CreateComplaintDto dto);
        Task<IEnumerable<ComplaintDto>> GetComplaintsAsync(Guid ownerId, Guid? branchId = null, string? status = null);
        Task<bool> UpdateStatusAsync(Guid complaintId, UpdateComplaintStatusDto dto);
    }
}
