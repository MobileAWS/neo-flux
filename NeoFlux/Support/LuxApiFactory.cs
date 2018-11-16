using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Neo.Lux.Core;
using NeoFlux.Model;
using NeoFlux.NeoLux.Core;
using Serilog;

namespace NeoFlux.Support
{
    public static class LuxApiFactory
    {
        public static IConfiguration Configuration { set; get; }        
        public static IHostingEnvironment Environment { set; get; }
        public static NeoNode[] EnabledNodes { set; get; }
        public static NeoNode[] DisabledNodes { set; get; }
        private static int currentNode = 0;

        public static NeoAPI GetLuxApi()
        {
            NeoRPC resultInstance = BaseRpcInstance();
            resultInstance.rpcEndpoint = NextNode();
            resultInstance.UseRetries = false;
            return resultInstance;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void UpdateNodes(NeoNode[] availableNodes, NeoNode[] slownNodes)
        {
            EnabledNodes = (NeoNode[])availableNodes.Clone();
            DisabledNodes = (NeoNode[])slownNodes.Clone();                       
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static string NextNode()
        {            
            if (currentNode >= EnabledNodes.Length - 1)
            {
                currentNode = 0;
            }

            NeoNode result = null;
            if (currentNode < EnabledNodes.Length)
            {
                result = EnabledNodes[currentNode];
                currentNode++;
            }
            else
            {
                result = DisabledNodes[0];
                currentNode = 0;
            }
            Log.Debug($"Using {result} for call");
            return result.URL;
        }

        private static NeoRPC BaseRpcInstance()
        {
            if (Configuration["Neo:Network"] == "Main")
            {
                return NeoRPC.ForMainNet();
            }

            if (Configuration["Neo:Network"] == "Test")
            {
                return NeoRPC.ForTestNet();
            }

            if (Configuration["Neo:Network"] == "AllCode")
            {
                return new NeoFluxNeoRPC("http://ec2-54-89-131-225.compute-1.amazonaws.com");
            }

            return null;
        }
    }
}