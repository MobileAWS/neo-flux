using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NeoFlux.Model;
using NeoFlux.Support;
using Serilog;

namespace NeoFlux.Tasks
{
    public class RpcNodesChecker : BackgroundService
    {
        private IConfiguration _config;
        private readonly NeoNode[] baseNodes;        

        public RpcNodesChecker(IConfiguration config)
        {
            _config = config;
            baseNodes = _config.GetSection("Neo:RpcNodes").GetChildren().Select(x => new NeoNode(x.Value, 0)).ToArray();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            LuxApiFactory.UpdateNodes(baseNodes, new NeoNode[0]);
            if (_config["CheckServersAvailability"].Equals("false"))
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                CheckNodes();
                await Task.Delay(20000, stoppingToken);
            }
        }

        private void CheckNodes()
        {
            var toProcess = baseNodes.Length;
            var nodes = baseNodes;
            var enabledNodes = new List<NeoNode>();
            var slowNodes = new List<NeoNode>();
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                foreach (var nodeObj in nodes)
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        var node = state as NeoNode;
                        try
                        {
                            Log.Debug($"Checking availability for {node.URL}");
                            var request = (HttpWebRequest) WebRequest.Create(node.URL);
                            request.Timeout = 30000;
                            var timer = new Stopwatch();
                            timer.Start();
                            var response = (HttpWebResponse) request.GetResponse();
                            response.Close();
                            timer.Stop();
                            node.Latency = timer.Elapsed.TotalMilliseconds;
                            if (timer.Elapsed.TotalMilliseconds <= 1000)
                            {
                                enabledNodes.Add(node);
                            }
                            else
                            {
                                slowNodes.Add(node);
                            }
                        }
                        catch (Exception e)
                        {
                           Log.Error($"Failed to check, rpc node is not reachable {node.URL}: {e.Message}");
                            node.Latency = 999999999;
                            slowNodes.Add(node);
                        }

                        if (Interlocked.Decrement(ref toProcess) == 0)
                        {
                            resetEvent.Set();
                        }
                    }, nodeObj);
                }                
                resetEvent.WaitOne();
                LuxApiFactory.UpdateNodes(enabledNodes.ToArray(), slowNodes.ToArray());
            }
        }
    }
}