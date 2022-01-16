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
    public enum OscillatorStatus
    {
        Overbought,
        Oversold,
        Bullish,
        Bearish,
        NotIdentified

    }

    public enum OscillatorReversal
    {
        BullishReversal,
        BearishReversal,
        NotIdentified
    }

    public class Stochastic
    {
        private int OB = 0;
        private int OS = 0;
        Candle ReferringCandle = null;

        public OscillatorReversal OscillatorReversal
        {
            get
            {
               if(ReferringCandle.PreviousCandle?.AllIndicators?.Stochastic?.OscillatorStatus== OscillatorStatus.Oversold && this.fast >this.slow)
                {
                    return OscillatorReversal.BullishReversal;
                }
               else if   (ReferringCandle.PreviousCandle?.AllIndicators?.Stochastic?.OscillatorStatus == OscillatorStatus.Overbought && this.fast < this.slow)
                    {
                        return OscillatorReversal.BearishReversal;
                    }
                else
                {
                    return OscillatorReversal.NotIdentified;
                }
            }
        }

        public OscillatorStatus OscillatorStatus
        {
            get
            {
                if (this.fast >= OB)
                {
                    return OscillatorStatus.Overbought;
                }
                else if (this.fast <= OS)
                {
                    return OscillatorStatus.Oversold;
                }
                else if (this.fast < OB && this.slow < OB && this.fast > OS && this.slow > OS && this.fast > this.slow)
                {
                    return OscillatorStatus.Bullish;
                }
                else if (this.fast > OS && this.slow > OS && this.fast < OB && this.slow < OB && this.fast < this.slow)
                {
                    return OscillatorStatus.Bearish;
                }
                else
                {
                    return OscillatorStatus.NotIdentified;
                }
            }


        }
        public Stochastic(int overBought, int oversSold, Candle candle)
        {
            OB = overBought;
            OS = oversSold;
            ReferringCandle = candle;
        }
        public double fast { get; set; }

        public double slow { get; set; }

        public double IsOver { get; set; }
    }

}

