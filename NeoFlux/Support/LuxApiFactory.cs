using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Neo.Lux.Core;
using NeoFlux.NeoLux.Core;

namespace NeoFlux.Support
{
    public static class LuxApiFactory
    {
        public static IConfiguration Configuration { set; get; }
        public static IHostingEnvironment Environment { set; get; }
        private static string[] enabledNodes;
        private static string[] disabledNodes;
        private static int currentNode = 0;

        public static NeoAPI GetLuxApi()
        {
            NeoRPC resultInstance = BaseRpcInstance();
            resultInstance.rpcEndpoint = NextNode();
            return resultInstance;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void UpdateNodes(string[] availableNodes, string[] slownNodes)
        {
            enabledNodes = (string[])availableNodes.Clone();
            disabledNodes = (string[])slownNodes.Clone();                       
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static string NextNode()
        {            
            if (currentNode >= enabledNodes.Length - 1)
            {
                currentNode = 0;
            }
            var result = enabledNodes[currentNode];
            currentNode++;
            Console.WriteLine($"Using {result}");
            return result;
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