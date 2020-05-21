using Microsoft.VisualBasic;
using NetTrader.Indicator;
using Newtonsoft.Json.Linq;
using NSA;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BAL;
using Model;

public class TokenChannel : IDisposable
{


    public delegate void WriteToLogEventHandler(int ServerityLevel, string LogText, string Method);
    public delegate void UpdateStatusEventHandler(int tokenslno, string channelId, decimal tokenregistrationid, string StatusText);

    private Equity _tntReq;
    private Thread _thdWork;
    private bool _stop = false;
    private System.Threading.ManualResetEvent _manualResetEvent = new System.Threading.ManualResetEvent(false);
    private string _inUse;
    private string _channelId;



    public string ChannelId
    {
        get
        {
            return _channelId;
        }
    }


    public string InUse
    {
        get
        {
            return _inUse;
        }
        set
        {
            _inUse = value;
        }
    }

    public StockData ChannelProcessData
    {
        get;
        set;
    }


    public TokenChannel(string threadname)
    {
        _channelId = threadname;
        _thdWork = new System.Threading.Thread(Work);
        // _thdWork.Name = "TokenChannel"
        _thdWork.Name = threadname;
        _thdWork.IsBackground = true;
        _thdWork.Start();
    }


    public void ProcessTnT(Equity tntMessage)
    {
        _inUse = "True";
        _tntReq = tntMessage;
        _manualResetEvent.Set();
    }


    private void Work()
    {
        while (!_stop == true)
        {
            _manualResetEvent.WaitOne();
            DoTnT();
            _inUse = "False";
            _manualResetEvent.Reset();

            Thread.Sleep(10);
        }
    }


    private void DoTnT()
    {
        try
        {
            _tntReq.TokenRegStartTime = DateTime.Now;
            if (_tntReq.Category == 1)
            {
                Process5MinutePSAAStocks();
            }
            else if (_tntReq.Category == 2)
            {
                PreapareFiles5();
            }
            else if (_tntReq.Category == 3)
            {
                // Process15MinutesSuperTrendStocks();
            }
            else if (_tntReq.Category == 4)
            {
                Process30MinutesStocks();
            }
            else if (_tntReq.Category == 5)
            {
                ProcessHourlyStocks();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            _tntReq = null;
        }
    }


    private void ProcessHourlyStocks()
    {
        try
        {
            //  CalculateIndicators60();
            Thread.Sleep(100);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void Process30MinutesStocks()
    {
        try
        {
            // CalculateIndicators30();
            Thread.Sleep(100);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    private void Process5MinutePSAAStocks()
    {
        try
        {
            //CalculateIndicators();
            Thread.Sleep(100);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    private void PreapareFiles5()
    {
        try
        {
            // CalculateIndicatorsLoadOnce();
            Thread.Sleep(100);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public static StockData Process15MinutesSuperTrendStocks(Equity _tntReq)
    {
        try
        {
            DataTable dt = null;
            string FileName = string.Empty;
            FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\Data\15\" + _tntReq.Name + ".csv";


            OleDbConnection conn = new OleDbConnection
                   ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                     Path.GetDirectoryName(FileName) +
                     "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

            conn.Open();

            OleDbDataAdapter adapter = new OleDbDataAdapter
                   ("SELECT * FROM " + Path.GetFileName(FileName), conn);

            DataSet ds = new DataSet("Temp");
            adapter.Fill(ds);

            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Columns[0].ColumnName = "Period";
            ds.Tables[0].Columns[0].Caption = "Period";
            ds.Tables[0].AcceptChanges();
            ds.Tables[0].Columns.Add("Candle", typeof(string), "IIF([f2] > [f5],'G',IIF([f2] = [f5],'D','R'))");
            conn.Close();

            var rows1 = ds.Tables[0].Select("f2 = f3 and f2=f4 and f2= f5");
            int count = rows1.Count();
            foreach (var row in rows1)
                row.Delete();

            ds.Tables[0].AcceptChanges();


            int backTestDay = _tntReq.backTestDay;
            dt = ds.Tables[0];
            ds.Tables[0].AcceptChanges();

            Indicator ind1 = new Indicator();
            ind1.IndicatorName = "SuperTrend";
            List<Indicator> xInd = new List<Indicator>();
            xInd.Add(ind1);
            Indicators.AddIndicators(ref ds, xInd);


            int startOfTheWeekIndex = 0;

            //need to commented while running

            int cont = 0;

            for (int i = ds.Tables[0].Rows.Count - 1; i > ds.Tables[0].Rows.Count - 10000; i--)
            {
                if (string.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[i][0])))
                {
                    startOfTheWeekIndex = i;
                    if (backTestDay == cont)
                    {
                        break;
                    }
                    else
                    {
                        cont++;
                    }
                }
                else if (string.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[i - 1][0])))
                {
                    startOfTheWeekIndex = i - 1;
                    if (backTestDay == cont)
                    {

                        break;
                    }
                    else
                    {
                        cont++;
                        i = i - 1;
                    }

                }
                else if (Math.Abs(Convert.ToInt32(ds.Tables[0].Rows[i]["period"]) - Convert.ToInt32(ds.Tables[0].Rows[i - 1]["period"])) > 5)
                {
                    startOfTheWeekIndex = i;
                    if (backTestDay == cont)
                    {
                        break;
                    }
                    else
                    {
                        cont++;
                    }
                }
            }



            int xL = startOfTheWeekIndex - 1;
            for (int deleteIndex = xL + _tntReq.TestAtMinute + 3; deleteIndex < ds.Tables[0].Rows.Count; deleteIndex++)
            {
                ds.Tables[0].Rows[deleteIndex].Delete();
            }
            ds.Tables[0].AcceptChanges();


            //list.Add(new SR { price = 0, LevelName = "D20MA" });
            startOfTheWeekIndex = startOfTheWeekIndex + _tntReq.TestAtMinute / 3;

            List<SR> list = new List<SR>();
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.dPP, 1), LevelName = "dPP" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.dR1, 1), LevelName = "dR1" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.dR2, 1), LevelName = "dR2" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.dR3, 1), LevelName = "dR3" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.dS1, 1), LevelName = "dS1" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.dS2, 1), LevelName = "dS2" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.dS3, 1), LevelName = "dS3" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.wPP, 1), LevelName = "wPP" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.wR1, 1), LevelName = "wR1" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.wR2, 1), LevelName = "wR2" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.wR3, 1), LevelName = "wR3" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.wS1, 1), LevelName = "wS1" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.wS2, 1), LevelName = "wS2" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.wS3, 1), LevelName = "wS3" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.mPP, 1), LevelName = "mPP" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.mR1, 1), LevelName = "mR1" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.mR2, 1), LevelName = "mR2" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.mR3, 1), LevelName = "mR3" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.mS1, 1), LevelName = "mS1" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.mS2, 1), LevelName = "mS2" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.mS3, 1), LevelName = "mS3" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.yPP, 1), LevelName = "yPP" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.yR1, 1), LevelName = "yR1" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.yR2, 1), LevelName = "yR2" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.yR3, 1), LevelName = "yR3" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.yS1, 1), LevelName = "yS1" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.yS2, 1), LevelName = "yS2" });
            list.Add(new SR { price = Math.Round(_tntReq.todaysLevel.yS3, 1), LevelName = "yS3" });
            //    list.Add(new SR { price = Math.Round(Convert.ToDouble(drFirst["SuperTrend"]), 2), LevelName = "ST15" });

            double low = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f4"]);
            double high = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f3"]);
            double close = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f2"]);
            double open = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f5"]);

            list = WildAnalysis(Math.Round(low, 1), Math.Round(high, 1), Math.Round(close, 1), 0, list);


            if (list.Where(a => a.LevelName.Contains("ST15")).Count() > 0 && close > list.Where(a => a.LevelName.Contains("ST15")).First().price && close > open)
            {
                return new StockData() { Direction = "BM", Quantity = Convert.ToInt32(0), Risk = (Math.Abs(close - list.Where(a => a.LevelName.Contains("ST15")).First().price) / close) * 100, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("w")).First().price, Levels = null, Symbol = _tntReq.Name, Category = 3 };
            }
            else if (list.Where(a => a.LevelName.Contains("ST15")).Count() > 0 && close < list.Where(a => a.LevelName.Contains("ST15")).First().price && open > close)
            {
                return new StockData() { Direction = "SM", Quantity = Convert.ToInt32(0), Risk = (Math.Abs(close - list.Where(a => a.LevelName.Contains("ST15")).First().price) / close) * 100, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("w")).First().price, Levels = null, Symbol = _tntReq.Name, Category = 3 };
            }
        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
        }
        return null;
    }





    public static double DiffToClose(double v, double c)
    {
        return Math.Round((Math.Abs(v - c) / c) * 100);
    }

    public static StringBuilder sbArff = new StringBuilder();











    public static int GetWeekOfMonth(DateTime date)
    {
        DateTime beginningOfMonth = new DateTime(date.Year, date.Month, 1);

        while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
            date = date.AddDays(1);

        return (int)Math.Truncate((double)date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;
    }

    public static DateTime GetTimeStamp(int tam, DateTime tradingDate, int period = 5)
    {
        return tradingDate.AddHours(9).AddMinutes(15).AddMinutes((tam + 3) * 5).AddMinutes(0 - period);
    }

    public static int GetMinuteNumber(DateTime tradingDate)
    {
        System.TimeZoneInfo INDIAN_ZONE = System.TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        DateTime CurrentTime = System.TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        // DateTime CurrentTime = DateTime.Now;
        CurrentTime = CurrentTime.AddSeconds(-CurrentTime.Second);
        TimeSpan t = CurrentTime - tradingDate.AddHours(9).AddMinutes(15);
        int totalNumberOfCandles = Convert.ToInt32(t.TotalMinutes) / 5;
        return totalNumberOfCandles - 2;
    }

    public static int GetMinuteNumber(DateTime tradingDate, DateTime orderDate)
    {
        // DateTime CurrentTime = DateTime.Now;
        orderDate = orderDate.AddSeconds(-orderDate.Second);
        TimeSpan t = orderDate - tradingDate.AddHours(9).AddMinutes(15);
        int totalNumberOfCandles = Convert.ToInt32(t.TotalMinutes) / 5;
        return totalNumberOfCandles - 2;
    }

    public static DateTime GetTimeStamp30(int tam, DateTime tradingDate)
    {
        return tradingDate.AddHours(9).AddMinutes(15).AddMinutes((tam + 3) * 5).AddMinutes(-30);
    }

    public static DateTime GetTimeStamp60(int tam, DateTime tradingDate)
    {
        return tradingDate.AddHours(9).AddMinutes(15).AddMinutes((tam + 3) * 5).AddMinutes(-60);
    }

    public static DateTime GetTimeStamp15(int tam, DateTime tradingDate)
    {
        return tradingDate.AddHours(9).AddMinutes(15).AddMinutes((tam + 3) * 5).AddMinutes(-15);
    }



    public static StockData CalculateIndicators60(Equity _tntReq)
    {
        try
        {
            string scrip = _tntReq.Name;
            StockData todaysLevel = _tntReq.todaysLevel;
            DataTable dt = null;
            string FileName = string.Empty;
            FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\Data\Hourly\" + scrip.Replace("-", string.Empty) + ".csv";


            OleDbConnection conn = new OleDbConnection
                   ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                     Path.GetDirectoryName(FileName) +
                     "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

            conn.Open();

            OleDbDataAdapter adapter = new OleDbDataAdapter
                   ("SELECT * FROM " + Path.GetFileName(FileName), conn);

            DataSet ds = new DataSet("Temp");
            adapter.Fill(ds);

            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Columns[0].ColumnName = "Period";
            ds.Tables[0].Columns[0].Caption = "Period";
            ds.Tables[0].AcceptChanges();
            ds.Tables[0].Columns.Add("Candle", typeof(string), "IIF([f2] > [f5],'G',IIF([f2] = [f5],'D','R'))");
            conn.Close();

            var rows1 = ds.Tables[0].Select("f2 = f3 and f2=f4 and f2= f5");
            int count = rows1.Count();
            foreach (var row in rows1)
                row.Delete();

            ds.Tables[0].AcceptChanges();


            dt = ds.Tables[0];
            ds.Tables[0].AcceptChanges();

            Indicator ind1 = new Indicator();
            ind1.IndicatorName = "SuperTrend";
            List<Indicator> xInd = new List<Indicator>();
            xInd.Add(ind1);

            //Indicator ind2 = new Indicator();
            //ind2.IndicatorName = "RSI";              
            //xInd.Add(ind2);

            Indicators.AddIndicators(ref ds, xInd);
            //Indicators.AddSwinIndicator(ref ds);


            int startOfTheWeekIndex = 0;

            //need to commented while running

            int cont = 0;

            for (int i = ds.Tables[0].Rows.Count - 1; i > ds.Tables[0].Rows.Count - 10000; i--)
            {
                if (string.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[i][0])))
                {
                    startOfTheWeekIndex = i;
                    if (_tntReq.backTestDay == cont)
                    {
                        break;
                    }
                    else
                    {
                        cont++;
                    }
                }
                else if (string.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[i - 1][0])))
                {
                    startOfTheWeekIndex = i - 1;
                    if (_tntReq.backTestDay == cont)
                    {

                        break;
                    }
                    else
                    {
                        cont++;
                        i = i - 1;
                    }

                }
                else if (Math.Abs(Convert.ToInt32(ds.Tables[0].Rows[i]["period"]) - Convert.ToInt32(ds.Tables[0].Rows[i - 1]["period"])) > 5)
                {
                    startOfTheWeekIndex = i;
                    if (_tntReq.backTestDay == cont)
                    {
                        break;
                    }
                    else
                    {
                        cont++;
                    }
                }
            }


            //if (System.Configuration.ConfigurationSettings.AppSettings["BackLiveTest"] == "True")
            //{
            //    int xL = startOfTheWeekIndex - 1;
            //    for (int deleteIndex = xL + goTimeCount; deleteIndex < ds.Tables[0].Rows.Count; deleteIndex++)
            //    {
            //        ds.Tables[0].Rows[deleteIndex].Delete();
            //    }
            //    ds.Tables[0].AcceptChanges();
            //}




            List<SR> list = new List<SR>();

            list.Add(new SR { price = Math.Round(todaysLevel.dPP, 1), LevelName = "dPP" });
            list.Add(new SR { price = Math.Round(todaysLevel.dR1, 1), LevelName = "dR1" });
            list.Add(new SR { price = Math.Round(todaysLevel.dR2, 1), LevelName = "dR2" });
            list.Add(new SR { price = Math.Round(todaysLevel.dR3, 1), LevelName = "dR3" });
            list.Add(new SR { price = Math.Round(todaysLevel.dS1, 1), LevelName = "dS1" });
            list.Add(new SR { price = Math.Round(todaysLevel.dS2, 1), LevelName = "dS2" });
            list.Add(new SR { price = Math.Round(todaysLevel.dS3, 1), LevelName = "dS3" });
            list.Add(new SR { price = Math.Round(todaysLevel.wPP, 1), LevelName = "wPP" });
            list.Add(new SR { price = Math.Round(todaysLevel.wR1, 1), LevelName = "wR1" });
            list.Add(new SR { price = Math.Round(todaysLevel.wR2, 1), LevelName = "wR2" });
            list.Add(new SR { price = Math.Round(todaysLevel.wR3, 1), LevelName = "wR3" });
            list.Add(new SR { price = Math.Round(todaysLevel.wS1, 1), LevelName = "wS1" });
            list.Add(new SR { price = Math.Round(todaysLevel.wS2, 1), LevelName = "wS2" });
            list.Add(new SR { price = Math.Round(todaysLevel.wS3, 1), LevelName = "wS3" });
            list.Add(new SR { price = Math.Round(todaysLevel.mPP, 1), LevelName = "mPP" });
            list.Add(new SR { price = Math.Round(todaysLevel.mR1, 1), LevelName = "mR1" });
            list.Add(new SR { price = Math.Round(todaysLevel.mR2, 1), LevelName = "mR2" });
            list.Add(new SR { price = Math.Round(todaysLevel.mR3, 1), LevelName = "mR3" });
            list.Add(new SR { price = Math.Round(todaysLevel.mS1, 1), LevelName = "mS1" });
            list.Add(new SR { price = Math.Round(todaysLevel.mS2, 1), LevelName = "mS2" });
            list.Add(new SR { price = Math.Round(todaysLevel.mS3, 1), LevelName = "mS3" });
            list.Add(new SR { price = Math.Round(todaysLevel.yPP, 1), LevelName = "yPP" });
            list.Add(new SR { price = Math.Round(todaysLevel.yR1, 1), LevelName = "yR1" });
            list.Add(new SR { price = Math.Round(todaysLevel.yR2, 1), LevelName = "yR2" });
            list.Add(new SR { price = Math.Round(todaysLevel.yR3, 1), LevelName = "yR3" });
            list.Add(new SR { price = Math.Round(todaysLevel.yS1, 1), LevelName = "yS1" });
            list.Add(new SR { price = Math.Round(todaysLevel.yS2, 1), LevelName = "yS2" });
            list.Add(new SR { price = Math.Round(todaysLevel.yS3, 1), LevelName = "yS3" });
            list.Add(new SR { price = Math.Round(Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["SuperTrend"]), 2), LevelName = "STH" });
            //list.Add(new SR { price = 0, LevelName = "D20MA" });
            var listX = new List<SR>();
            listX = list;
            double low = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f4"]);
            double high = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f3"]);
            double close = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f2"]);
            double open = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f5"]);

            list = WildAnalysis(Math.Round(low, 1), Math.Round(high, 1), Math.Round(close, 1), 0, list);
            int numberOfStocks = Convert.ToInt32(1000000 / close);
            double risk = numberOfStocks * (close - low);
            //allStocks.Add(scrip, new StockData() { Direction = "BM", Quantity = 0, Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = 0, Levels = listX });
            if ((high - close) < (close - open) && list.Count(a => a.SupportOrResistance == "S") > list.Count(a => a.SupportOrResistance == "R") && list.Where(a => a.LevelName.Contains("w")).Count() > 0 && close > list.Where(a => a.LevelName.Contains("w")).First().price && open < list.Where(a => a.LevelName.Contains("w")).First().price && close > open)
            {



                foreach (SR s in list)
                {
                    if (high >= s.price && close < s.price)
                    {
                        return null;
                    }
                }
                return new StockData() { Direction = "BM", Quantity = Convert.ToInt32(numberOfStocks), Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("w")).First().price, Levels = listX, Symbol = scrip, Category = 5 };
                //Stocks60.Add(scrip, new StockData() { Direction = "BM", Quantity = Convert.ToInt32(numberOfStocks), Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("w")).First().price, Levels = listX });

            }
            else if ((close - low) < (open - close) && list.Count(a => a.SupportOrResistance == "R") > list.Count(a => a.SupportOrResistance == "S") && list.Where(a => a.LevelName.Contains("w")).Count() > 0 && open > list.Where(a => a.LevelName.Contains("w")).First().price && close < list.Where(a => a.LevelName.Contains("w")).First().price && open > close)
            {
                foreach (SR s in list)
                {
                    if (low <= s.price && close > s.price)
                    {
                        return null;
                    }
                }


                return new StockData() { Direction = "SM", Quantity = Convert.ToInt32(numberOfStocks), Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("w")).First().price, Levels = listX, Symbol = scrip, Category = 5 };


            }


        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
        }
        return null;
    }

    //public void CalculateIndicators60()
    //{
    //    try
    //    {
    //        string scrip = _tntReq.Name;
    //        StockData todaysLevel = _tntReq.todaysLevel;
    //        DataTable dt = null;
    //        string FileName = string.Empty;
    //        FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\Data\Hourly\" + scrip.Replace("-", string.Empty) + ".csv";


    //        OleDbConnection conn = new OleDbConnection
    //               ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
    //                 Path.GetDirectoryName(FileName) +
    //                 "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

    //        conn.Open();

    //        OleDbDataAdapter adapter = new OleDbDataAdapter
    //               ("SELECT * FROM " + Path.GetFileName(FileName), conn);

    //        DataSet ds = new DataSet("Temp");
    //        adapter.Fill(ds);

    //        ds.Tables[0].Rows.RemoveAt(0);
    //        ds.Tables[0].Rows.RemoveAt(0);
    //        ds.Tables[0].Rows.RemoveAt(0);
    //        ds.Tables[0].Rows.RemoveAt(0);
    //        ds.Tables[0].Rows.RemoveAt(0);
    //        ds.Tables[0].Rows.RemoveAt(0);
    //        ds.Tables[0].Columns[0].ColumnName = "Period";
    //        ds.Tables[0].Columns[0].Caption = "Period";
    //        ds.Tables[0].AcceptChanges();
    //        ds.Tables[0].Columns.Add("Candle", typeof(string), "IIF([f2] > [f5],'G',IIF([f2] = [f5],'D','R'))");
    //        conn.Close();

    //        var rows1 = ds.Tables[0].Select("f2 = f3 and f2=f4 and f2= f5");
    //        int count = rows1.Count();
    //        foreach (var row in rows1)
    //            row.Delete();

    //        ds.Tables[0].AcceptChanges();


    //        dt = ds.Tables[0];
    //        ds.Tables[0].AcceptChanges();

    //        Indicator ind1 = new Indicator();
    //        ind1.IndicatorName = "SuperTrend";
    //        List<Indicator> xInd = new List<Indicator>();
    //        xInd.Add(ind1);

    //        //Indicator ind2 = new Indicator();
    //        //ind2.IndicatorName = "RSI";              
    //        //xInd.Add(ind2);

    //        Indicators.AddIndicators(ref ds, xInd);
    //        //Indicators.AddSwinIndicator(ref ds);


    //        int startOfTheWeekIndex = 0;

    //        //need to commented while running

    //        int cont = 0;

    //        for (int i = ds.Tables[0].Rows.Count - 1; i > ds.Tables[0].Rows.Count - 10000; i--)
    //        {
    //            if (string.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[i][0])))
    //            {
    //                startOfTheWeekIndex = i;
    //                if (_tntReq.backTestDay == cont)
    //                {
    //                    break;
    //                }
    //                else
    //                {
    //                    cont++;
    //                }
    //            }
    //            else if (string.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[i - 1][0])))
    //            {
    //                startOfTheWeekIndex = i - 1;
    //                if (_tntReq.backTestDay == cont)
    //                {

    //                    break;
    //                }
    //                else
    //                {
    //                    cont++;
    //                    i = i - 1;
    //                }

    //            }
    //            else if (Math.Abs(Convert.ToInt32(ds.Tables[0].Rows[i]["period"]) - Convert.ToInt32(ds.Tables[0].Rows[i - 1]["period"])) > 5)
    //            {
    //                startOfTheWeekIndex = i;
    //                if (_tntReq.backTestDay == cont)
    //                {
    //                    break;
    //                }
    //                else
    //                {
    //                    cont++;
    //                }
    //            }
    //        }


    //        //if (System.Configuration.ConfigurationSettings.AppSettings["BackLiveTest"] == "True")
    //        //{
    //        //    int xL = startOfTheWeekIndex - 1;
    //        //    for (int deleteIndex = xL + goTimeCount; deleteIndex < ds.Tables[0].Rows.Count; deleteIndex++)
    //        //    {
    //        //        ds.Tables[0].Rows[deleteIndex].Delete();
    //        //    }
    //        //    ds.Tables[0].AcceptChanges();
    //        //}




    //        List<SR> list = new List<SR>();

    //        list.Add(new SR { price = Math.Round(todaysLevel.dPP, 1), LevelName = "dPP" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.dR1, 1), LevelName = "dR1" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.dR2, 1), LevelName = "dR2" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.dR3, 1), LevelName = "dR3" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.dS1, 1), LevelName = "dS1" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.dS2, 1), LevelName = "dS2" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.dS3, 1), LevelName = "dS3" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.wPP, 1), LevelName = "wPP" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.wR1, 1), LevelName = "wR1" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.wR2, 1), LevelName = "wR2" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.wR3, 1), LevelName = "wR3" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.wS1, 1), LevelName = "wS1" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.wS2, 1), LevelName = "wS2" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.wS3, 1), LevelName = "wS3" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.mPP, 1), LevelName = "mPP" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.mR1, 1), LevelName = "mR1" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.mR2, 1), LevelName = "mR2" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.mR3, 1), LevelName = "mR3" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.mS1, 1), LevelName = "mS1" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.mS2, 1), LevelName = "mS2" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.mS3, 1), LevelName = "mS3" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.yPP, 1), LevelName = "yPP" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.yR1, 1), LevelName = "yR1" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.yR2, 1), LevelName = "yR2" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.yR3, 1), LevelName = "yR3" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.yS1, 1), LevelName = "yS1" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.yS2, 1), LevelName = "yS2" });
    //        list.Add(new SR { price = Math.Round(todaysLevel.yS3, 1), LevelName = "yS3" });
    //        list.Add(new SR { price = Math.Round(Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["SuperTrend"]), 2), LevelName = "STH" });
    //        //list.Add(new SR { price = 0, LevelName = "D20MA" });
    //        var listX = new List<SR>();
    //        listX = list;
    //        double low = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f4"]);
    //        double high = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f3"]);
    //        double close = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f2"]);
    //        double open = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f5"]);

    //        list = WildAnalysis(Math.Round(low, 1), Math.Round(high, 1), Math.Round(close, 1), 0, list);
    //        int numberOfStocks = Convert.ToInt32(1000000 / close);
    //        double risk = numberOfStocks * (close - low);
    //        //allStocks.Add(scrip, new StockData() { Direction = "BM", Quantity = 0, Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = 0, Levels = listX });
    //        if ((high - close) < (close - open) && list.Count(a => a.SupportOrResistance == "S") > list.Count(a => a.SupportOrResistance == "R") && list.Where(a => a.LevelName.Contains("w")).Count() > 0 && close > list.Where(a => a.LevelName.Contains("w")).First().price && open < list.Where(a => a.LevelName.Contains("w")).First().price && close > open)
    //        {
    //            foreach (SR s in list)
    //            {
    //                if (high >= s.price && close < s.price)
    //                {
    //                    return;
    //                }
    //            }
    //            return (new StockData() { Direction = "BM", Quantity = Convert.ToInt32(numberOfStocks), Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("w")).First().price, Levels = listX, Symbol = scrip, Category = 5 };
    //            //Stocks60.Add(scrip, new StockData() { Direction = "BM", Quantity = Convert.ToInt32(numberOfStocks), Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("w")).First().price, Levels = listX });

    //        }
    //        else if ((close - low) < (open - close) && list.Count(a => a.SupportOrResistance == "R") > list.Count(a => a.SupportOrResistance == "S") && list.Where(a => a.LevelName.Contains("w")).Count() > 0 && open > list.Where(a => a.LevelName.Contains("w")).First().price && close < list.Where(a => a.LevelName.Contains("w")).First().price && open > close)
    //        {
    //            foreach (SR s in list)
    //            {
    //                if (low <= s.price && close > s.price)
    //                {
    //                    return;
    //                }
    //            }


    //            return new StockData() { Direction = "SM", Quantity = Convert.ToInt32(numberOfStocks), Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("w")).First().price, Levels = listX, Symbol = scrip, Category = 5 };


    //        }


    //    }
    //    catch (Exception ex)
    //    {
    //        File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
    //    }

    //    //rXs.StocksIdentified = allStocks.OrderBy(a => a.Value.Risk).ToDictionary(a => a.Key, b => b.Value);

    //}

    public static StockData CalculateIndicators30(Equity _tntReq)
    {
        try
        {
            DataTable dt = null;
            string FileName = string.Empty;
            FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\30\" + _tntReq.Name.Replace("-", string.Empty) + "30.csv";

            OleDbConnection conn = new OleDbConnection
                   ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                     Path.GetDirectoryName(FileName) +
                     "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

            conn.Open();

            OleDbDataAdapter adapter = new OleDbDataAdapter
                   ("SELECT * FROM " + Path.GetFileName(FileName), conn);

            DataSet ds = new DataSet("Temp");
            adapter.Fill(ds);

            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Columns[0].ColumnName = "Period";
            ds.Tables[0].Columns[0].Caption = "Period";
            ds.Tables[0].AcceptChanges();
            ds.Tables[0].Columns.Add("Candle", typeof(string), "IIF([f2] > [f5],'G',IIF([f2] = [f5],'D','R'))");
            conn.Close();


            dt = ds.Tables[0];

            var rows1 = ds.Tables[0].Select("f2 = f3 and f2=f4 and f2= f5");
            int count = rows1.Count();
            foreach (var row in rows1)
                row.Delete();

            ds.Tables[0].AcceptChanges();

            dt.Columns.Add("20", typeof(double));
            dt.Columns.Add("50", typeof(double));
            dt.Columns.Add("200", typeof(double));
            dt.Columns.Add("macd", typeof(double));
            dt.Columns.Add("macd9", typeof(double));
            dt.Columns.Add("RSI14", typeof(double));
            dt.Columns.Add("BS", typeof(string));


            double pEMA9 = 0, pEMA12 = 0, pEMA26 = 0, macd = 0;





            ds.Tables[0].AcceptChanges();

            int startOfTheWeekIndex = 0;

            //need to commented while running

            int cont = 0;

            for (int i = ds.Tables[0].Rows.Count - 1; i > ds.Tables[0].Rows.Count - 10000; i--)
            {
                if (string.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[i][0])))
                {
                    startOfTheWeekIndex = i;
                    if (_tntReq.backTestDay == cont)
                    {
                        break;
                    }
                    else
                    {
                        cont++;
                    }
                }
                else if (string.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[i - 1][0])))
                {
                    startOfTheWeekIndex = i - 1;
                    if (_tntReq.backTestDay == cont)
                    {

                        break;
                    }
                    else
                    {
                        cont++;
                        i = i - 1;
                    }

                }
                else if (Math.Abs(Convert.ToInt32(ds.Tables[0].Rows[i]["period"]) - Convert.ToInt32(ds.Tables[0].Rows[i - 1]["period"])) > 5)
                {
                    startOfTheWeekIndex = i;
                    if (_tntReq.backTestDay == cont)
                    {
                        break;
                    }
                    else
                    {
                        cont++;
                    }
                }
            }

            double per = (Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f5"]) - Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex - 1]["f2"])) / Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex - 1]["f2"])) * 100;
            return new StockData() { Direction = "SM", Quantity = Convert.ToInt32(0), Risk = per, Low = 0, High = 0, Close = 0, Open = 0, stopLoss = 0, Levels = null, Symbol = _tntReq.Name, Category = 4 };

        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
        }

        return null;
    }



    public static StockData CalculateIndicators30Zerodha(Equity _tntReq, bool onQuotes)
    {
        try
        {
            string scrip = _tntReq.Name;
            StockData todaysLevel = _tntReq.todaysLevel;

            double low = _tntReq.Low;
            double high = _tntReq.High;
            double open = _tntReq.Open;
            double close = _tntReq.Close;

            List<SR> list = new List<SR>();

            list.Add(new SR { price = Math.Round(todaysLevel.dPP, 1), LevelName = "dPP" });
            list.Add(new SR { price = Math.Round(todaysLevel.dR1, 1), LevelName = "dR1" });
            list.Add(new SR { price = Math.Round(todaysLevel.dR2, 1), LevelName = "dR2" });
            list.Add(new SR { price = Math.Round(todaysLevel.dR3, 1), LevelName = "dR3" });
            list.Add(new SR { price = Math.Round(todaysLevel.dS1, 1), LevelName = "dS1" });
            list.Add(new SR { price = Math.Round(todaysLevel.dS2, 1), LevelName = "dS2" });
            list.Add(new SR { price = Math.Round(todaysLevel.dS3, 1), LevelName = "dS3" });
            list.Add(new SR { price = Math.Round(todaysLevel.wPP, 1), LevelName = "SMAwPP" });
            list.Add(new SR { price = Math.Round(todaysLevel.wR1, 1), LevelName = "SMAwR1" });
            list.Add(new SR { price = Math.Round(todaysLevel.wR2, 1), LevelName = "SMAwR2" });
            list.Add(new SR { price = Math.Round(todaysLevel.wR3, 1), LevelName = "SMAwR3" });
            list.Add(new SR { price = Math.Round(todaysLevel.wS1, 1), LevelName = "SMAwS1" });
            list.Add(new SR { price = Math.Round(todaysLevel.wS2, 1), LevelName = "SMAwS2" });
            list.Add(new SR { price = Math.Round(todaysLevel.wS3, 1), LevelName = "SMAwS3" });
            list.Add(new SR { price = Math.Round(todaysLevel.mPP, 1), LevelName = "SMAmPP" });
            list.Add(new SR { price = Math.Round(todaysLevel.mR1, 1), LevelName = "SMAmR1" });
            list.Add(new SR { price = Math.Round(todaysLevel.mR2, 1), LevelName = "SMAmR2" });
            list.Add(new SR { price = Math.Round(todaysLevel.mR3, 1), LevelName = "SMAmR3" });
            list.Add(new SR { price = Math.Round(todaysLevel.mS1, 1), LevelName = "SMAmS1" });
            list.Add(new SR { price = Math.Round(todaysLevel.mS2, 1), LevelName = "SMAmS2" });
            list.Add(new SR { price = Math.Round(todaysLevel.mS3, 1), LevelName = "SMAmS3" });
            list.Add(new SR { price = Math.Round(todaysLevel.yPP, 1), LevelName = "SMAyPP" });
            list.Add(new SR { price = Math.Round(todaysLevel.yR1, 1), LevelName = "SMAyR1" });
            list.Add(new SR { price = Math.Round(todaysLevel.yR2, 1), LevelName = "SMAyR2" });
            list.Add(new SR { price = Math.Round(todaysLevel.yR3, 1), LevelName = "SMAyR3" });
            list.Add(new SR { price = Math.Round(todaysLevel.yS1, 1), LevelName = "SMAyS1" });
            list.Add(new SR { price = Math.Round(todaysLevel.yS2, 1), LevelName = "SMAyS2" });
            list.Add(new SR { price = Math.Round(todaysLevel.yS3, 1), LevelName = "SMAyS3" });

            list = WildAnalysis(Math.Round(low, 1), Math.Round(high, 1), Math.Round(close, 1), 0, list);
            double prevDayClose = todaysLevel.dClose;

            if ((high - close) < (close - open) && list.Where(a => a.LevelName.Contains("SMA")).Count() > 1 && close > list.Where(a => a.LevelName.Contains("SMA")).Max(b => b.price) && close > open)
            {
                double per = (Math.Abs(open - prevDayClose) / Convert.ToDouble(close)) * 100;
                return new StockData() { Direction = "SM", Quantity = Convert.ToInt32(0), Risk = per, Low = 0, High = 0, Close = 0, Open = 0, stopLoss = 0, Levels = null, Symbol = _tntReq.Name, Category = 4 };
            }
            else if ((close - low) < (open - close) && list.Where(a => a.LevelName.Contains("SMA")).Count() > 1 && close < list.Where(a => a.LevelName.Contains("SMA")).Min(b => b.price) && open > close)
            {
                double per = (Math.Abs(open - prevDayClose) / Convert.ToDouble(close)) * 100;
                return new StockData() { Direction = "SM", Quantity = Convert.ToInt32(0), Risk = per, Low = 0, High = 0, Close = 0, Open = 0, stopLoss = 0, Levels = null, Symbol = _tntReq.Name, Category = 4 };
            }
        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
        }

        return null;
    }





    public static bool CheckFullValue(int toRound, int todaysHigh, int todaysLow, string direction)
    {

        int modulous = 0;
        if (direction == "BM")
        {
            //toRound = Convert.ToInt32(toRound + (toRound * 0.02));
            if (Convert.ToInt32(toRound + (todaysHigh - todaysLow)) >= todaysHigh)
            {
                todaysHigh = Convert.ToInt32(toRound + (todaysHigh - todaysLow));
            }
        }
        else if (direction == "SM")
        {
            //toRound = Convert.ToInt32(toRound - (toRound * 0.02));
            if (Convert.ToInt32(toRound - (todaysHigh - todaysLow)) <= todaysLow)
            {
                todaysLow = Convert.ToInt32(toRound - ((todaysHigh - todaysLow)));
            }
        }

        switch (toRound.ToString().Length)
        {
            case 6:
                modulous = 1000;
                break;
            case 5:
                modulous = 100;
                break;
            case 4:
                modulous = 100;
                break;
            case 3:
                modulous = 100;
                break;
            case 2:
                modulous = 10;
                break;
            default:
                modulous = 1;
                break;

        }
        int high = 0;
        int low = 0;
        if (toRound % modulous == 0) high = toRound;
        high = (modulous - toRound % modulous) + toRound;
        low = toRound - toRound % modulous;
        if (low >= todaysLow && low <= todaysHigh && direction == "SM")
        {
            return false;
        }
        if (high <= todaysLow && high >= todaysLow && direction == "BM")
        {
            return false;
        }

        return true;
    }

    public static int RoundDown(int toRound)
    {
        return toRound - toRound % 10;
    }

    public static void CalculateIndicatorsLoadOnce(Equity e)
    {
        try
        {
            string scrip = e.Name;

            DataTable dt = null;
            string FileName = string.Empty;

            FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\" + scrip.Replace("-", string.Empty) + ".csv";


            OleDbConnection conn = new OleDbConnection
                   ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                     Path.GetDirectoryName(FileName) +
                     "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

            conn.Open();

            OleDbDataAdapter adapter = new OleDbDataAdapter
                   ("SELECT * FROM " + Path.GetFileName(FileName), conn);

            DataSet ds = new DataSet("Temp");
            adapter.Fill(ds);
            conn.Close();
            conn.Dispose();

            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Rows.RemoveAt(0);
            ds.Tables[0].Columns[0].ColumnName = "Period";
            ds.Tables[0].Columns[0].Caption = "Period";
            ds.Tables[0].AcceptChanges();
            ds.Tables[0].Columns.Add("Candle", typeof(string), "IIF([f2] > [f5],'G',IIF([f2] = [f5],'D','R'))");
            conn.Close();




            dt = ds.Tables[0];

            Indicator ind1 = new Indicator();
            ind1.IndicatorName = "SuperTrend";
            List<Indicator> xInd = new List<Indicator>();
            xInd.Add(ind1);

            Indicator ind2 = new Indicator();
            ind2.IndicatorName = "SMA20";
            xInd.Add(ind2);

            Indicator ind3 = new Indicator();
            ind3.IndicatorName = "SMA50";
            xInd.Add(ind3);

            Indicator ind4 = new Indicator();
            ind4.IndicatorName = "SMA200";
            xInd.Add(ind4);

            Indicators.AddIndicators(ref ds, xInd);
            ds.Tables[0].Columns.Add("BS", typeof(string));
            bool prev = false;
            int prevTrend = -2;

            int ii = 0;
            foreach (DataRow dr in dt.Rows)
            {
                ii++;


                double sma201 = Convert.ToDouble(dr["20"]);
                double sma501 = Convert.ToDouble(dr["50"]);
                double sma2001 = Convert.ToDouble(dr["200"]);

                double maxSma = Math.Max(Math.Max(sma201, sma501), sma2001);
                double minSma = Math.Min(Math.Min(sma201, sma501), sma2001);
                int trend = Convert.ToInt16(dr["Trend"]);

                double pc1 = Convert.ToDouble(dr["f2"]);
                double pl1 = Convert.ToDouble(dr["f4"]);
                double ph1 = Convert.ToDouble(dr["f3"]);
                double pv1 = Convert.ToDouble(dr["f6"]) * Convert.ToDouble(dr["f2"]);

                double smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;

                if (pl1 <= sma2001 + 0.2 && ph1 >= sma201 - 0.2 && sma201 >= sma501 && sma501 >= sma2001 && pc1 >= sma201 && trend >= 1)
                {
                    prev = true;
                    prevTrend = trend;
                }

                smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;
                if (ph1 >= sma2001 - 0.2 && pl1 <= sma201 + 0.2 && sma201 <= sma501 && sma501 <= sma2001 && pc1 <= sma201 && trend < 0)
                {
                    prev = true;
                    prevTrend = trend;
                }

                if (prevTrend != -2 && trend != prevTrend && prev == true)
                {
                    if (trend >= 1 && pc1 > maxSma)
                    {
                        dr["BS"] = "BM";
                    }
                    else if (trend >= 0 && pc1 < minSma)
                    {
                        dr["BS"] = "SM";

                    }
                    prevTrend = trend;
                }
            }

            var rows1 = ds.Tables[0].Select("f2 = f3 and f2=f4 and f2= f5");
            int count = rows1.Count();
            foreach (var row in rows1)
                row.Delete();


            ds.Tables[0].AcceptChanges();


            StringBuilder sb = new StringBuilder();

            string[] columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName).
                                              ToArray();
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                string[] fields = row.ItemArray.Select(field => field.ToString()).
                                                ToArray();
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\backup\" + scrip.Replace("-", string.Empty) + ".csv", sb.ToString());

        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + e.Name + Environment.NewLine);
        }
    }

    //public static DataSet ConvertToJasonQuotes(string jSon)
    //{
    //    DataSet ds = new DataSet();
    //    DataTable dt = new DataTable();
    //    dt.Columns.Add("instrument_token", typeof(double));
    //    dt.Columns.Add("TimeStamp", typeof(string));
    //    dt.Columns.Add("f2", typeof(double));
    //    dt.Columns.Add("f3", typeof(double));
    //    dt.Columns.Add("f4", typeof(double));
    //    dt.Columns.Add("f5", typeof(double));
    //    dt.Columns.Add("f6", typeof(double));
    //    dt.Columns.Add("b", typeof(double));
    //    dt.Columns.Add("s", typeof(double));
    //    dt.Columns.Add("d", typeof(double));
    //    dt.Columns.Add("t", typeof(double));
    //    dt.Columns.Add("Pattern", typeof(string));
    //    int ii = 0;
    //    string[] listOfCandles = jSon.Split(new string[] { "],", "[[" }, StringSplitOptions.RemoveEmptyEntries);

    //    Candle lastCandle = new Candle();
    //    Candle secondLastCandle = new Candle();
    //    Candle latestCandle = new Candle();
    //    double b = 0, s = 0, t = 0;
    //    foreach (string jCandle in listOfCandles)
    //    {
    //        ii++;
    //        try
    //        {
    //            if (ii == dt.Rows.Count)
    //                break;
    //            string pattern = @"[a-z_:]*";
    //            string replacement = "";
    //            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(pattern);
    //            string result = rgx.Replace(jCandle, replacement);

    //            string[] candleAttributes = result.Split(new string[] { ",", "]", "[", "}", "{" }, StringSplitOptions.RemoveEmptyEntries);

    //            if (ii == 1)
    //            {

    //                latestCandle = new Candle() { Volume = Convert.ToDouble(candleAttributes[8]), Token = Convert.ToDouble(candleAttributes[1]), CandleType = candleAttributes[2].ToString().Substring(0, candleAttributes[2].Length - 2), Close = Convert.ToDouble(candleAttributes[4]), High = Convert.ToDouble(candleAttributes[15]), Low = Convert.ToDouble(candleAttributes[16]), Open = Convert.ToDouble(candleAttributes[14]) };
    //                b = Convert.ToDouble(candleAttributes[6]);
    //                s = Convert.ToDouble(candleAttributes[7]);
    //                t = Convert.ToDouble(candleAttributes[8]) * Convert.ToDouble(candleAttributes[4]);

    //            }
    //            else
    //            {
    //                latestCandle = new Candle() { Volume = Convert.ToDouble(candleAttributes[23]), Token = Convert.ToDouble(candleAttributes[16]), CandleType = candleAttributes[17].ToString().Substring(0, candleAttributes[17].Length - 2), Close = Convert.ToDouble(candleAttributes[19]), High = Convert.ToDouble(candleAttributes[30]), Low = Convert.ToDouble(candleAttributes[31]), Open = Convert.ToDouble(candleAttributes[29]) };
    //                b = Convert.ToDouble(candleAttributes[21]);
    //                s = Convert.ToDouble(candleAttributes[22]);
    //                t = Convert.ToDouble(candleAttributes[23]) * Convert.ToDouble(candleAttributes[19]);
    //            }

    //            dt.Rows.Add(latestCandle.Token, latestCandle.CandleType, latestCandle.Close, latestCandle.High, latestCandle.Low, latestCandle.Open, latestCandle.Volume, b, s, ((b - s) / (b + s)) * 100, t / 10000000, "");
    //            // sb.Append(candleAttributes[0] + "," + candleAttributes[1] + "," + candleAttributes[2] + "," + candleAttributes[3] + "," + candleAttributes[4] + "," + candleAttributes[5] + Environment.NewLine);
    //            secondLastCandle = lastCandle;
    //            lastCandle = latestCandle;
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }
    //    dt.Columns.Add("Candle", typeof(string), "IIF([f2] > [f5],'G',IIF([f2] = [f5],'D','R'))");


    //    ds.Tables.Add(dt);
    //    return ds;
    //}
    public static List<Candle> ConvertToJason(string jSon, bool heikenAshi = false)
    {
        List<Candle> history = new List<Candle>();

        int ii = 0;
        string[] listOfCandles = jSon.Split(new string[] { "],", "[[" }, StringSplitOptions.RemoveEmptyEntries);
        Candle lastCandle = new Candle();
        Candle secondLastCandle = new Candle();
        Candle latestCandle = new Candle();
        IMovingAverage avg20 = new SimpleMovingAverage(20);
        IMovingAverage avg50 = new SimpleMovingAverage(50);
        IMovingAverage avg200 = new SimpleMovingAverage(200);
        double cClose = 0;
        double cHigh = 0;
        double cLow = 0;
        double cOpen = 0;
        string xHCandleType = string.Empty;
        double xC = 0, xH = 0, xLow = 0, xO = 0, pxH = 0, pxO = 0, pxC = 0, pxL = 0;
        foreach (string jCandle in listOfCandles)
        {
            ii++;
            if (ii == 1)
            {
                continue;

            }


            string[] candleAttributes = jCandle.Split(new string[] { ",", "]", "[" }, StringSplitOptions.RemoveEmptyEntries);

            cClose = Convert.ToDouble(candleAttributes[4]);
            cHigh = Convert.ToDouble(candleAttributes[2]);
            cLow = Convert.ToDouble(candleAttributes[3]);
            cOpen = Convert.ToDouble(candleAttributes[1]);


            if (ii == 2)
            {
                xC = cClose;
                xH = cHigh;
                xLow = cLow;
                xO = cOpen;
            }
            else
            {
                pxH = lastCandle.Close;

                xC = (cClose + cHigh + cLow + cOpen) / 4;
                xO = (pxO + pxC) / 2;
                xLow = Math.Min(Math.Min(xC, xO), cLow);
                xH = Math.Max(Math.Max(xC, xO), cHigh);
            }
            if (xLow == xO)
            {
                xHCandleType = "G";
            }
            else if (xH == xO)
            {
                xHCandleType = "R";
            }
            else if (xC > xO)
            {
                xHCandleType = "GD";
            }
            else if (xC < xO)
            {
                xHCandleType = "RD";
            }


            pxC = xC;
            pxH = xH;
            pxL = xLow;
            pxO = xO;

            double cATR21 = 0;
            double cATR7 = 0;
            SuperTrendInd sObj = new SuperTrendInd();
            if (!heikenAshi)
            {
                cATR21 = ATR(ii - 2, cHigh, cLow, cClose, lastCandle.ATRStopLoss, lastCandle.Close, 21);
                avg20.AddSample((float)cClose);
                avg50.AddSample((float)cClose);
                avg200.AddSample((float)cClose);
                cATR7 = ATR(ii - 2, cHigh, cLow, cClose, lastCandle.ATR7, lastCandle.Close, 7);
                sObj = GetSuperTrend(ii - 2, cATR7, cHigh, cLow, cClose, lastCandle.Close, lastCandle.STrend, 7);
            }
            else
            {
                avg20.AddSample((float)xC);
                avg50.AddSample((float)xC);
                avg200.AddSample((float)xC);
                cATR7 = ATR(ii - 2, xH, xLow, xC, lastCandle.ATR7, lastCandle.HClose, 7);
                sObj = GetSuperTrend(ii - 2, cATR7, xH, xLow, xC, lastCandle.HClose, lastCandle.STrend, 7);
            }


            latestCandle = new Candle()
            {
                TimeStamp = Convert.ToDateTime(candleAttributes[0].Substring(1, 19).Replace("T", " ")),
                Close = cClose,
                High = cHigh,
                Low = cLow,
                Open = Convert.ToDouble(candleAttributes[1]),
                Volume = Convert.ToDouble(candleAttributes[5]),
                SMA20 = avg20.Average,
                SMA50 = avg50.Average,
                SMA200 = avg200.Average,
                ATR = ATR(ii - 2, cHigh, cLow, cClose, lastCandle.ATR, lastCandle.Close, 14),
                ATR7 = cATR7,
                STrend = sObj,
                ATRStopLoss = cATR21,
                HCandleType = xHCandleType,
                HOpen = xO,
                HClose = xC,
                HLow = xLow,
                HHigh = xH

            };
            latestCandle.CP = CandlePattern(secondLastCandle, lastCandle, latestCandle);
            latestCandle.Treding = Trending(latestCandle, lastCandle);
            history.Add(latestCandle);


            secondLastCandle = lastCandle;
            lastCandle = latestCandle;
        }

        return history;
    }

    public static double ATR(int rowIndex, double high, double low, double close, double prevAtr, double prevclose, int period)
    {
        double atrValue = 0;
        if (rowIndex == period - 1)
        {
            atrValue = Math.Max(Math.Max(high - low, Math.Abs(high - prevclose)), Math.Abs(low - prevclose)) / period;
        }
        else if (rowIndex >= period)
        {
            atrValue = (prevAtr * (period - 1) + Math.Max(Math.Max(high - low, Math.Abs(high - prevclose)), Math.Abs(low - prevclose))) / period;
        }
        else if (rowIndex == 0)
        {
            atrValue = 0;
        }
        else
        {
            atrValue = prevAtr + Math.Max(Math.Max(high - low, Math.Abs(high - prevclose)), Math.Abs(low - prevclose));
        }
        return atrValue;
    }
    public static SuperTrendInd GetSuperTrend(int rowIndex, double ATR7, double high, double low, double close, double prevclose, SuperTrendInd prevSuperTrend, int period)
    {

        double FUB = 0;
        double LUB = 0;
        double SuperTrend = 0;
        double basicUpperBand = ((high + low) / 2) - (ATR7 * 3);
        double basicLowerBand = ((high + low) / 2) + (ATR7 * 3);
        if (rowIndex == 0)
        {

            return new SuperTrendInd
            {
                FinalLowerBand = 0,
                FinalUpperBand = 0,
                Trend = 1,
                SuperTrend = 0,
                AvgTr = 0

            };
        }
        else if (rowIndex < period)
        {

            return new SuperTrendInd
            {
                FinalLowerBand = 0,
                FinalUpperBand = 0,
                Trend = 1,
                SuperTrend = 0,
                AvgTr = prevSuperTrend.AvgTr + Math.Max(Math.Max(high - low, Math.Abs(high - prevclose)), Math.Abs(low - prevclose))
            };
        }
        else
        {

            if (prevclose > prevSuperTrend.FinalUpperBand)
            {
                FUB = Math.Max(basicUpperBand, prevSuperTrend.FinalUpperBand);
            }
            else
            {
                FUB = basicUpperBand;
            }

            if (prevclose < prevSuperTrend.FinalLowerBand)
            {
                LUB = Math.Min(basicLowerBand, prevSuperTrend.FinalLowerBand);
            }
            else
            {
                LUB = basicLowerBand;
            }

            double trend = close > LUB ? 1 : (close < FUB ? -1 : prevSuperTrend.Trend);
            if (trend == 1)
            {
                SuperTrend = FUB;
            }
            else
            {
                SuperTrend = LUB;
            }
            return new SuperTrendInd
            {
                FinalLowerBand = LUB,
                FinalUpperBand = FUB,
                Trend = trend,
                SuperTrend = SuperTrend

            };
        }

    }

    public static Order OpenEntry(Equity e, bool startFromScratch)
    {

        try
        {
            if (e.TestAtMinute < -2 || e.TestAtMinute > 70)
            {
                return null;
            }
            Order returnOrder = null;
            string scrip = e.Name;
            int k = 0;
            switch (e.TestAtMinute)
            {
                case 9:
                    k = 0;
                    break;
                case 21:
                    k = 1;
                    break;
                case 33:
                    k = 2;
                    break;
                case 45:
                    k = 3;
                    break;
                case 57:
                    k = 4;
                    break;
            }
            List<Candle> allCandle = e.allDaa;
            List<Candle> data = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp60(e.TestAtMinute, e.TransactionTradingDate)).ToList();
            List<Candle> prevDayData = allCandle.Where(a => a.TimeStamp.Date == allCandle[allCandle.IndexOf(data.First()) - 1].TimeStamp.Date).ToList();
            List<Candle> prevDayData1 = allCandle.Where(a => a.TimeStamp.Date == allCandle[allCandle.IndexOf(prevDayData.First()) - 1].TimeStamp.Date).ToList();
            //   if (prevDayData.Where(a => a.HCandleType == "G" || a.HCandleType == "R").Count() == 0)
            if ((Math.Abs(prevDayData.Last().Close - prevDayData1.Last().Close) / prevDayData1.Last().Close) * 100 > 4)
            {
                Candle drLast = null;
                Candle drFirst = data[k];
                if (k == 0)
                {
                    drLast = prevDayData.Last();
                }
                else
                {
                    drLast = data[k - 1];
                }
                //Candle drPrevious = data[k + 1];

                Candle drCurrentDayfirst = data[0];

                //double prevHigh = drPrevious.High;
                //double prevLow = drPrevious.Low;


                double currentDayOpen = drCurrentDayfirst.Open;
                double open = drFirst.Open;
                double close = drFirst.Close;
                double high = drFirst.High;
                double low = drFirst.Low;
                double vol = drFirst.Volume;

                string candle = drFirst.CandleType;



                double sma501 = drFirst.SMA50;
                double sma2001 = drFirst.SMA200;
                double sma201 = drFirst.SMA20;
                double atr = drFirst.ATR * 1.25;
                double supertrend = drFirst.STrend.SuperTrend;

                List<double> l = new List<double>();
                l.Add(sma201);
                l.Add(sma501);
                l.Add(sma2001);
                l.Add(supertrend);





                DateTime timeStamp = GetTimeStamp(-2, e.TransactionTradingDate);
                DateTime timeStampLast = GetTimeStamp(k, e.TransactionTradingDate);

                double previousDayMaxVolume = prevDayData.Max(a => a.Volume);
                double breakoutHigh = data.Max(a => a.High);
                double bHVol = data.Where(a => a.High == breakoutHigh).First().Volume;

                double breakoutLow = data.Min(a => a.Low);
                double bLVol = data.Where(a => a.Low == breakoutLow).First().Volume;
                double maxVolume = drCurrentDayfirst.Close * drCurrentDayfirst.Volume;
                double maxV = drCurrentDayfirst.Volume;
                string maxVCandle = drCurrentDayfirst.CandleType;

                double per = (drFirst.ATRStopLoss * 3 / close) * 100;
                var dataToProcess = data.ToList();
                int cB = data.Where(a => a.Close < a.SMA20 || a.Close < a.SMA50 || a.Close < a.SMA200 || a.Close < a.STrend.SuperTrend).Count();
                int cS = data.Where(a => a.Close > a.SMA20 || a.Close > a.SMA50 || a.Close > a.SMA200 || a.Close > a.STrend.SuperTrend).Count();

                List<Liner> listOfClose = new List<Liner>();


                File.AppendAllText(@"C:\Jai Sri Thakur Ji\abc.txt", e.Name + e.TransactionTradingDate + Environment.NewLine);


                //if (drFirst.HCandleType == "G" && drFirst.CandleType == "G" && drFirst.HClose > l.Max() && drFirst.HOpen > l.Max() && (k == 0 || data[k - 1].HCandleType != "G"))
                //{
                //    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / Math.Abs(drFirst.HClose - drFirst.HOpen)), Scrip = e.Name, Stoploss = drFirst.HOpen, Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                //}
                //else if (drFirst.HCandleType == "R" && drFirst.CandleType == "R" && drFirst.HClose < l.Min() && drFirst.HOpen < l.Min() && (k == 0 || data[k - 1].HCandleType != "R"))
                //{
                //    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / Math.Abs(drFirst.HClose - drFirst.HOpen)), Scrip = e.Name, Stoploss = drFirst.HOpen, Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                //}
                if (drFirst.HCandleType == "GD" || drFirst.HCandleType == "G")
                {
                    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / Math.Abs(drFirst.HClose - drFirst.HOpen)), Scrip = e.Name, Stoploss = drFirst.HOpen, Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                }
                else if (drFirst.HCandleType == "RD" || drFirst.HCandleType == "R")
                {
                    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / Math.Abs(drFirst.HClose - drFirst.HOpen)), Scrip = e.Name, Stoploss = drFirst.HOpen, Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                }

                // close = {High Low } = Trade Immediately
                //if (maxVCandle == "G" && drCurrentDayfirst.Close > l.Max() && drCurrentDayfirst.Open > l.Max() && drCurrentDayfirst.Close + drCurrentDayfirst.Close * 0.00005 >= drCurrentDayfirst.High)
                //{
                //    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.ATRStopLoss * 3)), Scrip = e.Name, Stoploss = close - (drFirst.ATRStopLoss * 3), Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                //}
                //else if (maxVCandle == "R" && drCurrentDayfirst.Close < l.Min() && drCurrentDayfirst.Open < l.Min() && drCurrentDayfirst.Close - drCurrentDayfirst.Close * 0.00005 <= drCurrentDayfirst.Low)
                //{
                //    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.ATRStopLoss * 3)), Scrip = e.Name, Stoploss = (close + drFirst.ATRStopLoss * 3), Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                //}
                //if (maxVCandle == "G" && drCurrentDayfirst.Close > l.Max() && drCurrentDayfirst.Open > l.Max() && Math.Abs(drCurrentDayfirst.Close - drCurrentDayfirst.Open) * 2.5 < (Math.Min(drCurrentDayfirst.Open, drCurrentDayfirst.Close) - drCurrentDayfirst.Low))
                //{
                //    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.ATRStopLoss * 3)), Scrip = e.Name, Stoploss = close - (drFirst.ATRStopLoss * 3), Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                //}
                //else if (maxVCandle == "R" && drCurrentDayfirst.Close < l.Min() && drCurrentDayfirst.Open < l.Min() && Math.Abs(drCurrentDayfirst.Close - drCurrentDayfirst.Open) * 2 < (drCurrentDayfirst.High - Math.Max(drCurrentDayfirst.Open, drCurrentDayfirst.Close)))
                //{
                //    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.ATRStopLoss * 3)), Scrip = e.Name, Stoploss = (close + drFirst.ATRStopLoss * 3), Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                //}
                //else if (maxVCandle == "G" && drCurrentDayfirst.Close > l.Max() && drCurrentDayfirst.Open > l.Max() && drFirst.Treding == "BULLTRENDING" && high < breakoutHigh)
                //{
                //    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.ATRStopLoss * 3)), Scrip = e.Name, Stoploss = close - (drFirst.ATRStopLoss * 3), Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                //}
                //else if (maxVCandle == "R" && drCurrentDayfirst.Close < l.Min() && drCurrentDayfirst.Open < l.Min() && drFirst.Treding == "BEARTRENDING" && low > breakoutLow)
                //{
                //    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.ATRStopLoss * 3)), Scrip = e.Name, Stoploss = (close + drFirst.ATRStopLoss * 3), Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                //}
                //else if (maxVCandle == "R" && drCurrentDayfirst.Close > l.Max() && drCurrentDayfirst.Open > l.Max() && drCurrentDayfirst.Open + drCurrentDayfirst.Open * 0.00005 >= drCurrentDayfirst.High)
                //{
                //    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.ATRStopLoss * 3)), Scrip = e.Name, Stoploss = (close + drFirst.ATRStopLoss * 3), Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                //}
                //else if (maxVCandle == "G" && drCurrentDayfirst.Close < l.Min() && drCurrentDayfirst.Open < l.Min() && drCurrentDayfirst.Open - drCurrentDayfirst.Open * 0.00005 <= drCurrentDayfirst.Low)
                //{
                //    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.ATRStopLoss * 3)), Scrip = e.Name, Stoploss = close - (drFirst.ATRStopLoss * 3), Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                //}



                //}
                return returnOrder;
            }


        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + e.Name + Environment.NewLine);
        }

        return null;
    }

    public static Order ContinuationSetup(Equity e, List<Candle> nifty)
    {

        try
        {
            if (e.TestAtMinute < -2 || e.TestAtMinute > 60)
            {
                return null;
            }
            Order returnOrder = null;
            string scrip = e.Name;
            int k = e.TestAtMinute;
            //if(e.)
            //switch (e.TestAtMinute)
            //{
            //    case 9:
            //        k = 0;
            //        break;
            //    case 21:
            //        k = 1;
            //        break;
            //    case 33:
            //        k = 2;
            //        break;
            //    case 45:
            //        k = 3;
            //        break;
            //    case 57:
            //        k = 4;
            //        break;
            //}
            List<Candle> allCandle = e.allDaa;
            List<Candle> data = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();
            List<Candle> dataNifty = nifty.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();
            List<Candle> prevDayData = allCandle.Where(a => a.TimeStamp.Date == allCandle[allCandle.IndexOf(data.First()) - 1].TimeStamp.Date).ToList();
            List<Candle> prevDayDatNifty = nifty.Where(a => a.TimeStamp.Date == nifty[nifty.IndexOf(dataNifty.First()) - 1].TimeStamp.Date).ToList();
            //List<Candle> prevDayData1 = allCandle.Where(a => a.TimeStamp.Date == allCandle[allCandle.IndexOf(prevDayData.First()) - 1].TimeStamp.Date).ToList();
            //   if (prevDayData.Where(a => a.HCandleType == "G" || a.HCandleType == "R").Count() == 0)
            if (true)
            {
                double NiftyClose = dataNifty[k + 2].Close;
                double prevDayCloseNIfty = prevDayDatNifty.Last().Close;




                Candle drLast = null;
                Candle drFirst = data[k + 2];

                //if (k == 0)
                //{
                //    drLast = prevDayData.Last();
                //}
                //else
                //{
                //    drLast = data[k - 1];
                //}
                //Candle drPrevious = data[k + 1];

                Candle drCurrentDayfirst = data[0];

                //double prevHigh = drPrevious.High;
                //double prevLow = drPrevious.Low;


                double currentDayOpen = drCurrentDayfirst.Open;
                double open = drFirst.Open;
                double close = drFirst.Close;
                double high = drFirst.High;
                double low = drFirst.Low;
                double vol = drFirst.Volume;

                string candle = drFirst.CandleType;



                double sma501 = drFirst.SMA50;
                double sma2001 = drFirst.SMA200;
                double sma201 = drFirst.SMA20;
                double atr = drFirst.ATR * 1.25;
                double supertrend = drFirst.STrend.SuperTrend;

                List<double> l = new List<double>();
                l.Add(sma201);
                l.Add(sma501);
                l.Add(sma2001);
                l.Add(supertrend);





                DateTime timeStamp = GetTimeStamp(-2, e.TransactionTradingDate);
                DateTime timeStampLast = GetTimeStamp(k, e.TransactionTradingDate);

                double previousDayMaxVolume = prevDayData.Max(a => a.Volume);
                double breakoutHigh = data.Max(a => a.High);
                double bHVol = data.Where(a => a.High == breakoutHigh).First().Volume;

                double breakoutLow = data.Min(a => a.Low);
                double bLVol = data.Where(a => a.Low == breakoutLow).First().Volume;
                double maxVolume = drCurrentDayfirst.Close * drCurrentDayfirst.Volume;
                double maxV = data.Max(a => a.Volume);
                string maxVCandle = data.Where(a => a.Volume == maxV).First().CandleType;

                double per = (drFirst.ATRStopLoss * 3 / close) * 100;
                var dataToProcess = data.ToList();
                int cB = data.Where(a => a.Close < a.SMA20 || a.Close < a.SMA50 || a.Close < a.SMA200 || a.Close < a.STrend.SuperTrend).Count();
                int cS = data.Where(a => a.Close > a.SMA20 || a.Close > a.SMA50 || a.Close > a.SMA200 || a.Close > a.STrend.SuperTrend).Count();

                List<Liner> listOfClose = new List<Liner>();
                int period = 5;

                File.AppendAllText(@"C:\Jai Sri Thakur Ji\abc.txt", e.Name + e.TransactionTradingDate + Environment.NewLine);

                double stopLoss = drFirst.CandleType == "G" ? drFirst.Low : drFirst.High;
                double d1 = Math.Abs(drFirst.Close - drFirst.STrend.SuperTrend);
                double d2 = Math.Abs(drFirst.Close - drFirst.SMA200);

                double quantity = Convert.ToInt32(e.MaxRisk / (breakoutHigh - breakoutLow));
                if (drFirst.Close > 100 && d2 <= d1 * 1.5) //Math.Abs(drFirst.Close - stopLoss) * quantity <= e.MaxRisk * 0.4)
                {
                    if (data.Where(a => a.STrend.Trend == -1).Count() == 0 && drFirst.Close > drFirst.SMA20 && drFirst.Close > drFirst.SMA50 && drFirst.Close > drFirst.SMA200 && drFirst.Close > drFirst.STrend.SuperTrend && drFirst.Low <= drFirst.SMA20 && drFirst.Close > drFirst.SMA20 && drFirst.CandleType == "G")
                    {

                        DateTime previousTouchTimeStamp1 = allCandle.Where(a => a.Low <= a.SMA20 && a.Close >= a.SMA20 && a.TimeStamp < drFirst.TimeStamp).Last().TimeStamp;
                        int val1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp && a.Close < a.SMA20).Count();
                        int x = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Count();
                        double max1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Max(a => a.Close);
                        DateTime previousTouchTimeStamp2 = allCandle.Where(a => a.Low <= a.SMA20 && a.Close >= a.SMA20 && a.TimeStamp < previousTouchTimeStamp1).Last().TimeStamp;
                        int val2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1 && a.Close < a.SMA20).Count();
                        int y = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Count();
                        double max2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Max(a => a.Close);
                        double stoploss = Math.Min(allCandle.Where(a => a.TimeStamp == previousTouchTimeStamp2).First().Low, l.Min());
                        DateTime previousTouchTimeStamp3 = allCandle.Where(a => a.Low <= a.SMA20 && a.Close >= a.SMA20 && a.TimeStamp < previousTouchTimeStamp2).Last().TimeStamp;
                        int val3 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp3 && a.TimeStamp < previousTouchTimeStamp2 && a.Close < a.SMA20).Count();
                        int z = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp3 && a.TimeStamp < previousTouchTimeStamp2).Count();
                        double max3 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp3 && a.TimeStamp < previousTouchTimeStamp2).Max(a => a.Close);
                        if (val1 == 0 && val2 == 0 && y >= 4 && x >= 4 && max1 > max2)
                        {
                            returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.Close - stoploss)), Scrip = e.Name, Stoploss = stoploss, Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                        }
                    }
                    else if (data.Where(a => a.STrend.Trend == 1).Count() == 0 && drFirst.Close < drFirst.SMA20 && drFirst.Close < drFirst.SMA50 && drFirst.Close < drFirst.SMA200 && drFirst.Close < drFirst.STrend.SuperTrend && drFirst.High >= drFirst.SMA20 && drFirst.Close < drFirst.SMA20 && drFirst.CandleType == "R")
                    {
                        DateTime previousTouchTimeStamp1 = allCandle.Where(a => a.High >= a.SMA20 && a.Close <= a.SMA20 && a.TimeStamp < drFirst.TimeStamp).Last().TimeStamp;
                        int x = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Count();
                        int val1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp && a.Close > a.SMA20).Count();
                        double min1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Min(a => a.Close);

                        DateTime previousTouchTimeStamp2 = allCandle.Where(a => a.High >= a.SMA20 && a.Close <= a.SMA20 && a.TimeStamp < previousTouchTimeStamp1).Last().TimeStamp;
                        int y = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Count();
                        int val2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1 && a.Close > a.SMA20).Count();
                        double min2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Min(a => a.Close);
                        double stoploss = Math.Max(allCandle.Where(a => a.TimeStamp == previousTouchTimeStamp2).First().High, l.Max());
                        DateTime previousTouchTimeStamp3 = allCandle.Where(a => a.High >= a.SMA20 && a.Close <= a.SMA20 && a.TimeStamp < previousTouchTimeStamp2).Last().TimeStamp;
                        int z = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp3 && a.TimeStamp < previousTouchTimeStamp2).Count();
                        int val3 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp3 && a.TimeStamp < previousTouchTimeStamp2 && a.Close > a.SMA20).Count();
                        double min3 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp3 && a.TimeStamp < previousTouchTimeStamp2).Min(a => a.Close);

                        if (val1 == 0 && val2 == 0 && y >= 4 && x >= 4 && min1 < min2)
                        {
                            returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (stoploss - drFirst.Close)), Scrip = e.Name, Stoploss = stoploss, Strategy = "1", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                        }
                    }

                }
                return returnOrder;
            }


        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + e.Name + Environment.NewLine);
        }

        return null;
    }

    public static Order FailureSetup(Equity e, List<Candle> nifty)
    {

        try
        {
            if (e.TestAtMinute < -2 || e.TestAtMinute > 60)
            {
                return null;
            }
            Order returnOrder = null;
            string scrip = e.Name;
            int k = e.TestAtMinute;

            List<Candle> allCandle = e.allDaa;
            List<Candle> data = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();
            List<Candle> dataNifty = nifty.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();
            List<Candle> prevDayData = allCandle.Where(a => a.TimeStamp.Date == allCandle[allCandle.IndexOf(data.First()) - 1].TimeStamp.Date).ToList();
            List<Candle> prevDayDatNifty = nifty.Where(a => a.TimeStamp.Date == nifty[nifty.IndexOf(dataNifty.First()) - 1].TimeStamp.Date).ToList();

            if (true)
            {
                double NiftyClose = dataNifty[k + 2].Close;
                double prevDayCloseNIfty = prevDayDatNifty.Last().Close;




                Candle drLast = null;
                Candle drFirst = data[k + 2];


                Candle drCurrentDayfirst = data[0];




                double currentDayOpen = drCurrentDayfirst.Open;
                double open = drFirst.Open;
                double close = drFirst.Close;
                double high = drFirst.High;
                double low = drFirst.Low;
                double vol = drFirst.Volume;

                string candle = drFirst.CandleType;



                double sma501 = drFirst.SMA50;
                double sma2001 = drFirst.SMA200;
                double sma201 = drFirst.SMA20;
                double atr = drFirst.ATR * 1.25;
                double supertrend = drFirst.STrend.SuperTrend;

                List<double> l = new List<double>();
                l.Add(sma201);
                l.Add(sma501);
                l.Add(sma2001);
                l.Add(supertrend);





                DateTime timeStamp = GetTimeStamp(-2, e.TransactionTradingDate);
                DateTime timeStampLast = GetTimeStamp(k, e.TransactionTradingDate);

                double previousDayMaxVolume = prevDayData.Max(a => a.Volume);
                double breakoutHigh = data.Max(a => a.High);
                double bHVol = data.Where(a => a.High == breakoutHigh).First().Volume;

                double breakoutLow = data.Min(a => a.Low);
                double bLVol = data.Where(a => a.Low == breakoutLow).First().Volume;
                double maxVolume = drCurrentDayfirst.Close * drCurrentDayfirst.Volume;
                double maxV = data.Max(a => a.Volume);
                string maxVCandle = data.Where(a => a.Volume == maxV).First().CandleType;

                double per = (drFirst.ATRStopLoss * 3 / close) * 100;
                var dataToProcess = data.ToList();
                int cB = data.Where(a => a.Close < a.SMA20 || a.Close < a.SMA50 || a.Close < a.SMA200 || a.Close < a.STrend.SuperTrend).Count();
                int cS = data.Where(a => a.Close > a.SMA20 || a.Close > a.SMA50 || a.Close > a.SMA200 || a.Close > a.STrend.SuperTrend).Count();

                List<Liner> listOfClose = new List<Liner>();
                int period = 5;

                File.AppendAllText(@"C:\Jai Sri Thakur Ji\abc.txt", e.Name + e.TransactionTradingDate + Environment.NewLine);

                double stopLoss = drFirst.CandleType == "G" ? drFirst.Low : drFirst.High;
                double d1 = Math.Abs(drFirst.Close - drFirst.STrend.SuperTrend);
                double d2 = Math.Abs(drFirst.Close - drFirst.SMA200);

                double quantity = Convert.ToInt32(e.MaxRisk / (breakoutHigh - breakoutLow));
                if (drFirst.Close > 100) //Math.Abs(drFirst.Close - stopLoss) * quantity <= e.MaxRisk * 0.4)
                {
                    if (drFirst.Close > drFirst.SMA20 && drFirst.Open < drFirst.SMA20 && drFirst.CandleType == "G" && drFirst.Close > l.Max())
                    {

                        DateTime previousTouchTimeStamp1 = allCandle.Where(a => a.High >= a.SMA20 && a.Close <= a.SMA20 && a.TimeStamp < drFirst.TimeStamp).Last().TimeStamp;
                        int x = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Count();
                        int val1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp && a.Close > a.SMA20).Count();
                        double min1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Min(a => a.Close);

                        DateTime previousTouchTimeStamp2 = allCandle.Where(a => a.High >= a.SMA20 && a.Close <= a.SMA20 && a.TimeStamp < previousTouchTimeStamp1).Last().TimeStamp;
                        int y = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Count();
                        int val2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1 && a.Close > a.SMA20).Count();




                        double stoploss = Math.Min(allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2).Min(b => b.Low), l.Min());
                        if (val1 == 0 && val2 == 0 && y >= 4 && x >= 4)
                        {
                            returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.Close - stoploss)), Scrip = e.Name, Stoploss = stoploss, Strategy = "Failure", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                        }
                    }
                    else if (drFirst.Close < drFirst.SMA20 && drFirst.Open > drFirst.SMA20 && drFirst.CandleType == "R" && drFirst.Close < l.Min())
                    {
                        DateTime previousTouchTimeStamp1 = allCandle.Where(a => a.Low <= a.SMA20 && a.Close >= a.SMA20 && a.TimeStamp < drFirst.TimeStamp).Last().TimeStamp;
                        int val1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp && a.Close < a.SMA20).Count();
                        int x = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Count();
                        double max1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Max(a => a.Close);
                        DateTime previousTouchTimeStamp2 = allCandle.Where(a => a.Low <= a.SMA20 && a.Close >= a.SMA20 && a.TimeStamp < previousTouchTimeStamp1).Last().TimeStamp;
                        int val2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1 && a.Close < a.SMA20).Count();
                        int y = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Count();
                        double max2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Max(a => a.Close);
                        double stoploss = Math.Max(allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2).Max(b => b.High), l.Max());

                        if (val1 == 0 && val2 == 0 && y >= 4 && x >= 4)
                        {
                            returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (stoploss - drFirst.Close)), Scrip = e.Name, Stoploss = stoploss, Strategy = "Failure", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                        }
                    }

                }
                return returnOrder;
            }


        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + e.Name + Environment.NewLine);
        }

        return null;
    }

    public static Order MyTradeSetup(Equity e, List<Candle> nifty)
    {
        if (e.Name == "NIFTY 50")
            return null;
        try
        {
            if (e.TestAtMinute < -2 || e.TestAtMinute > 60)
            {
                return null;
            }
            Order returnOrder = null;
            string scrip = e.Name;
            int k = e.TestAtMinute;

            List<Candle> allCandle = e.allDaa;
            List<Candle> data = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();
            List<Candle> dataNifty = nifty.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();
            List<Candle> prevDayData = allCandle.Where(a => a.TimeStamp.Date == allCandle[allCandle.IndexOf(data.First()) - 1].TimeStamp.Date).ToList();
            List<Candle> prevDayData1 = allCandle.Where(a => a.TimeStamp.Date == allCandle[allCandle.IndexOf(prevDayData.First()) - 1].TimeStamp.Date).ToList();
            List<Candle> prevDayData2 = allCandle.Where(a => a.TimeStamp.Date == allCandle[allCandle.IndexOf(prevDayData1.First()) - 1].TimeStamp.Date).ToList();
            List<Candle> prevDayData3 = allCandle.Where(a => a.TimeStamp.Date == allCandle[allCandle.IndexOf(prevDayData2.First()) - 1].TimeStamp.Date).ToList();
            //List<Candle> prevDayData4 = allCandle.Where(a => a.TimeStamp.Date == allCandle[allCandle.IndexOf(prevDayData3.First()) - 1].TimeStamp.Date).ToList();
            List<Candle> prevDayDatNifty = nifty.Where(a => a.TimeStamp.Date == nifty[nifty.IndexOf(dataNifty.First()) - 1].TimeStamp.Date).ToList();

            if (true)
            {
                double NiftyClose = dataNifty[k + 2].Close;
                double prevDayCloseNIfty = prevDayDatNifty.Last().Close;

                Candle drFirst = data[k + 2];

                Candle drCurrentDayfirst = data[0];

                double currentDayOpen = drCurrentDayfirst.Open;
                double open = drFirst.Open;
                double close = drFirst.Close;
                double high = drFirst.High;
                double low = drFirst.Low;
                double vol = drFirst.Volume;

                string candle = drFirst.CandleType;

                double sma501 = drFirst.SMA50;
                double sma2001 = drFirst.SMA200;
                double sma201 = drFirst.SMA20;
                double atr = drFirst.ATR * 1.25;
                double supertrend = drFirst.STrend.SuperTrend;

                List<double> l = new List<double>();
                l.Add(sma201);
                l.Add(sma501);
                l.Add(sma2001);
                l.Add(supertrend);

                DateTime timeStamp = GetTimeStamp(-2, e.TransactionTradingDate);
                DateTime timeStampLast = GetTimeStamp(k, e.TransactionTradingDate);


                double previousDayMaxVolume = prevDayData.Max(a => a.Volume);
                double previousDayMaxVolume1 = prevDayData1.Max(a => a.Volume);
                double previousDayMaxVolume2 = prevDayData2.Max(a => a.Volume);
                double previousDayMaxVolume3 = prevDayData3.Max(a => a.Volume);
                //double previousDayMaxVolume4 = prevDayData4.Max(a => a.Volume);
                double breakoutHigh = data.Max(a => a.High);
                double bHVol = data.Where(a => a.High == breakoutHigh).First().Volume;

                double breakoutLow = data.Min(a => a.Low);
                double bLVol = data.Where(a => a.Low == breakoutLow).First().Volume;
                double maxVolume = drCurrentDayfirst.Close * drCurrentDayfirst.Volume;
                double maxV = data.Max(a => a.Volume);
                double maxV1 = prevDayData[0].Volume;
                double maxV2 = prevDayData1[0].Volume;
                double maxV3 = prevDayData2[0].Volume;
                double maxV4 = prevDayData3[0].Volume;
                //double maxV5 = prevDayData4[0].Volume;


                double maxV22 = 0;

                if (e.TestAtMinute > -2)
                {
                    maxV22 = data.Where(a => a.Volume < maxV).Max(a => a.Volume);
                }


                string maxVCandle = data.Where(a => a.Volume == maxV).First().CandleType;

                List<Liner> listOfClose = new List<Liner>();





                double d1 = Math.Abs(drFirst.Close - drFirst.STrend.SuperTrend);
                double d2 = Math.Abs(drFirst.Close - drFirst.SMA200);
                if (e.TestAtMinute == 10)
                {
                    //if ((maxV >= 3 * previousDayMaxVolume || maxV1 > 3 * previousDayMaxVolume1 || maxV2 > 3 * previousDayMaxVolume2 || maxV3 > 3 * previousDayMaxVolume3 || maxV4 > 3 * previousDayMaxVolume4))
                    //{
                    if (data.All(a => a.Close <= drCurrentDayfirst.High && a.Close >= drCurrentDayfirst.Low))
                    {
                        DateTime buyB = DateTime.MaxValue;
                        DateTime sellB = DateTime.MaxValue;
                        var buyX = e.allDaa.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date && a.TimeStamp > drFirst.TimeStamp && a.Close > drCurrentDayfirst.High && a.TimeStamp < TokenChannel.GetTimeStamp(55, a.TimeStamp.Date));
                        var buyY = e.allDaa.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date && a.TimeStamp > drFirst.TimeStamp && a.Close < drCurrentDayfirst.Low && a.TimeStamp < TokenChannel.GetTimeStamp(55, a.TimeStamp.Date));
                        if (buyX.Count() > 0)
                        {
                            buyB = buyX.First().TimeStamp;
                        }
                        else if (buyY.Count() > 0)
                        {
                            sellB = buyY.First().TimeStamp;
                        }
                        if (buyB < sellB)
                        {
                            returnOrder = new Order() { EntryPrice = buyX.First().Close, High = drCurrentDayfirst.High, Low = drCurrentDayfirst.Low, Quantity = Convert.ToInt32(e.MaxRisk / (drCurrentDayfirst.High - drCurrentDayfirst.Low)), Scrip = e.Name, Stoploss = drCurrentDayfirst.Low, Strategy = "Gap", TimeStamp = buyX.First().TimeStamp, TransactionType = "BM", Volume = vol };
                        }
                        else if (sellB < buyB)
                        {
                            returnOrder = new Order() { EntryPrice = buyY.First().Close, High = drCurrentDayfirst.High, Low = drCurrentDayfirst.Low, Quantity = Convert.ToInt32(e.MaxRisk / (drCurrentDayfirst.High - drCurrentDayfirst.Low)), Scrip = e.Name, Stoploss = drCurrentDayfirst.High, Strategy = "Gap", TimeStamp = buyY.First().TimeStamp, TransactionType = "SM", Volume = vol };
                        }
                    }
                    //if (maxVolume > 10000000)
                    //{
                    //    if (drFirst.CandleType == "G" && close > l.Max() && low <= l.Max() && sma201 > sma501 && sma501 > sma2001)
                    //    {
                    //        returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (breakoutHigh - breakoutLow)), Scrip = e.Name, Stoploss = drFirst.Low, Strategy = "Gap", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                    //    }
                    //    else if (drFirst.CandleType == "R" && close < l.Min() && high >= l.Min() && sma201 < sma501 && sma501 < sma2001)
                    //    {
                    //        returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (breakoutHigh - breakoutLow)), Scrip = e.Name, Stoploss = drFirst.High, Strategy = "Gap", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                    //    }
                    //}
                    //}
                }
                return returnOrder;

                //if (drFirst.Close > 100)
                //{
                //    if (e.TestAtMinute > 1)
                //    {
                //        List<Candle> str = data.Where(a => a.TimeStamp < drFirst.TimeStamp).OrderByDescending(a => a.TimeStamp).Take(4).OrderBy(a => a.TimeStamp).ToList();
                //        double strHigh = str.Max(a => a.High);
                //        double strLow = str.Min(a => a.Low);
                //        double std = ((breakoutHigh - breakoutLow) * 70 / 100);
                //        if (str[3].Volume < maxV22 && maxV > 3 * previousDayMaxVolume)
                //        {
                //            if (breakoutHigh == strHigh && str.Where(a => a.CandleType == "G").Count() >= 4 && drFirst.CandleType == "R" && str[0].Close < str[3].Close && str.Where(a => a.Close - a.Open > a.High - a.Close && a.Close - a.Open > a.Open - a.Low).Count() >= 3)
                //            {
                //                double stoploss = (str[3].High + str[0].Low) / 2;
                //                returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / std), Scrip = e.Name, Stoploss = drFirst.Close - std, Strategy = "Strength", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };

                //            }
                //            else if (breakoutLow == strLow && drFirst.Close < drCurrentDayfirst.Open && str.Where(a => a.CandleType == "R").Count() >= 4 && drFirst.CandleType == "G" && str[0].Close > str[3].Close && str.Where(a => a.Open - a.Close > a.High - a.Open && a.Open - a.Close > a.Close - a.Low).Count() >= 3)
                //            {

                //                double stoploss = (str[0].High + str[3].Low) / 2;
                //                returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / std), Scrip = e.Name, Stoploss = drFirst.Close + std, Strategy = "Strength", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                //            }
                //        }
                //    }



                //    if (e.TestAtMinute < 5 && maxV > previousDayMaxVolume && maxVolume > 10000000)
                //    {
                //        if (drFirst.Close > l.Max() && breakoutLow < l.Min())
                //        {

                //            returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.Close - breakoutLow)), Scrip = e.Name, Stoploss = l.Min(), Strategy = "AllCross", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };

                //        }
                //        else if (drFirst.Close < l.Min() && breakoutHigh > l.Max())
                //        {

                //            returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (breakoutHigh - drFirst.Close)), Scrip = e.Name, Stoploss = l.Max(), Strategy = "AllCross", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                //        }
                //    }



                //    if (drFirst.Close > l.Max() && drFirst.Close > drFirst.SMA20 && drFirst.Low <= drFirst.SMA20)
                //    {
                //        DateTime previousTouchTimeStamp1 = allCandle.Where(a => a.Low <= a.SMA20 && a.Close >= a.SMA20 && a.TimeStamp < drFirst.TimeStamp).Last().TimeStamp;
                //        int val1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp && a.Close < a.SMA20).Count();
                //        var e1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp);
                //        int x = e1.Count();
                //        if (x > 0)
                //        {
                //            double max1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Max(a => a.Close);
                //            DateTime previousTouchTimeStamp2 = allCandle.Where(a => a.Low <= a.SMA20 && a.Close >= a.SMA20 && a.TimeStamp < previousTouchTimeStamp1).Last().TimeStamp;
                //            int val2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1 && a.Close < a.SMA20).Count();
                //            var e2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1);
                //            int y = e2.Count();
                //            if (y > 0)
                //            {
                //                double max2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Max(a => a.Close);


                //                if (val1 == 0 && val2 == 0 && y >= 4 && x >= 4 && max1 > max2 && d2 <= d1 * 1.5 && data.Where(a => a.STrend.Trend == -1).Count() == 0 && drFirst.CandleType == "G")
                //                {
                //                    double stoploss = Math.Min(allCandle.Where(a => a.TimeStamp >= previousTouchTimeStamp2).First().Low, l.Min());
                //                    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.Close - stoploss)), Scrip = e.Name, Stoploss = stoploss, Strategy = "Continuation", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                //                }
                //            }
                //        }

                //    }
                //    else if (drFirst.High >= drFirst.SMA20 && drFirst.Close < l.Min() && drFirst.Close < drFirst.STrend.SuperTrend && drFirst.Close < drFirst.SMA20)
                //    {
                //        DateTime previousTouchTimeStamp1 = allCandle.Where(a => a.High >= a.SMA20 && a.Close <= a.SMA20 && a.TimeStamp < drFirst.TimeStamp).Last().TimeStamp;
                //        var e1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp);
                //        int x = e1.Count();
                //        if (x > 0)
                //        {
                //            int val1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp && a.Close > a.SMA20).Count();

                //            double min1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Min(a => a.Close);

                //            DateTime previousTouchTimeStamp2 = allCandle.Where(a => a.High >= a.SMA20 && a.Close <= a.SMA20 && a.TimeStamp < previousTouchTimeStamp1).Last().TimeStamp;
                //            var e2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1);
                //            int y = e2.Count();
                //            if (y > 0)
                //            {
                //                int val2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1 && a.Close > a.SMA20).Count();
                //                double min2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Min(a => a.Close);


                //                if (val1 == 0 && val2 == 0 && y >= 4 && x >= 4 && min1 < min2 && d2 <= d1 * 1.5 && data.Where(a => a.STrend.Trend == 1).Count() == 0 && drFirst.CandleType == "R")
                //                {
                //                    double stoploss = Math.Max(allCandle.Where(a => a.TimeStamp >= previousTouchTimeStamp2).First().High, l.Max());
                //                    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (stoploss - drFirst.Close)), Scrip = e.Name, Stoploss = stoploss, Strategy = "Continuation", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                //                }
                //            }
                //        }

                //    }



                //    if (drFirst.Close > drFirst.SMA20 && drFirst.Open < drFirst.SMA20 && drFirst.CandleType == "G" && drFirst.Close > l.Max())
                //    {

                //        DateTime previousTouchTimeStamp1 = allCandle.Where(a => a.High >= a.SMA20 && a.Close <= a.SMA20 && a.TimeStamp < drFirst.TimeStamp).Last().TimeStamp;
                //        int x = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Count();
                //        int val1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp && a.Close > a.SMA20).Count();


                //        DateTime previousTouchTimeStamp2 = allCandle.Where(a => a.High >= a.SMA20 && a.Close <= a.SMA20 && a.TimeStamp < previousTouchTimeStamp1).Last().TimeStamp;
                //        int y = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Count();
                //        int val2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1 && a.Close > a.SMA20).Count();



                //        if (val1 == 0 && val2 == 0 && y >= 4 && x >= 4)
                //        {

                //            double stoploss = Math.Min(allCandle.Where(a => a.TimeStamp >= previousTouchTimeStamp2).Min(b => b.Low), l.Min());
                //            returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.Close - stoploss)), Scrip = e.Name, Stoploss = stoploss, Strategy = "Failure", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                //        }
                //    }


                //    else if (drFirst.Close < drFirst.SMA20 && drFirst.Open > drFirst.SMA20 && drFirst.CandleType == "R" && drFirst.Close < l.Min())
                //    {
                //        DateTime previousTouchTimeStamp1 = allCandle.Where(a => a.Low <= a.SMA20 && a.Close >= a.SMA20 && a.TimeStamp < drFirst.TimeStamp).Last().TimeStamp;
                //        int val1 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp && a.Close < a.SMA20).Count();
                //        int x = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp1 && a.TimeStamp < drFirst.TimeStamp).Count();

                //        DateTime previousTouchTimeStamp2 = allCandle.Where(a => a.Low <= a.SMA20 && a.Close >= a.SMA20 && a.TimeStamp < previousTouchTimeStamp1).Last().TimeStamp;
                //        int val2 = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1 && a.Close < a.SMA20).Count();
                //        int y = allCandle.Where(a => a.TimeStamp > previousTouchTimeStamp2 && a.TimeStamp < previousTouchTimeStamp1).Count();



                //        if (val1 == 0 && val2 == 0 && y >= 4 && x >= 4)
                //        {
                //            double stoploss = Math.Max(allCandle.Where(a => a.TimeStamp >= previousTouchTimeStamp2).Max(b => b.High), l.Max());
                //            returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / (stoploss - drFirst.Close)), Scrip = e.Name, Stoploss = stoploss, Strategy = "Failure", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                //        }

                //    }


                //}
                return returnOrder;
            }


        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + e.Name + Environment.NewLine);
        }

        return null;

    }


    public static Order MyGapSetup(Equity e, List<Candle> nifty)
    {
        if (e.Name == "NIFTY 50")
            return null;
        try
        {
            if (e.TestAtMinute < -2 || e.TestAtMinute > 60)
            {
                return null;
            }
            Order returnOrder = null;
            string scrip = e.Name;
            int k = e.TestAtMinute;

            List<Candle> allCandle = e.allDaa;
            List<Candle> data = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();



            Candle drFirst = data.Last();

            Candle drCurrentDayfirst = data[0];

            double currentDayOpen = drCurrentDayfirst.Open;
            double open = drFirst.Open;
            double close = drFirst.Close;
            double high = drFirst.High;
            double low = drFirst.Low;
            double vol = drFirst.Volume;

            string candle = drFirst.CandleType;

            double sma501 = drFirst.SMA50;
            double sma2001 = drFirst.SMA200;
            double sma201 = drFirst.SMA20;
            double atr = drFirst.ATR * 1.25;
            //double supertrend = drFirst.STrend.SuperTrend;

            //List<double> l = new List<double>();
            //l.Add(sma201);
            //l.Add(sma501);
            //l.Add(sma2001);
            //l.Add(supertrend);

            DateTime timeStamp = GetTimeStamp(-2, e.TransactionTradingDate);
            DateTime timeStampLast = GetTimeStamp(k, e.TransactionTradingDate);



            if (e.TestAtMinute >= -2)
            {
                List<Candle> dataTill9 = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(9, e.TransactionTradingDate)).ToList();

                if (drCurrentDayfirst.CandleType == "G")
                {
                    //if (((close - data.Min(b => b.Low)) / close) * 100 >= 1)
                    ////{
                    returnOrder = new Order() { EntryPrice = drFirst.Close, High = dataTill9.Max(a => a.High), Low = dataTill9.Min(a => a.Low), Quantity = Convert.ToInt32((e.MaxRisk / 3) / ((drCurrentDayfirst.High - drCurrentDayfirst.Low) / 2)), Scrip = e.Name, Stoploss = drCurrentDayfirst.Close - ((drCurrentDayfirst.High - drCurrentDayfirst.Low) / 2), Strategy = "Gap", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                    //}
                }
                else if (drCurrentDayfirst.CandleType == "R")
                {
                    //if (((data.Max(b => b.High) - drFirst.Close)) * 100 >= 1)
                    //{
                    returnOrder = new Order() { EntryPrice = drFirst.Close, High = dataTill9.Max(a => a.High), Low = dataTill9.Min(a => a.Low), Quantity = Convert.ToInt32((e.MaxRisk / 3) / ((drCurrentDayfirst.High - drCurrentDayfirst.Low) / 2)), Scrip = e.Name, Stoploss = drCurrentDayfirst.Close + ((drCurrentDayfirst.High - drCurrentDayfirst.Low) / 2), Strategy = "Gap", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };

                    //}
                }

            }
            return returnOrder;

        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + e.Name + Environment.NewLine);
        }
        return null;
    }

    public static Order MyTrendSetup(Equity e, List<Candle> nifty)
    {
        if (e.Name == "NIFTY 50")
            return null;
        try
        {
            if (e.TestAtMinute < -2 || e.TestAtMinute > 60)
            {
                return null;
            }
            Order returnOrder = null;
            string scrip = e.Name;
            int k = e.TestAtMinute;

            List<Candle> allCandle = e.allDaa;
            List<Candle> data = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();



            Candle drFirst = data.Last();

            Candle drCurrentDayfirst = data[0];

            List<Candle> prevDayData = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();
            double previousLow = e.allDaa.Where(b => b.TimeStamp.Date == e.allDaa[e.allDaa.IndexOf(drCurrentDayfirst) - 1].TimeStamp.Date).Min(b => b.Low);
            double previousHigh = e.allDaa.Where(b => b.TimeStamp.Date == e.allDaa[e.allDaa.IndexOf(drCurrentDayfirst) - 1].TimeStamp.Date).Max(b => b.High);

            double currentDayOpen = drCurrentDayfirst.Open;
            double open = drFirst.Open;
            double close = drFirst.Close;
            double high = drFirst.High;
            double low = drFirst.Low;
            double vol = drFirst.Volume;

            string candle = drFirst.CandleType;

            double sma501 = drFirst.SMA50;
            double sma2001 = drFirst.SMA200;
            double sma201 = drFirst.SMA20;
            double atr = drFirst.ATR * 1.25;
            double supertrend = drFirst.STrend.SuperTrend;

            List<double> l = new List<double>();
            l.Add(sma201);
            l.Add(sma501);
            l.Add(sma2001);
            l.Add(supertrend);

            DateTime timeStamp = GetTimeStamp(-2, e.TransactionTradingDate);
            DateTime timeStampLast = GetTimeStamp(k, e.TransactionTradingDate);



            if (e.TestAtMinute >= -2)
            {
                List<Candle> dataTill9 = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();

                if (drFirst.STrend.Trend == 1)
                {

                    returnOrder = new Order() { EntryPrice = drFirst.STrend.SuperTrend, High = dataTill9.Max(a => a.High), Low = dataTill9.Min(a => a.Low), Quantity = Convert.ToInt32((e.MaxRisk / 3) / ((drFirst.High - drFirst.STrend.SuperTrend))), Scrip = e.Name, Stoploss = drFirst.STrend.SuperTrend - (drFirst.High - drFirst.STrend.SuperTrend), Strategy = "AllCross", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };

                }
                else if (drFirst.STrend.Trend == -1)
                {
                    returnOrder = new Order() { EntryPrice = drFirst.STrend.SuperTrend, High = dataTill9.Max(a => a.High), Low = dataTill9.Min(a => a.Low), Quantity = Convert.ToInt32((e.MaxRisk / 3) / ((drFirst.STrend.SuperTrend - drFirst.Low))), Scrip = e.Name, Stoploss = drFirst.STrend.SuperTrend + (drFirst.STrend.SuperTrend - drFirst.Low), Strategy = "AllCross", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };

                }

            }
            return returnOrder;

        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + e.Name + Environment.NewLine);
        }
        return null;
    }

    public static Order MyDoziSetup(Equity e, List<Candle> nifty)
    {
        if (e.Name == "NIFTY 50")
            return null;
        try
        {
            if (e.TestAtMinute < -2 || e.TestAtMinute > 60)
            {
                return null;
            }
            Order returnOrder = null;
            string scrip = e.Name;
            int k = e.TestAtMinute;

            List<Candle> allCandle = e.allDaa;
            List<Candle> data = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();



            Candle drFirst = data.Last();

            Candle drCurrentDayfirst = data[0];

            List<Candle> prevDayData = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();
            double previousLow = e.allDaa.Where(b => b.TimeStamp.Date == e.allDaa[e.allDaa.IndexOf(drCurrentDayfirst) - 1].TimeStamp.Date).Min(b => b.Low);
            double previousHigh = e.allDaa.Where(b => b.TimeStamp.Date == e.allDaa[e.allDaa.IndexOf(drCurrentDayfirst) - 1].TimeStamp.Date).Max(b => b.High);

            double currentDayOpen = drCurrentDayfirst.Open;
            double open = drFirst.Open;
            double close = drFirst.Close;
            double high = drFirst.High;
            double low = drFirst.Low;
            double vol = drFirst.Volume;

            string candle = drFirst.CandleType;

            double sma501 = drFirst.SMA50;
            double sma2001 = drFirst.SMA200;
            double sma201 = drFirst.SMA20;
            double atr = drFirst.ATR * 1.25;
            double supertrend = drFirst.STrend.SuperTrend;

            List<double> l = new List<double>();
            l.Add(sma201);
            l.Add(sma501);
            l.Add(sma2001);
            l.Add(supertrend);

            DateTime timeStamp = GetTimeStamp(-2, e.TransactionTradingDate);
            DateTime timeStampLast = GetTimeStamp(k, e.TransactionTradingDate);



            if (e.TestAtMinute >= -2)
            {
                List<Candle> dataTill9 = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();

                if (drFirst.CandleType == "G")
                {

                    returnOrder = new Order() { EntryPrice = drFirst.Close, High = dataTill9.Max(a => a.High), Low = dataTill9.Min(a => a.Low), Quantity = Convert.ToInt32((e.MaxRisk / 3) / ((drFirst.Close - drFirst.Low))), Scrip = e.Name, Stoploss = drFirst.Low, Strategy = "Strength", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };

                }
                else if (drFirst.CandleType == "R")
                {
                    returnOrder = new Order() { EntryPrice = drFirst.Close, High = dataTill9.Max(a => a.High), Low = dataTill9.Min(a => a.Low), Quantity = Convert.ToInt32((e.MaxRisk / 3) / ((drFirst.High - drFirst.Close))), Scrip = e.Name, Stoploss = drFirst.High, Strategy = "Strength", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };

                }

            }
            return returnOrder;

        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + e.Name + Environment.NewLine);
        }
        return null;
    }

    public static Order MyBreakoutSetup(Equity e, List<Candle> nifty)
    {
        if (e.Name == "NIFTY 50")
            return null;
        try
        {
            if (e.TestAtMinute < 9 || e.TestAtMinute > 60)
            {
                return null;
            }
            Order returnOrder = null;
            string scrip = e.Name;
            int k = e.TestAtMinute;

            List<Candle> allCandle = e.allDaa;
            List<Candle> data = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(e.TestAtMinute, e.TransactionTradingDate)).ToList();



            Candle drFirst = data[k + 2];

            Candle drCurrentDayfirst = data[0];

            double currentDayOpen = drCurrentDayfirst.Open;
            double open = drFirst.Open;
            double close = drFirst.Close;
            double high = drFirst.High;
            double low = drFirst.Low;
            double vol = drFirst.Volume;

            string candle = drFirst.CandleType;

            double sma501 = drFirst.SMA50;
            double sma2001 = drFirst.SMA200;
            double sma201 = drFirst.SMA20;
            double atr = drFirst.ATR * 1.25;
            double supertrend = drFirst.STrend.SuperTrend;

            List<double> l = new List<double>();
            l.Add(sma201);
            l.Add(sma501);
            l.Add(sma2001);
            l.Add(supertrend);

            DateTime timeStamp = GetTimeStamp(-2, e.TransactionTradingDate);
            DateTime timeStampLast = GetTimeStamp(k, e.TransactionTradingDate);



            if (e.TestAtMinute > 9)
            {
                List<Candle> dataTill9 = allCandle.Where(a => a.TimeStamp.Date == e.TransactionTradingDate.Date & a.TimeStamp <= GetTimeStamp(9, e.TransactionTradingDate)).ToList();

                if (dataTill9.All(a => a.Close <= drCurrentDayfirst.High && a.Close >= drCurrentDayfirst.Low))
                {


                    if (drFirst.Close > dataTill9.Max(a => a.High))
                    {
                        if (data.Any(a => a.Low < a.SMA20) && data.Any(a => a.STrend.Trend == -1) && data.Any(a => a.Low < a.SMA50) && close > Math.Max(sma201, sma501) && drFirst.STrend.Trend == 1)
                            returnOrder = new Order() { EntryPrice = drFirst.Close, High = dataTill9.Max(a => a.High), Low = dataTill9.Min(a => a.Low), Quantity = Convert.ToInt32(e.MaxRisk / (drFirst.Close - drCurrentDayfirst.Low)), Scrip = e.Name, Stoploss = drCurrentDayfirst.Low, Strategy = "Gap", TimeStamp = drFirst.TimeStamp, TransactionType = "BM", Volume = vol };
                    }
                    else if (drFirst.Close < dataTill9.Min(a => a.Low))
                    {
                        if (data.Any(a => a.High > a.SMA20) && data.Any(a => a.STrend.Trend == 1) && data.Any(a => a.High > a.SMA50) && close < Math.Min(sma201, sma501) && drFirst.STrend.Trend == -1)
                            returnOrder = new Order() { EntryPrice = drFirst.Close, High = dataTill9.Max(a => a.High), Low = dataTill9.Min(a => a.Low), Quantity = Convert.ToInt32(e.MaxRisk / (drCurrentDayfirst.High - drFirst.Close)), Scrip = e.Name, Stoploss = drCurrentDayfirst.High, Strategy = "Gap", TimeStamp = drFirst.TimeStamp, TransactionType = "SM", Volume = vol };
                    }
                }

            }
            return returnOrder;

        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + e.Name + Environment.NewLine);
        }
        return null;
    }



    //public static Order ADX30Minute(Equity e, bool startFromScratch)
    //{

    //    try
    //    {
    //        if (e.TestAtMinute < -2 || e.TestAtMinute > 70)
    //        {
    //            return null;
    //        }
    //        Order returnOrder = null;
    //        string scrip = e.Name;

    //        string FileName = string.Empty;
    //        FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\5\" + scrip.Replace("-", string.Empty) + ".txt";
    //        string json = File.ReadAllText(FileName).Replace("]}}", string.Empty).Replace("\"", "").Replace("T", " ").Replace("+", " ");

    //        DataSet ds = new DataSet();
    //        ds.ReadXml(FileName);

    //        DataTable dt = ds.Tables[0];


    //        bool prev = false;
    //        List<Ohlc> ohlcList = new List<Ohlc>();
    //        foreach (DataRow dr in ds.Tables[0].Rows)
    //        {
    //            Ohlc ohlc = new Ohlc();
    //            ohlc.Date = Convert.ToDateTime(dr["TimeStamp"]);
    //            ohlc.Open = Convert.ToDouble(dr["f5"]);
    //            ohlc.High = Convert.ToDouble(dr["f3"]);
    //            ohlc.Low = Convert.ToDouble(dr["f4"]);
    //            ohlc.Close = Convert.ToDouble(dr["f2"]);
    //            ohlc.Volume = Convert.ToDouble(dr["f6"]);
    //            ohlc.AdjClose = Convert.ToDouble(dr["f2"]);

    //            ohlcList.Add(ohlc);

    //        }
    //        DateTime dateIndicator = e.TransactionTradingDate;

    //        ADX adx = new ADX(14);
    //        // fill ohlcList
    //        adx.Load(ohlcList);
    //        ADXSerie serie = adx.Calculate();
    //        ds.Tables[0].Columns.Add("ADX", typeof(double));
    //        ds.Tables[0].Columns.Add("DIP", typeof(double));
    //        ds.Tables[0].Columns.Add("DIN", typeof(double));
    //        ds.Tables[0].Columns.Add("adb", typeof(int));

    //        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
    //        {
    //            ds.Tables[0].Rows[i]["ADX"] = serie.ADX[i] ?? 0;
    //            ds.Tables[0].Rows[i]["DIP"] = serie.DIPositive[i] ?? 0;
    //            ds.Tables[0].Rows[i]["DIN"] = serie.DINegative[i] ?? 0;
    //            if ((serie.ADX[i] ?? 0) <= (serie.DIPositive[i] ?? 0) && (serie.ADX[i] ?? 0) <= (serie.DINegative[i] ?? 0))
    //            {
    //                ds.Tables[0].Rows[i]["adb"] = 1;
    //            }
    //            else
    //            {
    //                ds.Tables[0].Rows[i]["adb"] = 0;
    //            }

    //        }


    //        startFromScratch = false;


    //        //finalCandle = 60;
    //        for (int k = e.TestAtMinute; k <= e.TestAtMinute; k++)
    //        {
    //            DataRow drFirst = ds.Tables[0].Select("TimeStamp='" + GetTimeStamp(k, e.TransactionTradingDate) + "'")[0];
    //            DataRow drLast = ds.Tables[0].Rows[ds.Tables[0].Rows.IndexOf(drFirst) - 1];
    //            DataRow drCurrentDayfirst = ds.Tables[0].Select("TimeStamp='" + GetTimeStamp(-2, e.TransactionTradingDate) + "'")[0];
    //            DataRow drLastPrev = ds.Tables[0].Rows[ds.Tables[0].Rows.IndexOf(drCurrentDayfirst) - 1];

    //            DateTime previousDay = Convert.ToDateTime(drLastPrev["timestamp"]).Date;

    //            DataRow drLast1 = ds.Tables[0].Rows[ds.Tables[0].Rows.IndexOf(drFirst) - 2];
    //            DataRow drLast2 = ds.Tables[0].Rows[ds.Tables[0].Rows.IndexOf(drFirst) - 3];
    //            DataRow drLast3 = ds.Tables[0].Rows[ds.Tables[0].Rows.IndexOf(drFirst) - 4];

    //            double adx0 = Convert.ToDouble(drFirst["adb"]);


    //            double adx1 = Convert.ToDouble(drLast1["adb"]);


    //            double adx2 = Convert.ToDouble(drLast2["adb"]);

    //            double adx3 = Convert.ToDouble(drLast2["adb"]);






    //            double currentDayOpen = Convert.ToDouble(drCurrentDayfirst["f5"]);
    //            double open = Convert.ToDouble(drFirst["f5"]);
    //            double close = Convert.ToDouble(drFirst["f2"]);
    //            double high = Convert.ToDouble(drFirst["f3"]);
    //            double low = Convert.ToDouble(drFirst["f4"]);
    //            double vol = Convert.ToDouble(drFirst["f6"]);

    //            string candle = Convert.ToString(drFirst["candle"]);
    //            string prevCandle = Convert.ToString(drLast["candle"]);
    //            string pattern = Convert.ToString(drFirst["pattern"]);

    //            double sma501 = Convert.ToDouble(drFirst["50"]);
    //            double sma2001 = Convert.ToDouble(drFirst["200"]);
    //            double sma201 = Convert.ToDouble(drFirst["20"]);
    //            double atr = Convert.ToDouble(drFirst["atr14"]) * 1.25;
    //            double supertrend = Convert.ToDouble(drFirst["supertrend"]);

    //            List<double> l = new List<double>();
    //            l.Add(sma201);
    //            l.Add(sma501);
    //            l.Add(sma2001);
    //            l.Add(supertrend);



    //            date timeStamp = GetTimeStamp(-2, e.TransactionTradingDate);
    //            string timeStampLast = GetTimeStamp(k, e.TransactionTradingDate);
    //            string prevTimeStamp = GetTimeStamp(-2, previousDay);
    //            string prevTimeStampLast = drLastPrev["timestamp"].ToString();

    //            var data = ds.Tables[0].AsEnumerable().Where(a => Convert.ToDateTime(a.Field<String>("Timestamp")) >= Convert.ToDateTime(timeStamp) && Convert.ToDateTime(a.Field<String>("Timestamp")) <= Convert.ToDateTime(timeStampLast));
    //            var previousDayMaxVolume = ds.Tables[0].AsEnumerable().Where(a => Convert.ToDateTime(a.Field<String>("Timestamp")) >= Convert.ToDateTime(prevTimeStamp) && Convert.ToDateTime(a.Field<String>("Timestamp")) <= Convert.ToDateTime(prevTimeStampLast)).Max(a => a.Field<double>("f6"));

    //            double breakoutHigh = data.Max(a => a.Field<double>("f3"));
    //            double breakoutLow = data.Min(a => a.Field<double>("f4"));
    //            double maxVolume = data.Max(a => a.Field<double>("f2") * a.Field<double>("f6"));
    //            double maxV = data.Max(a => a.Field<double>("f6"));
    //            string maxVCandle = data.Where(a => a.Field<double>("f6") == maxV).First().Field<string>("candle");
    //            DataRow bullP = null;
    //            DataRow bearP = null;
    //            double distFromS = Math.Abs(close - supertrend);

    //            double atrStopLoss = Math.Max(atr, distFromS);
    //            double per = (atrStopLoss / close) * 100;
    //            var dataToProcess = data.ToList();
    //            List<Liner> listOfClose = new List<Liner>();

    //            if (adx0 == 0 && adx1 == 1 && adx2 == 1 && adx3 == 1)
    //            {
    //                if (candle == "G")
    //                {

    //                    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / atrStopLoss), Scrip = e.Name, Stoploss = close - atrStopLoss, Strategy = "2", TimeStamp = Convert.ToDateTime(drFirst["Timestamp"]), TransactionType = "BM", Volume = vol };
    //                }
    //                else if (candle == "R")
    //                {

    //                    returnOrder = new Order() { EntryPrice = close, High = high, Low = low, Quantity = Convert.ToInt32(e.MaxRisk / atrStopLoss), Scrip = e.Name, Stoploss = close + atrStopLoss, Strategy = "2", TimeStamp = Convert.ToDateTime(drFirst["Timestamp"]), TransactionType = "SM", Volume = vol };
    //                }
    //            }


    //            return returnOrder;
    //        }


    //    }
    //    catch (Exception ex)
    //    {
    //        File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + e.Name + Environment.NewLine);
    //    }

    //    return null;
    //}


    public static bool isBullorBear(Candle c)
    {
        double upperWick = 0, body = 0, lowerwick = 0, bullpoint = 0, bearpoint = 0;
        if (c.CandleType == "G")
        {
            //upperWick = c.High - c.Close;
            //lowerwick = c.Close - c.Low;
            //body = c.Close - c.Low;
            //bullpoint = lowerwick + body - upperWick;

            upperWick = c.High - c.Close;
            lowerwick = c.Close - c.Low;
            body = c.Close - c.Open;
            bullpoint = lowerwick + body - upperWick;
        }
        else if (c.CandleType == "R")
        {
            //upperWick = c.High - c.Open;
            //lowerwick = c.Close - c.Low;
            //body = c.Open - c.Close;
            //bearpoint = upperWick + body - lowerwick;

            upperWick = c.High - c.Open;
            lowerwick = c.Close - c.Low;
            body = c.Open - c.Close;
            bearpoint = upperWick + body - lowerwick;
        }
        else
        {
            upperWick = c.High - c.Close;
            lowerwick = c.Close - c.Low;
            bullpoint = lowerwick - upperWick;
            bearpoint = upperWick - lowerwick;
        }




        if (bullpoint > bearpoint)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public static string GetPattern(double close, double low, double high, double open, double r1, double s1, double p, double st, double s50, double s20, double pdc, double ps20, double ps50, int TAM, string candleType)
    {
        string finalPattern = Touches(r1, low, high) + Touches(s1, low, high) + Touches(st, low, high) + Touches(p, low, high) + Touches(s50, low, high) + Touches(s20, low, high);
        finalPattern += Closes(r1, close) + Closes(s1, close) + Closes(st, close) + Closes(p, close) + Closes(s50, close) + Closes(s20, close) + Closes(pdc, close) + Closes(s20, s50) + Closes(ps20, ps50);
        finalPattern += candleType;
        return finalPattern;
    }

    public static string Closes(double level, double close)
    {
        if (close >= level)
        {
            return "1";
        }
        else
        {
            return "0";
        }
    }

    public static string Touches(double level, double low, double high)
    {
        if (low <= level && high >= level)
        {
            return "1";
        }
        else
        {
            return "0";
        }
    }





















    public static List<SR> WildAnalysis(double low, double high, double close, double open, List<SR> fullList)
    {
        List<SR> resultSet = new List<SR>();

        foreach (SR srObje in fullList)
        {
            if (close < srObje.price && high > srObje.price)
            {
                SR x = new SR();
                x.price = srObje.price;
                x.LevelName = srObje.LevelName;
                x.SupportOrResistance = "R";
                resultSet.Add(x);
            }
            else if (close > srObje.price && low < srObje.price)
            {
                SR x = new SR();
                x.price = srObje.price;
                x.LevelName = srObje.LevelName;
                x.SupportOrResistance = "S";
                resultSet.Add(x);
            }
        }
        return resultSet;

    }

    private bool _disposedValue = false; // To detect redundant calls
    // IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
                _stop = true;
        }
        _disposedValue = true;
    }


    // This code added by Visual Basic to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }



    public static bool iSBearishEngulfing(Candle c1, Candle c2)
    {
        if (c2.Open >= c1.Close && c1.CandleType == "G" && c2.CandleType == "R" && c2.Close <= c1.Low)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool isStockBullishUsingVolume(params Candle[] c)
    {
        double bullVol = 0, bearVol = 0;
        foreach (Candle obj in c)
        {
            if (obj.CandleType == "G" || obj.CandleType == "D")
            {
                bullVol += obj.Volume;
            }
            else
            {
                bearVol += obj.Volume;
            }
        }
        if (bullVol > bearVol)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public static double GetLowest(params Candle[] c)
    {
        double lowest = 999999;
        foreach (Candle obj in c)
        {
            if (obj.Low < lowest)
            {
                lowest = obj.Low;
            }

        }

        return lowest;
    }

    public static double GetHighest(params Candle[] c)
    {
        double highest = 0;
        foreach (Candle obj in c)
        {
            if (obj.High > highest)
            {
                highest = obj.High;
            }

        }

        return highest;
    }
    public static bool isMACDCrossBearishOver(Candle c1, Candle c2)
    {
        if (c1.MACD - c1.MACD9 >= 0 && c2.MACD - c2.MACD9 < 0)
        {
            return true;
        }
        else
        {
            return false;

        }
    }

    public static bool isMACDBearish(params Candle[] c)
    {
        foreach (Candle cd in c)
        {
            if (cd.MACD - cd.MACD9 >= 0)
            {
                return false;
            }
        }

        return true;

    }

    public static bool isMACDBullish(params Candle[] c)
    {
        foreach (Candle cd in c)
        {
            if (cd.MACD - cd.MACD9 <= 0)
            {
                return false;
            }
        }

        return true;

    }

    public static bool isMACDBullishIncreasing(params Candle[] c)
    {
        double x = 999999;
        foreach (Candle cd in c)
        {
            if (cd.MACD - cd.MACD9 <= 0 && (cd.MACD - cd.MACD9) < x)
            {

                return false;
            }
            x = cd.MACD - cd.MACD9;
        }

        return true;
    }



    public static bool isLongerBearShadow(Candle c)
    {
        if ((c.Open - c.Close) <= (c.Close - c.Low))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool isSpinningBottomBull(Candle c)
    {
        if ((c.High - c.Close) >= 2 * (c.Open - c.Low) & (c.High - c.Close) >= 2 * (c.Close - c.Open))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool isSpinningBottomBear(Candle c)
    {
        if ((c.High - c.Open) >= 1.25 * (c.Close - c.Low) & (c.High - c.Open) >= 2 * (c.Open - c.Close))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool isBearWithNoBottomShadow(Candle c)
    {
        if (((c.Close - c.Low) / c.Close) * 100 < 0.15)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool isBullWithNoUpperShadow(Candle c)
    {
        if (((c.High - c.Close) / c.Close) * 100 < 0.15)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool isBearishMaribozu(Candle c)
    {
        if (((c.Open - c.Close) / c.Close) * 100 > 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool isBullishMaribozu(Candle c)
    {
        if (((c.Close - c.Open) / c.Open) * 100 > 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public static string CandlePattern(Candle c2, Candle c1, Candle c)
    {
        string Label = string.Empty;
        double O = c.Open;
        double C = c.Close;
        double L = c.Low;
        double H = c.High;
        double O1 = c1.Open;
        double C1 = c1.Close;
        double L1 = c1.Low;
        double H1 = c1.High;
        double O2 = c2.Open;
        double C2 = c2.Close;
        double L2 = c2.Low;
        double H2 = c2.High;
        double V = c.Volume;

        if (c.Open == c.Close || Math.Abs(c.Open - c.Close) <= ((c.High - c.Low) * 0.1))
        {
            Label = "DOZI";
        }
        if (((O1 > C1) && (C > O) && (C >= O1) && (C1 >= O) && ((C - O) > (O1 - C1))))
        {
            Label = "BULLISHENGULFING";
        }
        if (((C1 > O1) && (O > C) && (O >= C1) && (O1 >= C) && ((O - C) > (C1 - O1))))
        {
            Label = "BEARISHENGULFING";
        }
        if ((((H - L) > 3 * (O - C) && ((C - L) / (.001 + H - L) > 0.6) && ((O - L) / (.001 + H - L) > 0.6))))
        {
            Label = "HAMMER";
        }
        if (((((H - L) > 4 * Math.Abs(O - C)) && ((C - L) / (.001 + H - L) >= 0.75) && ((O - L) / (.001 + H - L) >= .075))))
        {
            Label = "HANGINGMAN";
        }
        if ((C1 < O1) && (((O1 + C1) / 2) < C) && (O < C) && (O < C1) && (C < O1) && ((C - O) / (.001 + (H - L)) > 0.6))
        {
            Label = "PIERCINGLINE";
        }
        if ((C1 > O1) && (((C1 + O1) / 2) > C) && (O > C) && (O > C1) && (C > O1) && ((O - C) / (.001 + (H - L)) > .6))
        {
            Label = "DARKCLOUD";
        }
        if ((O1 > C1) && (C > O) && (C <= O1) && (C1 <= O) && ((C - O) < (O1 - C1)))
        {
            Label = "BULLISHHARAMI";
        }
        if ((C1 > O1) && (O > C) && (O <= C1) && (O1 <= C) && ((O - C) < (C1 - O1)))
        {
            Label = "BEARISHHARAMI";
        }
        if ((O2 > C2) && ((O2 - C2) / (.001 + H2 - L2) > .6) && (C2 > O1) && (O1 > C1) && ((H1 - L1) > (3 * (C1 - O1))) && (C > O) && (O > O1))
        {
            Label = "MORNINGSTAR";
        }
        if ((O1 > C1) && (O >= O1) && (C > O))
        {
            Label = "BULLISHKICKER";
        }
        if ((O1 < C1) && (O <= O1) && (C <= O))
        {
            Label = "BEARISHKICKER";
        }
        if (((H - L) > 4 * (O - C)) && ((H - C) / (.001 + H - L) >= 0.75) && ((H - O) / (.001 + H - L) >= 0.75))
        {
            Label = "SHOOTINGSTAR";
        }
        if (((H - L) > 3 * (O - C)) && ((H - C) / (.001 + H - L) > 0.6) && ((H - O) / (.001 + H - L) > 0.6))
        {
            Label = "INVERTEDHAMMER";
        }



        return Label;

    }

    public static string Trending(Candle c, Candle c1)
    {
        string Label = string.Empty;
        double O = c.Open;
        double C = c.Close;
        double L = c.Low;
        double H = c.High;
        double O1 = c1.Open;
        double C1 = c1.Close;
        double L1 = c1.Low;
        double H1 = c1.High;
        double V = c.Volume;
        if (C > O && (C - O) > (H - C) && (O - L) > (H - C) && (H - C) < (C - O) && (H - C) < (O - L) && (H - C) < C * 0.002 && C > C1 && (H - L) > C * 0.005 && (H - L) < C * 0.01 && (O - L) > C * 0.002)
        {
            Label = "BULLTRENDING";
        }
        else if (O > C && (O - C) > (C - L) && (H - O) > (C - L) && (C - L) < (O - C) && (C - L) < (H - O) && (C - L) < C * 0.002 && C < C1 && (H - L) > C * 0.005 && (H - L) > C * 0.01 && (H - O) > C * 0.002)
        {
            Label = "BEARTRENDING";
        }
        return Label;
    }

    //public static string[] BearishPattern = new string[] { "SHOOTINGSTAR", "BEARISHKICKER", "BEARISHHARAMI", "DARKCLOUD", "HANGINGMAN", "BEARISHENGULFING", "DOZI", "INVERTEDHAMMER", "BULLTRENDING" };
    //public static string[] BullishPattern = new string[] { "BULLISHKICKER", "MORNINGSTAR", "BULLISHHARAMI", "PIERCINGLINE", "HAMMER", "BULLISHENGULFING", "DOZI", "BEARTRENDING" };

    public static string[] BearishPattern = new string[] { "BEARTRENDING" };
    public static string[] BullishPattern = new string[] { "BULLTRENDING" };

    public static List<SR> GetPredifinedLevels(StockData todaysLevel)
    {
        List<SR> list = new List<SR>();
        list.Add(new SR { price = Math.Round(todaysLevel.dPP, 1), LevelName = "dPP" });
        list.Add(new SR { price = Math.Round(todaysLevel.dR1, 1), LevelName = "dR1" });
        list.Add(new SR { price = Math.Round(todaysLevel.dR2, 1), LevelName = "dR2" });
        list.Add(new SR { price = Math.Round(todaysLevel.dR3, 1), LevelName = "dR3" });
        list.Add(new SR { price = Math.Round(todaysLevel.dS1, 1), LevelName = "dS1" });
        list.Add(new SR { price = Math.Round(todaysLevel.dS2, 1), LevelName = "dS2" });
        list.Add(new SR { price = Math.Round(todaysLevel.dS3, 1), LevelName = "dS3" });
        list.Add(new SR { price = Math.Round(todaysLevel.wPP, 1), LevelName = "wPP" });
        list.Add(new SR { price = Math.Round(todaysLevel.wR1, 1), LevelName = "wR1" });
        list.Add(new SR { price = Math.Round(todaysLevel.wR2, 1), LevelName = "wR2" });
        list.Add(new SR { price = Math.Round(todaysLevel.wR3, 1), LevelName = "wR3" });
        list.Add(new SR { price = Math.Round(todaysLevel.wS1, 1), LevelName = "wS1" });
        list.Add(new SR { price = Math.Round(todaysLevel.wS2, 1), LevelName = "wS2" });
        list.Add(new SR { price = Math.Round(todaysLevel.wS3, 1), LevelName = "wS3" });
        list.Add(new SR { price = Math.Round(todaysLevel.mPP, 1), LevelName = "mPP" });
        list.Add(new SR { price = Math.Round(todaysLevel.mR1, 1), LevelName = "mR1" });
        list.Add(new SR { price = Math.Round(todaysLevel.mR2, 1), LevelName = "mR2" });
        list.Add(new SR { price = Math.Round(todaysLevel.mR3, 1), LevelName = "mR3" });
        list.Add(new SR { price = Math.Round(todaysLevel.mS1, 1), LevelName = "mS1" });
        list.Add(new SR { price = Math.Round(todaysLevel.mS2, 1), LevelName = "mS2" });
        list.Add(new SR { price = Math.Round(todaysLevel.mS3, 1), LevelName = "mS3" });
        list.Add(new SR { price = Math.Round(todaysLevel.yPP, 1), LevelName = "yPP" });
        list.Add(new SR { price = Math.Round(todaysLevel.yR1, 1), LevelName = "yR1" });
        list.Add(new SR { price = Math.Round(todaysLevel.yR2, 1), LevelName = "yR2" });
        list.Add(new SR { price = Math.Round(todaysLevel.yR3, 1), LevelName = "yR3" });
        list.Add(new SR { price = Math.Round(todaysLevel.yS1, 1), LevelName = "yS1" });
        list.Add(new SR { price = Math.Round(todaysLevel.yS2, 1), LevelName = "yS2" });
        list.Add(new SR { price = Math.Round(todaysLevel.yS3, 1), LevelName = "yS3" });
        list.Add(new SR { price = Math.Round(todaysLevel.SuperTrend, 1), LevelName = "DST" });
        list.Add(new SR { price = Math.Round(todaysLevel.SMA20, 1), LevelName = "D20" });
        list.Add(new SR { price = Math.Round(todaysLevel.SMA50, 1), LevelName = "D50" });
        list.Add(new SR { price = Math.Round(todaysLevel.SMA200, 1), LevelName = "D200" });
        return list;
    }

    public static void Backup()
    {
        DirectoryInfo dr = new DirectoryInfo(@"C:\Jai Sri Thakur Ji\Nifty Analysis\ZERODHA");

        string sourcePath = @"C:\Jai Sri Thakur Ji\Nifty Analysis\ZERODHA";

        string destFolder = @"C:\Jai Sri Thakur Ji\";

        string destFilename = string.Format("Backup_{0}.zip", DateTime.Now.ToString("ddMMyyyyhhmmss"));

        Telerik.WinControls.Zip.Extensions.ZipFile.CreateFromDirectory(sourcePath, destFolder + destFilename);

    }




}
public class Trendline
{
    /// <summary>
    /// Get's the line's best fit slope of the line
    /// </summary>
    public double Slope { get; private set; }

    /// <summary>
    /// Get's the Mia
    /// </summary>
    public double Offset { get; private set; }

    public double[] ValuesX { get; private set; }

    public double[] ValuesY { get; private set; }

    private double sumY;

    private double sumX;

    private double sumXY;

    private double sumX2;

    private double n;

    public Trendline(double[] x, double[] y)
    {
        this.ValuesX = x;
        this.ValuesY = y;

        this.sumXY = this.calculateSumXsYsProduct(this.ValuesX, this.ValuesY);
        this.sumX = this.calculateSumXs(this.ValuesX);
        this.sumY = this.calculateSumYs(this.ValuesY);
        this.sumX2 = this.calculateSumXsSquare(this.ValuesX);
        this.n = this.ValuesX.Length;

        this.calculateSlope();
        this.calculateOffset();
    }

    public Trendline(double[] y)
    {
        //Assinging Y Values
        this.ValuesY = y;
        int length = y.Length;
        //Assinging X Values
        this.ValuesX = new double[length];
        for (int i = 0; i < length; i++)
        {
            this.ValuesX[i] = i;
        }


        this.sumXY = this.calculateSumXsYsProduct(this.ValuesX, this.ValuesY);
        this.sumX = this.calculateSumXs(this.ValuesX);
        this.sumY = this.calculateSumYs(this.ValuesY);
        this.sumX2 = this.calculateSumXsSquare(this.ValuesX);
        this.n = this.ValuesX.Length;
        this.calculateSlope();
        this.calculateOffset();
    }

    public Trendline(double[][] xy)
    {
        double[] xs = new double[xy.Length];
        double[] ys = new double[xy.Length];
        for (int i = 0; i < xy.Length; i++)
        {
            xs[i] = xy[i][0];
            ys[i] = xy[i][1];
        }
        this.ValuesX = xs;
        this.ValuesY = ys;
    }

    private double calculateSumXsYsProduct(double[] xs, double[] ys)
    {
        double sum = 0;
        for (int i = 0; i < xs.Length; i++)
        {
            sum += xs[i] * ys[i];
        }
        return sum;
    }

    private double calculateSumXs(double[] xs)
    {
        double sum = 0;
        foreach (double x in xs)
        {
            sum += x;
        }
        return sum;
    }

    private double calculateSumYs(double[] ys)
    {
        double sum = 0;
        foreach (double y in ys)
        {
            sum += y;
        }
        return sum;
    }

    private double calculateSumXsSquare(double[] xs)
    {
        double sum = 0;
        foreach (double x in xs)
        {
            sum += System.Math.Pow(x, 2);
        }
        return sum;
    }

    private void calculateSlope()
    {
        try
        {
            Slope = (n * sumXY - sumX * sumY) / (n * sumX2 - System.Math.Pow(sumX, 2));
        }
        catch (DivideByZeroException)
        {
            Slope = 0;
        }
    }

    private void calculateOffset()
    {
        try
        {
            Offset = (sumY - Slope * sumX) / n;
        }
        catch (DivideByZeroException) { }
    }
}











