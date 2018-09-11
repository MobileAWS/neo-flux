using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Lux.Core;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using Neo.SmartContract.Framework;
using NeoFlux.NeoLux.Core;
using NeoFlux.Support;
using Newtonsoft.Json.Linq;

namespace NeoFlux.Controllers
{
    [Route("api/[controller]")]    
    public class ContractController : BaseController
    {
        [HttpPost]
        [Route("info")]
        [Authorize]
        public JsonResult Info([FromBody] JObject jsonData)
        {
            var contractScripHash = jsonData.GetValue("contractScriptHash").Value<string>();           
            try
            {                               
                var token = new NeoFluxNEP5(LuxApiFactory.GetLuxApi(), contractScripHash, null, GetDecimalsValueFromJson(jsonData));
                dynamic result = new JObject();
                result.symbol = token.Symbol;
                result.name = token.Name;
                result.decimals = token.Decimals;
                result.totalSupply = token.TotalSupply;
                return Json(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return JsonError($"Unable to make transaction, {e.Message}");
            }            
        }

        [HttpPost]
        [Route("total_supply")]
        [Authorize]
        public JsonResult TotalSupply([FromBody]JObject jsonData)
        {
            var contractScripHash = jsonData.GetValue("contractScriptHash").Value<string>();
            var token = new NeoFluxNEP5(LuxApiFactory.GetLuxApi(), contractScripHash, null, GetDecimalsValueFromJson(jsonData));
            return JsonResultObject((long)token.TotalSupply);            
        }
        
        [HttpPost]
        [Route("generate_to")]
        [Authorize]
        public JsonResult GenerateTo([FromBody]JObject jsonData)
        {
            /** Get transfer data */
            var contractScripHash = jsonData.GetValue("contractScriptHash").Value<string>();            
            var toAddress = jsonData.GetValue("recipientAddress").Value<string>();
            var amount = jsonData.GetValue("amount").Value<int>();            
            
            /* Get owner data from private key */
            var fromPrivateKey = jsonData.GetValue("ownerPrivateKeyHash").Value<string>();            
            var fromKeyPair = new KeyPair(LuxUtils.HexToBytes(fromPrivateKey));

            var token = new NeoFluxNEP5(LuxApiFactory.GetLuxApi(), contractScripHash, null, GetDecimalsValueFromJson(jsonData));
            try
            {
                var transactionResult = token.GenerateTo(fromKeyPair, toAddress, new BigInteger(amount));
                return JsonResultObject(GetTransactionResult(transactionResult));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return JsonError($"Unable to make transaction, {e.Message}");
            }                      
        }     

        [HttpPost]
        [Route("balance_of")]
        [Authorize]
        public JsonResult BalanceOf([FromBody]JObject jsonData)
        {            
            var contractScripHash = jsonData.GetValue("contractScriptHash").Value<string>();            
            var targetAddress = jsonData.GetValue("targetAddress").Value<string>();                              
            var token = new NeoFluxNEP5(LuxApiFactory.GetLuxApi(), contractScripHash, null, GetDecimalsValueFromJson(jsonData));
            return JsonResultObject(token.BalanceForAddress(targetAddress));                             
        }     

        [HttpPost]
        [Route("transfer")]
        [Authorize]
        public JsonResult Transfer([FromBody]JObject jsonData)
        {
            /** Get transfer data */
            var contractScripHash = jsonData.GetValue("contractScriptHash").Value<string>();            
            var toAddress = jsonData.GetValue("recipientAddress").Value<string>();
            var amount = jsonData.GetValue("amount").Value<decimal>();            
            
            /* Get owner data from private key */
            var fromPrivateKey = jsonData.GetValue("ownerPrivateKeyHash").Value<string>();            
            var fromKeyPair = new KeyPair(LuxUtils.HexToBytes(fromPrivateKey));

            var token = new NeoFluxNEP5(LuxApiFactory.GetLuxApi(), contractScripHash, null, GetDecimalsValueFromJson(jsonData));
            try
            {
                var transactionResult = token.Transfer(fromKeyPair, toAddress, amount);
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
