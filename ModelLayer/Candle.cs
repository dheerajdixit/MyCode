using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Model
{
    

    public class Candle
    {
        public double Token { get; set; }

        public double ATRStopLoss { get; set; }

        public string CP { get; set; }

        public double Lowest { get; set; }

        public double Highest { get; set; }

        public string Stock { get; set; }

        [XmlIgnore]
        public Candle PreviousCandle { get; set; }

        public AllTechnicals AllIndicators { get; set; }

        public string Treding { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Open { get; set; }

        public double Close { get; set; }

        public double HHigh { get; set; }

        public double HLow { get; set; }

        public double HOpen { get; set; }

        public double HClose { get; set; }

        public double SMA20 { get; set; }

        public double SMA50 { get; set; }

        public double SMA200 { get; set; }

        public double MACD { get; set; }

        public double MACD9 { get; set; }

        public double SuperTrend { get; set; }

        public double SuperTrendDaily { get; set; }

        public double MACDDaily { get; set; }

        public double SMA20Daily { get; set; }

        public double RSI14 { get; set; }

        public double ATR7 { get; set; }

        public string CandleType =>
            ((this.Close <= this.Open) ? ((this.Close >= this.Open) ? "D" : "R") : "G");

        public double Volume { get; set; }

        public string HCandleType { get; set; }

        public double SMA5 { get; set; }

        public double Histogram =>
            (this.MACD - this.MACD9);

        public double ATR { get; set; }

        public DateTime TimeStamp { get; set; }

        public SuperTrendInd STrend { get; set; }

        public Trade Trade { get; set; }
    }

    


}

