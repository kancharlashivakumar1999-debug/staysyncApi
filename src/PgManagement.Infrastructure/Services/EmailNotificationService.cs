using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Infrastructure.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly ILedgerRepository _ledgerRepo;
        private readonly IGuestRepository _guestRepo;

        public EmailNotificationService(
            IConfiguration config,
            ILogger<EmailNotificationService> logger,
            ILedgerRepository ledgerRepo,
            IGuestRepository guestRepo)
        {
            _config = config;
            _logger = logger;
            _ledgerRepo = ledgerRepo;
            _guestRepo = guestRepo;
        }

        public async Task<bool> SendCheckInEmailAsync(
            Guest guest,
            string branchName,
            string roomNumber,
            string bedNumber)
        {
            if (string.IsNullOrWhiteSpace(guest.Email))
            {
                _logger.LogInformation("No email provided for guest {Name}. Skipping check-in email.", guest.FullName);
                return false;
            }

            var subject = $"Welcome to {branchName}! - Check-in Confirmation ({guest.GuestCode})";
            var body = $"""
                <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 10px;">
                    <h2 style="color: #2563eb; text-align: center;">Welcome to {branchName}! 🎉</h2>
                    <p>Dear <strong>{guest.FullName}</strong>,</p>
                    <p>Your check-in has been successfully registered. Here are your onboarding details:</p>
                    <table style="width: 100%; border-collapse: collapse; margin: 15px 0;">
                        <tr style="background: #f8fafc;"><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Resident Code:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">{guest.GuestCode}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Room & Bed:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">Room {roomNumber} • Bed {bedNumber}</td></tr>
                        <tr style="background: #f8fafc;"><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Monthly Rent:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">₹{guest.MonthlyRent:N0}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Advance Deposit:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">₹{guest.AdvanceAmount:N0}</td></tr>
                        <tr style="background: #f8fafc;"><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Joining Date:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">{guest.JoiningDate:dd MMM yyyy}</td></tr>
                    </table>
                    <p>For any maintenance or support inquiries, feel free to contact management.</p>
                    <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;" />
                    <p style="font-size: 0.9em; color: #64748b; text-align: center;">— From <strong>{branchName} Management</strong></p>
                </div>
                """;

            return await SendEmailAsync(guest.Email, subject, body, guest.FullName);
        }

        public async Task<bool> SendPaymentReceiptEmailAsync(
            TenantPayment payment,
            string guestName,
            string guestEmail,
            string branchName,
            string? roomNumber,
            string? bedNumber)
        {
            if (string.IsNullOrWhiteSpace(guestEmail))
            {
                _logger.LogInformation("No email provided for guest {Name}. Skipping payment receipt email.", guestName);
                return false;
            }

            var subject = $"Payment Receipt: ₹{payment.AmountPaid:N0} received - {payment.PaymentCode}";
            var roomInfo = !string.IsNullOrWhiteSpace(roomNumber) ? $"Room {roomNumber} • Bed {bedNumber}" : "PG Resident";

            var body = $"""
                <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 10px;">
                    <h2 style="color: #16a34a; text-align: center;">Payment Receipt Confirmation 🧾</h2>
                    <p>Dear <strong>{guestName}</strong> ({roomInfo}),</p>
                    <p>Thank you! We have received your payment with the following details:</p>
                    <table style="width: 100%; border-collapse: collapse; margin: 15px 0;">
                        <tr style="background: #f8fafc;"><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Receipt Code:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">{payment.PaymentCode}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Amount Paid:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0; color: #16a34a; font-weight: bold; font-size: 1.1em;">₹{payment.AmountPaid:N0}</td></tr>
                        <tr style="background: #f8fafc;"><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Payment Mode:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">{payment.PaymentMode}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Payment Date:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">{payment.PaymentDate:dd MMM yyyy, hh:mm tt}</td></tr>
                        <tr style="background: #f8fafc;"><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Billing Month:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">{payment.PaymentMonth}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Remaining Balance:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">₹{payment.PendingBalanceAfter:N0}</td></tr>
                    </table>
                    <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;" />
                    <p style="font-size: 0.9em; color: #64748b; text-align: center;">— From <strong>{branchName} Accounts</strong></p>
                </div>
                """;

            return await SendEmailAsync(guestEmail, subject, body, guestName);
        }

        public async Task<int> SendMonthlyDueEmailRemindersAsync(
            Guid ownerId,
            Guid? branchId = null,
            bool forceSend = false)
        {
            var dayOfMonth = DateTime.UtcNow.Day;

            // Only send between 1st and 5th of the month unless forceSend is specified
            if (!forceSend && (dayOfMonth < 1 || dayOfMonth > 5))
            {
                _logger.LogInformation("Email rent reminders run between 1st to 5th of each month. Current day: {Day}. Skipping automatic reminder.", dayOfMonth);
                return 0;
            }

            var pendingCharges = (await _ledgerRepo.GetPendingChargesByOwnerIdAsync(ownerId, branchId)).ToList();
            if (!pendingCharges.Any()) return 0;

            int sentCount = 0;
            var currentMonthName = DateTime.UtcNow.ToString("MMMM yyyy");

            foreach (var charge in pendingCharges)
            {
                if (charge.IsPaid) continue;

                var guest = await _guestRepo.GetByIdAsync(charge.GuestId);
                if (guest == null || string.IsNullOrWhiteSpace(guest.Email)) continue;

                var branchName = charge.BranchName ?? "Your PG";
                var roomNumber = charge.RoomNumber ?? "";
                var bedNumber = charge.BedNumber ?? "";

                var subject = $"Monthly Rent Reminder - {currentMonthName} ({branchName})";
                var body = $"""
                    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 10px;">
                        <h2 style="color: #dc2626; text-align: center;">Monthly Rent Due Reminder ⏰</h2>
                        <p>Dear <strong>{charge.GuestName}</strong>,</p>
                        <p>This is a reminder regarding your outstanding payment for Room {roomNumber} (Bed {bedNumber}):</p>
                        <table style="width: 100%; border-collapse: collapse; margin: 15px 0;">
                            <tr style="background: #f8fafc;"><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Charge:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">{charge.ChargeType} ({charge.BillingMonth})</td></tr>
                            <tr><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Due Amount:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0; color: #dc2626; font-weight: bold; font-size: 1.1em;">₹{charge.Amount:N0}</td></tr>
                            <tr style="background: #f8fafc;"><td style="padding: 8px; border: 1px solid #e2e8f0;"><strong>Due Date:</strong></td><td style="padding: 8px; border: 1px solid #e2e8f0;">{charge.DueDate:dd MMM yyyy}</td></tr>
                        </table>
                        <p>Kindly settle the balance at your earliest convenience.</p>
                        <p style="font-style: italic; color: #64748b;">(If paid please ignore)</p>
                        <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;" />
                        <p style="font-size: 0.9em; color: #64748b; text-align: center;">— From <strong>{branchName} Management</strong></p>
                    </div>
                    """;

                var success = await SendEmailAsync(guest.Email, subject, body, charge.GuestName);
                if (success) sentCount++;
            }

            return sentCount;
        }

        public async Task<bool> SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody,
            string? recipientName = null)
        {
            var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var portStr = _config["Email:SmtpPort"] ?? "587";
            var senderEmail = _config["Email:SenderEmail"];
            var appPassword = _config["Email:AppPassword"]; // 16-character Gmail App Password
            var senderDisplayName = _config["Email:SenderDisplayName"] ?? "PG Management";

            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword))
            {
                _logger.LogWarning("Gmail SMTP credentials not fully configured (Email:SenderEmail or Email:AppPassword missing). Email to {To} logged only.", toEmail);
                return false;
            }

            if (!int.TryParse(portStr, out var port))
            {
                port = 587;
            }

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail, appPassword)
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderDisplayName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(new MailAddress(toEmail, recipientName ?? toEmail));

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully via Gmail to {To} with Subject '{Subject}'", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} via Gmail SMTP", toEmail);
                return false;
            }
        }
    }
}
