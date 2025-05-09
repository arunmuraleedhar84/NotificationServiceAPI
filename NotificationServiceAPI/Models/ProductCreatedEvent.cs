namespace NotificationServiceAPI.Models
{
    public class ProductCreatedEvent
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int Stock { get; set; }

    }
}
