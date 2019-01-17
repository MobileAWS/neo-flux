using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using NeoFlux.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace NeoFlux.Controllers
{
    [Route("api/[controller]")]
    public class BlockChainController : BaseController
    {
        [HttpPost]
        [Route("get_block")]
        [Authorize]
        public JsonResult GetBlock([FromBody]JObject jsonData)
        {
            try
            {                
                var blockIndex = jsonData.GetValue("blockIndex").Value<uint>();
                var blockResult = LuxApiFactory.GetLuxApi().GetBlock(blockIndex);
                if (blockResult != null)
                {
                    dynamic result = new JObject();
                    result.version = blockResult.Version;
                    result.height = blockResult.Height;
                    result.merkleRoot = blockResult.MerkleRoot;
                    result.timestamp = blockResult.Timestamp;
                    result.timestamp = blockResult.Timestamp;
                    result.previousHash = blockResult.PreviousHash.ToString();
                    dynamic transactionIds = new JArray();
                    foreach (var transaction in blockResult.transactions)
                    {
                        string key = transaction.Hash.ToString();
                        transactionIds.Add(key);
                    }
                    result.transactions = transactionIds;
                    return Json(result);
                }

                return null;
            }
            catch (Exception e)
            {
                Log.Error(e.Message + "\n" + e.StackTrace);
                return JsonError($"Unable to get block information, {e.Message}");
            }
        }
        
        [HttpPost]
        [Route("get_best_block")]
        [Authorize]
        public JsonResult GetBestBlock([FromBody]JObject jsonData)
        {
            try
            {                
                var bestBlockIndex = LuxApiFactory.GetLuxApi().GetBlockHeight() - 1;
                var blockResult = LuxApiFactory.GetLuxApi().GetBlock(bestBlockIndex);
                if (blockResult != null)
                {
                    dynamic result = new JObject();
                    result.version = blockResult.Version;
                    result.height = blockResult.Height;
                    result.merkleRoot = blockResult.MerkleRoot;
                    result.timestamp = blockResult.Timestamp;
                    result.timestamp = blockResult.Timestamp;
                    result.previousHash = blockResult.PreviousHash.ToString();
                    dynamic transactionIds = new JArray();
                    foreach (var transaction in blockResult.transactions)
                    {
                        string key = transaction.Hash.ToString();
                        transactionIds.Add(key);
                    }
                    result.transactions = transactionIds;
                    return Json(result);
                }

                return null;
            }
            catch (Exception e)
            {
                Log.Error(e.Message + "\n" + e.StackTrace);
                return JsonError($"Unable to get block information, {e.Message}");
            }
        }
        
        [HttpPost]
        [Route("get_block_count")]
        [Authorize]
        public JsonResult GetBlockCount([FromBody]JObject jsonData)
        {
            try
            {                
                var bestBlockIndex = LuxApiFactory.GetLuxApi().GetBlockHeight();
                return JsonResultObject(bestBlockIndex);
            }
            catch (Exception e)
            {
                Log.Error(e.Message + "\n" + e.StackTrace);
                return JsonError($"Unable to get block information, {e.Message}");
            }
        }
    }
}