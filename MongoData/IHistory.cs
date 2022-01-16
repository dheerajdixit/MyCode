using Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoData
{
    public abstract class IHistory
    {
        IMongoDatabase database;
        public virtual string databaseName { get { return "TradeHistoryDaily"; } }
        public virtual string folderName { get { return @"C:\Users\dheeraj_kumar_dixit\Downloads\allData\allData\BackupDaily\"; } }
        public int cores = Environment.ProcessorCount;
        public IHistory()
        {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            database = client.GetDatabase(databaseName);
        }

        public ConcurrentBag<Candle> GetHistory(string collectionName, string stockName, DateTime fromDate, DateTime toDate)
        {
            ConcurrentBag<Candle> objCandles = new ConcurrentBag<Candle>();

            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(collectionName);

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Lt("date", toDate.AddDays(1)) & builder.Gt("date", fromDate);



            List<BsonDocument> document = collection.Find(filter).ToList();

            Parallel.ForEach(document, new ParallelOptions { MaxDegreeOfParallelism = cores }, (i) =>
          {
              BsonElement be;
              if (i.TryGetElement("close", out be))
              {
                  objCandles.Add(new Candle { TimeStamp = Convert.ToDateTime(i.GetElement("date").Value).AddMinutes(330), Stock = stockName, Close = Convert.ToDouble(i.GetElement("close").Value), Low = Convert.ToDouble(i.GetElement("low").Value), High = Convert.ToDouble(i.GetElement("high").Value), Open = Convert.ToDouble(i.GetElement("open").Value), Volume = Convert.ToDouble(i.GetElement("volume").Value) });
              }
          });

            return objCandles;
        }

        public List<Candle> GetHistoryFromFile(string folderName, string stockName, DateTime fromDate, DateTime toDate)
        {
            
            /// ConcurrentBag<Candle> result = new ConcurrentBag<Candle>();
            if (System.IO.File.Exists(folderName + stockName + ".json"))
            {
                return ConvertToJason(System.IO.File.ReadAllText(folderName + stockName + ".json"),stockName).Where(a => a.TimeStamp >= fromDate && a.TimeStamp <= toDate).ToList();
            }
            return new List<Candle>();
        }

        public static List<Candle> ConvertToJason(string jSon, string stockName,bool heikenAshi = false)
        {
            List<Candle> history = new List<Candle>();

            int ii = 0;
            string[] listOfCandles = jSon.Split(new string[] { "],", "[[" }, StringSplitOptions.RemoveEmptyEntries);
            Candle lastCandle = new Candle();
            Candle secondLastCandle = new Candle();
            Candle latestCandle = new Candle();
            //IMovingAverage avg20 = new SimpleMovingAverage(20);
            //IMovingAverage avg50 = new SimpleMovingAverage(50);
            //IMovingAverage avg200 = new SimpleMovingAverage(200);
            double cClose = 0;
            double cHigh = 0;
            double cLow = 0;
            double cOpen = 0;
            string xHCandleType = string.Empty;
            double xC = 0, xH = 0, xLow = 0, xO = 0, pxH = 0, pxO = 0, pxC = 0, pxL = 0;
            foreach (string jCandle in listOfCandles)
            {
                ii++;
                if (ii == 1)
                {
                    continue;

                }


                string[] candleAttributes = jCandle.Split(new string[] { ",", "]", "[" }, StringSplitOptions.RemoveEmptyEntries);

                cClose = Convert.ToDouble(candleAttributes[4]);
                cHigh = Convert.ToDouble(candleAttributes[2]);
                cLow = Convert.ToDouble(candleAttributes[3]);
                cOpen = Convert.ToDouble(candleAttributes[1]);


                if (ii == 2)
                {
                    xC = cClose;
                    xH = cHigh;
                    xLow = cLow;
                    xO = cOpen;
                }
                else
                {
                    pxH = lastCandle.Close;

                    xC = (cClose + cHigh + cLow + cOpen) / 4;
                    xO = (pxO + pxC) / 2;
                    xLow = Math.Min(Math.Min(xC, xO), cLow);
                    xH = Math.Max(Math.Max(xC, xO), cHigh);
                }
                if (xLow == xO)
                {
                    xHCandleType = "G";
                }
                else if (xH == xO)
                {
                    xHCandleType = "R";
                }
                else if (xC > xO)
                {
                    xHCandleType = "GD";
                }
                else if (xC < xO)
                {
                    xHCandleType = "RD";
                }


                pxC = xC;
                pxH = xH;
                pxL = xLow;
                pxO = xO;

                double cATR21 = 0;
                double cATR7 = 0;


                latestCandle = new Candle()
                {
                    TimeStamp = Convert.ToDateTime(candleAttributes[0].Substring(1, 19).Replace("T", " ")),
                    Close = cClose,
                    High = cHigh,
                    Low = cLow,
                    Open = Convert.ToDouble(candleAttributes[1]),
                    Volume = Convert.ToDouble(candleAttributes[5]),
                    ATRStopLoss = cATR21,
                    HCandleType = xHCandleType,
                    HOpen = xO,
                    HClose = xC,
                    HLow = xLow,
                    HHigh = xH,
                    Stock= stockName


                };
                //latestCandle.CP = CandlePattern(secondLastCandle, lastCandle, latestCandle);
                //latestCandle.Treding = Trending(latestCandle, lastCandle);
                history.Add(latestCandle);


                secondLastCandle = lastCandle;
                lastCandle = latestCandle;
            }

            return history;
        }

        public async void InsertHistory(string collectionName, string json)
        {
            try
            {

                List<BsonDocument> bs = new List<BsonDocument>();
                IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(collectionName);
                BsonDocument d = BsonDocument.Parse(json);
                BsonArray ba = ((MongoDB.Bson.BsonArray)d.Values.ToList()[1].ToBsonDocument().ToList()[0].Value);
                foreach (var a in ba.Values)
                {
                    var j = ((MongoDB.Bson.BsonArray)a).Values.ToArray();
                    BsonDocument doc = new BsonDocument {
    { "date", Convert.ToDateTime( j[0]).AddHours(5).AddMinutes(30) },
        { "high", j[2] },
     { "low", j[3] },
     { "close", j[4] },
     { "open", j[1] },
     { "volume", j[5] },
};
                    bs.Add(doc);
                }



                //BsonDocument da = new BsonDocument(ba.Values);


                //var a = new BsonDocument {
                //    { "values", BsonSerializer.Deserialize<BsonArray>(ba) };


                //List<BsonDocument> document = ((MongoDB.Bson.BsonArray)((MongoDB.Bson.BsonDocument)d.Values.ToList()[1]).Values.ToList()[0]).Values.ToList();
                await collection.InsertManyAsync(bs);
            }
            catch (Exception ex)
            {

            }

        }
    }
}
