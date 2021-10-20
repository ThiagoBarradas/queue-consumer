using System;
using System.Collections.Generic;
using System.Linq;

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
            this.Condition = Environment.GetEnvironmentVariable("Condition");
            this.StatusCodeAcceptToSuccess = Environment.GetEnvironmentVariable("StatusCodeAcceptToSuccess") ?? "200;201;202;204";
            this.StatusCodeAcceptToSuccessList = null;
            this.ShouldUseUrlWithDynamicMatch = bool.Parse(Environment.GetEnvironmentVariable("ShouldUseUrlWithDynamicMatch") ?? "false");
        }

        public int RetryCount { get; set; }

        public bool CreateQueue { get; set; }

        public int RetryTTL { get; set; }

        public int PopulateQueueQuantity { get; set; }

        public int MaxThreads { get; set; }

        public string QueueConnectionString { get; set; }

        public string Condition { get; set; }

        public string QueueName { get; set; }

        public string Url { get; set; }

        public string User { get; set; }

        public string Pass { get; set; }

        public string AuthToken { get; set; }

        public bool ShouldUseUrlWithDynamicMatch { get; set; }

        private string StatusCodeAcceptToSuccess { get; set; }

        private IList<int> _StatusCodeAcceptToSuccessList { get; set; }

        public IList<int> StatusCodeAcceptToSuccessList
        {
            get => _StatusCodeAcceptToSuccessList;
            private set
            {
                _StatusCodeAcceptToSuccessList = StatusCodeAcceptToSuccess?.Split(";").Select(int.Parse).ToList();
            }
        }

        public int TimeoutInSeconds { get; set; }

        public static QueueConsumerConfiguration Create()
        {
            return new QueueConsumerConfiguration();
        }

        public static QueueConsumerConfiguration CreateForDebug(bool populate)
        {
            return new QueueConsumerConfiguration
            {
                AuthToken = "token",
                CreateQueue = true,
                MaxThreads = 100,
                Pass = "pass",
                PopulateQueueQuantity = populate ? 100000 : 0,
                QueueConnectionString = "amqp://guest:guest@localhost:5672/",
                QueueName = "debug",
                RetryCount = 5,
                RetryTTL = 30000,
                TimeoutInSeconds = 30,
                Url = "http://pruu.herokuapp.com/dump/queue-consumer",
                User = "user",
                StatusCodeAcceptToSuccess = "200;201;202;204",
                StatusCodeAcceptToSuccessList = null,
            };
        }
    }
}