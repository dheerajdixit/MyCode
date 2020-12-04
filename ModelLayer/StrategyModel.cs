using System;
using System.Runtime.CompilerServices;

namespace Model
{

    public class StrategyModel
    {
        public string Stock { get; set; }

        public double Volume { get; set; }

        public double Range { get; set; }

        public DateTime Date { get; set; }

        public double Close { get; set; }

        public double Open { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public string CandleType =>
            ((this.Close <= this.Open) ? ((this.Close >= this.Open) ? "D" : "R") : "G");

        public Trade Trade { get; set; }

        public double PreviousClose { get; set; }

        public double Imp1 { get; set; }

        public Candle CurrentCandle { get; set; }

        
    }
    public enum Trade
    {
        BUY,
        SELL,
        NONE
    }
}

