using MongoData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zerodha;
using Model;
using System.Diagnostics;
using CommonFeatures;

namespace DAL
{
    public class DataAcess
    {
        public int cores = Environment.ProcessorCount;
        public ConcurrentBag<ConcurrentBag<Candle>> GetHistory(ProgressDelegate progress, int period, DateTime fromDate, DateTime toDate)
        {
            Dictionary<string, string> instrumnets = ZerodhaOperations.LoadInstruments();
            ConcurrentBag<ConcurrentBag<Candle>> resultCollection = new ConcurrentBag<ConcurrentBag<Candle>>();
            IHistory h;
            switch (period)
            {
                case 60:
                    h = new TradeHistory60();
                    break;
                case 30:
                    h = new TradeHistory30();
                    break;
                case 15:
                    h = new TradeHistory15();
                    break;
                case 5:
                    h = new TradeHistory5();
                    break;
                case 10:
                    h = new TradeHistory10();
                    break;
                default:
                    h = new TradeHistoryDaily();
                    break;
            }

            int cores = Environment.ProcessorCount;
            Parallel.ForEach(instrumnets, new ParallelOptions { MaxDegreeOfParallelism = cores }, (i) =>
            {
                resultCollection.Add(h.GetHistory(i.Key, i.Value, fromDate, toDate));
                progress(string.Format("Loading Data ... {0}", i.Value));

            });

            return resultCollection;
        }


        public void InsertHistory(string collectionName , int period, string json)
        {
            IHistory h;
            switch (period)
            {
                case 60:
                    h = new TradeHistory60();
                    break;
                case 30:
                    h = new TradeHistory30();
                    break;
                case 15:
                    h = new TradeHistory15();
                    break;
                case 5:
                    h = new TradeHistory5();
                    break;
                case 10:
                    h = new TradeHistory10();
                    break;
                default:
                    h = new TradeHistoryDaily();
                    break;
            }

            h.InsertHistory(collectionName, json);
        }

        public ConcurrentBag<ConcurrentBag<Candle>> GetHistory(string InstrumentToken, int period, DateTime fromDate, DateTime toDate)
        {
            //Dictionary<string, string> instrumnets = ZerodhaOperations.LoadInstruments();
            ConcurrentBag<ConcurrentBag<Candle>> resultCollection = new ConcurrentBag<ConcurrentBag<Candle>>();
            IHistory h;
            switch (period)
            {
                case 60:
                    h = new TradeHistory60();
                    break;
                case 30:
                    h = new TradeHistory30();
                    break;
                case 15:
                    h = new TradeHistory15();
                    break;
                case 5:
                    h = new TradeHistory5();
                    break;
                case 10:
                    h = new TradeHistory10();
                    break;
                default:
                    h = new TradeHistoryDaily();
                    break;
            }

            resultCollection.Add(h.GetHistory(InstrumentToken, string.Empty, fromDate, toDate));
            return resultCollection;
        }
    }
}
