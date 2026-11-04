using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class VisitorService : IVisitorService
    {
        private readonly IVisitorRepository _visitorRepo;

        public VisitorService(IVisitorRepository visitorRepo)
        {
            _visitorRepo = visitorRepo;
        }

        public async Task<VisitorDto> LogVisitorAsync(LogVisitorDto dto)
        {
            var v = new Visitor
            {
                OwnerId = dto.OwnerId,
                BranchId = dto.BranchId,
                GuestId = dto.GuestId,
                VisitorName = dto.VisitorName,
                VisitorPhone = dto.VisitorPhone,
                RelationToGuest = dto.RelationToGuest,
                PurposeOfVisit = dto.PurposeOfVisit,
                VehicleNumber = dto.VehicleNumber,
                IdProofType = dto.IdProofType,
                IdProofNumber = dto.IdProofNumber,
                CheckInTime = DateTime.UtcNow
            };

            var id = await _visitorRepo.CreateAsync(v);
            return new VisitorDto
            {
                VisitorId = id,
                VisitorCode = v.VisitorCode,
                OwnerId = v.OwnerId,
                BranchId = v.BranchId,
                GuestId = v.GuestId,
                VisitorName = v.VisitorName,
                VisitorPhone = v.VisitorPhone,
                RelationToGuest = v.RelationToGuest,
                PurposeOfVisit = v.PurposeOfVisit,
                VehicleNumber = v.VehicleNumber,
                CheckInTime = v.CheckInTime,
                IdProofType = v.IdProofType,
                IdProofNumber = v.IdProofNumber
            };
        }

        public async Task<IEnumerable<VisitorDto>> GetVisitorsAsync(Guid ownerId, Guid? branchId = null, Guid? guestId = null, DateTime? date = null) =>
            await _visitorRepo.GetVisitorsAsync(ownerId, branchId, guestId, date);

        public async Task<bool> CheckOutVisitorAsync(Guid visitorId) =>
            await _visitorRepo.CheckOutVisitorAsync(visitorId, DateTime.UtcNow);
    }
}
