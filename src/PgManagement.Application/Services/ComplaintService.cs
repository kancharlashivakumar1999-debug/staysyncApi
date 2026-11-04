using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _complaintRepo;

        public ComplaintService(IComplaintRepository complaintRepo)
        {
            _complaintRepo = complaintRepo;
        }

        public async Task<ComplaintDto> CreateComplaintAsync(CreateComplaintDto dto)
        {
            var c = new Complaint
            {
                OwnerId = dto.OwnerId,
                BranchId = dto.BranchId,
                GuestId = dto.GuestId,
                RoomId = dto.RoomId,
                Category = dto.Category,
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Status = "New",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var id = await _complaintRepo.CreateAsync(c);
            var created = await _complaintRepo.GetByIdAsync(id);
            return created ?? new ComplaintDto
            {
                ComplaintId = id,
                ComplaintCode = c.ComplaintCode,
                OwnerId = c.OwnerId,
                BranchId = c.BranchId,
                GuestId = c.GuestId,
                RoomId = c.RoomId,
                Category = c.Category,
                Title = c.Title,
                Description = c.Description,
                Priority = c.Priority,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }

        public async Task<IEnumerable<ComplaintDto>> GetComplaintsAsync(Guid ownerId, Guid? branchId = null, string? status = null) =>
            await _complaintRepo.GetComplaintsAsync(ownerId, branchId, status);

        public async Task<bool> UpdateStatusAsync(Guid complaintId, UpdateComplaintStatusDto dto)
        {
            DateTime? resolvedAt = (dto.Status == "Resolved" || dto.Status == "Closed") ? DateTime.UtcNow : null;
            return await _complaintRepo.UpdateStatusAsync(complaintId, dto.Status, dto.AssignedTo, dto.ResolutionNotes, resolvedAt);
        }
    }
}
