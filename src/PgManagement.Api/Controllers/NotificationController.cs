using Microsoft.AspNetCore.Mvc;
using PgManagement.Application.DTOs;
using PgManagement.Application.Interfaces;

namespace PgManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IWhatsAppNotificationService _whatsApp;
        private readonly IEmailNotificationService _email;

        public NotificationController(
            IWhatsAppNotificationService whatsApp,
            IEmailNotificationService email)
        {
            _whatsApp = whatsApp;
            _email = email;
        }

        /// <summary>
        /// Sends rent payment reminders via Email to all tenants with pending dues.
        /// By default, runs only between the 1st and 5th of the month.
        /// Pass forceSend=true to override and send immediately.
        /// </summary>
        [HttpPost("email/due-reminders")]
        public async Task<ActionResult> SendEmailDueReminders(
            [FromQuery] Guid ownerId,
            [FromQuery] Guid? branchId = null,
            [FromQuery] bool forceSend = false)
        {
            if (ownerId == Guid.Empty)
            {
                return BadRequest(new { message = "OwnerId is required." });
            }

            var count = await _email.SendMonthlyDueEmailRemindersAsync(ownerId, branchId, forceSend);
            return Ok(new
            {
                success = true,
                remindersSent = count,
                message = count > 0
                    ? $"Successfully dispatched {count} rent reminder email(s)."
                    : (forceSend ? "No pending dues or guest emails found to remind." : "Rent reminders only run between the 1st and 5th of the month, or no pending dues were found.")
            });
        }

        /// <summary>
        /// Sends rent payment reminders to all tenants with pending dues via WhatsApp.
        /// By default, runs only between the 1st and 5th of the month.
        /// Pass forceSend=true to override and send immediately.
        /// </summary>
        [HttpPost("whatsapp/due-reminders")]
        public async Task<ActionResult> SendDueReminders(
            [FromQuery] Guid ownerId,
            [FromQuery] Guid? branchId = null,
            [FromQuery] bool forceSend = false)
        {
            if (ownerId == Guid.Empty)
            {
                return BadRequest(new { message = "OwnerId is required." });
            }

            var count = await _whatsApp.SendMonthlyDueRemindersAsync(ownerId, branchId, forceSend);
            return Ok(new
            {
                success = true,
                remindersSent = count,
                message = count > 0 
                    ? $"Successfully dispatched {count} rent reminder(s) via WhatsApp."
                    : (forceSend ? "No pending dues found to remind." : "Rent reminders only run between the 1st and 5th of the month, or no pending dues were found.")
            });
        }

        /// <summary>
        /// Send a single custom WhatsApp message or notification.
        /// </summary>
        [HttpPost("whatsapp/send")]
        public async Task<ActionResult> SendCustomMessage([FromBody] SendWhatsAppMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber) || string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest(new { message = "PhoneNumber and Message are required." });
            }

            var result = await _whatsApp.SendCustomNotificationAsync(dto);
            return Ok(result);
        }
    }
}
