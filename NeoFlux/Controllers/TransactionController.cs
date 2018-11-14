using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using NeoFlux.NeoLux.Core;
using NeoFlux.Support;
using Newtonsoft.Json.Linq;
using Serilog;

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
            return DoWithRetry(GetRetryValueFromJson(jsonData), (retryCount) =>
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
                    Log.Error(e.Message + "\n" + e.StackTrace);   
                }

                return null;
            });
        }
        
        [HttpPost]
        [Route("get_info")]
        [Authorize]
        public JsonResult GetTransaction([FromBody]JObject jsonData)
        {
            try
            {                
                var txId = jsonData.GetValue("txId").Value<string>();
                var transactionResult = LuxApiFactory.GetLuxApi().GetTransaction(txId);
                if (transactionResult != null)
                {
                    dynamic result = new JObject();
                    result.hash = transactionResult.Hash.ToString();
                    result.type = transactionResult.type;
                    result.version = transactionResult.version;
                    result.block = transactionResult.block;
                    dynamic inputs = new JArray();
                    foreach (var input in transactionResult.inputs)
                    {
                        dynamic inputJson = new JObject();
                        inputJson.prevHash = input.prevHash.ToString();
                        inputJson.prevIndex = input.prevIndex;
                        inputs.Add(inputJson);

                    }
                    result.inputs = inputs;
                    dynamic outputs = new JArray();
                    foreach (var output in transactionResult.outputs)
                    {
                        dynamic outputJson = new JObject();
                        outputJson.asset = output.assetID;
                        outputJson.address = output.scriptHash.ToAddress();
                        outputJson.value = output.value;
                        outputs.Add(outputJson);

                    }
                    result.outputs = outputs;
                    return Json(result);
                }

                return null;
            }
            catch (Exception e)
            {
                Log.Error(e.Message + "\n" + e.StackTrace);
                return JsonError($"Unable to get transaction information, {e.Message}");
            }
        }
    }
}