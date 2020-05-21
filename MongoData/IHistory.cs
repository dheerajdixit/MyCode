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
