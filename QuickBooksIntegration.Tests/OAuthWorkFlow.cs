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

			System.Console.WriteLine(jwtToken.access_token);

			((string)jwtToken.refresh_token).Should().NotBeEmpty();
		}

		[Test]
		public async Task GetResourceWithAccessToken()
		{
			var data = new Dictionary<string, string>
			{
				{"code", "Q011508835392OyPsflbjNvlths5DWxaODZNZIXqFdMIHm0hnl" },
				{"grant_type", "authorization_code" },
				{"redirect_uri", "https://developer.intuit.com/v2/OAuth2Playground/RedirectUrl" }
			};
			var authClient = new HttpClient() { BaseAddress = new Uri("https://oauth.platform.intuit.com") };
			authClient.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Basic", "UTA1SHB4a0QzRG8yb0dMMjU0VUdNdWlZQXprVk1HaXpudDhHSXhMUlc1bll5Vnp2eDQ6WlM2cE8yamc1QTN4RHpDcEZRTG4zMkZWdWQ2UlZhOW1kdE9MQlhZRQ==");
			authClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var getAccessTokenResponse = await authClient.PostAsync("oauth2/v1/tokens/bearer", new FormUrlEncodedContent(data));

			var jwtToken = await getAccessTokenResponse.Content.ReadAsAsync<dynamic>();

			string accessToken = jwtToken.access_token;

			//string accessToken = "eyJlbmMiOiJBMTI4Q0JDLUhTMjU2IiwiYWxnIjoiZGlyIn0..lddXflRKr3eklU3oPJMSxg._eN05xc5zzSjFVqNl_0-t2UUL6mkpBwH0x1Lw4KH1PeJ4TZ3mHdAXMh7J2HPm1aLTFb3WkOIeX6xmf3-Fgnk5pF4mbjltDQ-9M6zuC0Bncmy4b5Y40IRUZnEdUKIxfz1MMC6tiwJBaArWza_Bln_drwu9zlRwGgyLzKDLUm9WR9v_Zc_MFFomTzKA6iU7NafcaE64qxWeIp6moPfkB6fYjeRMgjZqldE7ALt0DB8gNWXfrd3m-n5pObO5pkvfmIEIadxKcqegUW_Y3eiIG1h6xLSq6IVSI_MvXH_APfV7JdJ9PT-DUgI5N0fxuAfE67rJhL7eP9nVI6oqDAozjhD5RM0l3CBmpkLH-zU7MUfhug0WGwlj3zrAdRxDWbMvthQwug0KLrn5jtNsCRNK-ugteAnMypA1Zl-uJigmMtVgyN-BLrX_Cij04hmH8vZ9G2Uoo4OBPNEeuodauR5ZZG0wDbYLgEgAY9xgRemmIo6FwY_tK042PzZgE2AZEb8ERgZjLLTCjClWm-W6ellyaJBuVncCRCkR6RyvRHLZsL1beF8fqAJkKz9zFb51cz8D-yvZN-myGiIOFDGElIua_YgDsfwUxDnZaunf3VH0QiwuNhqB4qD3_m7hiSWhX4nHUQxPkBimzlKtS0yJn0LiQp2OZpFumrGEZOunKfGJ8oh_9UNlpUksH7yDf-HBcfJHsWj3JVXy8u5tS-pp7o3nB_SWVRMSWLnI1JhaqgVW8SjKUc.QeetnOpKxmx1mjLtAmW4UQ";
			var resourceClient = new HttpClient() { BaseAddress = new Uri("https://sandbox-quickbooks.api.intuit.com") };
			resourceClient.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", accessToken);
			resourceClient.DefaultRequestHeaders.Accept.Clear();
			resourceClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var getResourceResponse = await resourceClient.GetAsync("v3/company/193514651873324/companyinfo/193514651873324");

			Console.WriteLine(await getResourceResponse.Content.ReadAsStringAsync());

			var company = await getResourceResponse.Content.ReadAsAsync<dynamic>();

			((string)company.CompanyInfo.CompanyName).Should().Be("Sandbox Company_US_1");
		}
	}
}