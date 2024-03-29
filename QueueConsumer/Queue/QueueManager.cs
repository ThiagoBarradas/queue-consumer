using NewRelic.Api.Agent;

using Newtonsoft.Json;

using QueueConsumer.Models;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

using System;
using System.Collections.Generic;
using System.Text;

namespace QueueConsumer.Queue;

public class QueueManager : IDisposable
{
    private IModel Channel;

    public Action<string, int, string, ulong> ReceiveMessage;

    public QueueConsumerConfiguration Configuration { get; set; }

    public QueueManager(QueueConsumerConfiguration configuration)
    {
        this.Configuration = configuration;
    }

    public void AddRetryMessage(string message, int retryCount, string url)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        this.Channel.BasicPublish(
            exchange: "",
            routingKey: $"{this.Configuration.QueueName}-retry",
            basicProperties: new BasicProperties
            {
                Persistent = true,
                Headers = this.GetHeaders(retryCount, url)
            },
            body: buffer);
    }

    public void AddDeadMessage(string message, string url)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        this.Channel.BasicPublish(
            exchange: "",
            routingKey: $"{this.Configuration.QueueName}-dead",
            basicProperties: new BasicProperties
            {
                Persistent = true,
                Headers = this.GetHeaders(null, url)
            },
            body: buffer);
    }

    public void AddMessage(string message, string url)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        this.Channel.BasicPublish(
            exchange: "",
            routingKey: this.Configuration.QueueName,
            basicProperties: new BasicProperties
            {
                Persistent = true,
                Headers = this.GetHeaders(null, url)
            },
            body: buffer);
    }

    public void Ack(ulong deliveryTag)
    {
        Channel.BasicAck(deliveryTag, false);
    }

    public void NAck(ulong deliveryTag, bool requeued = true)
    {
        Channel.BasicNack(deliveryTag, false, requeued);
    }

    [Transaction]
    public void TryConnect()
    {
        try
        {
            ChannelFactory.CloseConnection();

            var connectionFactory = new ConnectionFactory()
            {
                RequestedHeartbeat = 30,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                TopologyRecoveryEnabled = true,
                Uri = new Uri(this.Configuration.QueueConnectionString)
            };

            this.Channel = ChannelFactory.Create(connectionFactory);
            Logger.LogLineWithLevel("OK", "TryConnect: Successfully connected!");

            this.Configure();
        }
        catch (Exception e)
        {
            Logger.LogLineWithLevel("ERROR", "TryConnect: An exception occurred");
            Logger.LogLineWithLevel("ERROR", "Message: {0}", e.Message);
            throw;
        }
    }

    private void Configure()
    {
        if (this.Configuration.CreateQueue)
        {
            Logger.LogLineWithLevel("OK", "CreateQueue with TTL: {0} ", this.Configuration.RetryTTL);
            this.Channel.ExchangeDeclare($"{this.Configuration.QueueName}-exchange", "direct", true);
            this.Channel.QueueDeclare(this.Configuration.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            this.Channel.QueueBind(this.Configuration.QueueName,
                                    $"{this.Configuration.QueueName}-exchange",
                                    $"{this.Configuration.QueueName}-routing-key", null);

            var retryQueueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", $"{this.Configuration.QueueName}-exchange"},
                { "x-dead-letter-routing-key", $"{this.Configuration.QueueName}-routing-key"},
                { "x-message-ttl", this.Configuration.RetryTTL}
            };

            this.Channel.ExchangeDeclare($"{this.Configuration.QueueName}-retry-exchange", "direct", true);
            this.Channel.QueueDeclare($"{this.Configuration.QueueName}-retry", true, false, false, retryQueueArgs);
            this.Channel.QueueBind($"{this.Configuration.QueueName}-retry",
                                    $"{this.Configuration.QueueName}-retry-exchange",
                                    $"{this.Configuration.QueueName}-retry-routing-key", null);

            this.Channel.ExchangeDeclare($"{this.Configuration.QueueName}-dead-exchange", "direct", true);
            this.Channel.QueueDeclare($"{this.Configuration.QueueName}-dead", true, false, false, null);
            this.Channel.QueueBind($"{this.Configuration.QueueName}-dead",
                                    $"{this.Configuration.QueueName}-dead-exchange",
                                    $"{this.Configuration.QueueName}-dead-routing-key", null);
        }

        if (this.Configuration.PopulateQueueQuantity > 0)
        {
            Logger.LogLineWithLevel("OK", "PopulateQueueQuantity: {0} ", this.Configuration.PopulateQueueQuantity);
            Console.CursorVisible = false;
            for (int i = 0; i < this.Configuration.PopulateQueueQuantity; i++)
            {
                this.AddMessage(JsonConvert.SerializeObject(new { message = i }), null);
            }
            Console.CursorVisible = true;
            Console.WriteLine();
        }

        var consumer = new EventingBasicConsumer(this.Channel);

        consumer.Received += this.Received;

        Channel.BasicQos(0, (ushort)this.Configuration.MaxThreads, false);
        Channel.BasicConsume(queue: this.Configuration.QueueName, consumer: consumer);
    }

    private void Received(object model, BasicDeliverEventArgs eventArgs)
    {
        object retryHeaderValue = null;
        object urlHeaderValue = null;
        try
        {
            eventArgs?.BasicProperties?.Headers?.TryGetValue("retry_count", out retryHeaderValue);
            eventArgs?.BasicProperties?.Headers?.TryGetValue("url", out urlHeaderValue);
        }
        catch (Exception) { }

        int retryCount = retryHeaderValue != null ? (int)retryHeaderValue : 0;
        string url = urlHeaderValue != null ? urlHeaderValue.ToString() : null;

        var message = Encoding.UTF8.GetString(eventArgs.Body);
        ReceiveMessage?.Invoke(message, retryCount, url, eventArgs.DeliveryTag);
    }

    private Dictionary<string, object> GetHeaders(int? retryCount, string url)
    {
        var headers = new Dictionary<string, object>();

        if (retryCount != null)
        {
            headers.Add("retry_count", retryCount.Value);
        };

        if (string.IsNullOrWhiteSpace(url) == false)
        {
            headers.Add("url", url);
        }

        return headers;
    }

    public void Dispose()
    {
        ChannelFactory.CloseConnection();
    }
}