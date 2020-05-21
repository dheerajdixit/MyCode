using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }

    public class CIndexes
    {
        public string company { get; set; }
        public int L { get; set; }
        public int I1 { get; set; }
        public int I2 { get; set; }
        public int I3 { get; set; }
    }

    public class Indicator
    {
        public string IndicatorName { get; set; }
        public List<double> numericParametersCollection { get; set; }
        public List<string> textParametersCollection { get; set; }
    }

    public class SR
    {
        public double price { get; set; }
        public string LevelName { get; set; }
        public string SupportOrResistance { get; set; }

    }

  
    public class SuperTrendInd
    {
        public double FinalUpperBand { get; set; }
        public double FinalLowerBand { get; set; }
        public double Trend { get; set; }
        public double SuperTrend { get; set; }
        public double AvgTr { get; set; }
    }
    public class StockData
    {
        public double ATR14 { get; set; }
        public int Category { get; set; }
        public List<SR> Levels { get; set; }
        public string Symbol { get; set; }
        public DateTime TradingDate { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Vol { get; set; }
        public string Direction { get; set; }
        public int Quantity { get; set; }
        public double Risk { get; set; }
        public double stopLoss { get; set; }
        public String Patern { get; set; }
        public String Candle { get { if (Close > Open) return "G"; else if (Open > Close) return "R"; else return "D"; } }


        //daily
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

        //yearly
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

        //monthly
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


        //weekly
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


        //indicators
        public double MACD { get; set; }
        public double MACD9 { get; set; }
        public double HISTOGRAM { get; set; }
        public double SuperTrend { get; set; }
        public double SMA20 { get; set; }
        public double SMA50 { get; set; }
        public double SMA200 { get; set; }


    }
}
