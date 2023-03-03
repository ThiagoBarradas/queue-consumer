using Newtonsoft.Json;

using System;

namespace QueueConsumer.Models;

public class AccessTokenResponse
{
    private int _expiresIn;

    [JsonProperty("expires_in")]
    public int ExpiresInSeconds
    {
        get
        {
            return this._expiresIn;
        }
        set
        {
            var now = DateTime.UtcNow;
            this._expiresIn = value;

            // reduces 60 seconds as margin
            this.ExpiresInDate = now.AddSeconds(value - 60);
        }
    }

    [JsonIgnore]
    public DateTime ExpiresInDate { get; set; }

    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
}
