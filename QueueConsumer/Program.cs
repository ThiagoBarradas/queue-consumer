using Proto;
using QueueConsumer.Models;
using QueueConsumer.Notification;
using QueueConsumer.Queue;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace QueueConsumer
{
    public class Program
    {
        public static void Main(string[] args = null)
        {
            var config = QueueConsumerConfiguration.Create();
            Display(config);

            var queueManager = new QueueManager(config);
            var actor = new SendNotificationActor(config, queueManager);
            var props = Actor.FromProducer(() => actor);

            while (true)
            {
                try
                {
                    queueManager.ReceiveMessage += (message, deliveryTag) =>
                    {
                        var queueMessage = new QueueMessage(message, deliveryTag);
                        Actor.Spawn(props).Tell(queueMessage);
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine("EXCEPTION: {0}", e.Message);
                    Console.WriteLine("Try reconnecting in 3 seconds...", e.Message);
                    Thread.Sleep(3000);
                    queueManager.TryReconnect();
                }
            }
        }

        private static void Display(QueueConsumerConfiguration config)
        {
            Console.WriteLine("Queue Consumer Application Started - {0}", DateTime.UtcNow);
            Console.WriteLine("Configuration:");
            Console.WriteLine("- QueueConnectionString: {0}", Regex.Replace(config.QueueConnectionString, "(\\:\\/\\/).*(\\@)", "://*****@"));
            Console.WriteLine("- QueueName: {0}", config.QueueName);
            Console.WriteLine("- QueueNameForFailed: {0}", config.QueueNameForFailed);
            Console.WriteLine("- Url: {0}", config.Url);
            Console.WriteLine("- User: {0}", config.User);
            Console.WriteLine("- Pass: {0}", string.IsNullOrWhiteSpace(config.Pass) ? "null" : "******");
            Console.WriteLine("- TimeoutInSeconds: {0}", config.TimeoutInSeconds);
        }

        //private static void Init()
        //{
        //    SetConfig();
        //    ExceptionLogger = new ExceptionLogger();
        //    ExceptionLogger.LogAndPrintInfo("Hubot Hal9000 Application Started - {0}", DateTime.UtcNow);
        //}
    }
}
