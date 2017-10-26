using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuickBooksIntegration.Tests
{
  class ResourceProviderTests
  {
    string AccessCode = "Q0115089935454CLNmrG3BWqhI7C1Xq3dyI68c5dczGThhEKvi";

    const string ResourceId = "193514651873324";
    const string ClientId = "Q05HpxkD3Do2oGL254UGMuiYAzkVMGiznt8GIxLRW5nYyVzvx4";
    const string ClientSecret = "ZS6pO2jg5A3xDzCpFQLn32FVud6RVa9mdtOLBXYE";

    TestResourceProvider resourceProvider;

    [OneTimeSetUp]
    public async Task SetUp()
    {
      var authClient = new IntuitAuthClient(ClientId, ClientSecret, new InMemoryStorage());

      await authClient.GetAccessToken(AccessCode);

      resourceProvider = new TestResourceProvider(authClient);
    }

    [Test]
    public async Task MakeSingleCallToGetResource()
    {
      var company = await resourceProvider.GetCompanyData(ResourceId);

      ((string)company.CompanyInfo.CompanyName).Should().Be("Sandbox Company_US_1");
    }

    [Test]
    public async Task MakeMultipleCallToGetResource()
    {
      Enumerable.Range(1, 5).ToList().ForEach(async x => await resourceProvider.GetCompanyData(ResourceId));

      var company = await resourceProvider.GetCompanyData(ResourceId);

      ((string)company.CompanyInfo.CompanyName).Should().Be("Sandbox Company_US_1");
    }

    class TestResourceProvider : IntuitResourceProvider
    {
      public TestResourceProvider(IntuitAuthClient authClient)
        : base(new HttpClient() { BaseAddress = new Uri("https://sandbox-quickbooks.api.intuit.com") }, authClient)
      {
      }

      public async Task<dynamic> GetCompanyData(string companyId)
      {
        var getResourceResponse = await SendAsync(HttpMethod.Get, $"v3/company/{companyId}/companyinfo/${companyId}");

        Console.WriteLine(await getResourceResponse.Content.ReadAsStringAsync());

        return await getResourceResponse.Content.ReadAsAsync<dynamic>();
      }
    }
  }
}