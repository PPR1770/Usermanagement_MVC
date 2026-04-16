namespace ChatApp.Models.ChatModels
{

    public class ChatMessage
    {
        public int Id { get; set; }

        // Sender & Receiver
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }

        // Message Content
        public string Message { get; set; }

        // Time
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Optional (for next phases)
        public bool IsRead { get; set; } = false;
        public bool IsDelivered { get; set; } = false;

        public string? FileUrl { get; set; }
        public string? FileType { get; set; } // image/pdf/video
        public string? FileName { get; set; }

        public ApplicationUser Sender { get; set; }
        public ApplicationUser Receiver { get; set; }
    }
}
