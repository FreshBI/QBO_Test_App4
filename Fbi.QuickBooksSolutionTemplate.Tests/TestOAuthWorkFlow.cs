using System;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net.Http.Headers;

namespace Fbi.QuickBooksSolutionTemplate
{
  class TestOAuthWorkFlow
  {
    string AccessCode = "Q011508992924KaeHXcy7mmnqoy2t0WxUYJFJbpMWm0yY8rwJB";

    const string ResourceId = "193514651873324";
    const string ClientId = "Q05HpxkD3Do2oGL254UGMuiYAzkVMGiznt8GIxLRW5nYyVzvx4";
    const string ClientSecret = "ZS6pO2jg5A3xDzCpFQLn32FVud6RVa9mdtOLBXYE";

    IntuitAuthClient authClient;
    string token;
    HttpClient resourceClient;

    [OneTimeSetUp]
    public async Task SetUp()
    {
      resourceClient = new HttpClient() { BaseAddress = new Uri("https://sandbox-quickbooks.api.intuit.com") };
      resourceClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      authClient = new IntuitAuthClient(ClientId, ClientSecret, new InMemoryStorage());

      token = await authClient.GetAccessToken(AccessCode);

      Console.WriteLine(token);
    }

    [Test]
    public async Task GetResourceWithAccessToken()
    {
      resourceClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

      var getResourceResponse = await resourceClient.GetAsync($"v3/company/{ResourceId}/companyinfo/{ResourceId}");

      Console.WriteLine(await getResourceResponse.Content.ReadAsStringAsync());

      var company = await getResourceResponse.Content.ReadAsAsync<dynamic>();

      ((string)company.CompanyInfo.CompanyName).Should().Be("Sandbox Company_US_1");
    }

    [Test]
    public async Task RefreashAccessTokenAndGetResource()
    {
      token = await authClient.RefreshAccessToken();

      Console.WriteLine($"Access token from refresh token\n{token}");

      resourceClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

      var getResourceResponse = await resourceClient.GetAsync($"v3/company/{ResourceId}/companyinfo/{ResourceId}");

      Console.WriteLine(await getResourceResponse.Content.ReadAsStringAsync());

      var company = await getResourceResponse.Content.ReadAsAsync<dynamic>();

      ((string)company.CompanyInfo.CompanyName).Should().Be("Sandbox Company_US_1");
    }
  }
}