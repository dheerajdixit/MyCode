using Model;
using System;
using System.Collections.Generic;

namespace NSA
{
    public class Receipt
    {
        public Dictionary<string, StockData> StocksIdentified
        {
            get;
            set;
        }

        public string StocksLeftOut
        {
            get;
            set;
        }
    }
}
