using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public enum Technical
    {
        SuperTrend = 0,
        SimpleMovingAverage = 1,
        MACD = 2,
        RSI = 3
    }

    public enum Range
    {
        Gap = 0,
        Normal = 1,
        GapUpDown = 0,
    }
    public enum OrderMultiples
    {
        One = 0,
        Two = 1,
        Three = 2,
    }
    public enum Sorting
    {
        VolumeFirst = 0,
        RangeFirst = 1
    }
    public enum CandleType
    {
        Solid = 0,
        Any = 1,
        ThreeCandle = 2
    }
    public enum BookProfit
    {
        OneTo1 = 0,
        OneTo2 = 1,
        OneTo3 = 2,
        DayEnd = 3,
        SpecificInterval = 4
    }
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
        public CandleType CandleType { get; set; }
        public Range Range { get; set; }
        public Sorting Sorting { get; set; }
        public OrderMultiples OrderMultiples { get; set; }
        public double Risk { get; set; }
        public BookProfit BookProfit { get; set; }
    }
}
