using System;
using LunarLabs.Parser;
using Neo.Lux.Core;
using NeoFlux.Model;

namespace NeoFlux.NeoLux.Core
{
    public class NeoFluxNeoRPC : NeoRPC
    {
        public NeoFluxNeoRPC(string neoscanURL) : base(neoscanURL)
        {
        }

        protected override string GetRPCEndpoint()
        {
            return rpcEndpoint;
        }
    }
}