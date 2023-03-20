using NewRelic.Api.Agent;

using Newtonsoft.Json.Linq;

using QueueConsumer.Models;

using RestSharp;
using RestSharp.Authenticators;

using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueueConsumer.Notification;

public class SendNotificationClient
{
    private readonly IRestClient _restClient;
    private readonly QueueConsumerJwt _queueConsumerJwt;

    public SendNotificationClient(QueueConsumerConfiguration configuration, QueueConsumerJwt queueConsumerJwt)
    {
        _restClient = GetRestClient(configuration);
        _queueConsumerJwt = queueConsumerJwt;
    }

    [Trace]
    public async Task<bool> SendNotification(QueueConsumerConfiguration configuration, string urlFromMessage, string message)
    {
        var request = new RestRequest(ExtractUrl(configuration, urlFromMessage, message), Method.POST);
        request.AddParameter("application/json; charset=utf-8", message, ParameterType.RequestBody);

        SetAuthenticationMethod(configuration);

        var response = await _restClient.ExecuteAsync(request);

        return configuration.StatusCodeAcceptToSuccessList.Contains((int)response.StatusCode);
    }

    private static IRestClient GetRestClient(QueueConsumerConfiguration configuration)
    {
        var config = new RestClientAutologConfiguration
        {
            EnabledLog = configuration.LogEnabled,
            RequestJsonBlacklist = configuration.LogBlacklistList.ToArray(),
            ResponseJsonBlacklist = configuration.LogBlacklistList.ToArray(),
        };

        var restClient = new RestClientAutolog(config)
        {
            Timeout = configuration.TimeoutInSeconds * 1000
        };

        if (!string.IsNullOrEmpty(configuration.UserAgent))
        {
            restClient.UserAgent = configuration.UserAgent;
        }

        return restClient;
    }

    private void SetAuthenticationMethod(QueueConsumerConfiguration configuration)
    {
        if (configuration.AuthenticationMethod == "AuthToken" && !_restClient.DefaultParameters.Any(x => x.Name == "Authorization"))
        {
            _restClient.AddDefaultHeader("Authorization", configuration.AuthToken);
        }
        else if (configuration.AuthenticationMethod == "Jwt")
        {
            _queueConsumerJwt.HandleAccessToken();
            _restClient.Authenticator = new JwtAuthenticator(_queueConsumerJwt.CurrentAccessToken.AccessToken);
        }
        else if (configuration.AuthenticationMethod == "Basic")
        {
            var user = configuration.User ?? "";
            var pass = configuration.Pass ?? "";
            _restClient.Authenticator = new HttpBasicAuthenticator(user, pass);
        }
    }

    private static string ExtractUrl(QueueConsumerConfiguration configuration, string urlFromMessage, string message)
    {
        var url = urlFromMessage ?? configuration.Url;

        if (!configuration.ShouldUseUrlWithDynamicMatch)
        {
            return url;
        }

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