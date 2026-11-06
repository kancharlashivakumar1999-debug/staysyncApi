using PgManagement.Domain.Entities;

namespace PgManagement.Application.Interfaces
{
    public interface IEmailNotificationService
    {
        Task<bool> SendCheckInEmailAsync(
            Guest guest,
            string branchName,
            string roomNumber,
            string bedNumber);

        Task<bool> SendPaymentReceiptEmailAsync(
            TenantPayment payment,
            string guestName,
            string guestEmail,
            string branchName,
            string? roomNumber,
            string? bedNumber);

        Task<int> SendMonthlyDueEmailRemindersAsync(
            Guid ownerId,
            Guid? branchId = null,
            bool forceSend = false);

        Task<bool> SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody,
            string? recipientName = null);
    }
}
