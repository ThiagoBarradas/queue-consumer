using Proto;
using QueueConsumer.Models;
using QueueConsumer.Queue;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Threading.Tasks;

namespace QueueConsumer.Notification
{
    public class SendNotificationActor : IActor
    {
        private QueueConsumerConfiguration Configuration { get; set; }

        private QueueManager QueueManager { get; set; }

        public IRestClient RestClient { get; set; }

        public SendNotificationActor(QueueConsumerConfiguration configuration, QueueManager queueManager)
        {
            this.Configuration = configuration;
            this.QueueManager = queueManager;
            this.RestClient = new RestClient(this.Configuration.Url);
            this.RestClient.Timeout = this.Configuration.TimeoutInSeconds * 1000;

            if (string.IsNullOrWhiteSpace(this.Configuration.User) == false ||
                string.IsNullOrWhiteSpace(this.Configuration.Pass) == false)
            {
                var user = this.Configuration.User ?? "";
                var pass = this.Configuration.Pass ?? "";
                this.RestClient.Authenticator = new HttpBasicAuthenticator(user, pass);
            }
        }

        public Task ReceiveAsync(IContext context)
        {
            var msg = context.Message;
            if (msg is QueueMessage queueMessage)
            {
                Console.WriteLine("Processing message {0}", queueMessage.DeliveryTag);

                try
                {
                    var success = this.Post(queueMessage.Content);

                    if (success)
                    {
                        this.QueueManager.Ack(queueMessage.DeliveryTag);
                    }
                    else if (string.IsNullOrWhiteSpace(this.Configuration.QueueNameForFailed) == false)
                    {
                        this.QueueManager.AddFailedMessage(queueMessage.Content);
                    }
                    else
                    {
                        this.QueueManager.NAck(queueMessage.DeliveryTag);
                    }
                }
                catch (Exception ex)
                {
                    this.QueueManager.NAck(queueMessage.DeliveryTag);
                    Console.WriteLine("EXCEPTION ACTOR: {0}", ex.Message);
                }
            }
            return Actor.Done;
        }

        private bool Post(string message)
        {
            RestRequest request = new RestRequest(Method.POST);
            request.AddParameter("application/json; charset=utf-8", message, ParameterType.RequestBody);

            var response = this.RestClient.Execute(request);

            return response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.Created ||
                   response.StatusCode == HttpStatusCode.Accepted ||
                   response.StatusCode == HttpStatusCode.NoContent;
        }
    }
}
