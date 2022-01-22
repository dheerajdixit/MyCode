using CommonFeatures;
using DAL;
using Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BAL
{


    public class StockOHLC
    {
        public Dictionary<string, List<Candle>> GetOHLC(DateTime startDate, DateTime endDate, int period, ProgressDelegate myProgres)
        {
            //ConcurrentBag<ConcurrentBag<Candle>> source = new DataAcess().GetHistory(myProgres, period, startDate, endDate);

            ConcurrentBag<List<Candle>> source = new DataAcess().GetHistoryFromFile(myProgres, period, startDate, endDate);
            Dictionary<string, List<Candle>> result = new Dictionary<string, List<Candle>>();
            List<List<Candle>> list = source.ToList<List<Candle>>();
            Queue<double> lowest = new Queue<double>();
            Queue<double> highest = new Queue<double>();
            list.ForEach(delegate (List<Candle> i)
            {
                IOrderedEnumerable<Candle> enumerable = from a in i
                                                        orderby a.TimeStamp
                                                        select a;
                Candle candle = new Candle();
                foreach (Candle candle2 in enumerable)
                {
                    lowest.Enqueue(candle2.Low);
                    highest.Enqueue(candle2.High);
                    candle2.PreviousCandle = candle;
                    candle = candle2;
                    candle2.Lowest = ((IEnumerable<double>)lowest).Min();
                    candle2.Highest = ((IEnumerable<double>)highest).Max();
                    if (lowest.Count > 100)
                    {
                        lowest.Dequeue();
                        highest.Dequeue();
                    }
                }
                if (enumerable.Count<Candle>() > 0)
                {
                    result.Add(i.First<Candle>().Stock, enumerable.ToList<Candle>());
                }
            });
            return result;
        }

        public List<Candle> GetOHLC(DateTime startDate, DateTime endDate, string InstrumentToken, int period)
        {
            ConcurrentBag<ConcurrentBag<Candle>> source = new DataAcess().GetHistory(InstrumentToken, period, startDate, endDate);
            List<Candle> result = new List<Candle>();
            List<ConcurrentBag<Candle>> list = source.ToList<ConcurrentBag<Candle>>();
            Queue<double> lowest = new Queue<double>();
            Queue<double> highest = new Queue<double>();
            list.ForEach(delegate (ConcurrentBag<Candle> i)
            {
                IOrderedEnumerable<Candle> enumerable = from a in i
                                                        orderby a.TimeStamp
                                                        select a;
                Candle candle = new Candle();
                foreach (Candle candle2 in enumerable)
                {
                    lowest.Enqueue(candle2.Low);
                    highest.Enqueue(candle2.High);
                    candle2.PreviousCandle = candle;
                    candle = candle2;
                    candle2.Lowest = ((IEnumerable<double>)lowest).Min();
                    candle2.Highest = ((IEnumerable<double>)highest).Max();
                    if (lowest.Count > 150)
                    {
                        lowest.Dequeue();
                        highest.Dequeue();
                    }
                }
                result = enumerable.ToList<Candle>();
            });
            return result;
        }

        public double GetRange(Candle b, Range r)
        {
            if (r == Range.Change)
            {
                var c = b;
                while (c.TimeStamp.Date == b.TimeStamp.Date)
                {
                    c = c.PreviousCandle;
                }
                //double p = b.PreviousCandle.
                return ((Math.Abs((double)(b.Close - c.Close)) / c.Close) * 100.0);
            }
            else if (r == Range.Engulfing)
            {
                return ((Math.Abs((double)(b.Close - b.PreviousCandle.Open)) / b.Close) * 100.0);

            }
            else if (r == Range.Top)
            {

                //double p = b.PreviousCandle.
                return ((Math.Abs((double)(b.High - b.Close)) / b.Close) * 100.0);
            }
            else
            {
                return (r != Range.Gap)
            ? ((r != Range.Normal)
                    ? ((r != Range.Gap) ? 0.0 : (Math.Abs((double)(b.Open - b.PreviousCandle.Close)) / b.PreviousCandle.Close))
                    : (Math.Abs((double)(b.Open - b.Close)) / b.PreviousCandle.Close))
            : ((Math.Abs((double)(b.Close - b.PreviousCandle.Close)) / b.Close) * 100.0);
            }
        }




        public Time GetTime(Idea i)
        {
            Time time = new Time();
            int hour = 9;
            DateTime time2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, 15, 0);
            time2 = time2.AddMinutes((double)((i.EntryStartCandle - 1) * i.Interval));
            DateTime time3 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 15, 0);
            time3 = time3.AddMinutes((double)((i.EntryFinishCandle - 1) * i.Interval));
            time.StartHour = time2.Hour;
            time.StartMinute = time2.Minute;
            time.EndHour = time3.Hour;
            time.EndMinute = time3.Minute;
            return time;
        }

        public Dictionary<Guid, StrategyModel> GetTopMostSolidGapOpenerDayWise(Dictionary<string, List<Candle>> myTestData, Idea selectedIdea, ProgressDelegate myProgres)
        {
            Time t = this.GetTime(selectedIdea);
            ConcurrentBag<List<StrategyModel>> filter1 = new ConcurrentBag<List<StrategyModel>>();

            Parallel.ForEach<KeyValuePair<string, List<Candle>>>(myTestData, delegate (KeyValuePair<string, List<Candle>> stock)
            {

                List<Candle> allCandles = stock.Value;
                if (myProgres != null)
                {
                    myProgres($"Collecting first candle for all days for {stock.Key}");
                }
                List<StrategyModel> selector = new List<StrategyModel>();

                filter1.Add(this.PrepareFirstLevelOfFiltering(allCandles, selectedIdea, t).Select(b =>
                    new StrategyModel
                    {
                        Stock = b.Stock,
                        Volume = b.Volume * b.Close,
                        Range = this.GetRange(b, selectedIdea.Range),
                        Date = b.TimeStamp,
                        Close = b.Close,
                        High = b.High,
                        Low = b.Low,
                        Open = b.Open,
                        PreviousClose = b.PreviousCandle.Close,
                        Imp1 = 0,
                        Trade = b.Trade,
                        CurrentCandle = b,
                    }).ToList());
            });

            Dictionary<DateTime, List<StrategyModel>> dictionary = new Dictionary<DateTime, List<StrategyModel>>();
            foreach (DateTime myDate in (from a in myTestData.First<KeyValuePair<string, List<Candle>>>().Value select a.TimeStamp.Date).Distinct<DateTime>())
            {
                List<StrategyModel> list = new List<StrategyModel>();
                foreach (List<StrategyModel> list2 in filter1)
                {
                    Func<StrategyModel, bool> pf = null;
                    if (myProgres != null)
                    {
                        myProgres($"Arranging stock date wise for {myDate.Date}");
                    }
                    Func<StrategyModel, bool> predicate = pf;
                    if (pf == null)
                    {
                        Func<StrategyModel, bool> local2 = pf;
                        predicate = pf = obj => obj.Date.Date == myDate.Date;
                    }
                    IEnumerable<StrategyModel> source = list2.Where<StrategyModel>(predicate);
                    if ((source != null) && (source.Count<StrategyModel>() >= 1))
                    {
                        list.Add(source.First<StrategyModel>());
                    }
                }
                dictionary.Add(myDate.Date, list);
            }

            Dictionary<Guid, StrategyModel> dictionary2 = new Dictionary<Guid, StrategyModel>();
            foreach (KeyValuePair<DateTime, List<StrategyModel>> pair2 in dictionary)
            {
                //if (pair2.Value.Count() == 17)
                //{
                //    var c1 = pair2.Value.OrderBy(a => a.Stock).ToList();

                //    XmlSerializer xs = new XmlSerializer(typeof(List<StrategyModel>));
                //    using (StreamWriter writer = new StreamWriter(@"C:\Jai Sri Thakur Ji\foo.xml"))
                //    {
                //        xs.Serialize(writer, c1);
                //    }
                //}
                if (myProgres != null)
                {
                    myProgres($"Selecting Most solid gap up stock for Date {pair2.Key}");
                }
                if (selectedIdea.Sorting == Sorting.VolumeFirst)
                {
                    int num = 0;
                    foreach (IGrouping<DateTime, StrategyModel> j in from a in pair2.Value
                                                                     orderby a.Date
                                                                     group a by a.Date)
                    {
                        Func<StrategyModel, bool> p2 = null;
                        Func<StrategyModel, bool> predicate = p2;
                        if (p2 == null)
                        {
                            Func<StrategyModel, bool> local5 = p2;
                            predicate = p2 = b => b.Date == j.Key;
                        }

                        //if (pair2.Value.Count() == 17)
                        //{
                        //    //var c1 = pair2.Where<StrategyModel>(predicate)  orderby b.Volume descending
                        //    //                                        select b).Take<StrategyModel>(selectedIdea.FilterByVolume).toli;

                        //    //XmlSerializer xs = new XmlSerializer(typeof(List<StrategyModel>));
                        //    //using (StreamWriter writer = new StreamWriter(@"C:\Jai Sri Thakur Ji\foo.xml"))
                        //    //{
                        //    //    xs.Serialize(writer, c1);
                        //    //}
                        //}


                        foreach (StrategyModel model in (from a in (from b in pair2.Value.Where<StrategyModel>(predicate)
                                                                    orderby b.Volume descending
                                                                    select b).Take<StrategyModel>(selectedIdea.FilterByVolume)
                                                         orderby a.Range descending
                                                         select a).Take<StrategyModel>(selectedIdea.TradePerSession).OrderByDescending(a => ((a.High - a.Low) / (a.Close)) * 100).Take(selectedIdea.TradePerSession)
                                                         )
                        {
                            dictionary2.Add(Guid.NewGuid(), model);
                            num++;
                            if (num == selectedIdea.TradePerSession)
                            {
                                break;
                            }
                        }

                    }
                    continue;
                }
                int num2 = 0;
                foreach (IGrouping<DateTime, StrategyModel> grouping1 in from a in pair2.Value
                                                                         orderby a.Date
                                                                         group a by a.Date)
                {
                    Func<StrategyModel, bool> p1 = null;
                    Func<StrategyModel, bool> predicate = p1;
                    if (p1 == null)
                    {
                        Func<StrategyModel, bool> local10 = p1;
                        predicate = p1 = b => b.Date == grouping1.Key;
                    }
                    double range = pair2.Value.Where<StrategyModel>(predicate).Max(a => a.Range);
                    foreach (StrategyModel model2 in (from b in pair2.Value.Where<StrategyModel>(predicate).Where(a => a.Range == range) orderby b.Volume ascending select b))

                    {
                        dictionary2.Add(Guid.NewGuid(), model2);
                        num2++;
                        if (num2 == selectedIdea.TradePerSession)
                        {
                            break;
                        }
                    }
                    if (num2 == selectedIdea.TradePerSession)
                    {
                        break;
                    }
                }
            }
            return dictionary2;
        }

        public Dictionary<Guid, StrategyModel> ApplyDualMomentumStrategyModel(Dictionary<string, List<Candle>> myTestDataLargeTimeFrame, Dictionary<string, List<Candle>> myTestDatSmallTimeFrame, Idea selectedIdea, ProgressDelegate myProgres)
        {
            Time t = this.GetTime(selectedIdea);
            ConcurrentBag<List<StrategyModel>> filter1 = new ConcurrentBag<List<StrategyModel>>();


            Parallel.ForEach<KeyValuePair<string, List<Candle>>>(myTestDataLargeTimeFrame, delegate (KeyValuePair<string, List<Candle>> stock)
            {

                if (myTestDatSmallTimeFrame.ContainsKey(stock.Key))
                {
                    List<Candle> allCandlesHigherTimeFrame = stock.Value;
                    List<Candle> allCandlesLowerTimeFrame = myTestDatSmallTimeFrame[stock.Key];

                    if (myProgres != null)
                    {
                        myProgres($"Collecting first candle for all days for {stock.Key}");
                    }
                    List<StrategyModel> selector = new List<StrategyModel>();


                    filter1.Add(this.FilterDualTimeFrameMomentumStocks(allCandlesHigherTimeFrame, allCandlesLowerTimeFrame, selectedIdea, t).Select(b =>
                    
                        new StrategyModel
                        {
                            Stock = b.Stock,
                            Volume = b.Volume * b.Close,
                            Range = this.GetRange(b, selectedIdea.Range),
                            Date = b.TimeStamp,
                            Close = b.Close,
                            High = b.High,
                            Low = b.Low,
                            Open = b.Open,
                            PreviousClose = b.PreviousCandle.Close,
                            Imp1 = 0,
                            Trade = b.Trade,
                            CurrentCandle = b,
                        }).ToList());
                }
            });
            Dictionary<Guid, StrategyModel> result = new Dictionary<Guid, StrategyModel>();
            foreach (var p in filter1)
            {
                foreach (var q in p)
                {
                 
                    result.Add(Guid.NewGuid(), q);
                }


            }

            return result;
        }

        public void InsertHistory(string collectionName, int period, string json)
        {
            new DataAcess().InsertHistory(collectionName, period, json);
        }

        public IEnumerable<Candle> FilterDualTimeFrameMomentumStocks(List<Candle> higherTimeFrame, List<Candle> lowerTimeFrame, Idea selctedIdea, Time t)
        {
            IEnumerable<Candle> enumerable = lowerTimeFrame;


            enumerable = from b in enumerable
                         where
                         ((b.AllIndicators.Stochastic?.OscillatorReversal != OscillatorReversal.NotIdentified))
                         select b;


            foreach (var c in enumerable)
            {
                DateTime reverstalTimeStmap = c.TimeStamp;

                //Candle lastCandleOnLargeTimeFrame =
                if (c.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BullishReversal)
                {
                    var g = higherTimeFrame.Where(b => b.TimeStamp < reverstalTimeStmap).LastOrDefault();
                    if (g != null && g.AllIndicators != null && g.AllIndicators.Stochastic != null)
                    {
                        
                        if (g.AllIndicators.Stochastic.OscillatorStatus == OscillatorStatus.Bullish || g.AllIndicators.Stochastic.OscillatorStatus == OscillatorStatus.Oversold)
                        {
                           
                                c.Trade = Trade.BUY;
                          
                        }
                        else
                        {
                            c.Trade = Trade.NONE;
                        }
                    }
                }
                else if (c.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BearishReversal)
                {
                    c.Trade = Trade.SELL;
                    var g = higherTimeFrame.Where(b => b.TimeStamp < reverstalTimeStmap).LastOrDefault();
                    if (g != null && g.AllIndicators != null && g.AllIndicators.Stochastic != null)
                    {
                        if (g.AllIndicators.Stochastic.OscillatorStatus == OscillatorStatus.Bearish || g.AllIndicators.Stochastic.OscillatorStatus==  OscillatorStatus.Overbought)
                        {
                           
                                c.Trade = Trade.SELL;
                           
                        }
                        else
                        {
                            c.Trade = Trade.NONE;
                        }
                    }
                }
                else
                    c.Trade = Trade.NONE;

            }

            var x = enumerable.Where(a => a.Trade != Trade.NONE);
           

            return x;


        }


        public IEnumerable<Candle> PrepareFirstLevelOfFiltering(List<Candle> allCandles, Idea selctedIdea, Time t)
        {
            IEnumerable<Candle> enumerable = allCandles;
            if (selctedIdea.Interval > 0)
                enumerable = from b in allCandles
                             where (b.Close > 50.0) && (b.PreviousCandle.Close > 0.0)
                             where ((b.TimeStamp.Hour >= t.StartHour) && (b.TimeStamp.Hour <= t.EndHour))
                             select b;

            if (selctedIdea.CandleType == CandleType.Solid)
            {
                enumerable = from b in enumerable
                             where ((b.High - b.Close) < ((b.Close - b.Open) / 2.0)) || ((b.Close - b.Low) < ((b.Open - b.Close) / 2.0))
                             select b;
            }
            else if (selctedIdea.CandleType == CandleType.ThreeCandle)
            {
                //enumerable = from b in enumerable
                //    where ((b.CandleType != "G") || ((b.PreviousCandle.CandleType != "R") || ((b.PreviousCandle.PreviousCandle.CandleType != "G") || ((b.Close <= Math.Max(Math.Max(Math.Max(b.AllIndicators.SMA20, b.AllIndicators.SMA50), b.AllIndicators.SMA200), b.AllIndicators.SuperTrend.SuperTrendValue)) || ((b.Low >= Math.Max(Math.Max(Math.Max(b.AllIndicators.SMA20, b.AllIndicators.SMA50), b.AllIndicators.SMA200), b.AllIndicators.SuperTrend.SuperTrendValue)) || ((b.AllIndicators.MACD.histogram <= 0.0) || ((b.Close <= b.PreviousCandle.PreviousCandle.Close) || ((b.AllIndicators.SMA20 <= b.AllIndicators.SMA50) || (b.AllIndicators.SMA50 <= b.AllIndicators.SMA200))))))))) ? ((IEnumerable<Candle>) (((b.CandleType == "R") && ((b.PreviousCandle.CandleType == "G") && ((b.PreviousCandle.PreviousCandle.CandleType == "R") && ((b.Close < Math.Min(Math.Min(Math.Min(b.AllIndicators.SMA20, b.AllIndicators.SMA50), b.AllIndicators.SMA200), b.AllIndicators.SuperTrend.SuperTrendValue)) && ((b.High > Math.Min(Math.Min(Math.Min(b.AllIndicators.SMA20, b.AllIndicators.SMA50), b.AllIndicators.SMA200), b.AllIndicators.SuperTrend.SuperTrendValue)) && ((b.AllIndicators.MACD.histogram < 0.0) && ((b.Close < b.PreviousCandle.PreviousCandle.Close) && (b.AllIndicators.SMA20 < b.AllIndicators.SMA50)))))))) && (b.AllIndicators.SMA50 < b.AllIndicators.SMA200))) : ((IEnumerable<Candle>) true)
                //    select b;
            }

            if (selctedIdea.Name == "Dual_Time_Frame_Momentum")
            {
                enumerable = from b in enumerable
                             where
                             ((b.AllIndicators.Stochastic.OscillatorReversal != OscillatorReversal.NotIdentified))
                             select b;


                foreach (var c in enumerable)
                {

                    if (c.AllIndicators.Stochastic.OscillatorReversal == OscillatorReversal.BullishReversal)
                        c.Trade = Trade.BUY;
                    else if (c.AllIndicators.Stochastic.OscillatorReversal == OscillatorReversal.BearishReversal)
                        c.Trade = Trade.SELL;
                    else
                        c.Trade = Trade.NONE;

                }
            }

            if (selctedIdea.Name == "BollingerBand3")
            {
                enumerable = from b in enumerable
                             where
                             (
                             (b.PreviousCandle.CandleType == "G" && b.PreviousCandle.Close > b.AllIndicators.BollingerBand.Upper && b.CandleType == "R")
                             ||
                              (b.PreviousCandle.CandleType == "R" && b.PreviousCandle.Close < b.AllIndicators.BollingerBand.Lower && b.CandleType == "G"))
                             select b;


                foreach (var c in enumerable)
                {

                    if (c.CandleType == "R")
                        c.Trade = Trade.BUY;
                    else if (c.CandleType == "G")
                        c.Trade = Trade.SELL;
                    else
                        c.Trade = Trade.NONE;

                }
            }

            var x = enumerable.Where(a => a.Trade != Trade.NONE);

            return x;
        }


        public bool SupportedOrResistanced(Candle c)
        {
            if (Contacted(c, c.AllIndicators.SMA20) || Contacted(c, c.AllIndicators.SuperTrend.SuperTrendValue))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Contacted(Candle c, double d)
        {
            if (c.High >= d && c.Low <= d)
            { return true; }
            else

            {
                return false;
            }
        }
        public Candle DayCandle(Candle d)
        {
            List<double> max = new List<double>();
            List<double> min = new List<double>();
            Candle c = d;
            double open = 0;
            while (c.TimeStamp.Date == d.TimeStamp.Date)
            {
                if (c.CandleType == "G")
                {
                    max.Add(c.Close);
                }
                else
                {
                    min.Add(c.Close);
                }
                open = c.Open;
                c = c.PreviousCandle;


            }

            return new Candle { High = max.Count() > 0 ? max.Max() : d.Close, Low = min.Count() > 0 ? min.Min() : d.Close, Open = open, Close = d.Close };
        }

        public Candle PreviousDayCandle(Candle d)
        {
            try
            {
                List<double> max = new List<double>();
                List<double> min = new List<double>();
                Candle c = d;
                double open = 0;
                while (c.TimeStamp.Date == d.TimeStamp.Date)
                {

                    c = c.PreviousCandle;


                }
                var x = c;
                if (c != null)
                {
                    double close = x.Close;
                    while (c.TimeStamp.Date == x.TimeStamp.Date)
                    {
                        max.Add(c.High);
                        min.Add(c.Low);
                        open = c.Open;
                        c = c.PreviousCandle;


                    }

                    return new Candle { High = max.Count() > 0 ? max.Max() : d.Close, Low = min.Count() > 0 ? min.Min() : d.Close, Open = open, Close = close };
                }
                else
                {
                    return new Candle { High = max.Count() > 0 ? max.Max() : d.Close, Low = min.Count() > 0 ? min.Min() : d.Close, Open = 0, Close = 0 };
                }
            }
            catch
            {
                return new Candle { High = 0, Low = 0, Open = 0, Close = 0 };
            }
        }

        bool Dozi(Candle c)
        {
            return (Math.Abs(c.Open - c.Close) <= ((c.High - c.Low) * 0.1)) && (GetUpperWick(c) > GetLowerWick(c) * 3 || GetLowerWick(c) > GetUpperWick(c) * 3);
        }

        double GetRange(Candle c)
        {
            return Math.Abs(c.High - c.Low);
        }
        double GetBody(Candle c)
        {
            return Math.Abs(c.Close - c.Open);
        }

        double GetLowerWick(Candle c)
        {
            if (c.CandleType == "D" || c.CandleType == "R")
            {
                return c.Close - c.Low;
            }
            else
            {
                return c.Open - c.Low;
            }

        }

        double GetUpperWick(Candle c)
        {
            if (c.CandleType == "D" || c.CandleType == "R")
            {
                return c.High - c.Open;
            }
            else
            {
                return c.High - c.Close;
            }

        }

        public List<PNL> TradeStocks(Dictionary<Guid, StrategyModel> filter3, Dictionary<string, List<Candle>> myTestData, Idea selectedIdea, ProgressDelegate myProgres)
        {
            ConcurrentBag<PNL> finalAmount = new ConcurrentBag<PNL>();
            Parallel.ForEach<KeyValuePair<Guid, StrategyModel>>(filter3, delegate (KeyValuePair<Guid, StrategyModel> gap)
            {
                if (gap.Value != null)
                {

                    StopTarget target = new StopTarget(gap.Value, selectedIdea);
                    double close = gap.Value.Close;
                    double mtm = 0.0;
                    double stopLossRange = target.StopLossRange;
                //int quantity = Convert.ToInt32(200000 / gap.Value.Close);
                int quantity = Convert.ToInt32((double)(selectedIdea.Risk / stopLossRange <= 0 ? 1 : selectedIdea.Risk / stopLossRange));
                    int num5 = quantity;
                    double stoploss = target.Stoploss;
                //stoploss = Convert.ToDouble(gap.Value.Trade == Trade.BUY ? gap.Value.Close - (6000 / quantity) : gap.Value.Close + (6000 / quantity));
                double num7 = stoploss;
                    double num8 = target.BookProfit1;
                    double num9 = target.BookProfit2;
                    List<Candle> list = (from b in myTestData[gap.Value.Stock]
                                         where (b.TimeStamp.Date == gap.Value.Date.Date) && (b.TimeStamp > gap.Value.Date)
                                         select b).OrderBy(b => b.TimeStamp).ToList<Candle>();
                //list.Remove(list.Last());
                List<Candle> list2 = (from b in myTestData[gap.Value.Stock]
                                          where b.TimeStamp.Date == gap.Value.Date.Date
                                          select b).ToList<Candle>();
                    bool flag2 = false;
                    bool flag3 = false;

                    if (gap.Value.Trade != Trade.BUY)
                    {
                        if (gap.Value.Trade == Trade.SELL)
                        {
                            int num11 = 0;
                            foreach (Candle candle2 in list)
                            {
                                if (candle2.High >= stoploss)
                                {
                                    mtm -= quantity * (stoploss - gap.Value.Close);
                                    quantity = 0;
                                }
                                else
                                {
                                    if (selectedIdea.BookProfit == BookProfit.OnlyOnLoss && candle2.Close > gap.Value.Close)
                                    {
                                        mtm -= quantity * (candle2.Close - gap.Value.Close);
                                        stoploss = candle2.Close;
                                        quantity = 0;
                                        flag2 = true;
                                        break;
                                    }
                                    else if (((candle2.Low <= num8) && !flag2) && (selectedIdea.OrderMultiples == OrderMultiples.Three))
                                    {
                                        mtm += (quantity / 3) * stopLossRange;
                                        stoploss = close;
                                        quantity -= quantity / 3;
                                        flag2 = true;
                                    }
                                    else if (((candle2.Low <= num8) && (!flag2 && (selectedIdea.OrderMultiples == OrderMultiples.One))) && (selectedIdea.BookProfit == BookProfit.OneTo1))
                                    {
                                        mtm += quantity * stopLossRange;
                                        stoploss = close;
                                        quantity = 0;
                                        flag2 = true;
                                        break;
                                    }
                                    if (((candle2.Low <= num9) && !flag3) && (selectedIdea.OrderMultiples == OrderMultiples.Three))
                                    {
                                        mtm += (quantity / 2) * (2.0 * stopLossRange);
                                        stoploss = close;
                                        flag3 = true;
                                    }
                                    else if (((candle2.Low <= num9) && (!flag3 && (selectedIdea.OrderMultiples == OrderMultiples.One))) && (selectedIdea.BookProfit == BookProfit.OneTo2))
                                    {
                                        mtm += quantity * (2.0 * stopLossRange);
                                        stoploss = close;
                                        flag3 = true;
                                        quantity = 0;
                                        break;
                                    }
                                    if ((num11 + 1) <= target.FinalCandle)
                                    {
                                        continue;
                                    }
                                }
                                break;
                            }
                            if ((quantity > 0) && (list2.Count<Candle>() > target.FinalCandle))
                            {
                                mtm += quantity * (close - list2[target.FinalCandle].Close);
                            }
                        }
                    }
                    else if (gap.Value.Trade == Trade.BUY)
                    {
                        int num10 = 0;
                        foreach (Candle candle in list)
                        {
                            if (candle.Low <= stoploss)
                            {
                                if (flag2)
                                {
                                    quantity = 0;
                                }
                                else
                                {
                                    mtm -= quantity * (gap.Value.Close - stoploss);
                                    quantity = 0;
                                }
                            }
                            else
                            {
                                if (selectedIdea.BookProfit == BookProfit.OnlyOnLoss && candle.Close < gap.Value.Close)
                                {
                                    mtm += quantity * (candle.Close - gap.Value.Close);
                                    stoploss = candle.Close;
                                    quantity = 0;
                                    flag2 = true;
                                    break;
                                }
                                else
                                if (((candle.High >= num8) && !flag2) && (selectedIdea.OrderMultiples == OrderMultiples.Three))
                                {
                                    mtm += (quantity / 3) * stopLossRange;
                                    stoploss = close;
                                    quantity -= quantity / 3;
                                    flag2 = true;
                                }
                                else if (((candle.High >= num8) && (!flag2 && (selectedIdea.OrderMultiples == OrderMultiples.One))) && (selectedIdea.BookProfit == BookProfit.OneTo1))
                                {
                                    mtm += quantity * stopLossRange;
                                    stoploss = close;
                                    quantity = 0;
                                    flag2 = true;
                                    break;
                                }
                                if (((candle.High >= num9) && !flag3) && (selectedIdea.OrderMultiples == OrderMultiples.Three))
                                {
                                    mtm += (quantity / 2) * (2.0 * stopLossRange);
                                    stoploss = close;
                                    quantity -= quantity / 2;
                                    flag3 = true;
                                }
                                else if (((candle.High >= num9) && (!flag3 && (selectedIdea.OrderMultiples == OrderMultiples.One))) && (selectedIdea.BookProfit == BookProfit.OneTo2))
                                {
                                    mtm += quantity * (2.0 * stopLossRange);
                                    stoploss = close;
                                    quantity = 0;
                                    flag3 = true;
                                    break;
                                }
                                if ((num10 + 1) <= target.FinalCandle)
                                {
                                    continue;
                                }
                            }
                            break;
                        }
                        if ((quantity > 0) && (list2.Count<Candle>() > target.FinalCandle))
                        {
                            mtm += quantity * (list2[target.FinalCandle].Close - close);
                        }
                    }
                    PNL pnl1 = new PNL();
                    pnl1.Amount = mtm;
                    pnl1.Date = gap.Value.Date;
                    pnl1.Stock = gap.Value.Stock;
                    pnl1.Entry = close;
                    pnl1.Quantity = num5;
                    pnl1.BookProfit1 = target.BookProfit1;
                    pnl1.BookProfit2 = target.BookProfit2;
                    if (target.FinalCandle > list2.Count + 1)
                    {
                        pnl1.BookProfit3 = list2[list2.Count - 1].Close;
                    }
                    else
                    {
                        pnl1.BookProfit3 = list2[target.FinalCandle].Close;
                    }
                    pnl1.Direction = gap.Value.Trade == Trade.BUY ? "BUY" : "SELL";
                    pnl1.ChartData = list2;
                    pnl1.Change = (Math.Abs(gap.Value.PreviousClose - gap.Value.Close) / gap.Value.PreviousClose) * 100;
                    PNL item = pnl1;
                    item.Stoploss = num7;
                    finalAmount.Add(item);
                    myProgres($"PNL for stock {gap.Value.Stock} for Day{gap.Value.Date.Date} is : {mtm}");
                }
            });
            List<PNL> source = (from a in finalAmount
                                orderby a.Date descending
                                select a).ToList<PNL>();
            double num = source.Sum<PNL>(a => a.Amount);
            myProgres($"Total PNL is : {num}");
            return source;
        }


    }
}

