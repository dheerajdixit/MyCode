using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace Model
{


    public class Idea
    {
        public string Name { get; set; }

        public int Interval { get; set; }

        public int Stoploss { get; set; }

        public int EntryStartCandle { get; set; }

        public int EntryFinishCandle { get; set; }

        public int TradePerSession { get; set; }

        public int FilterByVolume { get; set; }

        public List<Technical> TI { get; set; }

        public Model.CandleType CandleType { get; set; }

        public Model.Range Range { get; set; }

        public Model.Sorting Sorting { get; set; }

        public Model.OrderMultiples OrderMultiples { get; set; }

        public double Risk { get; set; }

        public Model.BookProfit BookProfit { get; set; }

        public int TryAfterContinuosError { get; set; }
        public int runOrder { get; set; }
    }
}

