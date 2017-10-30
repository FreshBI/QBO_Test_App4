using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fbi.QuickBooksSolutionTemplate.FunctionEntryPoint
{
    /// <summary>  
    ///  This wrapper inserts all the necessary components from a SetUpObject into a QBO Resource Proivder and stands up a connection to QBO
    /// </summary> 
    public class QboOAuthWrapper
    {
        ResouceProvider resourceProvider;

        //public string CompanyId { get; set; }
        string CompanyId = "123145774537644";

        public QboOAuthWrapper()
        {
            var authClient = new IntuitAuthClient(new QboRefreshTokenStorage());

            resourceProvider = new ResouceProvider(authClient);
        }

        public async Task<dynamic> ExecuteGet(string resourcePart)
        {
            var response = await resourceProvider.GetResourceData(CompanyId, resourcePart);

            return response;
        }

        class ResouceProvider : IntuitResourceProvider
        {
            public ResouceProvider(IntuitAuthClient authClient)
              : base(new HttpClient() { BaseAddress = new Uri("https://sandbox-quickbooks.api.intuit.com") }, authClient)
            {
            }

            public async Task<dynamic> GetResourceData(string companyId, string resourcePart)
            {
                var getResourceResponse = await SendAsync(HttpMethod.Get, $"v3/company/{ companyId }/{ resourcePart }");

                // Console.WriteLine(await getResourceResponse.Content.ReadAsStringAsync());

                return await getResourceResponse.Content.ReadAsAsync<dynamic>();
            }
        }

    }
}
