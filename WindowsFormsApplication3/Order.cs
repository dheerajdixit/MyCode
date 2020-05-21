using System;

namespace NSA
{
    public class Order
    {
        public string Scrip
        {
            get;
            set;
        }

        public DateTime TimeStamp
        {
            get;
            set;
        }

        public string TransactionType
        {
            get;
            set;
        }

        public int Quantity
        {
            get;
            set;
        }

        public double EntryPrice
        {
            get;
            set;
        }

        public double Stoploss
        {
            get;
            set;
        }

        public string Strategy
        {
            get;
            set;
        }

        public double High
        {
            get;
            set;
        }

        public double Low
        {
            get;
            set;
        }

        public double Volume
        {
            get;
            set;
        }
    }
}
