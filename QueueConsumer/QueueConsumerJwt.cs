using Newtonsoft.Json;

using QueueConsumer.Models;

using RestSharp;

using System;
using System.Net;
using System.Net.Http;

namespace QueueConsumer;

public class QueueConsumerJwt
{
    public object Lock { get; set; } = new();

    private readonly QueueConsumerConfiguration _configuration;
    public AccessTokenResponse CurrentAccessToken { get; private set; } = new AccessTokenResponse();
    public DateTime ExpiresIn { get; private set; } = DateTime.UtcNow;

    public QueueConsumerJwt(QueueConsumerConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AccessTokenResponse HandleAccessToken()
    {
        lock (Lock)
        {
            var now = DateTime.UtcNow;
            if (TokenIsExpired())
            {
                return this.CurrentAccessToken;
            }

            this.CurrentAccessToken = GenerateAccessToken();
        }

        return this.CurrentAccessToken;
    }

    public bool TokenIsExpired()
        => DateTime.UtcNow < this.CurrentAccessToken.ExpiresInDate;

    public AccessTokenResponse GenerateAccessToken()
    {
        var restClient = new RestClient(_configuration.AuthUrl);
        var request = new RestRequest(Method.POST);
        request.AddParameter("client_id", _configuration.ClientId, ParameterType.GetOrPost);
        request.AddParameter("grant_type", "client_credentials", ParameterType.GetOrPost);
        request.AddParameter("client_secret", _configuration.ClientSecret, ParameterType.GetOrPost);
        request.AddHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(_configuration.UserAgent))
        {
            request.AddHeader("User-Agent", _configuration.UserAgent);
        }

        var response = restClient.Execute(request);

        if (!response.IsSuccessful)
        {
            throw new HttpRequestException($"Invalid status code received from auth api: {response.StatusCode}");
        }

        return JsonConvert.DeserializeObject<AccessTokenResponse>(response.Content);
    }
}
