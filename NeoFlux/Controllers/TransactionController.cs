using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using NeoFlux.NeoLux.Core;
using NeoFlux.Support;
using Newtonsoft.Json.Linq;

namespace NeoFlux.Controllers
{
    [Route("api/[controller]")]
    public class TransactionController : BaseController
    {
        [HttpPost]
        [Route("transfer")]
        [Authorize]
        public JsonResult Transfer([FromBody] JObject jsonData)
        {
            return DoWithRetry(GetRetryValueFromJson(jsonData), retryCount =>
            {
                /** Get transfer data */
                var toAddress = jsonData.GetValue("recipientAddress").Value<string>();

                /** Get the type of currency */
                var amount = jsonData.GetValue("amount").Value<decimal>();
                var asset = jsonData.GetValue("asset").Value<string>();

                /* Get owner data from private key */
                var fromPrivateKey = jsonData.GetValue("ownerPrivateKeyHash").Value<string>();
                var fromKeyPair = new KeyPair(LuxUtils.HexToBytes(fromPrivateKey));

                try
                {
                    var transactionResult = LuxApiFactory.GetLuxApi().SendAsset(fromKeyPair, toAddress, asset, amount);
                    if (transactionResult != null)
                    {
                        dynamic result = new JObject();
                        result.result = transactionResult.ToString();
                        result.retries = retryCount;
                        return Json(result);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return null;
            });
        }
    }
}