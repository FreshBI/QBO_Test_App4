using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Fbi.QuickBooksSolutionTemplate.Core;

namespace Fbi.QuickBooksSolutionTemplate.FunctionEntryPoint
{
    public static class FunctionDemon
    {
        [FunctionName("FunctionDemon")]
        public static async void Run([TimerTrigger("*/30 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# function started at: {DateTime.Now}");

            var intuitOAuthWrapper = new QboOAuthWrapper();

            var gl = await intuitOAuthWrapper.ExecuteGet("/reports/GeneralLedger?date_macro=all&columns=tx_date&sort_order=ascend");
            var gl2 = await intuitOAuthWrapper.ExecuteGet("/reports/GeneralLedger?date_macro=all&columns=tx_date,txn_type,doc_num,name,item_name,account_name,subt_nat_amount,debt_amt,credit_amt,inv_date,quantity,rate,cust_name,vend_name,emp_name,split_acc,is_ar_paid&accounting_method=Accrual");
            var tb = await intuitOAuthWrapper.ExecuteGet("reports/TrialBalance");

            JArray tbParsedJSON = JArray.Parse(tb.Rows.Row.ToString());
            JArray glParsedJSON = JArray.Parse(gl.Rows.Row.ToString());
            JArray glParsedJSON2 = JArray.Parse(gl2.Rows.Row.ToString());

            QboEtl.TrialBalance(tbParsedJSON);

            var jsonObject = JsonConvert.DeserializeObject<RootObject[]>(glParsedJSON.ToString());

            var q = gl.ToString();
            var u = q.Replace("'", "''");
            var e = u.Replace(" ", "");

            var q2 = gl2.ToString();
            var u2 = q2.Replace("'", "''");
            var e2 = u2.Replace(" ", "");


            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            var r = e.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty);
            var r2 = e2.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty);


            AzureBlobStorage.OverWriteData(r, "myblob");
            AzureBlobStorage.OverWriteData(r2, "myblob2");

            log.Info($"Data is: {tb}");


        }
    }

}