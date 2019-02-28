using System;
using System.Numerics;

namespace NeoFlux.Model
{
    public class RawTransaction
    {
        public String TxID { set; get; }
        public String Contract { set; get; }
        public uint Block { set; get; }
        public String From { set; get; }
        public String To { set; get; }
        public BigInteger Amount { set; get; }
        public String EventType { set; get;  }
    }
}