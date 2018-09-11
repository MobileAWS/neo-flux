using System.Collections.Generic;
using System.Numerics;
using Neo.Lux.Core;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;

namespace NeoFlux.NeoLux.Core
{
    public class NeoFluxNEP5 : NEP5
    {
        public NeoFluxNEP5(NeoAPI api, string contractHash) : base(api,contractHash)
        {

        }
        public NeoFluxNEP5(NeoAPI api, byte[] contractHash) : base(api, contractHash)
        {

        }


        public NeoFluxNEP5(NeoAPI api, UInt160 contractHash): base(api, contractHash)
        {
        }

        public NeoFluxNEP5(NeoAPI api, string contractHash, string name, BigInteger decimals) : base(api, contractHash,
            name, decimals)
        {
        }
        
        public Transaction GenerateTo(KeyPair ownwerKey, string to_address, BigInteger amount)
        {                     
            var response = this.api.CallContract(ownwerKey, this.ScriptHash, "generate_tokens", new object[] { to_address.GetScriptHashFromAddress(), amount});
            return response;
        }

        public decimal BalanceForAddress(string address)
        {
            return BalanceForAddress(address.GetScriptHashFromAddress());
        }

        private decimal BalanceForAddress(byte[] addressHash)
        {
            var response = new InvokeResult();
            try
            {
                response = api.InvokeScript(ScriptHash, "balanceOf", new object[] { addressHash });
                var bytes = (byte[])response.stack[0];
                var balance = new BigInteger(bytes);
                return BigIntegerToDecimal(balance, this.Decimals);
            }
            catch
            {
                throw new NeoException("Api did not return a value." + response);
            }
        }

        private static decimal BigIntegerToDecimal(BigInteger value, BigInteger decimals)
        {
            return value == 0 ? 0 : value.ToDecimal((int)decimals);
        }
    }
}