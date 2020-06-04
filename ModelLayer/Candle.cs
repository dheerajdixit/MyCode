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

        public double dPP { get; set; }

        public double dR1 { get; set; }

        public double dR2 { get; set; }

        public double dR3 { get; set; }

        public double dS1 { get; set; }

        public double dS2 { get; set; }

        public double dS3 { get; set; }

        public double dHigh { get; set; }

        public double dLow { get; set; }

        public double dOpen { get; set; }

        public double dClose { get; set; }

        public DateTime YearStartDate { get; set; }

        public DateTime YearEndDate { get; set; }

        public double yPP { get; set; }

        public double yR1 { get; set; }

        public double yR2 { get; set; }

        public double yR3 { get; set; }

        public double yS1 { get; set; }

        public double yS2 { get; set; }

        public double yS3 { get; set; }

        public double yHigh { get; set; }

        public double yLow { get; set; }

        public double yOpen { get; set; }

        public double yClose { get; set; }

        public DateTime MonthStartDate { get; set; }

        public DateTime MonthEndDate { get; set; }

        public double mPP { get; set; }

        public double mR1 { get; set; }

        public double mR2 { get; set; }

        public double mR3 { get; set; }

        public double mS1 { get; set; }

        public double mS2 { get; set; }

        public double mS3 { get; set; }

        public double mHigh { get; set; }

        public double mLow { get; set; }

        public double mOpen { get; set; }

        public double mClose { get; set; }

        public double curMonthOpen { get; set; }

        public double curMonthLow { get; set; }

        public double curMonthHigh { get; set; }

        public double curMonthClose { get; set; }

        public DateTime WeekStartDate { get; set; }

        public DateTime WeekEndDate { get; set; }

        public double wPP { get; set; }

        public double wR1 { get; set; }

        public double wR2 { get; set; }

        public double wR3 { get; set; }

        public double wS1 { get; set; }

        public double wS2 { get; set; }

        public double wS3 { get; set; }

        public double wHigh { get; set; }

        public double wLow { get; set; }

        public double wOpen { get; set; }

        public double wClose { get; set; }

        public double curWeekOpen { get; set; }

        public double curWeekLow { get; set; }

        public double curWeekHigh { get; set; }

        public double curWeekClose { get; set; }

    }




}

