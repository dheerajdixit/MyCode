﻿using MongoDB.Bson;
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
    public class TradeHistoryDaily : IHistory
    {
        public override string databaseName => "TradeHistoryDaily";

    }

}
