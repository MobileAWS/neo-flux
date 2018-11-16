using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using NeoFlux.Support;
using Newtonsoft.Json.Linq;
using Serilog;

namespace NeoFlux.Controllers
{
    [Route("api/[controller]")]    
    public class WalletController : BaseController
    {
        [HttpPost]
        [Route("create")]
        [Authorize]
        public JsonResult Create()
        {
            try
            {
                var privateKey = new byte[32];
                new Random().NextBytes(privateKey);
                var keyPair =  new KeyPair(privateKey);
                dynamic result = new JObject();
                result.privateKey = keyPair.PrivateKey.ToHexString();
                result.publicKey = keyPair.PublicKey.ToHexString();
                result.wif = keyPair.WIF;
                result.address = keyPair.address;
                result.publicKeyHash = keyPair.PublicKeyHash.ToString();
                return Json(result);
            }
            catch (Exception e)
            {
                Log.Error(e.Message + "\n" + e.StackTrace);   
                return JsonError($"Unable to create wallet, {e.Message}");
            }            
        }
        
        [HttpPost]
        [Route("from_wif")]
        [Authorize]
        public JsonResult FromWif([FromBody]JObject jsonData)
        {
            try
            {                
                var wif = jsonData.GetValue("walletWif").Value<string>();                                                                
                var keyPair =  KeyPair.FromWIF(wif);
                dynamic result = new JObject();
                result.privateKey = keyPair.PrivateKey.ToHexString();
                result.publicKey = keyPair.PublicKey.ToHexString();
                result.wif = keyPair.WIF;
                result.address = keyPair.address;
                result.publicKeyHash = keyPair.PublicKeyHash.ToString();
                return Json(result);
            }
            catch (Exception e)
            {
                Log.Error(e.Message + "\n" + e.StackTrace);   
                return JsonError($"Unable to get wallet information, {e.Message}");
            }            
        }     
        
        [HttpPost]
        [Route("state")]
        [Authorize]
        public JsonResult State([FromBody]JObject jsonData)
        {
            try
            {                
                var address = jsonData.GetValue("walletAddress").Value<string>();
                var assetBalances = LuxApiFactory.GetLuxApi().GetAssetBalancesOf(address);
                dynamic result = new JObject();
                foreach (var asset in assetBalances.Keys)
                {
                    result.Add(asset, assetBalances[asset]);
                }                
                return Json(result);
            }
            catch (Exception e)
            {
                Log.Error(e.Message + "\n" + e.StackTrace);   
                return JsonError($"Unable to get wallet state information, {e.Message}");
            }            
        }  
    }
}
