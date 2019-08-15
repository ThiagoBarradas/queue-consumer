using QueueConsumer.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace QueueConsumer.Queue
{
    public class QueueManager
    {
        private IModel Channel;

        private ushort PrefetchCount = 10;

        public event Action<string, ulong> ReceiveMessage;

        public QueueConsumerConfiguration Configuration { get; set; }

        public QueueManager(QueueConsumerConfiguration configuration)
        {
            this.Configuration = configuration;
            this.TryConnect();
        }

        private void Configure()
        {
            var consumer = new EventingBasicConsumer(this.Channel);

            consumer.Received += (model, eventArgs) =>
            {
                var message = Encoding.UTF8.GetString(eventArgs.Body);
                ReceiveMessage?.Invoke(message, eventArgs.DeliveryTag);
            };

            Channel.BasicQos(0, this.PrefetchCount, false);
            Channel.BasicConsume(queue: this.Configuration.QueueName, consumer: consumer);
        }

        public void AddFailedMessage(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            this.Channel.BasicPublish(
                exchange: "", 
                routingKey: this.Configuration.QueueNameForFailed, 
                basicProperties: null, body: 
                buffer);
        }

        public void Ack(ulong deliveryTag)
        {
            Channel.BasicAck(deliveryTag, false);
        }

        public void NAck(ulong deliveryTag, bool requeued = true)
        {
            Channel.BasicNack(deliveryTag, false, requeued);
        }

        private void TryConnect()
        {
            var connectionFactory = new ConnectionFactory()
            {
                TopologyRecoveryEnabled = true,
                Uri = new Uri(this.Configuration.QueueConnectionString)
            };

            this.Channel = ChannelFactory.Create(connectionFactory);
            this.Configure();

            Console.WriteLine("Connected!");
        }

        public void TryReconnect()
        {
            try
            {
                ChannelFactory.CloseConnection();
                this.TryConnect();
            }
            catch (Exception) { }
        }
    }
}
