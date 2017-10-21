using System;

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Intuit.Ipp.OAuth2PlatformClient;
using System.Net;
using System.Collections.Generic;
using System.Web.UI;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using Intuit.Ipp.Exception;
using System.Linq;

namespace TestApp4
{
    public static class FunctionDemon
    {
        [FunctionName("FunctionDemon")]
        public static void Run([TimerTrigger("*/30 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# function started at: {DateTime.Now}");

            string secPwInKeyVault = SuperVars.GetVaultSecret();
            log.Info("My Secret – " + secPwInKeyVault);
            log.Info($"C# function executed at: {DateTime.Now}");

            var QBO = new QBO_Shit();

            QBO.GetDiscoveryData_JWKSkeys(log);
        }
    }
    public static class SuperVars
    {
        static string _vaultSecretUri = "https://qbokeyvalut.vault.azure.net/secrets/TestSecret/796c920df4744a31b47268fdecfd9b01";
        static string _vaultClientID = "24c1b85f-81a1-430d-b70c-817419628ce9";
        static string _vaultClientSecret = "/u5nA1A4pX4YOgZevDb+78gkcsKlKn3UDyl+XFFtmJs=";



        public static string GetVaultSecret()
        {
            string SecretUri = _vaultSecretUri;
            var kvToken = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetVaultToken));
            var kvSecret = kvToken.GetSecretAsync(SecretUri).Result;
            return kvSecret.Value;
        }

        public static async Task<string> GetVaultToken(string authority, string resource, string scope)
        {
            string ClientId = _vaultClientID;
            string ClientSecret = _vaultClientSecret;

            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(ClientId, ClientSecret);
            AuthenticationResult authResult = await authContext.AcquireTokenAsync(resource, clientCred);
            if (authResult == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");
            return authResult.AccessToken;
        }

    }


    public class QBO_Shit
    {
        
        //Authorization endpoint url
        static string authorizationEndpoint;
        //Token endpoint url
        static string tokenEndpoint;
        //UseInfo endpoint url
        static string userinfoEndPoint;
        //Revoke endpoint url
        static string revokeEndpoint;
        //Issuer endpoint Url 
        static string issuerUrl;

        static string stateVal;
        static string redirectURI = "redirectURI";
        static string discoveryUrl = "https://developer.api.intuit.com/.well-known/openid_sandbox_configuration/";
        static string clientID = "clientID";
        static string clientSecret = "clientSecret";
        static string logPath = "logPath";

        DiscoveryClient discoveryClient;
        DiscoveryResponse doc;
        //AuthorizeRequest request;
        public static IList<JsonWebKey> keys;
        public static Dictionary<string, string> dictionary = new Dictionary<string, string>();
        
        public static System.Web.HttpResponse Response { get; }


        public async void GetDiscoveryData_JWKSkeys(TraceWriter log)
        {
            log.Info("Enter Task");


            //Intialize DiscoverPolicy
            DiscoveryPolicy dpolicy = new DiscoveryPolicy();
            dpolicy.RequireHttps = true;
            dpolicy.ValidateIssuerName = true;

            //Assign the Sandbox Discovery url for the Apps' Dev clientid and clientsecret that you use
            //Or
            //Assign the Production Discovery url for the Apps' Production clientid and clientsecret that you use



            if (discoveryUrl != null && clientID != null && clientSecret != null)
            {
                discoveryClient = new DiscoveryClient(discoveryUrl);
                log.Info("Enterted if");
            }
            log.Info("Exit if");

            DiscoveryResponse doc = await discoveryClient.GetAsync();

            log.Info("Doc done");

            if (doc.StatusCode == HttpStatusCode.OK)
            {
                //Authorization endpoint url
                authorizationEndpoint = doc.AuthorizeEndpoint;

                //Token endpoint url
                tokenEndpoint = doc.TokenEndpoint;

                //UseInfo endpoint url
                userinfoEndPoint = doc.UserInfoEndpoint;

                //Revoke endpoint url
                revokeEndpoint = doc.RevocationEndpoint;

                //Issuer endpoint Url 
                issuerUrl = doc.Issuer;

                //JWKS Keys
                keys = doc.KeySet.Keys;
                log.Info("Discovery Data obtained.");
            }

            else
            {
                log.Info("Discovery error");
            }

        }

        /// <summary>
        /// Start Oauth by getting a code first
        /// </summary>
        /// <param name="callMadeBy"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task doOAuth(string callMadeBy, TraceWriter log)
        {
            log.Info("Intiating OAuth2 call to get code.");
            string authorizationRequest = "";
            string scopeVal = "";

            ////Save the state(CSRF token/Campaign Id/Tracking Id) in dictionary to verify after Oauth2 Callback. This is just for reference. 
            ////Actual CSRF handling should be done as per security standards in some hidden fields or encrypted permanent store

            stateVal = CryptoRandom.CreateUniqueId();

            if (!dictionary.ContainsKey("CSRF"))
            {
                dictionary.Add("CSRF", stateVal);
            }


            //decide scope based on which flow was initiated
            if (callMadeBy == "C2QB") //C2QB scopes
            {
                if (!dictionary.ContainsKey("callMadeBy"))
                {
                    dictionary.Add("callMadeBy", callMadeBy);
                }
                else
                {
                    dictionary["callMadeBy"] = callMadeBy;
                }

                scopeVal = OidcScopes.Accounting.GetStringValue() + " " + OidcScopes.Payment.GetStringValue();
            }
            else if (callMadeBy == "OpenId")//Get App Now scopes
            {
                if (!dictionary.ContainsKey("callMadeBy"))
                {
                    dictionary.Add("callMadeBy", callMadeBy);
                }
                else
                {
                    dictionary["callMadeBy"] = callMadeBy;
                }

                scopeVal = OidcScopes.Accounting.GetStringValue() + " " + OidcScopes.Payment.GetStringValue()
                    + " " + OidcScopes.OpenId.GetStringValue() + " " + OidcScopes.Address.GetStringValue()
                    + " " + OidcScopes.Email.GetStringValue() + " " + OidcScopes.Phone.GetStringValue()
                    + " " + OidcScopes.Profile.GetStringValue();
            }
            else if (callMadeBy == "SIWI")//Sign In With Intuit scopes
            {
                if (!dictionary.ContainsKey("callMadeBy"))
                {
                    dictionary.Add("callMadeBy", callMadeBy);
                }
                else
                {
                    dictionary["callMadeBy"] = callMadeBy;
                }

                scopeVal = OidcScopes.OpenId.GetStringValue() + " " + OidcScopes.Address.GetStringValue()
                    + " " + OidcScopes.Email.GetStringValue() + " " + OidcScopes.Phone.GetStringValue()
                    + " " + OidcScopes.Profile.GetStringValue();
            }

            log.Info("Setting up Authorize url");
            //Create the OAuth 2.0 authorization request.

            if (authorizationEndpoint != "" && authorizationEndpoint != null)
            {
                authorizationRequest = string.Format("{0}?client_id={1}&response_type=code&scope={2}&redirect_uri={3}&state={4}",
                        authorizationEndpoint,
                        clientID,
                        scopeVal,
                        System.Uri.EscapeDataString(redirectURI),
                        stateVal);


                log.Info("Calling AuthorizeUrl");
                if (callMadeBy == "C2QB" || callMadeBy == "SIWI")
                {
                    //redirect to authorization request url in pop-up
                    //new System.Web.Response.Redirect(authorizationRequest, "_blank", "menubar=0,scrollbars=1,width=780,height=900,top=10");

                }
                else
                {
                    //redirect to authorization request url
                    Response.Redirect(authorizationRequest);
                }


            }
            else
            {
                log.Info("Missing authorizationEndpoint url!");
            }


        }
    }
    //public static class ResponseHelper
    //{
    //    public static void Redirect(this System.Web.HttpResponse response, string url, string target, string windowFeatures)
    //    {

    //        if ((String.IsNullOrEmpty(target) || target.Equals("_self", StringComparison.OrdinalIgnoreCase)) && String.IsNullOrEmpty(windowFeatures))
    //        {
    //            response.Redirect(url);
    //        }
    //        else
    //        {
    //            var page = System.Web.HttpContext.Current.Handler;

    //            if (page == null)
    //            {
    //                throw new InvalidOperationException("Cannot redirect to new window outside Page context.");
    //            }
    //            url = page.ResolveClientUrl(url);

    //            string script;
    //            if (!String.IsNullOrEmpty(windowFeatures))
    //            {
    //                script = @"window.open(""{0}"", ""{1}"", ""{2}"");";
    //            }
    //            else
    //            {
    //                script = @"window.open(""{0}"", ""{1}"");";
    //            }
    //            script = String.Format(script, url, target, windowFeatures);
    //            ScriptManager.RegisterStartupScript(page, typeof(Page), "Redirect", script, true);
    //        }
    //    }
    //}
}
