using System;
using System.Runtime.CompilerServices;

namespace Model
{

    public class MACD
    {
        public double macd { get; set; }

        public double macd9 { get; set; }

        public double histogram { get; set; }
    }

    public class BollingerBand
    {
        public double Upper { get; set; }
        public double Middle { get; set; }
        public double Lower { get; set; }
    }
}

