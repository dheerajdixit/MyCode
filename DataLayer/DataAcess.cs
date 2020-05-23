using CommonFeatures;
using Model;
using MongoData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zerodha;
namespace DAL
{


    public class DataAcess
    {
        public int cores = Environment.ProcessorCount;

        public ConcurrentBag<ConcurrentBag<Candle>> GetHistory(ProgressDelegate progress, int period, DateTime fromDate, DateTime toDate)
        {
            IHistory h;
            int num;
            Dictionary<string, string> source = ZerodhaOperations.LoadInstruments();
            ConcurrentBag<ConcurrentBag<Candle>> resultCollection = new ConcurrentBag<ConcurrentBag<Candle>>();
            int num2 = period;
            if (num2 > 10)
            {
                if (num2 == 15)
                {
                    h = new TradeHistory15();
                    goto TR_0000;
                }
                else if (num2 == 30)
                {
                    h = new TradeHistory30();
                    goto TR_0000;
                }
                else if (num2 == 60)
                {
                    h = new TradeHistory60();
                    goto TR_0000;
                }
            }
            else if (num2 == 5)
            {
                h = new TradeHistory5();
                goto TR_0000;
            }
            else if (num2 == 10)
            {
                h = new TradeHistory10();
                goto TR_0000;
            }
            h = new TradeHistoryDaily();
            TR_0000:
            num = Environment.ProcessorCount;
            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = num;
            Parallel.ForEach<KeyValuePair<string, string>>(source, parallelOptions, delegate (KeyValuePair<string, string> i)
            {
                resultCollection.Add(h.GetHistory(h.folderName, i.Value, fromDate, toDate));
                progress($"Loading Data ... {i.Value}");
            });
            return resultCollection;
        }

        public ConcurrentBag<List<Candle>> GetHistoryFromFile(ProgressDelegate progress, int period, DateTime fromDate, DateTime toDate)
        {
            IHistory h;
            int num;
            Dictionary<string, string> source = ZerodhaOperations.LoadInstruments();
            ConcurrentBag<List<Candle>> resultCollection = new ConcurrentBag<List<Candle>>();
            int num2 = period;
            if (num2 > 10)
            {
                if (num2 == 15)
                {
                    h = new TradeHistory15();
                    goto TR_0000;
                }
                else if (num2 == 30)
                {
                    h = new TradeHistory30();
                    goto TR_0000;
                }
                else if (num2 == 60)
                {
                    h = new TradeHistory60();
                    goto TR_0000;
                }
            }
            else if (num2 == 5)
            {
                h = new TradeHistory5();
                goto TR_0000;
            }
            else if (num2 == 10)
            {
                h = new TradeHistory10();
                goto TR_0000;
            }
            h = new TradeHistoryDaily();
            TR_0000:
            num = Environment.ProcessorCount;
            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = num;
            Parallel.ForEach<KeyValuePair<string, string>>(source, parallelOptions, delegate (KeyValuePair<string, string> i)
            {
                resultCollection.Add(h.GetHistoryFromFile(h.folderName, i.Value, fromDate, toDate));
                progress($"Loading Data ... {i.Value}");
            });
            return resultCollection;
        }

        public ConcurrentBag<ConcurrentBag<Candle>> GetHistory(string InstrumentToken, int period, DateTime fromDate, DateTime toDate)
        {
            IHistory history;
            ConcurrentBag<ConcurrentBag<Candle>> bag = new ConcurrentBag<ConcurrentBag<Candle>>();
            int num = period;
            if (num > 10)
            {
                if (num == 15)
                {
                    history = new TradeHistory15();
                    goto TR_0000;
                }
                else if (num == 30)
                {
                    history = new TradeHistory30();
                    goto TR_0000;
                }
                else if (num == 60)
                {
                    history = new TradeHistory60();
                    goto TR_0000;
                }
            }
            else if (num == 5)
            {
                history = new TradeHistory5();
                goto TR_0000;
            }
            else if (num == 10)
            {
                history = new TradeHistory10();
                goto TR_0000;
            }
            history = new TradeHistoryDaily();
            TR_0000:
            bag.Add(history.GetHistory(InstrumentToken, string.Empty, fromDate, toDate));
            return bag;
        }

        public void InsertHistory(string collectionName, int period, string json)
        {
            IHistory history;
            int num = period;
            if (num > 10)
            {
                if (num == 15)
                {
                    history = new TradeHistory15();
                    goto TR_0000;
                }
                else if (num == 30)
                {
                    history = new TradeHistory30();
                    goto TR_0000;
                }
                else if (num == 60)
                {
                    history = new TradeHistory60();
                    goto TR_0000;
                }
            }
            else if (num == 5)
            {
                history = new TradeHistory5();
                goto TR_0000;
            }
            else if (num == 10)
            {
                history = new TradeHistory10();
                goto TR_0000;
            }
            history = new TradeHistoryDaily();
            TR_0000:
            history.InsertHistory(collectionName, json);
        }
    }
}

