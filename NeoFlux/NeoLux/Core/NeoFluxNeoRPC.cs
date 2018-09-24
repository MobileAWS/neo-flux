using Neo.Lux.Core;

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