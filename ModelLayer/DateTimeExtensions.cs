using System;
using System.Runtime.CompilerServices;

namespace Model
{
    
    public static class DateTimeExtensions
    {
        //[Extension]
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int num = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays((double) (-1 * num)).Date;
        }
    }
}

