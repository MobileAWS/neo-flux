using System;
using System.Numerics;
using System.Security.Permissions;
using Microsoft.AspNetCore.Mvc;
using Neo.Lux.Core;
using Newtonsoft.Json.Linq;

namespace NeoFlux.Controllers
{
    public class BaseController : Controller
    {
        public BigInteger GetDecimalsValueFromJson(JObject jsonData)
        {
            var decimalToken = jsonData.GetValue("decimals");
            var decimals = decimalToken?.Value<int>() ?? 8;
            return new BigInteger(decimals);
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
    }
}