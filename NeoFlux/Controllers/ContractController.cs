using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Lux.Core;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using Neo.SmartContract.Framework;
using NeoFlux.Model;
using NeoFlux.NeoLux.Core;
using NeoFlux.Support;
using Newtonsoft.Json.Linq;
using Serilog;

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
                var token = new NeoFluxNEP5(LuxApiFactory.GetLuxApi(), contractScripHash, null,
                    GetDecimalsValueFromJson(jsonData));
                dynamic result = new JObject();
                result.symbol = token.Symbol;
                result.name = token.Name;
                result.decimals = token.Decimals;
                result.totalSupply = token.TotalSupply;
                return Json(result);
            }
            catch (Exception e)
            {
                Log.Error(e.Message + "\n" + e.StackTrace);
                return JsonError($"Unable to make transaction, {e.Message}");
            }
        }

        [HttpPost]
        [Route("total_supply")]
        [Authorize]
        public JsonResult TotalSupply([FromBody] JObject jsonData)
        {
            var contractScripHash = jsonData.GetValue("contractScriptHash").Value<string>();
            var token = new NeoFluxNEP5(LuxApiFactory.GetLuxApi(), contractScripHash, null,
                GetDecimalsValueFromJson(jsonData));
            return JsonResultObject((long) token.TotalSupply);
        }

        [HttpPost]
        [Route("generate_to")]
        [Authorize]
        public JsonResult GenerateTo([FromBody] JObject jsonData)
        {
            return DoWithRetry(GetRetryValueFromJson(jsonData), retryCount =>
            {
                /** Get transfer data */
                var contractScripHash = jsonData.GetValue("contractScriptHash").Value<string>();
                var toAddress = jsonData.GetValue("recipientAddress").Value<string>();
                var amount = jsonData.GetValue("amount").Value<double>();
                Log.Debug($"GenerateTo called with recipient {toAddress} and value {amount}");                
                /* Get owner data from private key */
                var fromPrivateKey = jsonData.GetValue("ownerPrivateKeyHash").Value<string>();
                var fromKeyPair = new KeyPair(LuxUtils.HexToBytes(fromPrivateKey));

                var token = new NeoFluxNEP5(LuxApiFactory.GetLuxApi(), contractScripHash, null,
                    GetDecimalsValueFromJson(jsonData));
                try
                {
                    Log.Debug($"Running transaction for recipient {toAddress}");
                    var transactionResult = token.GenerateTo(fromKeyPair, toAddress, new BigInteger(amount));
                    if (transactionResult != null)
                    {
                        dynamic result = new JObject();
                        result.result = transactionResult.ToString();
                        result.retries = retryCount;
                        Log.Debug($"Sent {amount} to {toAddress}");
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
        [Route("balance_of")]
        [Authorize]
        public JsonResult BalanceOf([FromBody] JObject jsonData)
        {
            return DoWithRetry(GetRetryValueFromJson(jsonData), retryCount =>
            {
                try
                {
                    var contractScripHash = jsonData.GetValue("contractScriptHash").Value<string>();
                    var targetAddress = jsonData.GetValue("targetAddress").Value<string>();
                    var token = new NeoFluxNEP5(LuxApiFactory.GetLuxApi(), contractScripHash, null,
                        GetDecimalsValueFromJson(jsonData));
                    return JsonResultObject(token.BalanceForAddress(targetAddress));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);                    
                }
                return null;
            });
        }

        [HttpPost]
        [Route("transfer")]
        [Authorize]
        public JsonResult Transfer([FromBody] JObject jsonData)
        {
            return DoWithRetry(GetRetryValueFromJson(jsonData), (retryCount) =>
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
        [Route("get_transfer_transaction_info")]
        [Authorize]
        public JsonResult GetTransferTransactionInfo([FromBody]JObject jsonData)
        {
            return DoWithRetry(GetRetryValueFromJson(jsonData), retryCount =>
            {
                try
                {              
                    var txId = jsonData.GetValue("txId").Value<string>();
                    bool findBlock = jsonData.ContainsKey("findBlock") && jsonData.GetValue("findBlock").Value<bool>();
                    LuxApiExtensions api = new LuxApiExtensions((NeoRPC)LuxApiFactory.GetLuxApi());
                    RawTransaction transaction = api.GetTransferTransaction(txId, findBlock);
                    if(transaction != null){                   
                        dynamic result = new JObject();
                        result.txId = transaction.TxID;
                        result.block = transaction.Block;
                        result.asset = transaction.Contract;
                        result.addrFrom = transaction.From;
                        result.addrTo = transaction.To;
                        result.amount = transaction.Amount;
                        return JsonResultObject(result);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message + "\n" + e.StackTrace);
                    return JsonError($"Unable to get transaction information, {e.Message}");
                }
                return null;
            });
        }   
    }

    public class LuxApiExtension
    {
    }
}