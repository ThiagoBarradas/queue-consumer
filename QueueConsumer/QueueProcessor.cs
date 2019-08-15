using QueueConsumer.Models;
using QueueConsumer.Notification;
using QueueConsumer.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QueueConsumer
{
    public class QueueMessageProcessor : IDisposable
    {
        public List<Task> Threads = new List<Task>();

        public bool CanAddThread() => (this.Threads.Where(t => t.IsCompleted == false).Count() < this.Configuration.MaxThreads);

        public QueueManager QueueManager { get; set; }

        public QueueConsumerConfiguration Configuration { get; set; }

        public QueueMessageProcessor(QueueConsumerConfiguration configuration)
        {
            this.Configuration = configuration;
            this.QueueManager = new QueueManager(configuration);
        }

        public void Execute()
        {
            try
            {
                this.QueueManager.TryConnect();
                this.QueueManager.ReceiveMessage += (message, retryCount, deliveryTag) =>
                {
                    while (this.CanAddThread() == false)
                    {
                        Thread.Sleep(200);
                        this.Threads.RemoveAll(t => t.IsCompleted);
                    }

                    this.Threads.Add(HandleReceivedMessage(message, retryCount, deliveryTag));
                };

                while (true) { };
            }
            catch (Exception e)
            {
                Logger.LogLineWithLevel("ERROR", "Execute: An exception occurred");
                Logger.LogLineWithLevel("ERROR", "Message: {0}", e.Message);
                Thread.Sleep(1000);
            }
        }

        public async Task HandleReceivedMessage(string message, int retryCount, ulong deliveryTag)
        {
            Logger.LogLineWithLevel("OK", "HandleReceivedMessage: Processing message [{0}] started", deliveryTag);
            var success = await SendNotificationClient.SendNotification(this.Configuration, message);

            if (success)
            {
                this.QueueManager.Ack(deliveryTag);
                Logger.LogLineWithLevel("OK", "HandleReceivedMessage: Processing message [{0}] sucesfully", deliveryTag);
            }
            else if (retryCount < this.Configuration.RetryCount)
            {
                this.QueueManager.AddRetryMessage(message, retryCount + 1);
                this.QueueManager.Ack(deliveryTag);
                Logger.LogLineWithLevel("WARN", "HandleReceivedMessage: Processing message [{0}] failed - Sending to retry queue {1}/{2}", deliveryTag, retryCount + 1, this.Configuration.RetryCount);
            }
            else
            {
                this.QueueManager.AddDeadMessage(message);
                this.QueueManager.Ack(deliveryTag);
                Logger.LogLineWithLevel("WARN", "HandleReceivedMessage: Processing message [{0}] failed - Sending to dead queue", deliveryTag);
            }
        }

        public void Dispose()
        {
            this.QueueManager.Dispose();
            Logger.LogLineWithLevel("INFO","Queue Consumer Application Finish");
        }
    }
}