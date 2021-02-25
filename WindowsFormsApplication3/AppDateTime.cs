using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSA
{
    public static class AppDateTime
    {
        public static DateTime Get(this DateTime x, DateTime currentTradingDate)
        {
            TimeSpan t = DateTime.Now - currentTradingDate;
            return DateTime.Now.AddTicks(0 - t.Ticks);
        }

    }
}
