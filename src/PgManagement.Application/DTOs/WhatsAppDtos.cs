namespace PgManagement.Application.DTOs
{
    public class SendWhatsAppMessageDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // CheckIn, Payment, Reminder
    }

    public class WhatsAppNotificationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? DirectUrl { get; set; }
        public string? RecipientPhone { get; set; }
    }
}
