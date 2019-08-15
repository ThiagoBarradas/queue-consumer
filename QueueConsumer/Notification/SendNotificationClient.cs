using QueueConsumer.Models;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Threading.Tasks;

namespace QueueConsumer.Notification
{
    public static class SendNotificationClient
    {
        public static async Task<bool> SendNotification(QueueConsumerConfiguration configuration, string message)
        {
            RestRequest request = new RestRequest(Method.POST);
            request.AddParameter("application/json; charset=utf-8", message, ParameterType.RequestBody);

;           var response = await GetRestClient(configuration).ExecuteTaskAsync(request);

            return response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.Created ||
                   response.StatusCode == HttpStatusCode.Accepted ||
                   response.StatusCode == HttpStatusCode.NoContent;
        }

        private static IRestClient GetRestClient(QueueConsumerConfiguration configuration)
        {
            var restClient = new RestClient(configuration.Url);
            restClient.Timeout = configuration.TimeoutInSeconds * 1000;

            if (string.IsNullOrWhiteSpace(configuration.User) == false ||
                string.IsNullOrWhiteSpace(configuration.Pass) == false)
            {
                var user = configuration.User ?? "";
                var pass = configuration.Pass ?? "";
                restClient.Authenticator = new HttpBasicAuthenticator(user, pass);
            }

            return restClient;
        }
    }

    public static class RestClientExtensions
    {
        public static Task<IRestResponse> ExecuteTaskAsync(this RestClient restClient, RestRequest request)
        {
            if (restClient == null)
            {
                throw new NullReferenceException();
            }

            var taskCompletionSource = new TaskCompletionSource<IRestResponse>();

            restClient.ExecuteAsync(request, (response) =>
            {
                if (response.ErrorException != null)
                {
                    taskCompletionSource.TrySetException(response.ErrorException);
                }
                else
                {
                    taskCompletionSource.TrySetResult(response);
                }
            });

            return taskCompletionSource.Task;
        }
    }
}
