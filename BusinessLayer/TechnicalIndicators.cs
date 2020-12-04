using Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;

namespace BAL
{


    public class TechnicalIndicators
    {
        public static Dictionary<string, List<Candle>> AddIndicators(Dictionary<string, List<Candle>> dsList, List<Technical> listofIndicators, DateTime fromDate, DateTime toDate)
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
                bool bollingerBand = false;
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
                    if (technical == Technical.BollingerBand)
                    {
                        bollingerBand = true;
                    }
                }
                Parallel.ForEach<KeyValuePair<string, List<Candle>>>(dsList, delegate (KeyValuePair<string, List<Candle>> ds)
                {
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
                            average.AddSample((float)Convert.ToDouble(candle.Close));
                            candle.AllIndicators.SMA20 = average.Average;
                            if (bollingerBand)
                            {
                                candle.AllIndicators.BollingerBand = average.BollingerBand;
                            }
                            average2.AddSample((float)Convert.ToDouble(candle.Close));
                            candle.AllIndicators.SMA50 = average2.Average;
                            average3.AddSample((float)Convert.ToDouble(candle.Close));
                            candle.AllIndicators.SMA200 = average3.Average;
                        }
                        if (superTrend)
                        {
                            candle.AllIndicators.SuperTrend = new SuperTrend();
                            if (num == 6)
                            {
                                num2 = Math.Max(Math.Max(Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num].Low), Math.Abs((double)(Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num - 1].Close)))), Math.Abs((double)(Convert.ToDouble(ds.Value[num].Low) - Convert.ToDouble(ds.Value[num - 1].Close))));
                                candle.AllIndicators.SuperTrend.ATR7 = num2 / 7.0;
                                num3 = ((Convert.ToDouble(candle.High) + Convert.ToDouble(candle.Low)) / 2.0) - (Convert.ToDouble(candle.AllIndicators.SuperTrend.ATR7) * 3.0);
                                num4 = ((Convert.ToDouble(candle.High) + Convert.ToDouble(candle.Low)) / 2.0) + (Convert.ToDouble(candle.AllIndicators.SuperTrend.ATR7) * 3.0);
                                candle.AllIndicators.SuperTrend.FinalUpperBand = (Convert.ToDouble(ds.Value[num - 1].Close) <= Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalUpperBand)) ? num3 : Math.Max(num3, Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalUpperBand));
                                candle.AllIndicators.SuperTrend.FinalLowerBand = (Convert.ToDouble(ds.Value[num - 1].Close) >= Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalLowerBand)) ? num4 : Math.Min(num4, Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalLowerBand));
                                double num13 = (Convert.ToDouble(ds.Value[num].Close) > Convert.ToDouble(candle.AllIndicators.SuperTrend.FinalLowerBand)) ? 1.0 : ((Convert.ToDouble(ds.Value[num].Close) < Convert.ToDouble(candle.AllIndicators.SuperTrend.FinalUpperBand)) ? -1.0 : Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.Trend));
                                candle.AllIndicators.SuperTrend.SuperTrendValue = !(num13 == 1.0) ? candle.AllIndicators.SuperTrend.FinalLowerBand : candle.AllIndicators.SuperTrend.FinalUpperBand;
                                candle.AllIndicators.SuperTrend.Trend = (int)num13;
                            }
                            else if (num <= 6)
                            {
                                candle.AllIndicators.SuperTrend.ATR7 = 0.0;
                                candle.AllIndicators.SuperTrend.FinalUpperBand = 0.0;
                                candle.AllIndicators.SuperTrend.FinalLowerBand = 0.0;
                                candle.AllIndicators.SuperTrend.SuperTrendValue = 0.0;
                                candle.AllIndicators.SuperTrend.Trend = 1;
                                num2 = (num <= 0) ? 0.0 : (num2 + Math.Max(Math.Max(Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num].Low), Math.Abs((double)(Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num - 1].Close)))), Math.Abs((double)(Convert.ToDouble(ds.Value[num].Low) - Convert.ToDouble(ds.Value[num - 1].Close)))));
                            }
                            else
                            {
                                num2 = Math.Max(Math.Max(Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num].Low), Math.Abs((double)(Convert.ToDouble(ds.Value[num].High) - Convert.ToDouble(ds.Value[num - 1].Close)))), Math.Abs((double)(Convert.ToDouble(ds.Value[num].Low) - Convert.ToDouble(ds.Value[num - 1].Close))));
                                candle.AllIndicators.SuperTrend.ATR7 = ((Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.ATR7) * 6.0) + num2) / 7.0;
                                num3 = ((Convert.ToDouble(candle.High) + Convert.ToDouble(candle.Low)) / 2.0) - (Convert.ToDouble(candle.AllIndicators.SuperTrend.ATR7) * 3.0);
                                num4 = ((Convert.ToDouble(candle.High) + Convert.ToDouble(candle.Low)) / 2.0) + (Convert.ToDouble(candle.AllIndicators.SuperTrend.ATR7) * 3.0);
                                candle.AllIndicators.SuperTrend.FinalUpperBand = (Convert.ToDouble(ds.Value[num - 1].Close) <= Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalUpperBand)) ? num3 : Math.Max(num3, Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalUpperBand));
                                candle.AllIndicators.SuperTrend.FinalLowerBand = (Convert.ToDouble(ds.Value[num - 1].Close) >= Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalLowerBand)) ? num4 : Math.Min(num4, Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.FinalLowerBand));
                                double num14 = (Convert.ToDouble(ds.Value[num].Close) > Convert.ToDouble(candle.AllIndicators.SuperTrend.FinalLowerBand)) ? 1.0 : ((Convert.ToDouble(ds.Value[num].Close) < Convert.ToDouble(candle.AllIndicators.SuperTrend.FinalUpperBand)) ? -1.0 : Convert.ToDouble(ds.Value[num - 1].AllIndicators.SuperTrend.Trend));
                                candle.AllIndicators.SuperTrend.SuperTrendValue = !(num14 == 1.0) ? candle.AllIndicators.SuperTrend.FinalLowerBand : candle.AllIndicators.SuperTrend.FinalUpperBand;
                                candle.AllIndicators.SuperTrend.Trend = (int)num14;
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
                                average4.AddSample((float)num8);
                            }
                            average5.AddSample((float)candle.Close);
                            average6.AddSample((float)candle.Close);
                            candle.AllIndicators.MACD.macd = num8;
                            candle.AllIndicators.MACD.macd9 = num5;
                            candle.AllIndicators.MACD.histogram = num8 - num5;
                        }
                    }
                });
                //dictionary = dsList;
            }
            dictionary = new Dictionary<string, List<Candle>>();
            foreach (var a in dsList)
            {
                List<Candle> l = a.Value;
                var dateRangeCandles = l.Where(d => d.TimeStamp >= fromDate && d.TimeStamp <= toDate).ToList();
                dictionary.Add(a.Key, dateRangeCandles);
            }

            //dictionary = LoadDailyNPivotsDataZerodha(dictionary);
            return dictionary;


        }
        public void SetText()
        {

        }
        delegate void Message(string x);

        static List<Candle> pivotPoints = new List<Candle>();

        public static Dictionary<string, List<Candle>> LoadDailyNPivotsDataZerodha(Dictionary<string, List<Candle>> allCandles)
        {
            try
            {


                Dictionary<string, List<Model.Candle>> loadmydataDaily =
                 new StockOHLC().GetOHLC(DateTime.Now.AddYears(-10), DateTime.Now, 0, null);

                Dictionary<string, List<Model.Candle>> loadmydata5 =
               new StockOHLC().GetOHLC(DateTime.Now.AddYears(-10), DateTime.Now, 5, null);

                Parallel.ForEach(allCandles.Keys, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
                {
                    int i = 0;
                    foreach (Candle dr in allCandles[s])
                    {
                        //if (pivotPoints.Where(a => a.TimeStamp.Date == dr.TimeStamp.Date && a.Stock == dr.Stock).Count() > 0)
                        //{
                        //    dr.dPP = Math.Round(dailyPivot, 2);
                        //    dr.dR1 = Math.Round((2 * dailyPivot) - prevDayLow, 2);
                        //    dr.dS1 = Math.Round((2 * dailyPivot) - prevDayHigh, 2);
                        //    dr.dR2 = Math.Round(dailyPivot + (prevDayHigh - prevDayLow), 2);
                        //    dr.dS2 = Math.Round(dailyPivot - (prevDayHigh - prevDayLow), 2);
                        //    dr.dR3 = Math.Round(dailyPivot + 2 * (prevDayHigh - prevDayLow), 2);
                        //    dr.dS3 = Math.Round(dailyPivot - 2 * (prevDayHigh - prevDayLow), 2);
                        //    dr.dClose = prevDayClose;
                        //    dr.dHigh = prevDayHigh;
                        //    dr.dLow = prevDayLow;
                        //    dr.dOpen = prevDayOpen;
                        //    dr.yPP = Math.Round(yearPivot, 2);
                        //    dr.yR1 = Math.Round((2 * yearPivot) - lastYearLow, 2);
                        //    dr.yS1 = Math.Round((2 * yearPivot) - lastYearHigh, 2);
                        //    dr.yR2 = Math.Round(yearPivot + (lastYearHigh - lastYearLow), 2);
                        //    dr.yS2 = Math.Round(yearPivot - (lastYearHigh - lastYearLow), 2);
                        //    dr.yR3 = Math.Round(yearPivot + 2 * (lastYearHigh - lastYearLow), 2);
                        //    dr.yS3 = Math.Round(yearPivot - 2 * (lastYearHigh - lastYearLow), 2);
                        //    dr.yClose = lastYearClose;
                        //    dr.yHigh = lastYearHigh;
                        //    dr.yLow = lastYearLow;
                        //    dr.yOpen = lastYearOpen;
                        //    dr.YearStartDate = loadmydataDaily[s].Where(a => a.TimeStamp.Year == lastYear).OrderBy(a => a.TimeStamp.Date).First().TimeStamp.Date;
                        //    dr.YearEndDate = loadmydataDaily[s].Where(a => a.TimeStamp.Year == lastYear).OrderBy(a => a.TimeStamp.Date).Last().TimeStamp.Date;
                        //    dr.mPP = Math.Round(monthPivot, 2);
                        //    dr.mR1 = Math.Round((2 * monthPivot) - lastMonthLow, 2);
                        //    dr.mS1 = Math.Round((2 * monthPivot) - lastMonthHigh, 2);
                        //    dr.mR2 = Math.Round(monthPivot + (lastMonthHigh - lastMonthLow), 2);
                        //    dr.mS2 = Math.Round(monthPivot - (lastMonthHigh - lastMonthLow), 2);
                        //    dr.mR3 = Math.Round(monthPivot + 2 * (lastMonthHigh - lastMonthLow), 2);
                        //    dr.mS3 = Math.Round(monthPivot - 2 * (lastMonthHigh - lastMonthLow), 2);
                        //    dr.mClose = lastMonthClose;
                        //    dr.mHigh = lastMonthHigh;
                        //    dr.mLow = lastMonthLow;
                        //    dr.mOpen = lastMonthOpen;
                        //    dr.MonthStartDate = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == lastMonth).OrderBy(a => a.TimeStamp).First().TimeStamp.Date;
                        //    dr.MonthEndDate = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == lastMonth).OrderBy(a => a.TimeStamp).Last().TimeStamp.Date;
                        //    dr.curMonthClose = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == dr.TimeStamp.Month).OrderBy(a => a.TimeStamp).Last().Close;
                        //    dr.curMonthHigh = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == dr.TimeStamp.Month).Max(a => a.High);
                        //    dr.curMonthLow = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == dr.TimeStamp.Month).Min(a => a.Low);
                        //    dr.curMonthOpen = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == dr.TimeStamp.Month).First().Open;

                        //}
                        Candle dr1 = loadmydataDaily[s].Where(a => a.TimeStamp.Date == dr.TimeStamp.Date).First();
                        if (true)
                        {
                            DateTime currentDay = dr1.TimeStamp.Date;
                            DateTime prevDate = loadmydataDaily[s].Where(a => a.TimeStamp.Date < currentDay).Last().TimeStamp.Date;
                            Candle prevDayDailyCandle = loadmydataDaily[s].Where(a => a.TimeStamp.Date == prevDate).First();
                            double prevDayClose = loadmydata5[s].Where(a => a.TimeStamp.Date == prevDate).OrderBy(a => a.TimeStamp).Last().Close;


                            double prevDayOpen = prevDayDailyCandle.Open;
                            double prevDayHigh = prevDayDailyCandle.High;
                            double prevDayLow = prevDayDailyCandle.Low;
                            double dailyPivot = (prevDayClose + prevDayHigh + prevDayLow) / 3;
                            dr.dPP = Math.Round(dailyPivot, 2);
                            dr.dR1 = Math.Round((2 * dailyPivot) - prevDayLow, 2);
                            dr.dS1 = Math.Round((2 * dailyPivot) - prevDayHigh, 2);
                            dr.dR2 = Math.Round(dailyPivot + (prevDayHigh - prevDayLow), 2);
                            dr.dS2 = Math.Round(dailyPivot - (prevDayHigh - prevDayLow), 2);
                            dr.dR3 = Math.Round(dailyPivot + 2 * (prevDayHigh - prevDayLow), 2);
                            dr.dS3 = Math.Round(dailyPivot - 2 * (prevDayHigh - prevDayLow), 2);
                            dr.dClose = prevDayClose;
                            dr.dHigh = prevDayHigh;
                            dr.dLow = prevDayLow;
                            dr.dOpen = prevDayOpen;
                            //pivot calculation for monthly & weekly chart - range is one year
                            int currentYear = dr.TimeStamp.Date.Year;
                            int lastYear = currentYear - 1;
                            if (loadmydata5[s].Where(a => a.TimeStamp.Year == lastYear).Count() > 0)
                            {
                                double lastYearClose = loadmydataDaily[s].Where(a => a.TimeStamp.Year == lastYear).OrderBy(a => a.TimeStamp.Date).Last().Close;
                                double lastYearHigh = loadmydataDaily[s].Where(a => a.TimeStamp.Year == lastYear).Max(a => a.High);
                                double lastYearLow = loadmydataDaily[s].Where(a => a.TimeStamp.Year == lastYear).Min(a => a.Low);
                                double lastYearOpen = loadmydataDaily[s].Where(a => a.TimeStamp.Year == lastYear).First().Open;
                                double yearPivot = (lastYearClose + lastYearHigh + lastYearLow) / 3;
                                dr.yPP = Math.Round(yearPivot, 2);
                                dr.yR1 = Math.Round((2 * yearPivot) - lastYearLow, 2);
                                dr.yS1 = Math.Round((2 * yearPivot) - lastYearHigh, 2);
                                dr.yR2 = Math.Round(yearPivot + (lastYearHigh - lastYearLow), 2);
                                dr.yS2 = Math.Round(yearPivot - (lastYearHigh - lastYearLow), 2);
                                dr.yR3 = Math.Round(yearPivot + 2 * (lastYearHigh - lastYearLow), 2);
                                dr.yS3 = Math.Round(yearPivot - 2 * (lastYearHigh - lastYearLow), 2);
                                dr.yClose = lastYearClose;
                                dr.yHigh = lastYearHigh;
                                dr.yLow = lastYearLow;
                                dr.yOpen = lastYearOpen;
                                dr.YearStartDate = loadmydataDaily[s].Where(a => a.TimeStamp.Year == lastYear).OrderBy(a => a.TimeStamp.Date).First().TimeStamp.Date;
                                dr.YearEndDate = loadmydataDaily[s].Where(a => a.TimeStamp.Year == lastYear).OrderBy(a => a.TimeStamp.Date).Last().TimeStamp.Date;
                            }

                            //pivot calculation for daily chart -range is one month


                            int currentMonth = dr.TimeStamp.Month;
                            int lastMonth = currentMonth - 1;
                            if (lastMonth == 0)
                            {
                                lastMonth = 12;
                                currentYear = currentYear - 1;
                            }
                            if (loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == lastMonth).Count() > 0)
                            {
                                double lastMonthClose = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == lastMonth).OrderBy(a => a.TimeStamp.Date).Last().Close;
                                double lastMonthHigh = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == lastMonth).Max(a => a.High);
                                double lastMonthLow = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == lastMonth).Min(a => a.Low);
                                double lastMonthOpen = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == lastMonth).First().Open;
                                double monthPivot = (lastMonthClose + lastMonthHigh + lastMonthLow) / 3;
                                dr.mPP = Math.Round(monthPivot, 2);
                                dr.mR1 = Math.Round((2 * monthPivot) - lastMonthLow, 2);
                                dr.mS1 = Math.Round((2 * monthPivot) - lastMonthHigh, 2);
                                dr.mR2 = Math.Round(monthPivot + (lastMonthHigh - lastMonthLow), 2);
                                dr.mS2 = Math.Round(monthPivot - (lastMonthHigh - lastMonthLow), 2);
                                dr.mR3 = Math.Round(monthPivot + 2 * (lastMonthHigh - lastMonthLow), 2);
                                dr.mS3 = Math.Round(monthPivot - 2 * (lastMonthHigh - lastMonthLow), 2);
                                dr.mClose = lastMonthClose;
                                dr.mHigh = lastMonthHigh;
                                dr.mLow = lastMonthLow;
                                dr.mOpen = lastMonthOpen;
                                dr.MonthStartDate = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == lastMonth).OrderBy(a => a.TimeStamp).First().TimeStamp.Date;
                                dr.MonthEndDate = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == lastMonth).OrderBy(a => a.TimeStamp).Last().TimeStamp.Date;
                                try
                                {
                                    dr.curMonthClose = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == dr.TimeStamp.Month).OrderBy(a => a.TimeStamp).Last().Close;
                                    dr.curMonthHigh = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == dr.TimeStamp.Month).Max(a => a.High);
                                    dr.curMonthLow = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == dr.TimeStamp.Month).Min(a => a.Low);
                                    dr.curMonthOpen = loadmydataDaily[s].Where(a => a.TimeStamp.Year == currentYear && a.TimeStamp.Month == dr.TimeStamp.Month).First().Open;
                                }
                                catch
                                {
                                }
                            }
                            currentYear = dr.TimeStamp.Year;

                            //pivot calculation for 30 Min & 60 Min chart - range is 1 week


                            int curretnWeek = GetWeekOfMonth(dr.TimeStamp.Date);
                            int lastWeek = curretnWeek - 1;
                            if (lastWeek == 0)
                            {
                                if (currentMonth == 1 && curretnWeek == 1)
                                {
                                    currentYear = currentYear - 1;
                                    currentMonth = 12;
                                    curretnWeek = GetWeekOfMonth(new DateTime(currentYear, currentMonth, 27));
                                }
                                else if (curretnWeek == 1)
                                {
                                    currentMonth = 12;
                                    curretnWeek = GetWeekOfMonth(new DateTime(currentYear, currentMonth, 27));
                                }
                            }

                            DateTime thisWeekMonday = dr.TimeStamp.Date.StartOfWeek(DayOfWeek.Monday).AddDays(-7);
                            DateTime thisWeekTuesday = thisWeekMonday.AddDays(1);
                            DateTime thisWeekWednesday = thisWeekMonday.AddDays(2);
                            DateTime thisWeekThursday = thisWeekMonday.AddDays(3);
                            DateTime thisWeekFriday = thisWeekMonday.AddDays(4);
                            List<DateTime> weekDayList = new List<DateTime>();
                            weekDayList.Add(thisWeekMonday);
                            weekDayList.Add(thisWeekTuesday);
                            weekDayList.Add(thisWeekWednesday);
                            weekDayList.Add(thisWeekThursday);
                            weekDayList.Add(thisWeekFriday);

                            var lstWeekData = loadmydataDaily[s].Where(a => weekDayList.Contains(a.TimeStamp.Date)).OrderBy(a => a.TimeStamp.Date).ToList();


                            DateTime curWeekMonday = dr.TimeStamp.Date.StartOfWeek(DayOfWeek.Monday);
                            DateTime curWeekTuesday = curWeekMonday.AddDays(1);
                            DateTime curWeekWednesday = curWeekMonday.AddDays(2);
                            DateTime curWeekThursday = curWeekMonday.AddDays(3);
                            DateTime curWeekFriday = curWeekMonday.AddDays(4);
                            List<DateTime> curweekDayList = new List<DateTime>();
                            curweekDayList.Add(curWeekMonday);
                            curweekDayList.Add(curWeekTuesday);
                            curweekDayList.Add(curWeekWednesday);
                            curweekDayList.Add(curWeekThursday);
                            curweekDayList.Add(curWeekFriday);

                            var curWeekData = loadmydataDaily[s].Where(a => curweekDayList.Contains(a.TimeStamp.Date)).OrderBy(a => a.TimeStamp.Date).ToList();
                            if (curWeekData.Count() > 0)
                            {
                                dr.curWeekOpen = curWeekData.First().Open;
                                dr.curWeekHigh = curWeekData.Max(a => a.High);
                                dr.curWeekLow = curWeekData.Min(a => a.Low);
                                dr.curWeekClose = curWeekData.Last().Close;
                            }

                            if (lstWeekData.Count() > 0)
                            {

                                double lastWeekclose = lstWeekData.Last().Close;
                                double lastWeekHigh = lstWeekData.Max(a => a.High);
                                double lastWeekLow = lstWeekData.Min(a => a.Low);
                                double lastWeekOpen = lstWeekData.First().Open;
                                double weekPivot = (lastWeekclose + lastWeekHigh + lastWeekLow) / 3;
                                dr.wPP = Math.Round(weekPivot, 2);
                                dr.wR1 = Math.Round((2 * weekPivot) - lastWeekLow, 2);
                                dr.wS1 = Math.Round((2 * weekPivot) - lastWeekHigh, 2);
                                dr.wR2 = Math.Round(weekPivot + (lastWeekHigh - lastWeekLow), 2);
                                dr.wS2 = Math.Round(weekPivot - (lastWeekHigh - lastWeekLow), 2);
                                dr.wR3 = Math.Round(weekPivot + 2 * (lastWeekHigh - lastWeekLow), 2);
                                dr.wS3 = Math.Round(weekPivot - 2 * (lastWeekHigh - lastWeekLow), 2);
                                dr.wClose = lastWeekclose;
                                dr.wHigh = lastWeekHigh;
                                dr.wLow = lastWeekLow;
                                dr.wOpen = lastWeekOpen;
                                dr.WeekStartDate = lstWeekData.First().TimeStamp.Date;
                                dr.WeekEndDate = lstWeekData.Last().TimeStamp.Date;

                            }
                        }


                        i++;
                        //if (pivotPoints.Where(a => a.TimeStamp.Date == dr.TimeStamp.Date && a.Stock == dr.Stock).Count() == 0)
                        //{
                        //    pivotPoints.Add(dr);
                        //}
                    }
                    // assign values here



                });


            }
            catch (Exception ex)
            {
                throw ex;
            }

            return allCandles;
        }

        public static int GetWeekOfMonth(DateTime date)
        {
            DateTime beginningOfMonth = new DateTime(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            return (int)Math.Truncate((double)date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;
        }
    }
}

