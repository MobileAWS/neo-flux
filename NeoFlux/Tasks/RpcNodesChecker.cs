using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NeoFlux.Support;

namespace NeoFlux.Tasks
{
    public class RpcNodesChecker  : BackgroundService  
    {
        private IConfiguration _config;
        private string[] baseNodes;

        public RpcNodesChecker(IConfiguration config)
        {
            _config = config;
            baseNodes = _config.GetSection("Neo:RpcNodes").GetChildren().Select(x => x.Value).ToArray();
        }

        protected  override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            LuxApiFactory.UpdateNodes(baseNodes,new string[0]);
            while (!stoppingToken.IsCancellationRequested)
            {               
                CheckNodes();
                await Task.Delay(10000, stoppingToken);
            }
        }

        private void CheckNodes()
        {
            var nodes = baseNodes;
            var enabledNodes = new List<string>();
            var slowNodes = new List<string>();
            foreach (var node in nodes)
            {
                var request = (HttpWebRequest)WebRequest.Create(node);
                var timer = new Stopwatch();
                timer.Start();
                var response = (HttpWebResponse)request.GetResponse();
                response.Close ();
                timer.Stop();
                if (timer.Elapsed.TotalMilliseconds <= 700)
                {                   
                    enabledNodes.Add(node);
                }
                else
                {
                    slowNodes.Add(node);
                }
                LuxApiFactory.UpdateNodes(enabledNodes.ToArray(),slowNodes.ToArray());
            }
        }        
    }
}