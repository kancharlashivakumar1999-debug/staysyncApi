using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;
using PgManagement.Domain.Entities;

namespace PgManagement.Infrastructure.Services
{
    public class WhatsAppNotificationService : IWhatsAppNotificationService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<WhatsAppNotificationService> _logger;
        private readonly ILedgerRepository _ledgerRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IGuestRepository _guestRepo;
        private readonly HttpClient _httpClient;

        public WhatsAppNotificationService(
            IConfiguration config,
            ILogger<WhatsAppNotificationService> logger,
            ILedgerRepository ledgerRepo,
            IBranchRepository branchRepo,
            IGuestRepository guestRepo,
            HttpClient httpClient)
        {
            _config = config;
            _logger = logger;
            _ledgerRepo = ledgerRepo;
            _branchRepo = branchRepo;
            _guestRepo = guestRepo;
            _httpClient = httpClient;
        }

        public async Task<WhatsAppNotificationResultDto> SendCheckInNotificationAsync(
            Guest guest,
            string branchName,
            string roomNumber,
            string bedNumber)
        {
            var message =
                $"🎉 *Welcome to {branchName}!*\n\n" +
                $"Dear *{guest.FullName}*,\n" +
                $"Your check-in has been successfully confirmed.\n\n" +
                $"📋 *Resident Code:* {guest.GuestCode}\n" +
                $"🚪 *Room:* {roomNumber} | *Bed:* {bedNumber}\n" +
                $"💰 *Monthly Rent:* ₹{guest.MonthlyRent:N0}\n" +
                $"💼 *Advance Deposit:* ₹{guest.AdvanceAmount:N0}\n" +
                $"📅 *Joining Date:* {guest.JoiningDate:dd MMM yyyy}\n\n" +
                $"For any support or maintenance requests, please contact management.\n" +
                $"We wish you a pleasant and comfortable stay with us! 🌟\n\n" +
                $"— *From {branchName} Management*";

            return await DeliverWhatsAppAsync(guest.PhoneNumber, message, "CheckIn", guest.FullName);
        }

        public async Task<WhatsAppNotificationResultDto> SendPaymentReceiptNotificationAsync(
            TenantPayment payment,
            string guestName,
            string guestPhone,
            string branchName,
            string? roomNumber,
            string? bedNumber)
        {
            var roomInfo = !string.IsNullOrWhiteSpace(roomNumber) ? $"Room {roomNumber} • Bed {bedNumber}" : "PG Resident";

            var message =
                $"🧾 *Payment Receipt Confirmation*\n\n" +
                $"Dear *{guestName}* ({roomInfo}),\n" +
                $"We have received your payment with thanks.\n\n" +
                $"💵 *Amount Paid:* ₹{payment.AmountPaid:N0}\n" +
                $"🔖 *Receipt Code:* {payment.PaymentCode}\n" +
                $"💳 *Payment Mode:* {payment.PaymentMode}\n" +
                $"📅 *Payment Date:* {payment.PaymentDate:dd MMM yyyy, hh:mm tt}\n" +
                $"📆 *Billing Month:* {payment.PaymentMonth}\n";

            if (payment.AdvanceAllocated > 0)
                message += $"🔹 *Advance Cleared:* ₹{payment.AdvanceAllocated:N0}\n";
            if (payment.RentAllocated > 0)
                message += $"🔹 *Rent Cleared:* ₹{payment.RentAllocated:N0}\n";

            message += $"📊 *Remaining Pending Balance:* ₹{payment.PendingBalanceAfter:N0}\n\n";
            message += $"Thank you for choosing *{branchName}*!\n\n" +
                       $"— *From {branchName} Accounts*";

            return await DeliverWhatsAppAsync(guestPhone, message, "Payment", guestName);
        }

        public async Task<int> SendMonthlyDueRemindersAsync(Guid ownerId, Guid? branchId = null, bool forceSend = false)
        {
            var dayOfMonth = DateTime.UtcNow.Day;

            // Only send automatically between 1st and 5th of the month unless forceSend is requested
            if (!forceSend && (dayOfMonth < 1 || dayOfMonth > 5))
            {
                _logger.LogInformation("WhatsApp rent reminders run between 1st to 5th of each month. Current day: {Day}. Skipping automatic reminder.", dayOfMonth);
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
                if (guest == null || string.IsNullOrWhiteSpace(guest.PhoneNumber)) continue;

                var branchName = charge.BranchName ?? "Your PG";
                var roomNumber = charge.RoomNumber ?? "Your Room";
                var bedNumber = charge.BedNumber ?? "";

                var reminderMsg =
                    $"⏰ *Monthly Rent Reminder - {currentMonthName}*\n\n" +
                    $"Dear *{charge.GuestName}*,\n" +
                    $"This is a friendly reminder regarding your outstanding rent for Room {roomNumber} (Bed {bedNumber}).\n\n" +
                    $"📌 *Charge Details:* {charge.ChargeType} ({charge.BillingMonth})\n" +
                    $"💰 *Due Amount:* ₹{charge.Amount:N0}\n" +
                    $"📅 *Due Date:* {charge.DueDate:dd MMM yyyy}\n\n" +
                    $"Kindly clear the dues at your earliest convenience to avoid late fees.\n\n" +
                    $"_(If paid please ignore)_\n\n" +
                    $"— *From {branchName} Management*";

                var res = await DeliverWhatsAppAsync(guest.PhoneNumber, reminderMsg, "DueReminder", charge.GuestName ?? "Resident");
                if (res.Success) sentCount++;
            }

            return sentCount;
        }

        public async Task<WhatsAppNotificationResultDto> SendCustomNotificationAsync(SendWhatsAppMessageDto dto)
        {
            return await DeliverWhatsAppAsync(dto.PhoneNumber, dto.Message, dto.Type, dto.RecipientName);
        }

        private async Task<WhatsAppNotificationResultDto> DeliverWhatsAppAsync(
            string phoneNumber,
            string message,
            string category,
            string recipientName)
        {
            // Sanitize phone number (strip whitespace, dashes, plus)
            var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            if (cleanPhone.Length == 10)
            {
                cleanPhone = "91" + cleanPhone; // Default to India (+91) if 10-digit
            }

            var encodedText = Uri.EscapeDataString(message);
            var directClickUrl = $"https://api.whatsapp.com/send?phone={cleanPhone}&text={encodedText}";

            // Check if Cloud API / Webhook provider is configured
            var apiUrl = _config["WhatsApp:ApiUrl"];
            var apiKey = _config["WhatsApp:ApiKey"];

            if (!string.IsNullOrWhiteSpace(apiUrl) && !string.IsNullOrWhiteSpace(apiKey))
            {
                try
                {
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var payload = new
                    {
                        to = cleanPhone,
                        type = "text",
                        recipientName = recipientName,
                        category = category,
                        text = new { body = message }
                    };

                    var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("WhatsApp delivered successfully via Gateway to {Phone}", cleanPhone);
                        return new WhatsAppNotificationResultDto
                        {
                            Success = true,
                            Message = "WhatsApp message dispatched successfully via Gateway.",
                            DirectUrl = directClickUrl,
                            RecipientPhone = cleanPhone
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Direct WhatsApp API Gateway error. Falling back to universal URL link.");
                }
            }

            // Universal fallback: Return Direct WhatsApp Click-to-Chat URL
            _logger.LogInformation("WhatsApp Alert prepared for {Recipient} ({Phone}):\n{Message}", recipientName, cleanPhone, message);

            return new WhatsAppNotificationResultDto
            {
                Success = true,
                Message = "WhatsApp alert created & ready for delivery.",
                DirectUrl = directClickUrl,
                RecipientPhone = cleanPhone
            };
        }
    }
}
