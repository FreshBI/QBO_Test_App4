using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace Fbi.QuickBooksSolutionTemplate.Core
{
    public class AzureVault
    {
        private string _vaultSecretUri { get; set; }
        private string _vaultClientID { get; set; }
        private string _vaultClientSecret { get; set; }

        public AzureVault(string vaultSecretUri, string vaultClientID, string vaultClientSecret)
        {
            _vaultSecretUri = vaultSecretUri;
            _vaultClientID = vaultClientID;
            _vaultClientSecret = vaultClientSecret;
        }

        public AzureVault()
        {
            _vaultSecretUri = "https://qbokeyvalut.vault.azure.net/secrets/TestSecret/796c920df4744a31b47268fdecfd9b01";
            _vaultClientID = "24c1b85f-81a1-430d-b70c-817419628ce9";
            _vaultClientSecret = "/u5nA1A4pX4YOgZevDb+78gkcsKlKn3UDyl+XFFtmJs=";
        }
        
        public string GetVaultSecret()
        {
            var kvToken = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetVaultToken));
            var kvSecret = kvToken.GetSecretAsync(_vaultSecretUri).Result;
            return kvSecret.Value;
        }

        private async Task<string> GetVaultToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(_vaultClientID, _vaultClientSecret);
            AuthenticationResult authResult = await authContext.AcquireTokenAsync(resource, clientCred);
            if (authResult == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");
            return authResult.AccessToken;
        }

    }
}
