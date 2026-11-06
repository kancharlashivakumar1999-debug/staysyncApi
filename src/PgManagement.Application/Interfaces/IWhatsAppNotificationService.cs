using PgManagement.Application.DTOs;
using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IWhatsAppNotificationService
    {
        Task<WhatsAppNotificationResultDto> SendCheckInNotificationAsync(Guest guest, string branchName, string roomNumber, string bedNumber);
        Task<WhatsAppNotificationResultDto> SendPaymentReceiptNotificationAsync(TenantPayment payment, string guestName, string guestPhone, string branchName, string? roomNumber, string? bedNumber);
        Task<int> SendMonthlyDueRemindersAsync(Guid ownerId, Guid? branchId = null, bool forceSend = false);
        Task<WhatsAppNotificationResultDto> SendCustomNotificationAsync(SendWhatsAppMessageDto dto);
    }
}
