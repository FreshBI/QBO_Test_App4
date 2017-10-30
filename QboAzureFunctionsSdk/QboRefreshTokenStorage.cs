using Fbi.QuickBooksSolutionTemplate.Core;
using System.Collections.Generic;

namespace Fbi.QuickBooksSolutionTemplate.FunctionEntryPoint
{
    public class QboRefreshTokenStorage : ISecureStorage
    {
        readonly Dictionary<string, object> store = new Dictionary<string, object>();

        public T Retrieve<T>(string key)
        {
            var sqlMule = new SqlMule("Server=tcp:qbofbi.database.windows.net,1433;Initial Catalog=QBO_DW;Persist Security Info=False;User ID=MichaelB;Password=Cr@zyFresh;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            var result = sqlMule.Query("SELECT [RefreshKey],[KeyType] FROM [QBO_DW].[dbo].[QboTokens] WHERE [KeyType] = 'RefreshKey'");

            store[key] = result.ToArray()[0];

            sqlMule.DisposeOfConnection();

            return (T)store[key];
        }

        public void Store<T>(string key, T itemToStore)
        {
            var sqlMule = new SqlMule("Server=tcp:qbofbi.database.windows.net,1433;Initial Catalog=QBO_DW;Persist Security Info=False;User ID=MichaelB;Password=Cr@zyFresh;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            var update = sqlMule.InsertRefreshToken(itemToStore.ToString());

            sqlMule.DisposeOfConnection();
        }
    }
    
}
