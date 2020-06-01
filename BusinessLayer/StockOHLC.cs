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
                myProgres($"Collecting first candle for all days for {stock.Key}");

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
                    myProgres($"Arranging stock date wise for {myDate.Date}");
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
                myProgres($"Selecting Most solid gap up stock for Date {pair2.Key}");
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
                                                         select a).Take<StrategyModel>(selectedIdea.TradePerSession))
                        {
                            dictionary2.Add(Guid.NewGuid(), model);
                            num++;
                            if (num == selectedIdea.TradePerSession)
                            {
                                break;
                            }
                        }
                        if (num == selectedIdea.TradePerSession)
                        {
                            break;
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
                    foreach (StrategyModel model2 in (from b in pair2.Value.Where<StrategyModel>(predicate)
                                                      orderby b.Range descending
                                                      select b))

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

        public void InsertHistory(string collectionName, int period, string json)
        {
            new DataAcess().InsertHistory(collectionName, period, json);
        }

        public IEnumerable<Candle> PrepareFirstLevelOfFiltering(List<Candle> allCandles, Idea selctedIdea, Time t)
        {

            //var c1 = allCandles.OrderBy(a=>a.TimeStamp).ToList();

            //XmlSerializer xs = new XmlSerializer(typeof(List<Candle>));
            //using (StreamWriter writer = new StreamWriter(@"C:\Jai Sri Thakur Ji\foo.xml"))
            //{
            //    xs.Serialize(writer, c1);
            //}


            IEnumerable<Candle> enumerable = from b in allCandles
                                             where (b.Close > 50.0) && (b.PreviousCandle.Close > 0.0)
                                             where ((b.TimeStamp.Hour >= t.StartHour) && ((b.TimeStamp.Minute >= t.StartMinute) && (b.TimeStamp.Hour <= t.EndHour))) && (b.TimeStamp.Minute <= t.EndMinute)
                                             select b;
            //if (selctedIdea.TI.Contains(Technical.SuperTrend) && selctedIdea.TI.Contains(Technical.SimpleMovingAverage))
            //{
            //    enumerable = from b in enumerable
            //                 where (b.Open == b.High) || (b.Open == b.Low)
            //                 select b;
            //}
            //else if (selctedIdea.TI.Contains(Technical.SuperTrend))
            //{
            //}
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
            if (selctedIdea.Name.Contains("MyNewIdea"))
            {

                //foreach (var b in enumerable)
                //{
                //    if (b.TimeStamp.Date.Day == 28)
                //    {
                //        bool x = b.Low <= b.AllIndicators.SMA20 + b.Low * 0.001;
                //        x = (GetBody(b) > GetUpperWick(b) || GetBody(b) > GetLowerWick(b));
                //        x = b.AllIndicators.SMA50 < b.AllIndicators.SMA20;
                //        x = b.AllIndicators.SuperTrend.SuperTrendValue < b.AllIndicators.SMA50;
                //    }
                //}
                enumerable = from b in enumerable
                             where
                             b.Low <= b.AllIndicators.SMA20 + b.Low * 0.001
                             && (GetBody(b) > GetUpperWick(b) || GetBody(b) > GetLowerWick(b))
                             && b.AllIndicators.SMA50 < b.AllIndicators.SMA20
                             && b.AllIndicators.SuperTrend.SuperTrendValue < b.AllIndicators.SMA50
                             && b.CandleType == "G"
                             select b;

                foreach (var c in enumerable)
                {
                    if (c.CandleType == "G")
                        c.Trade = Trade.BUY;
                    else
                        c.Trade = Trade.SELL;

                    //enumerable = from b in enumerable
                    //             where ((b.PreviousCandle.CandleType == "G" && b.PreviousCandle.High == b.Highest && b.Open < b.PreviousCandle.High && b.Open > b.PreviousCandle.Close && b.Close > b.PreviousCandle.Close && b.Close < b.PreviousCandle.High)
                    //             || (b.PreviousCandle.CandleType == "R" && b.PreviousCandle.Low == b.Lowest && b.Open < b.PreviousCandle.Close && b.Open > b.PreviousCandle.Low && b.Close > b.PreviousCandle.Low && b.Close < b.PreviousCandle.Close))
                    //             select b;

                    //foreach (var c in enumerable)
                    //{
                    //    if (c.Close > c.PreviousCandle.Close)
                    //        c.Trade = Trade.SELL;
                    //    else
                    //        c.Trade = Trade.BUY;
                }

            }
            else if (selctedIdea.Name == "OHOL")
            {
                enumerable = from b in enumerable
                             where (b.Open == b.Low) || (b.High == b.Open)
                             select b;
                foreach (var c in enumerable)
                {
                    if (c.CandleType == "G")
                        c.Trade = Trade.BUY;
                    else
                        c.Trade = Trade.SELL;
                }

            }
            else if (selctedIdea.Name == "OHOLINV")
            {
                enumerable = from b in enumerable
                             where ((b.Open == b.Low && GetUpperWick(b) > GetBody(b) * 1.25) || (b.High == b.Open && GetLowerWick(b) > GetBody(b) * 1.25))
                             select b;
                foreach (var c in enumerable)
                {
                    if (c.CandleType == "G")
                        c.Trade = Trade.SELL;
                    else
                        c.Trade = Trade.BUY;
                }

            }
            else if (selctedIdea.Name == "MyOldIdea")
            {
                enumerable = from b in enumerable
                             where (Dozi(b)) && (Math.Abs(b.PreviousCandle.Close - b.PreviousCandle.Open) / b.PreviousCandle.Close) * 100 <= 1 && ((b.PreviousCandle.High - b.PreviousCandle.Low) / b.PreviousCandle.Close) * 100 <= 1.5
                             select b;
                foreach (var c in enumerable)
                {

                    if (c.PreviousCandle.CandleType == "G")
                        c.Trade = Trade.BUY;
                    else if (c.PreviousCandle.CandleType == "R")
                        c.Trade = Trade.SELL;
                }

            }
            else if (selctedIdea.Name == "CROSS2050MIN15")
            {
                enumerable = from b in enumerable
                             where ((b.CandleType == "G" && b.PreviousCandle.CandleType == "R" && b.PreviousCandle.PreviousCandle.CandleType == "G") || (b.CandleType == "R" && b.PreviousCandle.CandleType == "G" && b.PreviousCandle.PreviousCandle.CandleType == "R"))
                             select b;
                foreach (var c in enumerable)
                {

                    if (c.CandleType == "G")
                        c.Trade = Trade.BUY;
                    else
                        c.Trade = Trade.SELL;
                }

            }
            else if (selctedIdea.Name == "SuperTrendSupport")
            {
                enumerable = from b in enumerable
                             where ((b.AllIndicators.SuperTrend.Trend > 0 && b.Low < b.AllIndicators.SuperTrend.SuperTrendValue && b.Close > b.AllIndicators.SuperTrend.SuperTrendValue && b.AllIndicators.SuperTrend.SuperTrendValue == b.PreviousCandle.AllIndicators.SuperTrend.SuperTrendValue)
                             ||
                             (b.AllIndicators.SuperTrend.Trend < 0 && b.High > b.AllIndicators.SuperTrend.SuperTrendValue && b.Close < b.AllIndicators.SuperTrend.SuperTrendValue && b.AllIndicators.SuperTrend.SuperTrendValue == b.PreviousCandle.AllIndicators.SuperTrend.SuperTrendValue)
                             )
                             select b;
                foreach (var c in enumerable)
                {

                    if (c.AllIndicators.SuperTrend.Trend > 0)
                        c.Trade = Trade.BUY;
                    else
                        c.Trade = Trade.SELL;
                }
            }
            else if (selctedIdea.Name == "2050")
            {
                enumerable = from b in enumerable
                             where ((b.CandleType == "G" && b.Low < Math.Min(b.AllIndicators.SMA20, b.AllIndicators.SMA50) && b.Close > Math.Max(b.AllIndicators.SMA20, b.AllIndicators.SMA50))
                             ||
                             (b.CandleType == "R" && b.High > Math.Max(b.AllIndicators.SMA20, b.AllIndicators.SMA50) && b.Close < Math.Min(b.AllIndicators.SMA20, b.AllIndicators.SMA50)))

                             select b;
                foreach (var c in enumerable)
                {

                    if (c.CandleType == "G")
                        c.Trade = Trade.BUY;
                    else
                        c.Trade = Trade.SELL;
                }
            }
            else if (selctedIdea.Name == "SuperTrendInv")
            {
                enumerable = from b in enumerable
                             where (
                             (b.AllIndicators.SuperTrend.Trend != b.PreviousCandle.AllIndicators.SuperTrend.Trend))


                             select b;
                foreach (var c in enumerable)
                {

                    if (c.AllIndicators.SuperTrend.Trend == 1)
                        c.Trade = Trade.BUY;
                    else
                        c.Trade = Trade.SELL;
                }
            }
            else if (selctedIdea.Name == "5minuteCrossOver")
            {
                enumerable = from b in enumerable
                             where (
                             (b.High >= Math.Max(Math.Max(Math.Max(b.AllIndicators.SMA20, b.AllIndicators.SMA50), b.AllIndicators.SMA200), b.AllIndicators.SuperTrend.SuperTrendValue)
                             && b.Low < Math.Min(Math.Min(Math.Min(b.AllIndicators.SMA20, b.AllIndicators.SMA50), b.AllIndicators.SMA200), b.AllIndicators.SuperTrend.SuperTrendValue)
                             ))
                             select b;
                foreach (var c in enumerable)
                {

                    if (c.CandleType == "G")
                        c.Trade = Trade.BUY;
                    else
                        c.Trade = Trade.SELL;
                }
            }
            else if (selctedIdea.Name == "15minuteCrossOver")
            {
                enumerable = from b in enumerable
                             where (
                             (b.Close > Math.Max(Math.Max(Math.Max(b.AllIndicators.SMA20, b.AllIndicators.SMA50), b.AllIndicators.SMA200), b.AllIndicators.SuperTrend.SuperTrendValue)
                             && b.Low <= Math.Min(Math.Min(Math.Min(b.AllIndicators.SMA20, b.AllIndicators.SMA50), b.AllIndicators.SMA200), b.AllIndicators.SuperTrend.SuperTrendValue)
                             && b.CandleType == "G"
                             ) ||
                             (
                             b.High >= Math.Max(Math.Max(Math.Max(b.AllIndicators.SMA20, b.AllIndicators.SMA50), b.AllIndicators.SMA200), b.AllIndicators.SuperTrend.SuperTrendValue)
                             && b.Close < Math.Min(Math.Min(Math.Min(b.AllIndicators.SMA20, b.AllIndicators.SMA50), b.AllIndicators.SMA200), b.AllIndicators.SuperTrend.SuperTrendValue)
                             && b.CandleType == "R"

                             )
                             )
                             select b;
                foreach (var c in enumerable)
                {

                    if (c.CandleType == "G")
                        c.Trade = Trade.BUY;
                    else
                        c.Trade = Trade.SELL;
                }
            }



            return enumerable;
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
                    double num2 = 0.0;
                    double stopLossRange = target.StopLossRange;
                    int num4 = Convert.ToInt32((double)(selectedIdea.Risk / stopLossRange == 0 ? 1 : stopLossRange));
                    int num5 = num4;
                    double stoploss = target.Stoploss;
                    double num7 = stoploss;
                    double num8 = target.BookProfit1;
                    double num9 = target.BookProfit2;
                    List<Candle> list = (from b in myTestData[gap.Value.Stock]
                                         where (b.TimeStamp.Date == gap.Value.Date.Date) && (b.TimeStamp > gap.Value.Date)
                                         select b) .OrderBy(b=>b.TimeStamp).ToList<Candle>();
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
                                    num2 -= num4 * (stoploss - gap.Value.Close);
                                    num4 = 0;
                                }
                                else
                                {
                                    if (((candle2.Low <= num8) && !flag2) && (selectedIdea.OrderMultiples == OrderMultiples.Three))
                                    {
                                        num2 += (num4 / 3) * stopLossRange;
                                        stoploss = close;
                                        num4 -= num4 / 3;
                                        flag2 = true;
                                    }
                                    else if (((candle2.Low <= num8) && (!flag2 && (selectedIdea.OrderMultiples == OrderMultiples.One))) && (selectedIdea.BookProfit == BookProfit.OneTo1))
                                    {
                                        num2 += num4 * stopLossRange;
                                        stoploss = close;
                                        num4 = 0;
                                        flag2 = true;
                                        break;
                                    }
                                    if (((candle2.Low <= num9) && !flag3) && (selectedIdea.OrderMultiples == OrderMultiples.Three))
                                    {
                                        num2 += (num4 / 2) * (2.0 * stopLossRange);
                                        stoploss = close;
                                        flag3 = true;
                                    }
                                    else if (((candle2.Low <= num9) && (!flag3 && (selectedIdea.OrderMultiples == OrderMultiples.One))) && (selectedIdea.BookProfit == BookProfit.OneTo2))
                                    {
                                        num2 += num4 * (2.0 * stopLossRange);
                                        stoploss = close;
                                        flag3 = true;
                                        num4 = 0;
                                        break;
                                    }
                                    if ((num11 + 1) <= target.FinalCandle)
                                    {
                                        continue;
                                    }
                                }
                                break;
                            }
                            if ((num4 > 0) && (list2.Count<Candle>() > target.FinalCandle))
                            {
                                num2 += num4 * (close - list2[target.FinalCandle].Close);
                            }
                        }
                    }
                    else
                    {
                        int num10 = 0;
                        foreach (Candle candle in list)
                        {
                            if (candle.Low <= stoploss)
                            {
                                if (flag2)
                                {
                                    num4 = 0;
                                }
                                else
                                {
                                    num2 -= num4 * (gap.Value.Close - stoploss);
                                    num4 = 0;
                                }
                            }
                            else
                            {
                                if (((candle.High >= num8) && !flag2) && (selectedIdea.OrderMultiples == OrderMultiples.Three))
                                {
                                    num2 += (num4 / 3) * stopLossRange;
                                    stoploss = close;
                                    num4 -= num4 / 3;
                                    flag2 = true;
                                }
                                else if (((candle.High >= num8) && (!flag2 && (selectedIdea.OrderMultiples == OrderMultiples.One))) && (selectedIdea.BookProfit == BookProfit.OneTo1))
                                {
                                    num2 += num4 * stopLossRange;
                                    stoploss = close;
                                    num4 = 0;
                                    flag2 = true;
                                    break;
                                }
                                if (((candle.High >= num9) && !flag3) && (selectedIdea.OrderMultiples == OrderMultiples.Three))
                                {
                                    num2 += (num4 / 2) * (2.0 * stopLossRange);
                                    stoploss = close;
                                    num4 -= num4 / 2;
                                    flag3 = true;
                                }
                                else if (((candle.High >= num9) && (!flag3 && (selectedIdea.OrderMultiples == OrderMultiples.One))) && (selectedIdea.BookProfit == BookProfit.OneTo2))
                                {
                                    num2 += num4 * (2.0 * stopLossRange);
                                    stoploss = close;
                                    num4 = 0;
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
                        if ((num4 > 0) && (list2.Count<Candle>() > target.FinalCandle))
                        {
                            num2 += num4 * (list2[target.FinalCandle].Close - close);
                        }
                    }
                    PNL pnl1 = new PNL();
                    pnl1.Amount = num2;
                    pnl1.Date = gap.Value.Date;
                    pnl1.Stock = gap.Value.Stock;
                    pnl1.Entry = close;
                    pnl1.Quantity = num5;
                    pnl1.BookProfit1 = target.BookProfit1;
                    pnl1.BookProfit2 = target.BookProfit2;
                    pnl1.BookProfit3 = list2[target.FinalCandle].Close;
                    pnl1.Direction = gap.Value.Trade == Trade.BUY ? "BUY" : "SELL";
                    pnl1.ChartData = list2;
                    pnl1.Change = (Math.Abs(gap.Value.PreviousClose - gap.Value.Close) / gap.Value.PreviousClose) * 100;
                    PNL item = pnl1;
                    item.Stoploss = num7;


                    finalAmount.Add(item);
                    myProgres($"PNL for stock {gap.Value.Stock} for Day{gap.Value.Date.Date} is : {num2}");
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

