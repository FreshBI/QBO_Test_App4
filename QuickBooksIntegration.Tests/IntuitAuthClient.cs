using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace QuickBooksIntegration.Tests
{
  class IntuitAuthClient
  {
    const string refreshTokenStorageKey = "RefreshToken";
    readonly HttpClient client;
    readonly ISecureStorage secureStorage;

    public IntuitAuthClient(string clientId, string sharedSecret, ISecureStorage secureStorage)
    {
      this.secureStorage = secureStorage;
      client = new HttpClient() { BaseAddress = new Uri("https://oauth.platform.intuit.com") };
      client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", Base64EncodeClientSecret(clientId, sharedSecret));
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static string Base64EncodeClientSecret(string clientId, string sharedSecret)
    {
      return Convert.ToBase64String(
          System.Text.Encoding.UTF8.GetBytes(
              $"{clientId}:{sharedSecret}"));
    }

    public async Task<string> GetAccessToken(string accessCode)
    {
      var data = new Dictionary<string, string>
      {
        {"code", accessCode },
        {"grant_type", "authorization_code" },
        {"redirect_uri", "https://developer.intuit.com/v2/OAuth2Playground/RedirectUrl" }
      };

      return await GetAccessToken(data);
    }

    public async Task<string> RefreshAccessToken()
    {
      var refreshToken = secureStorage.Retrieve<string>(refreshTokenStorageKey);

      var data = new Dictionary<string, string>
      {
        {"grant_type", "refresh_token" },
        {"refresh_token", refreshToken }
      };

      return await GetAccessToken(data);
    }

    private async Task<string> GetAccessToken(IDictionary<string, string> data)
    {
      var getAccessTokenResponse = await client.PostAsync("oauth2/v1/tokens/bearer", new FormUrlEncodedContent(data));

      var token = await getAccessTokenResponse.Content.ReadAsAsync<Token>();

      secureStorage.Store(refreshTokenStorageKey, token.RefreshToken);

      return token.AccessToken;
    }
  }
}