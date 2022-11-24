using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSA
{

    public class DoubleSupport
    {
        public double FibRet { get; set; }
        public double FibExt { get; set; }

        public double DefinedThreshold { get; set; }
        public double ActualThreshold { get; set; }
        // public ABCDPattern ABCD { get; set; }

    }

    public class Trend
    {
        public Candle TrendStartCandle { get; set; }
        public Candle TrendContinuationCandle { get; set; }

        public Candle ReversalCandle { get; set; }
    }

    public class CommonFunctions
    {
        DataSet dsLots { get; set; }
        public CommonFunctions()
        {
            dsLots = new DataSet();
            LoadFnOTable();

        }
        public bool IsLeg(string strategy, int legNumber)
        {
            if (strategy.ToLower().Contains($"leg {legNumber}"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public Trend GetMajorBullTrend(List<Candle> higherTimeFrame, DateTime reversalTimeStamp, Candle reversalCandle)
        {
            Trend bullTrend = new Trend();
            var tradingPoint = higherTimeFrame.Where(b => b.TimeStamp < reversalTimeStamp.Date &&
                    (b.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BullishReversal)).OrderBy(b => b.Low).LastOrDefault();



            if (tradingPoint != null)
            {
                bullTrend.TrendContinuationCandle = higherTimeFrame.Where(c => c.TimeStamp < reversalTimeStamp.Date && c.TimeStamp.Date >= tradingPoint.TimeStamp.Date.AddDays(-4)).OrderBy(b => b.Low).FirstOrDefault();

                var NootherReversals = higherTimeFrame.Where(b => b.TimeStamp < reversalTimeStamp.Date
            && b.TimeStamp > bullTrend.TrendContinuationCandle.TimeStamp).Count(c => c.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BullishReversal);
                var currentStatus = higherTimeFrame.Where(b => b.TimeStamp < reversalTimeStamp.Date).OrderBy(a => a.TimeStamp).LastOrDefault().AllIndicators.Stochastic?.OscillatorStatus;
                if (NootherReversals <= 1 && currentStatus == OscillatorStatus.Bullish)
                {
                    var lastCorrectionReversal = higherTimeFrame.Where(b => b.TimeStamp < tradingPoint.TimeStamp &&
                            (b.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BearishReversal && b.AllIndicators.Stochastic?.OscillatorPriceRange == OscillatorPriceRange.Overbought)
                           ).LastOrDefault();
                    if (lastCorrectionReversal != null)
                    {
                        var bullishReversal = higherTimeFrame.Where(b => b.TimeStamp < lastCorrectionReversal.TimeStamp &&
                                (b.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BullishReversal && b.AllIndicators.Stochastic?.OscillatorPriceRange == OscillatorPriceRange.Oversold)
                               ).LastOrDefault();
                        if (bullishReversal != null)
                        {
                            bullTrend.TrendStartCandle = higherTimeFrame.Where(e => e.TimeStamp >= bullishReversal.TimeStamp.AddDays(-4) && e.TimeStamp < lastCorrectionReversal.TimeStamp).OrderBy(a => a.Low).FirstOrDefault();
                            //bullTrend.ReversalCandle = tradingPoint;
                        }
                    }
                }
            }
            if (bullTrend.TrendStartCandle != null && bullTrend.TrendStartCandle.Low <= bullTrend.TrendContinuationCandle.Low)
            {

                return bullTrend;
            }
            else
            {
                return new Trend();
            }
        }

        public Trend GetMajorBearTrend(List<Candle> higherTimeFrame, DateTime reversalTimeStamp, Candle reversalCandle)
        {
            Trend bearTrend = new Trend();
            var tradingPoint = higherTimeFrame.Where(b => b.TimeStamp < reversalTimeStamp.Date &&
                    (b.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BearishReversal && b.AllIndicators.Stochastic?.OscillatorPriceRange == OscillatorPriceRange.Overbought)
                   ).LastOrDefault();

            if (tradingPoint != null)
            {
                bearTrend.TrendContinuationCandle = higherTimeFrame.Where(c => c.TimeStamp < reversalTimeStamp.Date && c.TimeStamp.Date >= tradingPoint.TimeStamp.Date.AddDays(-4)).OrderBy(b => b.High).LastOrDefault();

                var NootherReversals = higherTimeFrame.Where(b => b.TimeStamp < reversalTimeStamp.Date
            && b.TimeStamp > bearTrend.TrendContinuationCandle.TimeStamp).Count(c => c.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BearishReversal);
                var currentStatus = higherTimeFrame.Where(b => b.TimeStamp < reversalTimeStamp.Date).OrderBy(a => a.TimeStamp).LastOrDefault().AllIndicators.Stochastic?.OscillatorStatus;
                if (NootherReversals <= 1 && currentStatus == OscillatorStatus.Bearish)
                {
                    var lastCorrectionReversal = higherTimeFrame.Where(b => b.TimeStamp < tradingPoint.TimeStamp &&
                        (b.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BullishReversal && b.AllIndicators.Stochastic?.OscillatorPriceRange == OscillatorPriceRange.Oversold)
                       ).LastOrDefault();
                    if (lastCorrectionReversal != null)
                    {
                        var bearishReversal = higherTimeFrame.Where(b => b.TimeStamp < lastCorrectionReversal.TimeStamp &&
                                (b.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BearishReversal && b.AllIndicators.Stochastic?.OscillatorPriceRange == OscillatorPriceRange.Overbought)
                                && b.High > tradingPoint.High
                               ).LastOrDefault();
                        if (bearishReversal != null)
                        {
                            bearTrend.TrendStartCandle = higherTimeFrame.Where(e => e.TimeStamp >= bearishReversal.TimeStamp.AddDays(-4) && e.TimeStamp < lastCorrectionReversal.TimeStamp).OrderBy(a => a.High).LastOrDefault();
                            //bearTrend.ReversalCandle = tradingPoint;
                        }
                    }
                }
            }
            if (bearTrend.TrendStartCandle != null && bearTrend.TrendStartCandle.High >= bearTrend.TrendContinuationCandle.High)
            {

                return bearTrend;
            }
            else
            {
                return new Trend();
            }
        }

        public double GetThreshold(int higherTimeFrame)
        {
            double threshold = 0.0;
            switch (higherTimeFrame)
            {
                case 100:
                    threshold = 1;
                    break;
                case 60:
                    threshold = 0.3;
                    break;
                case 200:
                    threshold = 2;
                    break;
                default:
                    threshold = 0.2;
                    break;
            }
            return threshold;
        }

        public int GetFnOQantity(string symbol, double price)
        {
            try
            {
                return this.dsLots.Tables[0].AsEnumerable().Where(a => a.Field<string>("SYMBOL") == symbol).First().Field<int>(dsLots.Tables[0].Columns[2].ColumnName);
            }
            catch (Exception ex)
            {
                return Convert.ToInt32(500000 / price);
            }
        }
        private void LoadFnOTable()
        {
            string FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\fo_mktlots.csv";
            OleDbConnection conn = new OleDbConnection
                   ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                     Path.GetDirectoryName(FileName) +
                     "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

            conn.Open();

            OleDbDataAdapter adapter = new OleDbDataAdapter
                   ("SELECT * FROM " + Path.GetFileName(FileName), conn);


            adapter.Fill(dsLots);
            dsLots.Tables[0].Columns[1].ColumnName = "SYMBOL";

        }

        public List<ABCD> GetAllABCDBearTrend(List<Candle> allData, Candle pointACandle, Candle pointDCandle)
        {
            var allAbcd = new List<ABCD>();
            //var minValue = allData.Where(a => a.TimeStamp >= pointACandle.TimeStamp && a.TimeStamp <= pointDCandle.TimeStamp)?.Min(b => b?.Low)??0;
            var pointA = pointACandle.High;
            //this is point C reversals
            var cBearishReversals = allData.Where(a => a.TimeStamp > pointACandle.TimeStamp.Date && a.TimeStamp < pointDCandle.TimeStamp && a.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal
          .BearishReversal);
            if (cBearishReversals.Count() > 0)
            {
                var bBullishReversals = allData.Where(a => a.TimeStamp > pointACandle.TimeStamp.Date && a.TimeStamp < cBearishReversals.Last().TimeStamp && a.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal
              .BullishReversal);
                double pointCc = 0;
                DateTime pointCTimestamp = DateTime.Now;
                double pointB = 0;
                DateTime pointBTimestamp = DateTime.Now;
                int debugHitCount = 0;
                if (bBullishReversals.Count() > 0)
                {
                    foreach (var bB in bBullishReversals)
                    {

                        var big31 = new List<Candle> { bB, bB.PreviousCandle, bB.PreviousCandle.PreviousCandle };
                        var finalCandle1 = big31.OrderBy(p => p.Low).FirstOrDefault();
                        pointB = finalCandle1.Low;
                        pointBTimestamp = finalCandle1.TimeStamp;

                        foreach (var xx in cBearishReversals.Where(tm => tm.TimeStamp > bB.TimeStamp))
                        {
                            var big3 = new List<Candle> { xx, xx.PreviousCandle, xx.PreviousCandle.PreviousCandle };
                            var finalCandle = big3.OrderBy(p => p.High).LastOrDefault();
                            if (finalCandle.High < pointA)
                            {
                                pointCc = finalCandle.High;
                                pointCTimestamp = finalCandle.TimeStamp;
                            }

                            if (pointA > 0 && pointB > 0 && pointCc > 0)
                            {
                                bool ABC = pointA - pointB <= pointCc - pointDCandle.Low;
                                if (ABC)
                                {

                                    Console.WriteLine($"HitCount {debugHitCount}");
                                    debugHitCount++;
                                    var minBetweenBC = allData.Where(d => d.TimeStamp > pointBTimestamp && d.TimeStamp < pointCTimestamp)?.Min(d => d?.Low) ?? 0;
                                    if (pointB <= minBetweenBC)
                                        allAbcd.Add(new ABCD { A = pointA, ATime = pointACandle.TimeStamp, B = pointB, BTime = pointBTimestamp, C = pointCc, CTime = pointCTimestamp, D = pointDCandle.Low, DTime = pointDCandle.TimeStamp });

                                }
                            }
                        }
                    }
                }
            }
            return allAbcd.OrderBy(a => Math.Abs(Math.Abs(a.A - a.B) - Math.Abs(a.C - a.D))).ToList();
        }

        public List<ABCD> GetAllABCDBullTrend(List<Candle> allData, Candle pointACandle, Candle pointDCandle)
        {
            var allAbcd = new List<ABCD>();

            //this is point C reversals
            var cBullishReversals = allData.Where(a => a.TimeStamp > pointACandle.TimeStamp.Date && a.TimeStamp < pointDCandle.TimeStamp && a.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal
          .BullishReversal);
            if (cBullishReversals.Count() > 0)
            {
                var bBearishReversal = allData.Where(a => a.TimeStamp > pointACandle.TimeStamp.Date && a.TimeStamp < cBullishReversals.Last().TimeStamp && a.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal
              .BearishReversal);
                double pointCc = 0;
                DateTime pointCTimestamp = DateTime.Now;
                double pointB = 0;
                DateTime pointBTimestamp = DateTime.Now;
                if (bBearishReversal.Count() > 0)
                {
                    foreach (var bB in bBearishReversal)
                    {

                        var big31 = new List<Candle> { bB, bB.PreviousCandle, bB.PreviousCandle.PreviousCandle };
                        var finalCandle1 = big31.OrderBy(p => p.High).LastOrDefault();
                        pointB = finalCandle1.High;
                        pointBTimestamp = finalCandle1.TimeStamp;

                        foreach (var xx in cBullishReversals.Where(tm => tm.TimeStamp > bB.TimeStamp))
                        {
                            var big3 = new List<Candle> { xx, xx.PreviousCandle, xx.PreviousCandle.PreviousCandle };
                            var finalCandle = big3.OrderBy(p => p.Low).FirstOrDefault();
                            if (finalCandle.Low > pointACandle.Low)
                            {
                                pointCc = finalCandle.Low;
                                pointCTimestamp = finalCandle.TimeStamp;
                            }

                            if (pointACandle.Low > 0 && pointB > 0 && pointCc > 0)
                            {
                                bool ABC = Math.Abs(pointACandle.Low) - pointB <= Math.Abs(pointCc - pointDCandle.Low);
                                if (ABC)
                                {
                                    var maxBetweenBC = allData.Where(d => d.TimeStamp > pointBTimestamp && d.TimeStamp < pointCTimestamp)?.Max(d => d?.High) ?? double.MaxValue;
                                    if (maxBetweenBC <= pointB)
                                        allAbcd.Add(new ABCD { A = pointACandle.Low, ATime = pointACandle.TimeStamp, B = pointB, BTime = pointBTimestamp, C = pointCc, CTime = pointCTimestamp, D = pointDCandle.High, DTime = pointDCandle.TimeStamp });
                                }
                            }
                        }
                    }
                }
            }

            return allAbcd.OrderBy(a => Math.Abs(Math.Abs(a.A - a.B) - Math.Abs(a.C - a.D))).ToList();
        }
    }
}
