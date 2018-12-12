using System;

namespace NeoFlux.Model
{
    public class NeoPythonNode
    {
        private String URL { set; get; }

        public NeoPythonNode(String url)
        {
            URL = url;
        }

        public override string ToString()
        {
            return URL;
        }
    }
}