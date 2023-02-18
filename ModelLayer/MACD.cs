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
    public enum OscillatorPriceRange
    {
        Overbought,
        Oversold,
        NotIdentified

    }

    public enum OscillatorStatus
    {
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
                if (this.fast > this.slow && ReferringCandle.PreviousCandle?.AllIndicators?.Stochastic?.fast <= ReferringCandle.PreviousCandle?.AllIndicators?.Stochastic?.slow)
                {
                    return OscillatorReversal.BullishReversal;
                }
                else if (this.fast < this.slow && ReferringCandle.PreviousCandle?.AllIndicators?.Stochastic?.fast >= ReferringCandle.PreviousCandle?.AllIndicators?.Stochastic?.slow)
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

                if (this.fast > this.slow)
                {
                    return OscillatorStatus.Bullish;
                }

                else if (this.fast < this.slow)
                {
                    return OscillatorStatus.Bearish;
                }
                else
                {
                    return OscillatorStatus.NotIdentified;
                }
            }


        }
        public OscillatorPriceRange OscillatorPriceRange
        {
            get
            {

                if (this.slow > OB || this.fast > OB)
                {
                    return OscillatorPriceRange.Overbought;
                }
                else if (this.slow < this.OS || this.fast < this.OS)
                {
                    return OscillatorPriceRange.Oversold;
                }
                else
                {
                    return OscillatorPriceRange.NotIdentified;
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

