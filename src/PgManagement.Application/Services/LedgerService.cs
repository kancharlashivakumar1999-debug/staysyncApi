using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Services
{
    public class LedgerService : ILedgerService
    {
        private readonly ILedgerRepository _ledgerRepo;
        private readonly IGuestRepository _guestRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IWhatsAppNotificationService _whatsApp;
        private readonly IEmailNotificationService _email;

        public LedgerService(
            ILedgerRepository ledgerRepo,
            IGuestRepository guestRepo,
            IRoomRepository roomRepo,
            IBranchRepository branchRepo,
            IWhatsAppNotificationService whatsApp,
            IEmailNotificationService email)
        {
            _ledgerRepo = ledgerRepo;
            _guestRepo = guestRepo;
            _roomRepo = roomRepo;
            _branchRepo = branchRepo;
            _whatsApp = whatsApp;
            _email = email;
        }

        public async Task<TenantChargeDto> AddChargeAsync(CreateChargeDto dto)
        {
            var charge = new TenantCharge
            {
                GuestId = dto.GuestId,
                OwnerId = dto.OwnerId,
                BranchId = dto.BranchId,
                ChargeType = dto.ChargeType,
                BillingMonth = dto.BillingMonth,
                Amount = dto.Amount,
                DueDate = dto.DueDate,
                Description = dto.Description,
                IsPaid = false
            };

            var id = await _ledgerRepo.CreateChargeAsync(charge);
            return new TenantChargeDto
            {
                ChargeId = id,
                ChargeCode = charge.ChargeCode,
                GuestId = charge.GuestId,
                OwnerId = charge.OwnerId,
                BranchId = charge.BranchId,
                ChargeType = charge.ChargeType,
                BillingMonth = charge.BillingMonth,
                Amount = charge.Amount,
                DueDate = charge.DueDate,
                IsPaid = charge.IsPaid,
                Description = charge.Description,
                CreatedAt = charge.CreatedAt
            };
        }

        public async Task<TenantPaymentDto> RecordPaymentAsync(RecordPaymentDto dto)
        {
            var payment = new TenantPayment
            {
                ChargeId = dto.ChargeId,
                GuestId = dto.GuestId,
                OwnerId = dto.OwnerId,
                BranchId = dto.BranchId,
                AmountPaid = dto.AmountPaid,
                PaymentMode = dto.PaymentMode,
                TransactionReference = dto.TransactionReference,
                ReceivedBy = dto.ReceivedBy,
                ReceiptNotes = dto.ReceiptNotes,
                PaymentMonth = !string.IsNullOrWhiteSpace(dto.PaymentMonth) ? dto.PaymentMonth : DateTime.UtcNow.ToString("yyyy-MM"),
                DaysPaid = dto.DaysPaid > 0 ? dto.DaysPaid : 30,
                AdvanceAllocated = dto.AdvanceAllocated,
                RentAllocated = dto.RentAllocated,
                PendingBalanceAfter = dto.PendingBalanceAfter,
                PaymentDate = DateTime.UtcNow
            };

            var id = await _ledgerRepo.RecordPaymentAsync(payment);
            if (dto.ChargeId.HasValue && dto.ChargeId.Value != Guid.Empty)
            {
                await _ledgerRepo.MarkChargeAsPaidAsync(dto.ChargeId.Value);
            }

            // WhatsApp Notification for Payment Receipt
            try
            {
                var guest = await _guestRepo.GetByIdAsync(payment.GuestId);
                if (guest != null && !string.IsNullOrWhiteSpace(guest.PhoneNumber))
                {
                    var branch = await _branchRepo.GetByIdAsync(payment.BranchId);
                    var room = await _roomRepo.GetByIdAsync(guest.RoomId);
                    var bed = await _roomRepo.GetBedByIdAsync(guest.BedId);

                    var branchName = branch?.BranchName ?? "Your PG";
                    var roomNumber = room?.RoomNumber;
                    var bedNumber = bed?.BedNumber;

                    await _whatsApp.SendPaymentReceiptNotificationAsync(
                        payment,
                        guest.FullName,
                        guest.PhoneNumber,
                        branchName,
                        roomNumber,
                        bedNumber);

                    if (!string.IsNullOrWhiteSpace(guest.Email))
                    {
                        await _email.SendPaymentReceiptEmailAsync(
                            payment,
                            guest.FullName,
                            guest.Email,
                            branchName,
                            roomNumber,
                            bedNumber);
                    }
                }
            }
            catch
            {
                // Payment recorded successfully; notification failure should not block or rollback
            }

            return new TenantPaymentDto
            {
                PaymentId = id,
                PaymentCode = payment.PaymentCode,
                ChargeId = payment.ChargeId,
                GuestId = payment.GuestId,
                BranchId = payment.BranchId,
                AmountPaid = payment.AmountPaid,
                PaymentMode = payment.PaymentMode,
                TransactionReference = payment.TransactionReference,
                PaymentDate = payment.PaymentDate,
                ReceivedBy = payment.ReceivedBy,
                ReceiptNotes = payment.ReceiptNotes,
                PaymentMonth = payment.PaymentMonth,
                DaysPaid = payment.DaysPaid,
                AdvanceAllocated = payment.AdvanceAllocated,
                RentAllocated = payment.RentAllocated,
                PendingBalanceAfter = payment.PendingBalanceAfter
            };
        }

        public async Task<TenantLedgerSummaryDto> GetGuestLedgerAsync(Guid guestId) =>
            await _ledgerRepo.GetGuestLedgerSummaryAsync(guestId);

        public async Task<TenantLedgerSummaryDto?> GetBedLedgerAsync(Guid bedId) =>
            await _ledgerRepo.GetBedLedgerSummaryAsync(bedId);

        public async Task<IEnumerable<TenantChargeDto>> GetPendingDuesAsync(Guid ownerId, Guid? branchId = null) =>
            await _ledgerRepo.GetPendingChargesByOwnerIdAsync(ownerId, branchId);

        public async Task<IEnumerable<TenantPaymentDto>> GetRecentPaymentsAsync(Guid ownerId, Guid? branchId = null) =>
            await _ledgerRepo.GetRecentPaymentsByOwnerIdAsync(ownerId, branchId);
    }
}
