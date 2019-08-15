namespace QueueConsumer.Models
{
    public class QueueConsumerConfiguration
    {
        public QueueConsumerConfiguration()
        {
            //this.QueueConnectionString = Environment.GetEnvironmentVariable("QueueConnectionString");
            //this.QueueName = Environment.GetEnvironmentVariable("QueueName");
            //this.QueueNameForFailed = Environment.GetEnvironmentVariable("QueueNameForFailed");
            //this.Url = Environment.GetEnvironmentVariable("Url");
            //this.User = Environment.GetEnvironmentVariable("User");
            //this.Pass = Environment.GetEnvironmentVariable("Pass");
            //this.TimeoutInSeconds = int.Parse(Environment.GetEnvironmentVariable("TimeoutInSeconds") ?? "60");

            this.QueueConnectionString = "amqp://user:password@localhost:35672/consumer-vh";
            this.QueueName = "consumer-origin-queue";
            this.QueueNameForFailed = "consumer-failed-queue";
            this.Url = "http://pruu.herokuapp.com/dump/test-consumer";
            this.User = "myuser";
            this.Pass = "pass123";
            this.TimeoutInSeconds = 30;
        }

        public string QueueConnectionString { get; set; }

        public string QueueName { get; set; }

        public string QueueNameForFailed { get; set; }

        public string Url { get; set; }

        public string User { get; set; }

        public string Pass { get; set; }

        public int TimeoutInSeconds { get; set; }

        public static QueueConsumerConfiguration Create()
        {
            return new QueueConsumerConfiguration();
        }
    }
}
