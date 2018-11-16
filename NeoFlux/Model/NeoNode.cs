using System;

namespace NeoFlux.Model
{
    public class NeoNode
    {
        public String URL { set; get; }
        public double Latency { set; get; }

        public NeoNode(String url, double latency)
        {
            URL = url;
            Latency = latency;
        }

        public override string ToString()
        {
            return URL;
        }
    }
}