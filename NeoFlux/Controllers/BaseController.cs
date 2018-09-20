using System;
using System.Numerics;
using System.Security.Permissions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Neo.Lux.Core;
using Newtonsoft.Json.Linq;
using SQLitePCL;

namespace NeoFlux.Controllers
{
    public class BaseController : Controller
    {
        private const int RETRY_LIMIT = 10;
        
        public BigInteger GetDecimalsValueFromJson(JObject jsonData)
        {
            var decimalToken = jsonData.GetValue("decimals");
            var decimals = decimalToken?.Value<int>() ?? 8;
            return new BigInteger(decimals);
        }

        public int GetRetryValueFromJson(JObject jsonData)
        {
            var disableRetry = jsonData.GetValue("disableRetry");
            if (disableRetry == null)
            {
                return RETRY_LIMIT;
            }
            return disableRetry.Value<bool>() ? 1 : RETRY_LIMIT;           
        }
        
        public string GetTransactionResult(Transaction tx)
        {
            if (tx == null)
            {
                return "Unable to make transaction, an unknown error occurred";
            }
            return tx.ToString();
        }

        public JsonResult JsonError(string message)
        {
            dynamic error = new JObject();
            error.message = message;
            return Json(error);
        }

        public JsonResult JsonResultObject(object value)
        {
            dynamic result = new JObject();
            result.result = value;
            return Json(result);
        }

        public JsonResult DoWithRetry(int limit, Func<int, JsonResult> f)
        {
            var count = 0;     
            while (count < limit)
            {
                var result = f(count);
                if (result != null)
                {
                    return result;
                }
                
                if (count < limit - 1)
                {
                    Task.Delay(1000 * count).Wait();   
                }                                
                count++;
            }
            return JsonError($"Unable to make transaction,  executed {limit} times and all failed");
        }
    }
}