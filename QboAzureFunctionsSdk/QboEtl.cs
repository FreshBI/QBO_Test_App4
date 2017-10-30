using Fbi.QuickBooksSolutionTemplate.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Fbi.QuickBooksSolutionTemplate.FunctionEntryPoint
{
    public static class QboEtl
    {
        public static void TrialBalance(JArray parsedJSON)
        {
            var jsonObject = JsonConvert.DeserializeObject<TBRootObject[]>(parsedJSON.ToString());

            var sqlMule = new SqlMule("Server=tcp:qbofbi.database.windows.net,1433;Initial Catalog=QBO_DW;Persist Security Info=False;User ID=MichaelB;Password=Cr@zyFresh;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            try
            {
                sqlMule.ArbitrarySqlCode("delete dbo_tb;");

                foreach (var row in jsonObject)
                {
                    var debit = string.Format("{0:#.00}", Convert.ToDecimal("0.00") / 100);
                    var credit = string.Format("{0:#.00}", Convert.ToDecimal("0.00") / 100);

                    var account = row.ColData[0].value;
                    var accountID = row.ColData[0].id;

                    if (row.ColData[1].value == "")
                    {
                        credit = string.Format("{0:#.00}", Convert.ToDecimal("0.00") / 100);
                    }
                    else
                    {
                        credit = string.Format("{0:#.00}", Convert.ToDecimal(row.ColData[1].value) / 100);
                    }

                    if (row.ColData[2].value == "")
                    {
                        credit = string.Format("{0:#.00}", Convert.ToDecimal("0.00") / 100);
                    }
                    else
                    {
                        credit = string.Format("{0:#.00}", Convert.ToDecimal(row.ColData[2].value) / 100);
                    }

                    sqlMule.ArbitrarySqlCode($"INSERT INTO [QBO_DW].[dbo].[QBO_TB] (Account, id, Debit, Credit) VALUES ('{account}', {accountID}, {debit}, {credit} )");


                }
            }
            catch
            {

            }

            sqlMule.DisposeOfConnection();
        }
    }
}
