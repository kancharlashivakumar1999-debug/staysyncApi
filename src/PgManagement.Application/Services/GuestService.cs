using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class GuestService : IGuestService
    {
        private readonly IGuestRepository _guestRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IWhatsAppNotificationService _whatsApp;
        private readonly IEmailNotificationService _email;

        public GuestService(
            IGuestRepository guestRepo,
            IRoomRepository roomRepo,
            IBranchRepository branchRepo,
            IWhatsAppNotificationService whatsApp,
            IEmailNotificationService email)
        {
            _guestRepo = guestRepo;
            _roomRepo = roomRepo;
            _branchRepo = branchRepo;
            _whatsApp = whatsApp;
            _email = email;
        }

        public async Task<IEnumerable<GuestDto>> GetGuestsAsync(Guid ownerId, Guid? branchId = null)
        {
            var guests = await _guestRepo.GetByOwnerIdAsync(ownerId, branchId);
            var list = new List<GuestDto>();

            foreach (var g in guests)
            {
                var room = await _roomRepo.GetByIdAsync(g.RoomId);
                var bed = await _roomRepo.GetBedByIdAsync(g.BedId);

                list.Add(new GuestDto
                {
                    GuestId = g.GuestId,
                    GuestCode = g.GuestCode,
                    OwnerId = g.OwnerId,
                    BranchId = g.BranchId,
                    FloorId = g.FloorId,
                    RoomId = g.RoomId,
                    RoomNumber = room?.RoomNumber ?? "Room",
                    BedId = g.BedId,
                    BedNumber = bed?.BedNumber ?? "Bed",
                    FullName = g.FullName,
                    Email = g.Email,
                    PhoneNumber = g.PhoneNumber,
                    EmergencyContact = g.EmergencyContact,
                    Age = g.Age,
                    Gender = g.Gender,
                    Occupation = g.Occupation,
                    WorkplaceOrCollege = g.WorkplaceOrCollege,
                    TenantType = g.TenantType,
                    AdvanceAmount = g.AdvanceAmount,
                    MonthlyRent = g.MonthlyRent,
                    DailyRent = g.DailyRent,
                    JoiningDate = g.JoiningDate,
                    ExpectedVacatingDate = g.ExpectedVacatingDate,
                    DaysBeforeVacateReminder = g.DaysBeforeVacateReminder,
                    VacateNoticeServed = g.VacateNoticeServed,
                    VacatingReturnedAmount = g.VacatingReturnedAmount,
                    DeductionsAmount = g.DeductionsAmount,
                    SettlementNotes = g.SettlementNotes,
                    ProofDocumentType = g.ProofDocumentType,
                    ProofDocumentNumber = g.ProofDocumentNumber,
                    ProofDocumentUrl = g.ProofDocumentUrl,
                    Status = g.Status
                });
            }
            return list;
        }

        public async Task<Guest> CheckInGuestAsync(GuestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new InvalidOperationException("Email address is mandatory for check-in.");
            }

            var bed = await _roomRepo.GetBedByIdAsync(dto.BedId);
            if (bed == null) throw new InvalidOperationException("Selected bed not found.");
            if (bed.IsOccupied) throw new InvalidOperationException("Bed is already occupied!");

            var guest = new Guest
            {
                OwnerId = dto.OwnerId,
                BranchId = dto.BranchId,
                FloorId = dto.FloorId != Guid.Empty ? dto.FloorId : bed.FloorId,
                RoomId = dto.RoomId,
                BedId = dto.BedId,
                FullName = dto.FullName,
                Email = dto.Email.Trim(),
                PhoneNumber = dto.PhoneNumber,
                EmergencyContact = dto.EmergencyContact,
                Age = dto.Age,
                Gender = dto.Gender,
                Occupation = dto.Occupation,
                WorkplaceOrCollege = dto.WorkplaceOrCollege,
                TenantType = dto.TenantType,
                AdvanceAmount = dto.AdvanceAmount,
                MonthlyRent = dto.MonthlyRent > 0 ? dto.MonthlyRent : bed.BedRent,
                DailyRent = dto.DailyRent > 0 ? dto.DailyRent : bed.DailyRate,
                JoiningDate = dto.JoiningDate != default ? dto.JoiningDate : DateTime.UtcNow.Date,
                ExpectedVacatingDate = dto.ExpectedVacatingDate,
                DaysBeforeVacateReminder = dto.DaysBeforeVacateReminder > 0 ? dto.DaysBeforeVacateReminder : 15,
                ProofDocumentType = dto.ProofDocumentType,
                ProofDocumentNumber = dto.ProofDocumentNumber,
                Status = "Active"
            };

            var id = await _guestRepo.CheckInGuestWithOccupancyAsync(guest);
            var created = (await _guestRepo.GetByIdAsync(id)) ?? guest;

            // Trigger WhatsApp Check-In Notification
            try
            {
                var branch = await _branchRepo.GetByIdAsync(created.BranchId);
                var room = await _roomRepo.GetByIdAsync(created.RoomId);
                var branchName = branch?.BranchName ?? "Your PG";
                var roomNum = room?.RoomNumber ?? "Room";
                var bedNum = bed?.BedNumber ?? "Bed";

                await _whatsApp.SendCheckInNotificationAsync(created, branchName, roomNum, bedNum);
                await _email.SendCheckInEmailAsync(created, branchName, roomNum, bedNum);
            }
            catch
            {
                // Notification failure should not abort successful check-in
            }

            return created;
        }

        public async Task<bool> VacateGuestAsync(Guid guestId, VacateSettlementDto dto)
        {
            var guest = await _guestRepo.GetByIdAsync(guestId);
            if (guest == null) return false;

            await _guestRepo.SettleAndVacateAsync(guestId, dto.ReturnedAmount, dto.DeductionsAmount, dto.SettlementNotes, dto.ActualVacatingDate);
            await _roomRepo.UpdateBedOccupancyAsync(guest.BedId, false, "Available");
            return true;
        }
    }
}
