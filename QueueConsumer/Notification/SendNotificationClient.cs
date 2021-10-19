using Newtonsoft.Json.Linq;
using QueueConsumer.Models;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueueConsumer.Notification
{
    public static class SendNotificationClient
    {
        public static async Task<bool> SendNotification(QueueConsumerConfiguration configuration, string message)
        {
            RestRequest request = new RestRequest(Method.POST);
            request.AddParameter("application/json; charset=utf-8", message, ParameterType.RequestBody);

            string url = ExtractUrl(configuration, message);

            var response = await GetRestClient(configuration, url).ExecuteTaskAsync(request);

            return configuration.StatusCodeAcceptToSuccessList.Contains((int)response.StatusCode);
        }

        private static IRestClient GetRestClient(QueueConsumerConfiguration configuration, string url)
        {
            var restClient = new RestClient(url);
            restClient.Timeout = configuration.TimeoutInSeconds * 1000;

            if (string.IsNullOrWhiteSpace(configuration.AuthToken) == false)
            {
                restClient.AddDefaultParameter("Authorization", configuration.AuthToken, ParameterType.HttpHeader);
            }

            if (string.IsNullOrWhiteSpace(configuration.User) == false ||
                string.IsNullOrWhiteSpace(configuration.Pass) == false)
            {
                var user = configuration.User ?? "";
                var pass = configuration.Pass ?? "";
                restClient.Authenticator = new HttpBasicAuthenticator(user, pass);
            }

            return restClient;
        }

        private static string ExtractUrl(QueueConsumerConfiguration configuration, string message)
        {
            if (!configuration.ShouldUseUrlWithDynamicMatch)
            {
                return configuration.Url;
            }

            string url = configuration.Url;

            var messageAsJsonObj = JObject.Parse(message);

            var fieldsPathToMatch = Regex.Matches(configuration.Url, @"{{[\w\.]*}}");

            foreach (var fieldPathAsObj in fieldsPathToMatch)
            {
                string fieldPath = fieldPathAsObj.ToString().Trim('{', '}');
                var fieldValue = messageAsJsonObj.SelectToken(fieldPath);
                url = url.Replace("{{" + fieldPath + "}}", fieldValue.Value<string>());
            }

            return url;
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