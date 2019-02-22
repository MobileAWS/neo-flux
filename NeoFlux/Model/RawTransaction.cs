using System;

namespace NeoFlux.Model
{
    public class RawTransaction
    {
        public String TxID { set; get; }
        public String Contract { set; get; }
        public uint Block { set; get; }
        public String From { set; get; }
        public String To { set; get; }
        public Decimal Amount { set; get; }
        public String EventType { set; get;  }
    }
}