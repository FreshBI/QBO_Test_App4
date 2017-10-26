using Newtonsoft.Json;

namespace QuickBooksIntegration.Tests
{
  class Token
  {
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
  }
}