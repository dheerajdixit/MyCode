namespace BAL
{
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class TechnicalIndicators
    {
        public static Dictionary<string, List<Candle>> AddIndicators(Dictionary<string, List<Candle>> dsList, List<Technical> listofIndicators)
        {
            Dictionary<string, List<Candle>> dictionary;
            if ((listofIndicators == null) || (listofIndicators.Count == 0))
            {
                dictionary = dsList;
            }
            else
            {
                bool superTrend = false;
                bool movingAverage = false;
                bool macdInd = false;
                foreach (Technical technical in listofIndicators)
                {
                    if (technical == Technical.SuperTrend)
                    {
                        superTrend = true;
                        continue;
                    }
                    if (technical == Technical.SimpleMovingAverage)
                    {
                        movingAverage = true;
                        continue;
                    }
                    if (technical == Technical.MACD)
                    {
                        macdInd = true;
                    }
                }
                Parallel.ForEach<KeyValuePair<string, List<Candle>>>(dsList, delegate (KeyValuePair<string, List<Candle>> ds) {
                    int num = 0;
                    double num2 = 0.0;
                    double num3 = 0.0;
                    double num4 = 0.0;
                    IMovingAverage average = null;
                    IMovingAverage average2 = null;
                    IMovingAverage average3 = null;
                    if (movingAverage)
                    {
                        average = new SimpleMovingAverage(20);
                        average2 = new SimpleMovingAverage(50);
                        average3 = new SimpleMovingAverage(200);
                    }
                    double num5 = 0.0;
                    double num6 = 0.0;
                    double num7 = 0.0;
                    double num8 = 0.0;
                    IMovingAverage average4 = null;
                    IMovingAverage average5 = null;
                    IMovingAverage average6 = null;
                    if (macdInd)
                    {
                        average4 = new SimpleMovingAverage(9);
                        average5 = new SimpleMovingAverage(12);
                        average6 = new SimpleMovingAverage(0x1a);
                    }
                    double trend = 0.0;
                    double trendPrice = 0.0;
                    int num11 = 0;
                    int num12 = 0;
                    foreach (Candle candle in ds.Value)
                    {
                        candle.AllIndicators = new Model.AllTechnicals();
                        if (movingAverage)
                        {
                            average.AddSample((float) Convert.ToDouble(candle.Close));
                            candle.AllIndicators.SMA20 = average.Average;
                            average2.AddSample((float) Convert.ToDouble(candle.Close));
                            candle.AllIndicators.SMA50 = average2.Average;
                            average3.AddSample((float) Convert.ToDouble(candle.Close));
                            candle.AllIndicators.SMA200 = average3.Average;
                        }
                        if (superTrend)
                        {
                            candle.AllIndicators.SuperTrend = new SuperTrend();
                            if (num == 6)
                            {
                                num2 = Math.Max(Math.Max(Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num].Low), Math.Abs((double) (Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num - 1].Close)))), Math.Abs((double) (Convert.ToDouble(ds.Value[num].Low) - Convert.ToDouble(ds.Value[num - 1].Close))));
                                candle.AllIndicators.SuperTrend.ATR7 = num2 / 7.0;
                                num3 = ((Convert.ToDouble(candle.High) + Convert.ToDouble(candle.Low)) / 2.0) - (Convert.ToDouble(candle.AllIndicators.SuperTrend.ATR7) * 3.0);
                                num4 = ((Convert.ToDouble(candle.High) + Convert.ToDouble(candle.Low)) / 2.0) + (Convert.ToDouble(candle.AllIndicators.SuperTrend.ATR7) * 3.0);
                                candle.AllIndicators.SuperTrend.FinalUpperBand = (Convert.ToDouble(ds.Value[num - 1].Close) <= Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalUpperBand)) ? num3 : Math.Max(num3, Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalUpperBand));
                                candle.AllIndicators.SuperTrend.FinalLowerBand = (Convert.ToDouble(ds.Value[num - 1].Close) >= Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalLowerBand)) ? num4 : Math.Min(num4, Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalLowerBand));
                                double num13 = (Convert.ToDouble(ds.Value[num].Close) > Convert.ToDouble(candle.AllIndicators.SuperTrend.FinalLowerBand)) ? 1.0 : ((Convert.ToDouble(ds.Value[num].Close) < Convert.ToDouble(candle.AllIndicators.SuperTrend.FinalUpperBand)) ? -1.0 : Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.Trend));
                                candle.AllIndicators.SuperTrend.SuperTrendValue = !(num13 == 1.0) ? candle.AllIndicators.SuperTrend.FinalLowerBand : candle.AllIndicators.SuperTrend.FinalUpperBand;
                                candle.AllIndicators.SuperTrend.Trend = (int) num13;
                            }
                            else if (num <= 6)
                            {
                                candle.AllIndicators.SuperTrend.ATR7 = 0.0;
                                candle.AllIndicators.SuperTrend.FinalUpperBand = 0.0;
                                candle.AllIndicators.SuperTrend.FinalLowerBand = 0.0;
                                candle.AllIndicators.SuperTrend.SuperTrendValue = 0.0;
                                candle.AllIndicators.SuperTrend.Trend = 1;
                                num2 = (num <= 0) ? 0.0 : (num2 + Math.Max(Math.Max(Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num].Low), Math.Abs((double) (Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num - 1].Close)))), Math.Abs((double) (Convert.ToDouble(ds.Value[num].Low) - Convert.ToDouble(ds.Value[num - 1].Close)))));
                            }
                            else
                            {
                                num2 = Math.Max(Math.Max(Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num].Low), Math.Abs((double) (Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num - 1].Close)))), Math.Abs((double) (Convert.ToDouble(ds.Value[num].Low) - Convert.ToDouble(ds.Value[num - 1].Close))));
                                candle.AllIndicators.SuperTrend.ATR7 = ((Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.ATR7) * 6.0) + num2) / 7.0;
                                num3 = ((Convert.ToDouble(candle.High) + Convert.ToDouble(candle.Low)) / 2.0) - (Convert.ToDouble(candle.AllIndicators.SuperTrend.ATR7) * 3.0);
                                num4 = ((Convert.ToDouble(candle.High) + Convert.ToDouble(candle.Low)) / 2.0) + (Convert.ToDouble(candle.AllIndicators.SuperTrend.ATR7) * 3.0);
                                candle.AllIndicators.SuperTrend.FinalUpperBand = (Convert.ToDouble(ds.Value[num - 1].Close) <= Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalUpperBand)) ? num3 : Math.Max(num3, Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalUpperBand));
                                candle.AllIndicators.SuperTrend.FinalLowerBand = (Convert.ToDouble(ds.Value[num - 1].Close) >= Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalLowerBand)) ? num4 : Math.Min(num4, Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalLowerBand));
                                double num14 = (Convert.ToDouble(ds.Value[num].Close) > Convert.ToDouble(candle.AllIndicators.SuperTrend.FinalLowerBand)) ? 1.0 : ((Convert.ToDouble(ds.Value[num].Close) < Convert.ToDouble(candle.AllIndicators.SuperTrend.FinalUpperBand)) ? -1.0 : Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.Trend));
                                candle.AllIndicators.SuperTrend.SuperTrendValue = !(num14 == 1.0) ? candle.AllIndicators.SuperTrend.FinalLowerBand : candle.AllIndicators.SuperTrend.FinalUpperBand;
                                candle.AllIndicators.SuperTrend.Trend = (int) num14;
                            }
                            if (trend == candle.AllIndicators.SuperTrend.Trend)
                            {
                                candle.AllIndicators.SuperTrend.TrendPrice = trendPrice;
                                num11++;
                                if ((candle.Low < candle.AllIndicators.SuperTrend.TrendPrice) && (candle.AllIndicators.SuperTrend.Trend == -1))
                                {
                                    num12++;
                                }
                                else if ((candle.High > candle.AllIndicators.SuperTrend.TrendPrice) && (candle.AllIndicators.SuperTrend.Trend == 1))
                                {
                                    num12++;
                                }
                            }
                            else
                            {
                                num12 = 0;
                                num11 = 0;
                                if (candle.AllIndicators.SuperTrend.Trend == 1)
                                {
                                    candle.AllIndicators.SuperTrend.TowardsBuy = true;
                                    candle.AllIndicators.SuperTrend.TrendPrice = candle.High;
                                }
                                else if (candle.AllIndicators.SuperTrend.Trend == -1)
                                {
                                    candle.AllIndicators.SuperTrend.TowardsBuy = false;
                                    candle.AllIndicators.SuperTrend.TrendPrice = candle.Low;
                                }
                                trendPrice = candle.AllIndicators.SuperTrend.TrendPrice;
                            }
                            candle.AllIndicators.SuperTrend.CountOfTrendPriceBroken = num12;
                            candle.AllIndicators.SuperTrend.CandlePastSinceLastChange = num11;
                            trend = candle.AllIndicators.SuperTrend.Trend;
                        }
                        num++;
                        if (macdInd)
                        {
                            candle.AllIndicators.MACD = new MACD();
                            if (num == 13)
                            {
                                num6 = average5.Average;
                            }
                            else if (num > 13)
                            {
                                num6 = (0.153 * (Convert.ToDouble(candle.Close) - num6)) + num6;
                            }
                            if (num == 0x1b)
                            {
                                num7 = average6.Average;
                            }
                            else if (num > 0x1b)
                            {
                                num8 = num6 - ((0.074 * (Convert.ToDouble(candle.Close) - num7)) + num7);
                            }
                            if (num == 0x25)
                            {
                                num5 = average4.Average;
                            }
                            else if (num > 0x25)
                            {
                                num5 = (0.2 * (num8 - num5)) + num5;
                            }
                            if ((num > 0x1b) && (num < 0x25))
                            {
                                average4.AddSample((float) num8);
                            }
                            average5.AddSample((float) candle.Close);
                            average6.AddSample((float) candle.Close);
                            candle.AllIndicators.MACD.macd = num8;
                            candle.AllIndicators.MACD.macd9 = num5;
                            candle.AllIndicators.MACD.histogram = num8 - num5;
                        }
                    }
                });
                dictionary = dsList;
            }
            return dictionary;
        }
    }
}

