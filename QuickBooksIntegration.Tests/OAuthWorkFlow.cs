using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net.Http.Headers;

namespace QuickBooksIntegration.Tests
{
	class OAuthWorkFlow
	{
		[Test]
		public async Task GetAccessToken()
		{
			var data = new Dictionary<string, string>
			{
				{"code", "Q011508832773ZtQ3qa4ShTskMtYRmYNVhlnhZ00Aj8TANtWEn" },
				{"grant_type", "authorization_code" },
				{"redirect_uri", "https://developer.intuit.com/v2/OAuth2Playground/RedirectUrl" }
			};
			var client = new HttpClient() { BaseAddress = new Uri("https://oauth.platform.intuit.com") };
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "UTA1SHB4a0QzRG8yb0dMMjU0VUdNdWlZQXprVk1HaXpudDhHSXhMUlc1bll5Vnp2eDQ6WlM2cE8yamc1QTN4RHpDcEZRTG4zMkZWdWQ2UlZhOW1kdE9MQlhZRQ==");
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var getAccessTokenResponse = await client.PostAsync("oauth2/v1/tokens/bearer", new FormUrlEncodedContent(data));

			var jwtToken = await getAccessTokenResponse.Content.ReadAsAsync<dynamic>();

			System.Console.WriteLine(jwtToken);

			((string)jwtToken.refresh_token).Should().NotBeEmpty();
		}
	}
}
