using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace QuickBooksIntegration.Tests
{
  class IntuitAuthClient
  {
    private readonly HttpClient client;

    public IntuitAuthClient(string clientId, string sharedSecret)
    {
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

    public async Task<Token> GetAccessToken(string accessCode)
    {
      var data = new Dictionary<string, string>
      {
        {"code", accessCode },
        {"grant_type", "authorization_code" },
        {"redirect_uri", "https://developer.intuit.com/v2/OAuth2Playground/RedirectUrl" }
      };

      var getAccessTokenResponse = await client.PostAsync("oauth2/v1/tokens/bearer", new FormUrlEncodedContent(data));

      return await getAccessTokenResponse.Content.ReadAsAsync<Token>();
    }

    public async Task<Token> RefreshAccessToken(Token token)
    {
      var data = new Dictionary<string, string>
      {
        {"grant_type", "refresh_token" },
        {"refresh_token", token.RefreshToken }
      };

      var getAccessTokenWithRefreshResponse = await client.PostAsync("oauth2/v1/tokens/bearer", new FormUrlEncodedContent(data));

      return await getAccessTokenWithRefreshResponse.Content.ReadAsAsync<Token>();
    }
  }

  class Token
  {
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
  }

  class OAuthWorkFlow
  {
    private string AccessCode = "Q011508988380EWqY6Jbq38YyDNEfnseZ7HGPg7CPyy69XvBQp";

    private const string ClientId = "Q05HpxkD3Do2oGL254UGMuiYAzkVMGiznt8GIxLRW5nYyVzvx4";
    private const string ClientSecret = "ZS6pO2jg5A3xDzCpFQLn32FVud6RVa9mdtOLBXYE";

    private IntuitAuthClient authClient;
    private Token token;
    private HttpClient resourceClient;

    [OneTimeSetUp]
    public async Task SetUp()
    {
      resourceClient = new HttpClient() { BaseAddress = new Uri("https://sandbox-quickbooks.api.intuit.com") };
      resourceClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      authClient = new IntuitAuthClient(ClientId, ClientSecret);

      token = await authClient.GetAccessToken(AccessCode);

      Console.WriteLine(token.AccessToken);
      Console.WriteLine(token.RefreshToken);
    }

    [Test]
    public async Task GetResourceWithAccessToken()
    {
      resourceClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token.AccessToken);

      var getResourceResponse = await resourceClient.GetAsync("v3/company/193514651873324/companyinfo/193514651873324");

      Console.WriteLine(await getResourceResponse.Content.ReadAsStringAsync());

      var company = await getResourceResponse.Content.ReadAsAsync<dynamic>();

      ((string)company.CompanyInfo.CompanyName).Should().Be("Sandbox Company_US_1");
    }

    [Test]
    public async Task RefreashAccessTokenAndGetResource()
    {
      token = await authClient.RefreshAccessToken(token);

      Console.WriteLine($"Access token from refresh token\n{token.AccessToken}");

      resourceClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token.AccessToken);

      var getResourceResponse = await resourceClient.GetAsync("v3/company/193514651873324/companyinfo/193514651873324");

      Console.WriteLine(await getResourceResponse.Content.ReadAsStringAsync());

      var company = await getResourceResponse.Content.ReadAsAsync<dynamic>();

      ((string)company.CompanyInfo.CompanyName).Should().Be("Sandbox Company_US_1");
    }
  }
}