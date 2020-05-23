using System;
using System.Runtime.CompilerServices;

namespace Model
{
    
    public class SuperTrend
    {
        private int _trend = 0;

        public double ATR7 { get; set; }

        public double FinalUpperBand { get; set; }

        public double FinalLowerBand { get; set; }

        public int Trend { get; set; }

        public double SuperTrendValue { get; set; }

        public double ATR14 { get; set; }

        public double TrendPrice { get; set; }

        public bool TowardsBuy { get; set; }

        public bool TowardsSell { get; set; }

        public int CandlePastSinceLastChange { get; set; }

        public int CountOfTrendPriceBroken { get; set; }
    }
}

