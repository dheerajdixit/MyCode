using System;
using System.Runtime.CompilerServices;
namespace Model
{
    

    public class AllTechnicals
    {
        public Model.SuperTrend SuperTrend { get; set; }

        public Model.MACD MACD { get; set; }

        public Model.RSI RSI { get; set; }

        public double SMA20 { get; set; }

        public double SMA50 { get; set; }

        public double SMA200 { get; set; }
    }
}

