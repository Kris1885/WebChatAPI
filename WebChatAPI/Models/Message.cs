namespace WebChatAPI.Models
{
    public class Message
    {
        public string Text { get; set; }
        public required int ReceiverId { get; set; }
        public required int FromId { get; set; }
    }
}
