using System;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using NeoFlux.Model;
using NeoFlux.Support;
using Newtonsoft.Json.Linq;

namespace NeoFlux.Controllers
{
    [Route("api/[controller]")]    
    public class ServerController : BaseController
    {
        [HttpPost]
        [Route("nodes")]
        [Authorize]
        public JsonResult Nodes()
        {
            JArray enabled = new JArray();
            JArray disabled = new JArray();
            foreach (var node in LuxApiFactory.EnabledNodes)
            {
                dynamic nodeObject = new JObject();
                nodeObject.url = node.URL;
                nodeObject.latency = node.Latency;
                enabled.Add(nodeObject);
            }

            foreach (var node in LuxApiFactory.DisabledNodes)
            {
                dynamic nodeObject = new JObject();
                nodeObject.url = node.URL;
                nodeObject.latency = node.Latency;
                disabled.Add(nodeObject);
            }
            
            dynamic result = new JObject();
            result.enabledNodes = enabled;
            result.disabledNodes = disabled;
            return Json(result);                    
        }        
    }
}
