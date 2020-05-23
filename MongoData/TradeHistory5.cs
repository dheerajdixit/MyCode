using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using CommonFeatures;

namespace MongoData
{
    public class TradeHistory5 : IHistory
    {
        public override string databaseName => "TradeHistory5";
        public override string folderName => @"C:\Users\dheeraj_kumar_dixit\Downloads\allData\allData\\Backup5\";
    }
}
