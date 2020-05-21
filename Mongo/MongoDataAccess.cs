
using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;

namespace Mongo
{
    public sealed class MongoDataAccess
    {
        private readonly static MongoDataAccess mongoDataAccessObject = new MongoDataAccess();
        const string connectionString = "mongodb://localhost:27012";

        private MongoDataAccess()
        {
        }

        public static MongoDataAccess GetInstance()
        {
            return mongoDataAccessObject;
        }
        public static string GetHistory(string scripName, int interval, DateTime startDate, DateTime endData)
        {

            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer();
            MongoDatabase database = server.GetDatabase("Test");
            MongoCollection symbolcollection = database.GetCollection("101121");
            return string.Empty;
        }



    }
}
