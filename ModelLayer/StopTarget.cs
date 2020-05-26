using System;
using System.Runtime.CompilerServices;

namespace Model
{
    
    public class StopTarget
    {
        public StopTarget(StrategyModel current, Idea stoplossidea)
        {
            int stoploss = stoplossidea.Stoploss;
            if (stoploss == 0)
            {
                this.StopLossRange = (current.High - current.Low) / 2.0;
                this.Stoploss = (current.CandleType == "G") ? (current.Close - this.StopLossRange) : (current.Close + this.StopLossRange);
                this.BookProfit1 = (current.CandleType == "G") ? (current.Close + this.StopLossRange) : (current.Close - this.StopLossRange);
                this.BookProfit2 = (current.CandleType == "G") ? (current.Close + (2.0 * this.StopLossRange)) : (current.Close - (2.0 * this.StopLossRange));
            }
            else if (stoploss == 1)
            {
                this.StopLossRange = (current.Trade == Trade.BUY) ? (current.High - current.Low) : (current.High - current.Low);
                this.Stoploss = (current.Trade == Trade.BUY) ? (current.Close - this.StopLossRange) : (current.Close + this.StopLossRange);
                this.BookProfit1 = (current.Trade == Trade.BUY) ? (current.Close + this.StopLossRange) : (current.Close - this.StopLossRange);
                this.BookProfit2 = (current.Trade == Trade.BUY) ? (current.Close + (2.0 * this.StopLossRange)) : (current.Close - (2.0 * this.StopLossRange));
            }
            else if (stoploss == 2)
            {
                Candle highLow = this.GetHighLow(current.CurrentCandle);
                this.StopLossRange = (current.Trade == Trade.BUY) ? (current.Close - highLow.Low) : (highLow.High - current.Close);
                this.Stoploss = (current.Trade == Trade.BUY) ? (current.Close - this.StopLossRange) : (current.Close + this.StopLossRange);
                this.BookProfit1 = (current.Trade == Trade.BUY) ? (current.Close + this.StopLossRange) : (current.Close - this.StopLossRange);
                this.BookProfit2 = (current.Trade == Trade.BUY) ? (current.Close + (2.0 * this.StopLossRange)) : (current.Close - (2.0 * this.StopLossRange));
            }
            else if (stoploss != 5)
            {
                this.StopLossRange = Math.Abs((double) (current.Close - current.Imp1));
                this.Stoploss = (current.CandleType == "G") ? (current.Close - this.StopLossRange) : (current.Close + this.StopLossRange);
                this.BookProfit1 = (current.CandleType == "G") ? (current.Close + this.StopLossRange) : (current.Close - this.StopLossRange);
                this.BookProfit2 = (current.CandleType == "G") ? (current.Close + (2.0 * this.StopLossRange)) : (current.Close - (2.0 * this.StopLossRange));
            }
            else
            {
                Candle highLow = this.GetHighLow(current.CurrentCandle);
                this.StopLossRange = (current.CandleType == "G") ? (current.Close - highLow.Low) : (highLow.High - current.Close);
                this.Stoploss = (current.CandleType == "G") ? (current.Close - this.StopLossRange) : (current.Close + this.StopLossRange);
                this.BookProfit1 = (current.CandleType == "G") ? (current.Close + this.StopLossRange) : (current.Close - this.StopLossRange);
                this.BookProfit2 = (current.CandleType == "G") ? (current.Close + (2.0 * this.StopLossRange)) : (current.Close - (2.0 * this.StopLossRange));
            }
            int interval = stoplossidea.Interval;
            if (interval <= 10)
            {
                if (interval == 5)
                {
                    this.FinalCandle = 0x49;
                }
                else if (interval == 10)
                {
                    this.FinalCandle = 0x24;
                }
            }
            else if (interval == 15)
            {
                this.FinalCandle = 0x17;
            }
            else if (interval == 30)
            {
                this.FinalCandle = 11;
            }
            else if (interval == 60)
            {
                this.FinalCandle = 5;
            }
        }

        private Candle GetHighLow(Candle c)
        {
            double high = 0.0;
            double low = 999999999.0;
            int num3 = 0;
            while (true)
            {
                if (num3 >= 3)
                {
                    Candle candle1 = new Candle();
                    candle1.High = high;
                    candle1.Low = low;
                    return candle1;
                }
                if ((c != null) && (c.High > 0.0))
                {
                    if (c.High > high)
                    {
                        high = c.High;
                    }
                    if (c.Low < low)
                    {
                        low = c.Low;
                    }
                    c = c.PreviousCandle;
                }
                num3++;
            }
        }

        public double Stoploss { get; set; }

        public double BookProfit1 { get; set; }

        public double BookProfit2 { get; set; }

        public double StopLossRange { get; set; }

        public int FinalCandle { get; set; }
    }
}

