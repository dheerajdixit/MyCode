﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Model
{
    public class CustomizedPNL
    {
        public List<PNL> Strategyoutput { get; set; }
        public int order { get; set; }
        public Idea selectedIdea { get; set; }
    }
    public class PNL
    {
        public string Stock { get; set; }

        public double Amount { get; set; }

        public DateTime Date { get; set; }

        public double Entry { get; set; }

        public double Stoploss { get; set; }

        public double BookProfit1 { get; set; }

        public double BookProfit2 { get; set; }

        public double BookProfit3 { get; set; }

        public double Exit1 { get; set; }

        public double Exit2 { get; set; }

        public double Exit3 { get; set; }

        public string Direction { get; set; }

        public int Quantity { get; set; }

        public List<Candle> ChartData { get; set; }

        public double Change { get; set; }
    }
}

