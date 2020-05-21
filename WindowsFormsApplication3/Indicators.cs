using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BAL;
using Model;

namespace NSA
{
    class Indicators
    {
        public static void AddSuperTrendIndicator(ref DataSet ds)
        {
            ds.Tables[0].Columns.Add("ATR", typeof(double));
            ds.Tables[0].Columns.Add("ATR7", typeof(double));
            ds.Tables[0].Columns.Add("RENKO", typeof(string));
            ds.Tables[0].Columns.Add("FinalUpperBand", typeof(double));
            ds.Tables[0].Columns.Add("FinalLowerBand", typeof(double));
            ds.Tables[0].Columns.Add("Trend", typeof(double));
            ds.Tables[0].Columns.Add("SuperTrend", typeof(double));


            int xI = 0;
            double avgTR = 0;

            double basicUpperBand = 0;
            double basicLowerBand = 0;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {

                if (xI == 6)
                {

                    avgTR = Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[xI]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[xI]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[xI]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[xI]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"])));
                    dr["ATR7"] = avgTR / 7;
                    basicUpperBand = ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2) - (Convert.ToDouble(dr["ATR7"]) * 3);
                    basicLowerBand = ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2) + (Convert.ToDouble(dr["ATR7"]) * 3);
                    //basicUpperBand < Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalUpperBand"]) ||
                    if (Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"]) > Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalUpperBand"]))
                    {
                        dr["FinalUpperBand"] = Math.Max(basicUpperBand, Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalUpperBand"]));
                    }
                    else
                    {
                        dr["FinalUpperBand"] = basicUpperBand;
                    }
                    //basicLowerBand > Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalLowerBand"]) ||
                    if (Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"]) < Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalLowerBand"]))
                    {
                        dr["FinalLowerBand"] = Math.Min(basicLowerBand, Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalLowerBand"])); ;
                    }
                    else
                    {

                        dr["FinalLowerBand"] = basicLowerBand;
                    }


                    double trend = Convert.ToDouble(ds.Tables[0].Rows[xI]["f2"]) > Convert.ToDouble(dr["FinalLowerBand"]) ? 1 : (Convert.ToDouble(ds.Tables[0].Rows[xI]["f2"]) < Convert.ToDouble(dr["FinalUpperBand"]) ? -1 : Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["Trend"]));
                    if (trend == 1)
                    {
                        dr["SuperTrend"] = dr["FinalUpperBand"];
                    }
                    else
                    {
                        dr["SuperTrend"] = dr["FinalLowerBand"];
                    }
                    dr["trend"] = trend;

                }
                else if (xI > 6)
                {

                    avgTR = Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[xI]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[xI]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[xI]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[xI]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"])));
                    dr["ATR7"] = (Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["ATR7"]) * 6 + avgTR) / 7;
                    basicUpperBand = ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2) - (Convert.ToDouble(dr["ATR7"]) * 3);
                    basicLowerBand = ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2) + (Convert.ToDouble(dr["ATR7"]) * 3);

                    //basicUpperBand < Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalUpperBand"]) ||
                    if (Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"]) > Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalUpperBand"]))
                    {
                        dr["FinalUpperBand"] = Math.Max(basicUpperBand, Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalUpperBand"]));
                    }
                    else
                    {
                        dr["FinalUpperBand"] = basicUpperBand;
                    }
                    //basicLowerBand > Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalLowerBand"]) ||
                    if (Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"]) < Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalLowerBand"]))
                    {
                        dr["FinalLowerBand"] = Math.Min(basicLowerBand, Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalLowerBand"])); ;
                    }
                    else
                    {

                        dr["FinalLowerBand"] = basicLowerBand;
                    }


                    double trend = Convert.ToDouble(ds.Tables[0].Rows[xI]["f2"]) > Convert.ToDouble(dr["FinalLowerBand"]) ? 1 : (Convert.ToDouble(ds.Tables[0].Rows[xI]["f2"]) < Convert.ToDouble(dr["FinalUpperBand"]) ? -1 : Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["Trend"]));
                    if (trend == 1)
                    {
                        dr["SuperTrend"] = dr["FinalUpperBand"];
                    }
                    else
                    {
                        dr["SuperTrend"] = dr["FinalLowerBand"];
                    }
                    dr["trend"] = trend;
                }
                else
                {
                    dr["ATR7"] = 0;
                    dr["FinalUpperBand"] = 0;
                    dr["FinalLowerBand"] = 0;
                    dr["SuperTrend"] = 0;
                    dr["Trend"] = 1;
                    if (xI > 0)
                    {
                        avgTR = avgTR + Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[xI]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[xI]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[xI]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[xI]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"])));
                    }
                    else
                    {
                        avgTR = 0;
                    }
                }
                xI++;

            }
            ds.Tables[0].AcceptChanges();

            ds.Tables[0].Columns.Remove("ATR");
            ds.Tables[0].Columns.Remove("ATR7");
            ds.Tables[0].Columns.Remove("RENKO");
            ds.Tables[0].Columns.Remove("FinalUpperBand");
            ds.Tables[0].Columns.Remove("FinalLowerBand");

        }

        public static void AddIndicators(ref DataSet ds, List<Indicator> listofIndicators, string timeStampforRenko = "")
        {
            bool macdInd = false;
            bool superTrend = false;
            bool rsi = false;
            bool sma20 = false;
            bool sma50 = false;
            bool sma200 = false;

            bool sma10 = false;
            bool sma30 = false;
            bool sma100 = false;
            bool renko = false;
            bool alligator = false;
            bool adx = false;
            //add columns
            foreach (Indicator ind in listofIndicators)
            {

                if (ind.IndicatorName == "SuperTrend")
                {
                    superTrend = true;
                    ds.Tables[0].Columns.Add("ATR7", typeof(double));
                    ds.Tables[0].Columns.Add("FinalUpperBand", typeof(double));
                    ds.Tables[0].Columns.Add("FinalLowerBand", typeof(double));
                    ds.Tables[0].Columns.Add("Trend", typeof(double));
                    ds.Tables[0].Columns.Add("SuperTrend", typeof(double));
                    ds.Tables[0].Columns.Add("ATR14", typeof(string));

                }
                if (ind.IndicatorName == "MACD")
                {
                    macdInd = true;
                    ds.Tables[0].Columns.Add("macd", typeof(double));
                    ds.Tables[0].Columns.Add("macd9", typeof(double));
                    ds.Tables[0].Columns.Add("Histogram", typeof(double));
                }

                if (ind.IndicatorName == "RSI")
                {
                    rsi = true;
                    ds.Tables[0].Columns.Add("RSI14", typeof(double));
                }

                if (ind.IndicatorName == "SMA20")
                {
                    sma20 = true;
                    ds.Tables[0].Columns.Add("20", typeof(double));
                }

                if (ind.IndicatorName == "SMA50")
                {
                    sma50 = true;
                    ds.Tables[0].Columns.Add("50", typeof(double));
                }
                if (ind.IndicatorName == "SMA200")
                {
                    sma200 = true;
                    ds.Tables[0].Columns.Add("200", typeof(double));
                }
                if (ind.IndicatorName == "SMA30")
                {
                    sma30 = true;
                    ds.Tables[0].Columns.Add("30", typeof(double));
                }
                if (ind.IndicatorName == "SMA100")
                {
                    sma100 = true;
                    ds.Tables[0].Columns.Add("100", typeof(double));
                }
                if (ind.IndicatorName == "SMA10")
                {
                    sma10 = true;
                    ds.Tables[0].Columns.Add("10", typeof(double));
                }
                if (ind.IndicatorName == "RENKO")
                {
                    renko = true;
                    ds.Tables[0].Columns.Add("ATR", typeof(double));
                    ds.Tables[0].Columns.Add("RENKO", typeof(string));
                }
                if (ind.IndicatorName == "ALLIGATOR")
                {
                    alligator = true;
                    ds.Tables[0].Columns.Add("L", typeof(double));
                    ds.Tables[0].Columns.Add("LOffset", typeof(double));
                    ds.Tables[0].Columns.Add("M", typeof(double));
                    ds.Tables[0].Columns.Add("MOffset", typeof(double));
                    ds.Tables[0].Columns.Add("S", typeof(double));
                    ds.Tables[0].Columns.Add("SOffset", typeof(double));
                    ds.Tables[0].Columns.Add("AC", typeof(double));
                }
                if (ind.IndicatorName == "ADX")
                {
                    adx = true;
                    ds.Tables[0].Columns.Add("pDi", typeof(double));
                    ds.Tables[0].Columns.Add("mDi", typeof(double));
                    ds.Tables[0].Columns.Add("ADX", typeof(double));

                }
            }

            //general
            int ii = 0;
            //super trend
            double avgTR = 0;
            double avgTRenko = 0;
            double basicUpperBand = 0;
            double basicLowerBand = 0;
            double smma = 0, sma = 0;
            double smma8 = 0, sma8 = 0;
            double smma5 = 0, sma5 = 0;
            //macd
            double pEMA9 = 0, pEMA12 = 0, pEMA26 = 0, macd = 0;

            IMovingAverage avg20 = new SimpleMovingAverage(20);
            IMovingAverage avg50 = new SimpleMovingAverage(50);
            IMovingAverage avg200 = new SimpleMovingAverage(200);
            IMovingAverage avg10 = new SimpleMovingAverage(10);
            IMovingAverage avg30 = new SimpleMovingAverage(30);
            IMovingAverage avg100 = new SimpleMovingAverage(100);


            IMovingAverage avg9 = new SimpleMovingAverage(9);
            IMovingAverage avg12 = new SimpleMovingAverage(12);
            IMovingAverage avg26 = new SimpleMovingAverage(26);

            IMovingAverage rsiGain14 = new SimpleMovingAverage(14);
            IMovingAverage rsiLoss14 = new SimpleMovingAverage(14);

            IMovingAverage avg5 = new SimpleMovingAverage(5);
            IMovingAverage avg34 = new SimpleMovingAverage(34);
            IMovingAverage avgAO5 = new SimpleMovingAverage(5);

            //alligator
            IMovingAverage avg13 = new SimpleMovingAverage(13);
            IMovingAverage avg8 = new SimpleMovingAverage(8);
            double[] close = new double[ds.Tables[0].Rows.Count];
            double[] pDI = new double[ds.Tables[0].Rows.Count];
            double[] mDI = new double[ds.Tables[0].Rows.Count];
            double[] adxV = new double[ds.Tables[0].Rows.Count];


            //calculate values
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                if (alligator)
                {
                    avg5.AddSample((float)(Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2);
                    avg34.AddSample((float)(Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2);
                    avgAO5.AddSample(avg5.Average - avg34.Average);
                    avg13.AddSample((float)(Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2);
                    avg8.AddSample((float)(Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2);

                    if (ii == 14)
                    {
                        sma = avg13.Average;
                        dr["L"] = sma;
                    }
                    else if (ii > 14)
                    {
                        smma = (sma * 12 + ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2)) / 13;
                        dr["L"] = smma;
                        sma = smma;
                    }

                    if (ii == 9)
                    {
                        sma8 = avg8.Average;
                        dr["M"] = sma8;
                    }
                    else if (ii > 9)
                    {
                        smma8 = (sma8 * 7 + ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2)) / 8;
                        dr["M"] = smma8;
                        sma8 = smma8;
                    }

                    if (ii == 6)
                    {
                        sma5 = avg5.Average;
                        dr["S"] = sma5;
                    }
                    else if (ii > 6)
                    {
                        smma5 = (sma5 * 4 + ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2)) / 5;
                        dr["S"] = smma5;
                        sma5 = smma5;
                    }


                    if (ii >= 9)
                    {
                        dr["LOffset"] = ds.Tables[0].Rows[ii - 9]["L"];
                    }

                    if (ii >= 6)
                    {
                        dr["MOffset"] = ds.Tables[0].Rows[ii - 6]["M"];
                    }

                    if (ii >= 4)
                    {
                        dr["SOffset"] = ds.Tables[0].Rows[ii - 4]["S"];
                    }

                    dr["AC"] = (avg5.Average - avg34.Average) - avgAO5.Average;
                }
                if (renko)
                {
                    if (ii == 13)
                    {
                        avgTRenko = Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"])));
                        dr["ATR"] = avgTRenko / 14;

                    }
                    else if (ii > 13)
                    {
                        dr["ATR"] = (Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["ATR"]) * 13 + avgTRenko) / 14;
                        avgTRenko = Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"])));
                    }
                    else if (ii > 0)
                    {
                        avgTR = avgTR + Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"])));
                    }
                    else
                    {
                        avgTR = 0;
                    }


                }
                if (true)
                {
                    if (ii == 13)
                    {
                        avgTR = Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"])));
                        dr["ATR14"] = avgTR / 14;

                    }
                    else if (ii > 13)
                    {
                        avgTR = Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"])));
                        dr["ATR14"] = (Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["ATR7"]) * 13 + avgTR) / 14;

                    }
                    else
                    {
                        dr["ATR14"] = 0;

                        if (ii > 0)
                        {
                            avgTR = avgTR + Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"])));
                        }
                        else
                        {
                            avgTR = 0;
                        }
                    }


                }
                if (superTrend)
                {

                    if (ii == 6)
                    {
                        avgTR = Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"])));
                        dr["ATR7"] = avgTR / 7;
                        basicUpperBand = ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2) - (Convert.ToDouble(dr["ATR7"]) * 3);
                        basicLowerBand = ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2) + (Convert.ToDouble(dr["ATR7"]) * 3);
                        //basicUpperBand < Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalUpperBand"]) ||
                        if (Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]) > Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["FinalUpperBand"]))
                        {
                            dr["FinalUpperBand"] = Math.Max(basicUpperBand, Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["FinalUpperBand"]));
                        }
                        else
                        {
                            dr["FinalUpperBand"] = basicUpperBand;
                        }
                        //basicLowerBand > Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalLowerBand"]) ||
                        if (Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]) < Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["FinalLowerBand"]))
                        {
                            dr["FinalLowerBand"] = Math.Min(basicLowerBand, Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["FinalLowerBand"])); ;
                        }
                        else
                        {

                            dr["FinalLowerBand"] = basicLowerBand;
                        }

                        double trend = Convert.ToDouble(ds.Tables[0].Rows[ii]["f2"]) > Convert.ToDouble(dr["FinalLowerBand"]) ? 1 : (Convert.ToDouble(ds.Tables[0].Rows[ii]["f2"]) < Convert.ToDouble(dr["FinalUpperBand"]) ? -1 : Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["Trend"]));
                        if (trend == 1)
                        {
                            dr["SuperTrend"] = dr["FinalUpperBand"];
                        }
                        else
                        {
                            dr["SuperTrend"] = dr["FinalLowerBand"];
                        }
                        dr["trend"] = trend;
                    }
                    else if (ii > 6)
                    {
                        avgTR = Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"])));
                        dr["ATR7"] = (Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["ATR7"]) * 6 + avgTR) / 7;
                        basicUpperBand = ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2) - (Convert.ToDouble(dr["ATR7"]) * 3);
                        basicLowerBand = ((Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"])) / 2) + (Convert.ToDouble(dr["ATR7"]) * 3);

                        //basicUpperBand < Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalUpperBand"]) ||
                        if (Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]) > Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["FinalUpperBand"]))
                        {
                            dr["FinalUpperBand"] = Math.Max(basicUpperBand, Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["FinalUpperBand"]));
                        }
                        else
                        {
                            dr["FinalUpperBand"] = basicUpperBand;
                        }
                        //basicLowerBand > Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["FinalLowerBand"]) ||
                        if (Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]) < Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["FinalLowerBand"]))
                        {
                            dr["FinalLowerBand"] = Math.Min(basicLowerBand, Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["FinalLowerBand"])); ;
                        }
                        else
                        {

                            dr["FinalLowerBand"] = basicLowerBand;
                        }


                        double trend = Convert.ToDouble(ds.Tables[0].Rows[ii]["f2"]) > Convert.ToDouble(dr["FinalLowerBand"]) ? 1 : (Convert.ToDouble(ds.Tables[0].Rows[ii]["f2"]) < Convert.ToDouble(dr["FinalUpperBand"]) ? -1 : Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["Trend"]));
                        if (trend == 1)
                        {
                            dr["SuperTrend"] = dr["FinalUpperBand"];
                        }
                        else
                        {
                            dr["SuperTrend"] = dr["FinalLowerBand"];
                        }
                        dr["trend"] = trend;
                    }
                    else
                    {
                        dr["ATR7"] = 0;
                        dr["FinalUpperBand"] = 0;
                        dr["FinalLowerBand"] = 0;
                        dr["SuperTrend"] = 0;
                        dr["Trend"] = 1;
                        if (ii > 0)
                        {
                            avgTR = avgTR + Math.Max(Math.Max(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f3"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"]))), Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[ii]["f4"]) - Convert.ToDouble(ds.Tables[0].Rows[ii - 1]["f2"])));
                        }
                        else
                        {
                            avgTR = 0;
                        }
                    }


                }
                ii++;
                if (macdInd)
                {


                    if (ii == 13)
                    {
                        pEMA12 = avg12.Average;
                    }
                    else if (ii > 13)
                    {
                        double ema = 0.153 * (Convert.ToDouble(dr["f2"]) - pEMA12) + pEMA12;
                        pEMA12 = ema;
                    }

                    if (ii == 27)
                    {
                        pEMA26 = avg26.Average;
                    }
                    else if (ii > 27)
                    {
                        double ema = 0.074 * (Convert.ToDouble(dr["f2"]) - pEMA26) + pEMA26;
                        pEMA26 = ema;
                        macd = pEMA12 - pEMA26;
                    }



                    if (ii == 37)
                    {
                        pEMA9 = avg9.Average;
                    }
                    else if (ii > 37)
                    {
                        double ema = 0.2 * (macd - pEMA9) + pEMA9;
                        pEMA9 = ema;
                    }

                    if (ii > 27 && ii < 37)
                    {
                        avg9.AddSample((float)(macd));
                    }

                    avg12.AddSample((float)Convert.ToDouble(dr["f2"]));
                    avg26.AddSample((float)Convert.ToDouble(dr["f2"]));


                    dr["macd"] = macd;
                    dr["macd9"] = pEMA9;
                    dr["Histogram"] = macd - pEMA9;
                }
                if (rsi)
                {
                    dr["rsi14"] = 0;
                    if (ii > 1)
                    {
                        if ((double)dr["f2"] > (double)ds.Tables[0].Rows[ii - 2]["f2"])
                        {
                            rsiGain14.AddSample((float)((double)dr["f2"] - (double)ds.Tables[0].Rows[ii - 2]["f2"]));
                            rsiLoss14.AddSample(0);
                        }
                        else if ((double)ds.Tables[0].Rows[ii - 2]["f2"] > (double)dr["f2"])
                        {
                            rsiLoss14.AddSample((float)((double)ds.Tables[0].Rows[ii - 2]["f2"] - (double)dr["f2"]));
                            rsiGain14.AddSample(0);
                        }
                        else
                        {
                            rsiGain14.AddSample(0);
                            rsiLoss14.AddSample(0);
                        }

                        dr["rsi14"] = 100 - (100 / (1 + (rsiGain14.Average / rsiLoss14.Average)));

                    }
                }

                if (sma10)
                {
                    avg10.AddSample((float)Convert.ToDouble(dr["f2"]));
                    dr["10"] = avg10.Average;
                }

                if (sma30)
                {
                    avg30.AddSample((float)Convert.ToDouble(dr["f2"]));
                    dr["30"] = avg30.Average;
                }

                if (sma100)
                {
                    avg100.AddSample((float)Convert.ToDouble(dr["f2"]));
                    dr["100"] = avg100.Average;
                }


                if (sma20)
                {
                    avg20.AddSample((float)Convert.ToDouble(dr["f2"]));
                    dr["20"] = avg20.Average;
                }

                if (sma50)
                {
                    avg50.AddSample((float)Convert.ToDouble(dr["f2"]));
                    dr["50"] = avg50.Average;
                }

                if (sma200)
                {
                    avg200.AddSample((float)Convert.ToDouble(dr["f2"]));
                    dr["200"] = avg200.Average;
                }
            }

            if (renko)
            {
                string Direction = string.Empty;
                double renkoStartPrice = 0;
                double negativeMovementValue = 0;
                double positiveMovementValue = 0;
                double boxSize = Convert.ToDouble(ds.Tables[0].Select("TimeStamp='" + timeStampforRenko + "'")[0]["ATR"]);
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (Direction == string.Empty && dr["Candle"].ToString() == "R")
                    {
                        Direction = "S";
                        renkoStartPrice = Convert.ToDouble(dr["f2"]);
                    }
                    if (Direction == string.Empty && dr["Candle"].ToString() == "G")
                    {
                        Direction = "B";
                        renkoStartPrice = Convert.ToDouble(dr["f2"]);
                    }

                    if (Direction == "S")
                    {
                        if (negativeMovementValue == 0)
                        {
                            negativeMovementValue = renkoStartPrice + 2 * boxSize;
                            positiveMovementValue = renkoStartPrice - boxSize;
                        }
                    }
                    else if (Direction == "B")
                    {
                        if (positiveMovementValue == 0)
                        {
                            negativeMovementValue = renkoStartPrice - 2 * boxSize;
                            positiveMovementValue = renkoStartPrice + boxSize;
                        }
                    }

                    if (Direction == "S")
                    {
                        if (Convert.ToDouble(dr["f2"]) <= positiveMovementValue)
                        {
                            dr["RENKO"] = "R";
                            negativeMovementValue = negativeMovementValue - boxSize;
                            positiveMovementValue = positiveMovementValue - boxSize;
                        }
                        else if (Convert.ToDouble(dr["f2"]) >= negativeMovementValue)
                        {
                            if (Convert.ToDouble(dr["f2"]) >= negativeMovementValue + boxSize)
                            {
                                dr["RENKO"] = "MG";
                            }
                            else
                            {
                                dr["RENKO"] = "G";
                            }

                            Direction = "B";
                            negativeMovementValue = negativeMovementValue - boxSize - boxSize;
                            positiveMovementValue = positiveMovementValue + boxSize + boxSize + boxSize + boxSize;
                        }
                        else
                        {
                            dr["RENKO"] = "R";
                        }
                    }
                    else if (Direction == "B")
                    {
                        if (Convert.ToDouble(dr["f2"]) >= positiveMovementValue)
                        {
                            dr["RENKO"] = "G";
                            negativeMovementValue = negativeMovementValue + boxSize;
                            positiveMovementValue = positiveMovementValue + boxSize;

                        }
                        else if (Convert.ToDouble(dr["f2"]) <= negativeMovementValue)
                        {
                            if (Convert.ToDouble(dr["f2"]) <= negativeMovementValue - boxSize)
                            {
                                dr["RENKO"] = "MR";
                            }
                            else
                            {
                                dr["RENKO"] = "R";
                            }
                            Direction = "S";
                            dr["RENKO"] = "R";
                            negativeMovementValue = negativeMovementValue + boxSize + boxSize;
                            positiveMovementValue = positiveMovementValue - boxSize - boxSize - boxSize - boxSize;
                        }
                        else
                        {
                            dr["RENKO"] = "G";
                        }
                    }
                }
            }
            //accept changes
            ds.Tables[0].AcceptChanges();
            //remove columns

            if (superTrend)
            {
                ds.Tables[0].Columns.Remove("ATR7");
                ds.Tables[0].Columns.Remove("FinalUpperBand");
                ds.Tables[0].Columns.Remove("FinalLowerBand");
            }
            if (macdInd)
            {
                //ds.Tables[0].Columns.Remove("macd");
                //ds.Tables[0].Columns.Remove("macd9");
            }

        }

     

        public static void ProvideHeikenashiForAdd(DataSet ds)
        {

            ds.Tables[0].Columns.Add("CandleH", typeof(string));
            //ds.Tables[0].Columns.Add("Candle", typeof(string));
            ds.Tables[0].Columns.Add("xC", typeof(double));
            ds.Tables[0].Columns.Add("xH", typeof(double));
            ds.Tables[0].Columns.Add("xL", typeof(double));
            ds.Tables[0].Columns.Add("xO", typeof(double));
            ds.Tables[0].AcceptChanges();


            double xH = 0, xLow = 0, xO = 0, xC = 0, pxH = 0, pxL = 0, pxO = 0, pxC = 0;
            int xI = 0;
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                if (xI == 0)
                {
                    xC = Convert.ToDouble(dr["f2"]);
                    xH = Convert.ToDouble(dr["f3"]);
                    xLow = Convert.ToDouble(dr["f4"]);
                    xO = Convert.ToDouble(dr["f5"]);
                }
                else
                {
                    pxH = Convert.ToDouble(ds.Tables[0].Rows[xI - 1]["f2"]);

                    xC = (Convert.ToDouble(dr["f2"]) + Convert.ToDouble(dr["f3"]) + Convert.ToDouble(dr["f4"]) + Convert.ToDouble(dr["f5"])) / 4;
                    xO = (pxO + pxC) / 2;
                    xLow = Math.Min(Math.Min(xC, xO), Convert.ToDouble(dr["f4"]));
                    xH = Math.Max(Math.Max(xC, xO), Convert.ToDouble(dr["f3"]));
                }
                if (xLow == xO)
                {
                    dr["CandleH"] = "G";
                }
                else if (xH == xO)
                {
                    dr["CandleH"] = "R";
                }
                else if (xC > xO)
                {
                    dr["CandleH"] = "GD";
                }
                else if (xC > xO)
                {
                    dr["CandleH"] = "RD";
                }

                xI++;
                pxC = xC;
                pxH = xH;
                pxL = xLow;
                pxO = xO;

            }
            ds.Tables[0].AcceptChanges();
        }




    }
}
