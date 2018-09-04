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

        public Transaction SimpleTransfer(KeyPair from_key, string to_address, BigInteger amount)
        {
            var sender_address_hash = from_key.address.GetScriptHashFromAddress();
            var response = api.CallContract(from_key, ScriptHash, "transfer", new object[] { sender_address_hash, to_address.GetScriptHashFromAddress(), amount });
            return response;
        }

    }
}