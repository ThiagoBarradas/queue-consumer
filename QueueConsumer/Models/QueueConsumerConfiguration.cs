using System;

namespace QueueConsumer.Models
{
    public class QueueConsumerConfiguration
    {
        public QueueConsumerConfiguration()
        {
            this.QueueConnectionString = Environment.GetEnvironmentVariable("QueueConnectionString");
            this.QueueName = Environment.GetEnvironmentVariable("QueueName");
            this.Url = Environment.GetEnvironmentVariable("Url");
            this.User = Environment.GetEnvironmentVariable("User");
            this.Pass = Environment.GetEnvironmentVariable("Pass");
            this.AuthToken = Environment.GetEnvironmentVariable("AuthToken");
            this.TimeoutInSeconds = int.Parse(Environment.GetEnvironmentVariable("TimeoutInSeconds") ?? "60");
            this.MaxThreads = int.Parse(Environment.GetEnvironmentVariable("MaxThreads") ?? "20");
            this.PopulateQueueQuantity = int.Parse(Environment.GetEnvironmentVariable("PopulateQueueQuantity") ?? "0");
            this.CreateQueue = bool.Parse(Environment.GetEnvironmentVariable("CreateQueue") ?? "false");
            this.RetryTTL = int.Parse(Environment.GetEnvironmentVariable("RetryTTL") ?? "60000");
            this.RetryCount = int.Parse(Environment.GetEnvironmentVariable("RetryCount") ?? "5");
        }

        public int RetryCount { get; set; }

        public bool CreateQueue { get; set; }

        public int RetryTTL { get; set; }

        public int PopulateQueueQuantity { get; set; }

        public int MaxThreads { get; set; }

        public string QueueConnectionString { get; set; }

        public string QueueName { get; set; }

        public string Url { get; set; }

        public string User { get; set; }

        public string Pass { get; set; }

        public string AuthToken { get; set; }


        public int TimeoutInSeconds { get; set; }

        public static QueueConsumerConfiguration Create()
        {
            return new QueueConsumerConfiguration();
        }
    }
}
