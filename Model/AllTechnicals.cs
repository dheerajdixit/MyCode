using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class AllTechnicals
    {
        public SuperTrend SuperTrend { get; set; }
        public MACD MACD { get; set; }
        public RSI RSI { get; set; }
        public double SMA20 { get; set; }
        public double SMA50 { get; set; }
        public double SMA200 { get; set; }
    }
}
