using Newtonsoft.Json.Linq;

using QueueConsumer.Models;

using RestSharp;
using RestSharp.Authenticators;

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueueConsumer.Notification;

public class SendNotificationClient
{
    private readonly IRestClient _restClient;
    private QueueConsumerJwt _queueConsumerJwt;

    public SendNotificationClient(QueueConsumerConfiguration configuration, QueueConsumerJwt queueConsumerJwt)
    {
        _restClient = GetRestClient(configuration, queueConsumerJwt);
        SetAuthenticationMethod(configuration, queueConsumerJwt);
        _queueConsumerJwt = queueConsumerJwt;
    }

    public async Task<bool> SendNotification(QueueConsumerConfiguration configuration, string message)
    {
        var request = new RestRequest(ExtractUrl(configuration, message), Method.POST);
        request.AddParameter("application/json; charset=utf-8", message, ParameterType.RequestBody);

        SetAuthenticationMethod(configuration, _queueConsumerJwt);

        var response = await _restClient.ExecuteTaskAsync(request);

        return configuration.StatusCodeAcceptToSuccessList.Contains((int)response.StatusCode);
    }

    private static IRestClient GetRestClient(QueueConsumerConfiguration configuration, QueueConsumerJwt queueConsumerJwt)
    {
        var restClient = new RestClient()
        {
            Timeout = configuration.TimeoutInSeconds * 1000
        };

        return restClient;
    }

    private void SetAuthenticationMethod(QueueConsumerConfiguration configuration, QueueConsumerJwt queueConsumerJwt)
    {
        if (configuration.AuthenticationMethod == "AuthToken")
        {
            _restClient.AddDefaultParameter("Authorization", configuration.AuthToken, ParameterType.HttpHeader);
        }
        else if (configuration.AuthenticationMethod == "Jwt")
        {
            queueConsumerJwt.HandleAccessToken();
            _restClient.AddDefaultParameter("Authorization", $"Bearer {queueConsumerJwt.CurrentAccessToken.AccessToken}", ParameterType.HttpHeader);
        }
        else if (configuration.AuthenticationMethod == "Basic")
        {
            var user = configuration.User ?? "";
            var pass = configuration.Pass ?? "";
            _restClient.Authenticator = new HttpBasicAuthenticator(user, pass);
        }
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