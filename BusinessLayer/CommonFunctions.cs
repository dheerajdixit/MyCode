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
    public enum InternalRetracementType
    {
        R382,
        R500,
        R618,
        R786
    };
    public enum AlternatePriceProjectionType
    {
        R618,
        R100,
        R162
    };
    public enum ExternalRetracementType
    {
        R127,
        R162,
        R262
    };
    public enum SupportType
    {
        InApp,
        InAppEx,
        None,
        InEx
    };
    public class InternalRetracement
    {
        public InternalRetracementType _internalRetracementType { get; set; }
        public double _internalRetracementValue { get; set; }
        public InternalRetracement(InternalRetracementType internalRetracementType, double value)
        {

            _internalRetracementType = internalRetracementType;
            _internalRetracementValue = value;

        }


    }

    public class ExternalRetracement
    {
        public ExternalRetracementType _externalRetracementType { get; set; }
        public double _externalRetracementValue { get; set; }
        public ExternalRetracement(ExternalRetracementType externalRetracementType, double value)
        {

            _externalRetracementType = externalRetracementType;
            _externalRetracementValue = value;

        }
    }

    public class AlternatePriceProjection
    {
        public AlternatePriceProjectionType _alternatePriceProjectionType { get; set; }
        public double _alternatePriceProjectionValue { get; set; }
        public AlternatePriceProjection(AlternatePriceProjectionType alternatePriceProjectionType, double value)
        {

            _alternatePriceProjectionType = alternatePriceProjectionType;
            _alternatePriceProjectionValue = value;

        }
    }
    public class Cloud
    {
        public double Max { get; set; }
        public double Min { get; set; }

        public double Diff { get; set; }

    }
    public class PriceRetracement
    {
        public InternalRetracement InternalRetracement { get; set; }
        public AlternatePriceProjection AlternatePriceProjection { get; set; }
        public ExternalRetracement ExternalRetracement { get; set; }
        public Cloud CloudDifference(SupportType supportType)
        {
            Cloud c = new Cloud();

            switch (supportType)
            {
                case SupportType.InApp:
                    c.Max = Math.Max(this.InternalRetracement._internalRetracementValue, this.AlternatePriceProjection._alternatePriceProjectionValue);
                    c.Min = Math.Min(this.InternalRetracement._internalRetracementValue, this.AlternatePriceProjection._alternatePriceProjectionValue);
                    c.Diff = c.Max - c.Min;
                    break;

                case SupportType.InAppEx:
                    c.Max = Math.Max(Math.Max(this.InternalRetracement._internalRetracementValue, this.ExternalRetracement._externalRetracementValue), this.AlternatePriceProjection._alternatePriceProjectionValue);
                    c.Min = Math.Min(Math.Min(this.InternalRetracement._internalRetracementValue, this.ExternalRetracement._externalRetracementValue), this.AlternatePriceProjection._alternatePriceProjectionValue);
                    c.Diff = c.Max - c.Min;
                    break;

                case SupportType.InEx:
                    c.Max = Math.Max(this.InternalRetracement._internalRetracementValue, this.ExternalRetracement._externalRetracementValue);
                    c.Min = Math.Min(this.InternalRetracement._internalRetracementValue, this.ExternalRetracement._externalRetracementValue);
                    c.Diff = c.Max - c.Min;
                    break;
            }


            return c;

        }
        public SupportType SupportType { get; set; }

    }
    public class InRet
    {
        public double R382 { get; set; }
        public double R500 { get; set; }
        public double R618 { get; set; }
        public double R786 { get; set; }
    }

    public class ExRet
    {
        public double R127 { get; set; }
        public double R162 { get; set; }
        public double R262 { get; set; }

    }

    public class APP
    {
        public double R618 { get; set; }
        public double R100 { get; set; }
        public double R162 { get; set; }

    }

    public class DoubleSupport
    {
        public bool IsDoublySupported { get; set; }
        public string Comment { get; set; }
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
        public TrendBasedFibTime TimeCycle { get; set; }
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

        public List<Trend> GetProbableBullTrends(List<Candle> higherTimeFrame, DateTime reversalTimeStamp, Candle reversalCandle)
        {

            List<Trend> allPossibleTrends = new List<Trend>();
            Trend bullTrend;
            var tradingPoint = higherTimeFrame.Where(b => b.TimeStamp < reversalTimeStamp.Date &&
                    (b.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BearishReversal)).OrderBy(a => a.TimeStamp).LastOrDefault();

            if (tradingPoint != null)
            {
                var listofTradingpoints = higherTimeFrame.Where(b => b.Low < tradingPoint.Low && b.TimeStamp < reversalTimeStamp.Date &&
                    (b.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BullishReversal)).OrderByDescending(b => b.TimeStamp);
                var l = double.MaxValue;
                foreach (var tp in listofTradingpoints)
                {
                    if (tp.Low < l)
                    {
                        bullTrend = new Trend();

                        bullTrend.TrendContinuationCandle = higherTimeFrame.Where(c => c.TimeStamp < reversalTimeStamp.Date && c.TimeStamp.Date >= tradingPoint.TimeStamp.Date.AddDays(-4)).OrderBy(b => b.Low).FirstOrDefault();

                        bullTrend.TrendStartCandle = higherTimeFrame.Where(a => a.TimeStamp >= tp.TimeStamp.AddDays(-4)
                        && a.TimeStamp <= tp.TimeStamp.AddDays(4) && a.TimeStamp.Date < reversalTimeStamp.Date).OrderBy(a => a.Low).FirstOrDefault();
                        var currentlow = bullTrend.TrendStartCandle.Low;

                        var trendFinishtimestamp = higherTimeFrame.Where(a => a.TimeStamp >= bullTrend.TrendStartCandle.TimeStamp && a.TimeStamp <= reversalTimeStamp).OrderBy(b => b.High).LastOrDefault().TimeStamp;
                        var trendTimeFrame = higherTimeFrame.Where(a => a.TimeStamp >= bullTrend.TrendStartCandle.TimeStamp
                       && a.TimeStamp <= trendFinishtimestamp).Count();
                        trendTimeFrame = Convert.ToInt32(trendTimeFrame + (trendTimeFrame * 61.8) / 100);
                        trendFinishtimestamp = higherTimeFrame.Where(a => a.TimeStamp >= bullTrend.TrendStartCandle.TimeStamp && a.TimeStamp <= reversalTimeStamp).OrderBy(b => b.High).LastOrDefault().TimeStamp;
                        trendTimeFrame = higherTimeFrame.Where(a => a.TimeStamp >= bullTrend.TrendStartCandle.TimeStamp
                      && a.TimeStamp <= trendFinishtimestamp).Count();
                        var tbfb = new TrendBasedFibTime();

                        var checkIndex2 = higherTimeFrame.IndexOf(higherTimeFrame.Where(a => a.TimeStamp == trendFinishtimestamp).FirstOrDefault());
                        var candleCount = higherTimeFrame.Count();
                        var idxCycle382 = Convert.ToInt32(checkIndex2 + (trendTimeFrame * 38.2 / 100));
                        var idxCycle500 = Convert.ToInt32(checkIndex2 + (trendTimeFrame * 50.0 / 100));
                        var idxCycle618 = Convert.ToInt32(checkIndex2 + (trendTimeFrame * 61.8 / 100));
                        var idxCycle1000 = Convert.ToInt32(checkIndex2 + (trendTimeFrame * 100.0 / 100));
                        var idxCycle1618 = Convert.ToInt32(checkIndex2 + (trendTimeFrame * 161.8 / 100));
                        if (candleCount > idxCycle382)
                            tbfb.Cycle382 = higherTimeFrame[idxCycle382].TimeStamp;
                        if (candleCount > idxCycle618)
                            tbfb.Cycle618 = higherTimeFrame[idxCycle618].TimeStamp;
                        if (candleCount > idxCycle500)
                            tbfb.Cycle500 = higherTimeFrame[idxCycle500].TimeStamp;
                        if (candleCount > idxCycle1000)
                            tbfb.Cycle1000 = higherTimeFrame[idxCycle1000].TimeStamp;
                        if (candleCount > idxCycle1618)
                            tbfb.Cycle1618 = higherTimeFrame[idxCycle1618].TimeStamp;
                        bullTrend.TimeCycle = tbfb;

                        allPossibleTrends.Add(bullTrend);
                        l = bullTrend.TrendStartCandle.Low;
                    }
                }
            }
            return allPossibleTrends;

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
                var currentPriceRange = higherTimeFrame.Where(b => b.TimeStamp < reversalTimeStamp.Date).OrderBy(a => a.TimeStamp).LastOrDefault().AllIndicators.Stochastic?.OscillatorPriceRange;
                if ((currentPriceRange == OscillatorPriceRange.Oversold))
                {
                    var lastCorrectionReversal = higherTimeFrame.Where(b => b.TimeStamp < tradingPoint.TimeStamp &&
                            (b.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BearishReversal && b.AllIndicators.Stochastic?.OscillatorPriceRange == OscillatorPriceRange.Overbought)
                           ).LastOrDefault();
                    if (lastCorrectionReversal != null)
                    {

                        var bullishReversal = higherTimeFrame.Where(b => b.TimeStamp < lastCorrectionReversal.TimeStamp &&
                                (b.AllIndicators.Stochastic?.OscillatorReversal == OscillatorReversal.BullishReversal && b.AllIndicators.Stochastic?.OscillatorPriceRange == OscillatorPriceRange.Oversold)
                               && b.Low <= bullTrend.TrendContinuationCandle.Low
                               ).LastOrDefault();


                        if (bullishReversal != null)
                        {
                            bullTrend.TrendStartCandle = higherTimeFrame.Where(e => e.TimeStamp >= bullishReversal.TimeStamp.AddDays(-4) && e.TimeStamp < lastCorrectionReversal.TimeStamp).OrderBy(a => a.Low).FirstOrDefault();
                            //bullTrend.ReversalCandle = tradingPoint;
                            var currentlow = bullTrend.TrendStartCandle.Low;

                            var trendFinishtimestamp = higherTimeFrame.Where(a => a.TimeStamp >= bullTrend.TrendStartCandle.TimeStamp && a.TimeStamp <= reversalTimeStamp).OrderBy(b => b.High).LastOrDefault().TimeStamp;
                            var trendTimeFrame = higherTimeFrame.Where(a => a.TimeStamp >= bullTrend.TrendStartCandle.TimeStamp
                           && a.TimeStamp <= trendFinishtimestamp).Count();
                            trendTimeFrame = Convert.ToInt32(trendTimeFrame + (trendTimeFrame * 61.8) / 100);

                            var checkIndex1 = higherTimeFrame.IndexOf(bullTrend.TrendStartCandle);

                            if (checkIndex1 - trendTimeFrame < 0)
                                return new Trend();
                            var checkCandle = higherTimeFrame[checkIndex1 - trendTimeFrame];
                            var checkLowCandle = higherTimeFrame.Where(a => a.TimeStamp < bullTrend.TrendStartCandle.TimeStamp && a.TimeStamp >= checkCandle.TimeStamp).OrderBy(b => b.Low).FirstOrDefault();
                            var checkLow = checkLowCandle.Low;



                            while (checkLow < currentlow)
                            {
                                currentlow = checkLow;
                                bullTrend.TrendStartCandle = checkLowCandle;

                                trendFinishtimestamp = checkLowCandle.TimeStamp;
                                checkIndex1 = higherTimeFrame.IndexOf(checkLowCandle);
                                if (checkIndex1 - trendTimeFrame < 0)
                                    return new Trend();
                                checkCandle = higherTimeFrame[checkIndex1 - trendTimeFrame];
                                checkLowCandle = higherTimeFrame.Where(a => a.TimeStamp < checkLowCandle.TimeStamp && a.TimeStamp >= checkCandle.TimeStamp).OrderBy(b => b.Low).FirstOrDefault();
                                checkLow = checkLowCandle.Low;
                            }

                            trendFinishtimestamp = higherTimeFrame.Where(a => a.TimeStamp >= bullTrend.TrendStartCandle.TimeStamp && a.TimeStamp <= reversalTimeStamp).OrderBy(b => b.High).LastOrDefault().TimeStamp;
                            trendTimeFrame = higherTimeFrame.Where(a => a.TimeStamp >= bullTrend.TrendStartCandle.TimeStamp
                          && a.TimeStamp <= trendFinishtimestamp).Count();
                            var tbfb = new TrendBasedFibTime();

                            var checkIndex2 = higherTimeFrame.IndexOf(higherTimeFrame.Where(a => a.TimeStamp == trendFinishtimestamp).FirstOrDefault());
                            var candleCount = higherTimeFrame.Count();
                            var idxCycle382 = Convert.ToInt32(checkIndex2 + (trendTimeFrame * 38.2 / 100));
                            var idxCycle500 = Convert.ToInt32(checkIndex2 + (trendTimeFrame * 50.0 / 100));
                            var idxCycle618 = Convert.ToInt32(checkIndex2 + (trendTimeFrame * 61.8 / 100));
                            var idxCycle1000 = Convert.ToInt32(checkIndex2 + (trendTimeFrame * 100.0 / 100));
                            var idxCycle1618 = Convert.ToInt32(checkIndex2 + (trendTimeFrame * 161.8 / 100));
                            if (candleCount > idxCycle382)
                                tbfb.Cycle382 = higherTimeFrame[idxCycle382].TimeStamp;
                            if (candleCount > idxCycle618)
                                tbfb.Cycle618 = higherTimeFrame[idxCycle618].TimeStamp;
                            if (candleCount > idxCycle500)
                                tbfb.Cycle500 = higherTimeFrame[idxCycle500].TimeStamp;
                            if (candleCount > idxCycle1000)
                                tbfb.Cycle1000 = higherTimeFrame[idxCycle1000].TimeStamp;
                            if (candleCount > idxCycle1618)
                                tbfb.Cycle1618 = higherTimeFrame[idxCycle1618].TimeStamp;
                            bullTrend.TimeCycle = tbfb;
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

        public AlternatePriceProjection APPVerificaiton(APP app, Candle c, double inRetValue)
        {
            double close = c.Close;

            if (Math.Abs(app.R618 - inRetValue) <= c.ATR * 1.5)
            {
                return new AlternatePriceProjection(AlternatePriceProjectionType.R618, app.R618);
            }
            else if (Math.Abs(app.R100 - inRetValue) <= c.ATR * 1.5)
            {
                return new AlternatePriceProjection(AlternatePriceProjectionType.R100, app.R100);
            }
            else if (Math.Abs(app.R162 - inRetValue) <= c.ATR * 1.5)
            {
                return new AlternatePriceProjection(AlternatePriceProjectionType.R162, app.R162);
            }

            return null;
        }

        public ExternalRetracement ExRetVerification(ExRet exRet, Candle c, double inRetValue)
        {
            double close = c.Close;

            if (Math.Abs(exRet.R127 - inRetValue) <= c.ATR * 1.5)
            {
                return new ExternalRetracement(ExternalRetracementType.R127, exRet.R127);
            }
            else if (Math.Abs(exRet.R162 - inRetValue) <= c.ATR * 1.5)
            {
                return new ExternalRetracement(ExternalRetracementType.R162, exRet.R162);
            }
            else if (Math.Abs(exRet.R262 - inRetValue) <= c.ATR * 1.5)
            {
                return new ExternalRetracement(ExternalRetracementType.R262, exRet.R262);
            }

            return null;
        }

        public ExternalRetracement ExRetVerification(ExRet exRet, Candle c, double inRet, double app)
        {
            double close = c.Close;

            if (Math.Abs(inRet - exRet.R127) <= c.ATR * 1.5
                || Math.Abs(app - exRet.R127) <= c.ATR * 1.5)
            {
                return new ExternalRetracement(ExternalRetracementType.R127, exRet.R127);
            }
            else if (Math.Abs(inRet - exRet.R162) <= c.ATR * 1.5
                || Math.Abs(app - exRet.R162) <= c.ATR * 1.5)
            {
                return new ExternalRetracement(ExternalRetracementType.R162, exRet.R162);
            }
            else if (Math.Abs(inRet - exRet.R262) <= c.ATR * 1.5
                || Math.Abs(app - exRet.R262) <= c.ATR * 1.5)
            {
                return new ExternalRetracement(ExternalRetracementType.R262, exRet.R262);
            }


            return null;
        }

        public InternalRetracement InRetVerificaiton(InRet inRet, Candle c)
        {
            double close = c.Close;

            if (close > inRet.R382)
            {
                return new InternalRetracement(InternalRetracementType.R382, inRet.R382);
            }
            else if (close > inRet.R500)
            {
                return new InternalRetracement(InternalRetracementType.R500, inRet.R500);
            }
            else if (close > inRet.R618)
            {
                return new InternalRetracement(InternalRetracementType.R618, inRet.R618);
            }
            else if (close > inRet.R786)
            {
                return new InternalRetracement(InternalRetracementType.R786, inRet.R786);
            }
            else if (c.High > inRet.R382 && c.Low < inRet.R382)
            {
                return new InternalRetracement(InternalRetracementType.R382, inRet.R382);
            }
            else if (c.High > inRet.R500 && c.Low < inRet.R500)
            {
                return new InternalRetracement(InternalRetracementType.R500, inRet.R500);
            }
            else if (c.High > inRet.R618 && c.Low < inRet.R618)
            {
                return new InternalRetracement(InternalRetracementType.R618, inRet.R618);
            }
            else if (c.High > inRet.R786 && c.Low < inRet.R786)
            {
                return new InternalRetracement(InternalRetracementType.R786, inRet.R786);
            }

            return null;
        }

        public PriceRetracement PriceRetracementAnalysis(InRet inRet, APP app, ExRet exRet, Candle c)
        {
            PriceRetracement priceRetracement = new PriceRetracement();
            priceRetracement.SupportType = SupportType.None;

            //Priority 1 In-Ret
            priceRetracement.InternalRetracement = InRetVerificaiton(inRet, c);

            //priority -2 APP
            if (priceRetracement.InternalRetracement != null)
                priceRetracement.AlternatePriceProjection = APPVerificaiton(app, c, priceRetracement.InternalRetracement._internalRetracementValue);

            //priority -3 Ex-Ret
            if (priceRetracement.AlternatePriceProjection == null && priceRetracement.InternalRetracement != null)
                priceRetracement.ExternalRetracement = ExRetVerification(exRet, c, priceRetracement.InternalRetracement._internalRetracementValue);

            else if (priceRetracement.AlternatePriceProjection != null)
                priceRetracement.ExternalRetracement = ExRetVerification(exRet, c, priceRetracement.InternalRetracement._internalRetracementValue, priceRetracement.AlternatePriceProjection._alternatePriceProjectionValue);

            if ((priceRetracement.InternalRetracement != null && priceRetracement.AlternatePriceProjection != null) && priceRetracement.ExternalRetracement != null)
            {
                var cloud = priceRetracement.CloudDifference(SupportType.InAppEx);
                double diff = cloud.Diff;
                if (diff <= c.ATR * 1.5 && c.Low <= cloud.Max)
                {
                    priceRetracement.SupportType = SupportType.InAppEx;
                }
            }

            if (priceRetracement.SupportType == SupportType.None && priceRetracement.InternalRetracement != null && priceRetracement.AlternatePriceProjection != null)
            {
                var cloud = priceRetracement.CloudDifference(SupportType.InApp);
                if (cloud.Diff <= c.ATR * 1.5 && c.Low <= cloud.Max)
                {
                    priceRetracement.SupportType = SupportType.InApp;
                }
            }

            if (priceRetracement.SupportType == SupportType.None && priceRetracement.InternalRetracement != null && priceRetracement.ExternalRetracement != null)
            {
                var cloud = priceRetracement.CloudDifference(SupportType.InEx);
                if (cloud.Diff <= c.ATR * 1.5 && c.Low <= cloud.Max)
                {
                    priceRetracement.SupportType = SupportType.InEx;
                }
            }


            return priceRetracement;
        }

        public PriceRetracement CheckDoubleSupport(ABCD bc, Candle c, Trend trend, double pointA)
        {

            //retracement
            var trend1 = (pointA - trend.TrendStartCandle.Low);
            var ret382 = trend1 * 38.2 / 100;
            var ret50 = trend1 * 50 / 100;
            var ret618 = trend1 * 61.8 / 100;
            var ret786 = trend1 * 78.6 / 100;


            var pointD = trend.TrendContinuationCandle.Low;
            double diffThreshold = this.GetThreshold(trend.TrendContinuationCandle);

            APP app = new APP();
            app.R100 = bc.C - (pointA - bc.B);
            app.R162 = bc.C - ((pointA - bc.B) + (pointA - bc.B) * 0.62);
            app.R618 = bc.C - ((pointA - bc.B) * 0.618);

            InRet inRet = new InRet();
            inRet.R786 = (pointA - ret786);
            inRet.R618 = (pointA - ret618);
            inRet.R500 = (pointA - ret50);
            inRet.R382 = (pointA - ret382);

            double waveB = Math.Abs(bc.B - bc.C);
            ExRet exRet = new ExRet();
            exRet.R127 = bc.C - (waveB * 1.27);
            exRet.R162 = bc.C - (waveB * 1.62);
            exRet.R262 = bc.C - (waveB * 2.62);

            var pRet = PriceRetracementAnalysis(inRet, app, exRet, c);

            //bool hitRet382 = trend.TrendContinuationCandle.Low <= pointA - ret382 && this.IsNearBy(c.High, (pointA - ret382), trend.TrendContinuationCandle.ATR, c.Low);
            //bool hitRet50 = trend.TrendContinuationCandle.Low <= pointA - ret50 && this.IsNearBy(c.High, (pointA - ret50), trend.TrendContinuationCandle.ATR, c.Low);
            //bool hitRet62 = trend.TrendContinuationCandle.Low <= pointA - ret618 && this.IsNearBy(c.High, (pointA - ret618), trend.TrendContinuationCandle.ATR, c.Low);
            //bool hitRet78 = trend.TrendContinuationCandle.Low <= pointA - ret786 && this.IsNearBy(c.High, (pointA - ret786), trend.TrendContinuationCandle.ATR, c.Low);


            return pRet;
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

        public double GetThreshold(Candle c)
        {
            return ((c.High - c.Low) / c.Close) * 100;
        }

        public int GetFnOQantity(string symbol, double price)
        {
            try
            {
                return Convert.ToInt32(500000 / price);
                //return this.dsLots.Tables[0].AsEnumerable().Where(a => a.Field<string>("SYMBOL") == symbol).First().Field<int>(dsLots.Tables[0].Columns[2].ColumnName);
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

                        var big31 = new List<Candle> { bB, bB.PreviousCandle, bB.PreviousCandle.PreviousCandle, bB.NextCandle, bB.NextCandle.NextCandle, bB.NextCandle.NextCandle.NextCandle };
                        var finalCandle1 = big31.Where(a => a.TimeStamp > pointACandle.TimeStamp).OrderBy(p => p.Low).FirstOrDefault();
                        if (finalCandle1 != null)
                        {
                            pointB = finalCandle1.Low;
                            pointBTimestamp = finalCandle1.TimeStamp;


                            foreach (var xx in cBearishReversals.Where(tm => tm.TimeStamp > pointBTimestamp))
                            {
                                var big3 = new List<Candle> { xx, xx.PreviousCandle, xx.PreviousCandle.PreviousCandle, xx.NextCandle, xx.NextCandle.NextCandle, xx.NextCandle.NextCandle.NextCandle };
                                if (big3 != null && big3.Count() > 0 && big3.All(a => a != null && a.TimeStamp != null))
                                {
                                    var finalCandle = big3.Where(a => a.TimeStamp > pointBTimestamp).OrderBy(p => p.High).LastOrDefault();
                                    if (finalCandle.High < pointA)
                                    {
                                        pointCc = finalCandle.High;
                                        pointCTimestamp = finalCandle.TimeStamp;
                                    }
                                }
                                if (pointA > 0 && pointB > 0 && pointCc > 0 && pointDCandle.Low < pointB)
                                {
                                    // bool ABC = pointA - pointB <= pointCc - pointDCandle.Low;


                                    Console.WriteLine($"HitCount {debugHitCount}");
                                    debugHitCount++;
                                    var minBetweenAC = allData.Where(d => d.TimeStamp > pointACandle.TimeStamp && d.TimeStamp < pointCTimestamp)?.Min(d => d?.Low) ?? 0;
                                    var maxBetweenCD = allData.Where(d => d.TimeStamp > pointCTimestamp && d.TimeStamp < pointDCandle.TimeStamp)?.Max(d => d?.High) ?? Double.MaxValue;
                                    var maxBetweenBC = allData.Where(d => d.TimeStamp <= pointCTimestamp && d.TimeStamp >= pointBTimestamp)?.Max(d => d?.High) ?? Double.MaxValue;
                                    if (pointB <= minBetweenAC && pointCc >= maxBetweenCD && pointCc >= maxBetweenBC)
                                        allAbcd.Add(new ABCD { A = pointA, ATime = pointACandle.TimeStamp, B = pointB, BTime = pointBTimestamp, C = pointCc, CTime = pointCTimestamp, D = pointDCandle.Low, DTime = pointDCandle.TimeStamp });


                                }
                            }
                        }
                    }
                }
            }
            return allAbcd.OrderBy(a => Math.Abs(Math.Abs(a.A - a.B) - Math.Abs(a.C - a.D))).ToList();
        }

        public bool IsNearBy(double high, double target, double c, double low)
        {
            return Math.Abs(high - target) < c || Math.Abs(low - target) < c;
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
                                    var maxBetweenAC = allData.Where(d => d.TimeStamp > pointACandle.TimeStamp && d.TimeStamp < pointCTimestamp)?.Max(d => d?.High) ?? double.MaxValue;

                                    var minBetweenCD = allData.Where(d => d.TimeStamp > pointCTimestamp && d.TimeStamp < pointDCandle.TimeStamp)?.Min(d => d?.Low) ?? 0;
                                    var minBetweenBC = allData.Where(d => d.TimeStamp <= pointCTimestamp && d.TimeStamp >= pointBTimestamp)?.Min(d => d?.Low) ?? 0;
                                    if (maxBetweenAC <= pointB && pointCc <= minBetweenCD && pointCc <= minBetweenBC)
                                    {
                                        allAbcd.Add(new ABCD { A = pointACandle.Low, ATime = pointACandle.TimeStamp, B = pointB, BTime = pointBTimestamp, C = pointCc, CTime = pointCTimestamp, D = pointDCandle.High, DTime = pointDCandle.TimeStamp });
                                    }
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
