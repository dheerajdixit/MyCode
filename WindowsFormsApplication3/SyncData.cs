using System;

namespace NSA
{
    public class SyncData
    {
        public string orders
        {
            get;
            set;
        }

        public double HighestProfitToday
        {
            get;
            set;
        }

        public double LowestRiskToday
        {
            get;
            set;
        }

        public int Minute
        {
            get;
            set;
        }
    }
}
