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
        public JsonResult Transfer([FromBody]JObject jsonData)
        {            
            /** Get transfer data */
            var toAddress = jsonData.GetValue("recipientAddress").Value<string>();
            
            /** Get the type of currency */
            var amount = jsonData.GetValue("amount").Value<int>();
            var asset = jsonData.GetValue("asset").Value<string>();
            
            /* Get owner data from private key */
            var fromPrivateKey = jsonData.GetValue("ownerPrivateKeyHash").Value<string>();            
            var fromKeyPair = new KeyPair(LuxUtils.HexToBytes(fromPrivateKey));

            try
            {
                var transactionResult = LuxApiFactory.GetLuxApi().SendAsset(fromKeyPair, toAddress, asset, amount);
                return JsonResultObject(GetTransactionResult(transactionResult));          
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return JsonError($"Unable to make transaction, {e.Message}");
            }
        }     
    }
}