namespace QueueConsumer.Models
{
    public class QueueMessage
    {
        public QueueMessage() { }

        public QueueMessage(string content, ulong deliveryTag)
        {
            this.Content = content;
            this.DeliveryTag = deliveryTag;
        }

        public string Content { get; set; }

        public ulong DeliveryTag { get; set; }
    }
}
