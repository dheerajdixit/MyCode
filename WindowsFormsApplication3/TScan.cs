using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using BAL;
using NSA;
using System.Linq;
using HtmlAgilityPack;

using System.Threading.Tasks;
using KiteConnect;
using System.Xml.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Reflection;
using Newtonsoft.Json;
using System.Collections;
using NetTrader.Indicator;
using CommonFeatures;
using System.Diagnostics;
using Model;
using MongoDB.Driver;
using DAL;
using System.Media;

namespace _15MCE
{

    public partial class TScan : Telerik.WinControls.UI.RadForm
    {
        DataTable orders = null;
        static string OrderPlacingMode = string.Empty;
        DataTable pivotPointsCurrent = new DataTable();
        DataTable pivotPointsAll = new DataTable();
        DataTable dma = new DataTable();
        int stockSelection = -2;
        bool appendStock = false;
        int goTimeCount = 2;
        DataSet dsLots = new DataSet();
        DataTable sortedDT;
        bool backTestStatus = true;
        public static int IncrementCounter = 3;

        public int SMAQuanitty
        {
            get
            {
                return Convert.ToInt16(txtSMA.Text);
            }
            set
            {
                if (this.txtSMA.InvokeRequired)
                {
                    this.txtSMA.BeginInvoke((MethodInvoker)delegate () { this.txtPSAA.Text = value.ToString(); });
                }
                else
                {
                    this.txtSMA.Text = value.ToString();
                }
            }

        }

        public double MaxRisk
        {
            get
            {
                return Convert.ToInt16(txtMaxRisk.Text);
            }
            set
            {
                txtMaxRisk.Text = value.ToString();
            }
        }
        public double Reward
        {
            get
            {
                return MaxRisk * 20;
            }
            set
            {
            }
        }

        public double TrailStart
        {
            get
            {
                return MaxRisk;
            }
            set
            {
            }
        }

        public double MaxTurnOver
        {
            get
            {
                return Convert.ToInt64(txtMaxTurnover.Text);
            }
            set
            {
            }
        }

        public string[] AllFNO
        {
            get
            {
                return Common.GetStocks().Select(a => a.StockName).ToArray();
            }
        }

        public string[] AllEQ
        {
            get
            {
                return Common.GetEQStocks().Select(a => a.StockName).ToArray();
            }
        }

        public int PSAAQuantity
        {
            get
            {
                return Convert.ToInt16(txtPSAA.Text);
            }
            set
            {
                if (this.txtPSAA.InvokeRequired)
                {
                    this.txtPSAA.BeginInvoke((MethodInvoker)delegate () { this.txtPSAA.Text = value.ToString(); });
                }
                else
                {
                    this.txtPSAA.Text = value.ToString();
                }
            }
        }

        public int _30MinQuantity
        {
            get
            {
                return Convert.ToInt16(txt30Min.Text);
            }
            set
            {
                if (this.txt30Min.InvokeRequired)
                {
                    this.txt30Min.BeginInvoke((MethodInvoker)delegate () { this.txt30Min.Text = value.ToString(); });
                }
                else
                {
                    this.txt30Min.Text = value.ToString();
                }
            }
        }

        public int _60MinQuantity
        {
            get
            {
                return Convert.ToInt16(txt60Min.Text);
            }
            set
            {
                if (this.txt60Min.InvokeRequired)
                {
                    this.txt60Min.BeginInvoke((MethodInvoker)delegate () { this.txt60Min.Text = value.ToString(); });
                }
                else
                {
                    this.txt60Min.Text = value.ToString();
                }
            }
        }

        public int SuperTrendQuanaity
        {
            get
            {
                return Convert.ToInt16(txtSuperTrend.Text);
            }
            set
            {
                if (this.txtSuperTrend.InvokeRequired)
                {
                    this.txtSuperTrend.BeginInvoke((MethodInvoker)delegate () { this.txtSuperTrend.Text = value.ToString(); });
                }
                else
                {
                    this.txtSuperTrend.Text = value.ToString();
                }
            }
        }


        public int BTD
        {
            get
            {
                if (txtBTD.Text == "")
                    return 0;
                else
                    return Convert.ToInt32(txtBTD.Text);

            }
        }
        #region Initialisation



        public static bool isSuccess = false;

        static List<string> lstExchange = new List<string>();
        static List<string> lstFeedType = new List<string>();
        public static List<string> lstExpiry = new List<string>();
        public static List<string> lstInstrumnent = new List<string>();

        public List<string> lstFeedScrips = new List<string>();
        public static List<string> lstDepthScrips = new List<string>();


        static List<string> lstMScripType = new List<string>();
        static List<string> lstNScripType = new List<string>();
        static List<string> lstBCScripType = new List<string>();
        static List<string> lstNXScripType = new List<string>();
        List<string> lstfeedOrOrder = new List<string>();




        #endregion
        public static Thread thProcessAmbiOrders = null;
        public static Thread thAmbiOrd = null;


        System.Media.SoundPlayer startSoundPlayer = new System.Media.SoundPlayer(@"C:\Jai Sri Thakur Ji\Balance Sheet\1.wav");
        System.Media.SoundPlayer feedErrorSound = new System.Media.SoundPlayer(@"C:\Jai Sri Thakur Ji\Balance Sheet\2.wav");

        double risk = 5000;

        List<int> xOrders = new List<int>();




        public string url = "https://finance.google.com/finance/getprices?q={0}&p={1}d&f=d,o,h,l,c,v&x=NSE&i=300";
        public string preLoadUrl = "https://finance.google.com/finance/getprices?q={0}&p={1}d&f=d,o,h,l,c,v&x=NSE&i={2}";
        public string dailyPivotPointsUrl = "https://finance.google.com/finance/getprices?q={0}&p=50d&f=d,o,h,l,c,v&x=NSE&i=86400";
        DataTable final = new DataTable();
        DataSet dtStocksBuy = new DataSet();
        DataSet dtStocksSell = new DataSet();
        DataSet dtStocksBuy2 = new DataSet();
        DataSet dtStocksSell2 = new DataSet();
        bool isLogin = false;
        ICalculator ifCalculator;

        public DateTime CurrentTradingDate
        {
            get;
            set;
        }
        List<CIndexes> listIndex = new List<CIndexes>();
        Dictionary<string, string> mySettings = new Dictionary<string, string>();
        int psa = 0;
        int sma = 0;
        bool DONT_DELETE = false;
        bool takeBackupOfFiles = false;
        public TScan(Dictionary<string, string> setting)
        {
            mySettings = setting;
            InitializeComponent();
            rgvStocks.TableElement.RowHeight = 50;
            LogStatus("Application Started !!");
            psa = Convert.ToInt16(mySettings["PSA"]);
            sma = Convert.ToInt16(mySettings["SMA"]);
            DONT_DELETE = Convert.ToBoolean(mySettings["DoNotRemove"]);
            takeBackupOfFiles = Convert.ToBoolean(mySettings["TakeBackupOfFilesAfterPlacingOrders"]);
            MaxRisk = Convert.ToDouble(mySettings["Capital"]) * Convert.ToDouble(mySettings["RiskPercent"]) / 100;
        }
        List<Pivots> pList = new List<Pivots>();
        private void radGroupBox1_Click(object sender, EventArgs e)
        {

        }
        Kite kite;
        string myAPIKey = "253tendzrkq911h2";
        string mySecret = "g0wzr0vk9cmpypexzhfzkslwn351x0n1";
        string myRequestToken = string.Empty;
        bool backLiveTest = false;

        //bool calculateBrokerage = Convert.ToBoolean(mySettings["Brokerage"]);
        //int sma = Convert.ToInt16(mySettings["SMA"]);

        DataSet instrToken = new DataSet();
        DataSet dateMapping = null;


        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        List<string> reportColumns = new List<string>();
        private void TScan_Load(object sender, EventArgs e)
        {

            try
            {


                if (File.Exists(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\ABC.csv"))
                {
                    File.Delete(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\ABC.csv");
                    File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\ABC.csv", "Name" + "," + "Timestamp" + "," + "BS" + "," + "Quantity" + "," + "close" + "," + "stoploss" + "," + "high" + "," + "low" + "," + "volume" + "," + "pattern" + "," + "level" + Environment.NewLine);
                }


                txtBTD.Text = Convert.ToString(mySettings["BTD"]);
                tmrClock.Enabled = false;
                tmrClock.Stop();
                dateMapping = new DataSet();
                string currentFileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\Users\Dates.csv";
                OleDbConnection dconn = new OleDbConnection
                       ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                         Path.GetDirectoryName(currentFileName) +
                         "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

                dconn.Open();

                OleDbDataAdapter dadapter = new OleDbDataAdapter
                       ("SELECT * FROM " + Path.GetFileName(currentFileName), dconn);


                dadapter.Fill(dateMapping);

                dconn.Close();

                CurrentTradingDate = Convert.ToDateTime(dateMapping.Tables[0].Rows[dateMapping.Tables[0].Rows.Count - 1 - BTD]["Date"]).Date;


                radLabel9.Text = CurrentTradingDate.ToString("dd-MMM-yyyy");

                if (!DONT_DELETE)
                    RemoveFiles();

                rgvStocks.TableElement.RowHeight = 20;


                panel1.Visible = false;
                risk = Convert.ToDouble(mySettings["risk"]);
                myAPIKey = Convert.ToString(mySettings["myAPIKey"]);
                mySecret = Convert.ToString(mySettings["mySecret"]);
                myRequestToken = Convert.ToString(mySettings["requestToken"]);
                backLiveTest = Convert.ToBoolean(mySettings["BackLiveTest"]);
                if (rdoLive.IsChecked)
                {
                    backLiveTest = false;
                }
                else if (rdoSimulation.IsChecked)
                {
                    backLiveTest = true;
                }

                orders = new DataTable();
                orders.Columns.Add("scrip", typeof(string));
                orders.Columns.Add("entry", typeof(double));
                orders.Columns.Add("high", typeof(double));
                orders.Columns.Add("low", typeof(double));
                orders.Columns.Add("quantity", typeof(int));
                orders.Columns.Add("target", typeof(double));
                orders.Columns.Add("stoploss", typeof(double));
                orders.Columns.Add("candle", typeof(DateTime));

                orders.Columns.Add("direction", typeof(string));
                orders.Columns.Add("Aentry", typeof(double));
                orders.Columns.Add("Aexit", typeof(double));
                orders.Columns.Add("strategy", typeof(string));
                orders.Columns.Add("ltp", typeof(double));
                orders.Columns.Add("ec", typeof(int));
                orders.Columns.Add("exlevel", typeof(string));
                orders.Columns.Add("BP", typeof(double));
                orders.Columns.Add("QTY", typeof(int));
                orders.Columns.Add("ClosingTime", typeof(string));
                orders.Columns["BP"].DefaultValue = 0;
                orders.Columns["target"].DefaultValue = 0;

                reportColumns.Add("scrip");
                reportColumns.Add("entry");
                reportColumns.Add("stoploss");
                reportColumns.Add("candle");
                reportColumns.Add("direction");
                reportColumns.Add("Aexit");
                reportColumns.Add("BP");
                reportColumns.Add("QTY");
                reportColumns.Add("ClosingTime");


                // Restore();
                try
                {
                    kite = new Kite(myAPIKey, Debug: true);
                    if (!DONT_DELETE)
                        radButton2_Click(this, EventArgs.Empty);

                    string loginURL = kite.GetLoginURL();
                    string html = string.Empty;
                    //  txtloginURL.Text = loginURL;

                }
                catch (Exception ex)
                {
                }

                //tmr_News.Interval = 1000;
                tmr_News.Start();
                tmr_News.Enabled = true;


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
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
                }
                catch (Exception ex1)
                {
                    MessageBox.Show(ex1.Message);
                }
            }
            try
            {
                ModuleCommon.ChangeThemeName(this, "Windows8");
                tmrClock.Enabled = true;
                tmrClock.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        double HighestProfitToday = 0;

        public void LongTermInvestment()
        {
            Parallel.ForEach(AllEQ, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
                    {
                        CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-2000).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "day");

                    });



        }

        public Candle MyCandle(List<Candle> candles, DateTime currentTimeStamp)
        {
            return new Candle { High = candles.Max(a => a.High), Low = candles.Min(a => a.Low), Open = candles.First().Open, Close = candles.Where(a => a.TimeStamp <= currentTimeStamp).Last().Close };

        }
        public Candle MyCandle(List<Candle> candles)
        {
            candles = candles.OrderBy(a => a.TimeStamp).ToList();
            return new Candle { High = candles.Max(a => a.High), Low = candles.Min(a => a.Low), Open = candles.First().Open, Close = candles.Last().Close };

        }
        public void GetHighProbabiltyLowRiskStock(DateTime CurrentTimeStamp)
        {


            if (Convert.ToInt32(txtTam.Text) >= -3)
            {
                List<StockData> sGap = new List<StockData>();

                // above moving average 50
                Parallel.ForEach(AllFNO, (a) =>
                {

                    var stock = allData["5minute"][a].Where(b => b.TimeStamp == CurrentTimeStamp).FirstOrDefault();
                    var todaysData = allData["5minute"][a].Where(b => b.TimeStamp <= CurrentTimeStamp && b.TimeStamp.Date == CurrentTradingDate);
                    var high = todaysData.Max(b => b.High);
                    var low = todaysData.Min(b => b.Low);
                    var firstCandleOfStock = allData["5minute"][a].Where(b => b.TimeStamp.Date == CurrentTradingDate).First();

                    //var niftyCandle = MyCandle(allData["day"]["Nifty50"].ToList(), CurrentTimeStamp);
                    //var todaysCandle = MyCandle((allData["day"]["Nifty50"].ToList(), currentTimeStamp);
                    if (stock.AllIndicators.Stochastic.OscillatorStatus == OscillatorStatus.Oversold)
                    {
                        if (high <= stock.dR1 && ((stock.Close > stock.SMA50 && stock.SMA20 > stock.SMA50 && stock.CandleType == "G" && stock.Close > stock.dPP && firstCandleOfStock.CandleType == "G" && ((stock.Close - stock.dPP) / stock.Close) * 100 * 4 <= ((stock.dR1 - stock.Close) / stock.Close) * 100 && stock.Low <= stock.SMA50 && stock.Low <= stock.dPP)
|| (stock.Close > stock.SMA20 && stock.SMA20 > stock.SMA50 && stock.Close > stock.dPP && stock.CandleType == "G" && firstCandleOfStock.CandleType == "G" && ((stock.Close - stock.dPP) / stock.Close) * 100 * 4 <= ((stock.dR1 - stock.Close) / stock.Close) * 100 && stock.Low <= stock.SMA20 && stock.Low <= stock.dPP))
                        )
                        {
                            sGap.Add(new StockData
                            {
                                Symbol = stock.Stock,
                                Open = 0,
                                Vol = stock.Volume,
                                //dHigh = tradingCandle.High,
                                //dLow = tradi,
                                High = stock.High,
                                Low = stock.Low,
                                Direction = "BM",
                                stopLoss = stock.Low,
                                TradingDate = stock.TimeStamp,
                                Quantity = Convert.ToInt32(MaxRisk / (stock.High - stock.Low + 0.2)),
                                dClose = 0,
                                Close = stock.Close
                            });
                        }
                    }

                    else if (stock.AllIndicators.Stochastic.OscillatorStatus == OscillatorStatus.Overbought)
                    {
                        if (low >= stock.dS1 && ((stock.Close < stock.SMA50 && stock.SMA20 < stock.SMA50 && stock.Close < stock.dPP && firstCandleOfStock.CandleType == "R" && stock.CandleType == "R" && ((stock.dPP - stock.Close) / stock.Close) * 100 * 4 <= ((stock.Close - stock.dS1) / stock.Close) * 100 && stock.High >= stock.SMA50 && stock.High >= stock.dPP)
|| (stock.Close < stock.SMA20 && stock.SMA20 < stock.SMA50 && stock.Close < stock.dPP && firstCandleOfStock.CandleType == "R" && stock.CandleType == "R" && ((stock.dPP - stock.Close) / stock.Close) * 100 * 4 <= ((stock.Close - stock.dS1) / stock.Close) * 100 && stock.High >= stock.SMA20 && stock.High >= stock.dPP)
                        ))
                        {
                            sGap.Add(new StockData
                            {
                                Symbol = stock.Stock,
                                Open = 0,
                                Vol = stock.Volume,
                                //dHigh = tradingCandle.High,
                                //dLow = tradi,
                                High = stock.High,
                                Low = stock.Low,
                                Direction = "SM",
                                stopLoss = stock.Low,
                                TradingDate = stock.TimeStamp,
                                Quantity = Convert.ToInt32(MaxRisk / (stock.High - stock.Low + 0.2)),
                                dClose = 0,
                                Close = stock.Close
                            });
                        }
                    }

                });



                foreach (var s in sGap.OrderByDescending(a => a.Vol * a.Open).Take(1))
                {
                    NSA.Order o = new NSA.Order() { EntryPrice = s.Direction == "SM" ? s.Low - 0.1 : s.High + 0.1, High = s.High, Low = s.Low, Quantity = s.Quantity, Scrip = s.Symbol, Stoploss = s.stopLoss, Strategy = $"DT_5Minute_High", TimeStamp = s.TradingDate, TransactionType = s.Direction, Volume = s.Vol };

                    if (o != null)
                    {
                        PlacePartialOrders(o);
                    }

                }


            }
        }
        public void GetDualTimeFrameStocks(int higherTimeFrame, int lowerTimeFrame)
        {
            string HT = "day";
            string LT = "minute";
            if (lowerTimeFrame < 100)
            {
                LT = lowerTimeFrame + "minute";
            }
            else if (lowerTimeFrame == 100)
            {
                LT = "day";
            }
            if (higherTimeFrame < 100)
            {
                HT = higherTimeFrame + "minute";
            }
            else if (higherTimeFrame == 100)
            {
                HT = "day";
            }

            else if (higherTimeFrame == 200)
            {
                HT = "week";
            }

            if (Convert.ToInt32(txtTam.Text) >= -3)
            {
                List<StockData> sGap = new List<StockData>();

                Dictionary<Guid, Model.StrategyModel> getTradedStocks = new StockOHLC().ApplyDualMomentumStrategyModel(allData[HT], allData[LT], Common.GetIdeas().Where(a => a.Name == "Dual_Time_Frame_Momentum").First(), null);

                var finalStocks = getTradedStocks.Where(b => b.Value.Date == TokenChannel.GetTimeStamp(Convert.ToInt32(txtTam.Text), CurrentTradingDate, lowerTimeFrame)).ToList();
                var lastDate = getTradedStocks.OrderBy(a => a.Value.Date).Last();

                if (LT == "day")
                {
                    finalStocks = getTradedStocks.Where(b => b.Value.Date == CurrentTradingDate.Date).ToList();
                }


                foreach (var a in finalStocks)
                {
                    try
                    {
                       
                        if (a.Value.Trade == Model.Trade.BUY && allData["day"][a.Value.Stock].Where(c=>c.TimeStamp.Date<a.Value.Date.Date)
                            .OrderByDescending(c=>c.TimeStamp.Date)
                            .First().AllIndicators.Stochastic?.OscillatorStatus== OscillatorStatus.Bullish)
                        {
                            
                           
                             
                                    sGap.Add(new StockData
                                    {
                                        Symbol = a.Value.Stock,
                                        Open = 0,
                                        Vol = a.Value.Volume,
                                        //dHigh = tradingCandle.High,
                                        //dLow = tradi,
                                        High = a.Value.High,
                                        Low = a.Value.Low,
                                        Direction = "BM",
                                        stopLoss = a.Value.Low - 0.1,
                                        TradingDate = a.Value.Date,
                                        Quantity = Convert.ToInt32(MaxRisk / (a.Value.High - a.Value.Low + 0.2)),
                                        dClose = 0,
                                        Close = a.Value.Close
                                    });

                        }
                        else if (a.Value.Trade== Model.Trade.SELL && allData["day"][a.Value.Stock].Where(c => c.TimeStamp.Date < a.Value.Date.Date)
                            .OrderByDescending(c => c.TimeStamp.Date)
                            .First().AllIndicators.Stochastic?.OscillatorStatus == OscillatorStatus.Bearish)
                        {
                            sGap.Add(new StockData
                            {
                                Symbol = a.Value.Stock,
                                Open = 0,
                                Vol = a.Value.Volume,
                                //dHigh = tradingCandle.High,
                                //dLow = tradi,
                                High = a.Value.High,
                                Low = a.Value.Low,
                                Direction = "SM",
                                stopLoss = a.Value.High - 0.2,
                                TradingDate = a.Value.Date,
                                Quantity = Convert.ToInt32(MaxRisk / (a.Value.High - a.Value.Low + 0.2)),
                                dClose = 0,
                                Close = a.Value.Close
                            });
                        }


                    }
                    catch (Exception ex)
                    {
                    }

                }

                foreach (var s in sGap.OrderByDescending(a => a.Vol * a.Close).Take(1))
                {
                    NSA.Order o = new NSA.Order() { EntryPrice = s.Direction == "SM" ? s.Low - 0.1 : s.High + 0.1, High = s.High, Low = s.Low, Quantity = s.Quantity, Scrip = s.Symbol, Stoploss = s.stopLoss, Strategy = $"DT_{HT}_{LT}", TimeStamp = s.TradingDate, TransactionType = s.Direction, Volume = s.Vol };

                    if (o != null)
                    {
                        PlacePartialOrders(o);
                    }

                }


            }
        }
        List<DataRow> stocksIdentified = new List<DataRow>();
        public void WaitForOverSoldPositionInThQueue()
        {

        }
        public void RefreshData()
        {
            try
            {
                //topBSTrades();
                DateTime f1 = DateTime.Now;
                finalList.Columns.Clear();
                finalList.Rows.Clear();
                finalList.Columns.Add("stock", typeof(string));
                finalList.Columns.Add("direction", typeof(string));
                finalList.Columns.Add("per", typeof(double));
                finalList.Columns.Add("high", typeof(double));
                finalList.Columns.Add("low", typeof(double));
                finalList.Columns.Add("isBreakOut", typeof(bool));
                finalList.Columns.Add("Type", typeof(string));


                orderDetails.Clear();


                //GetDualTimeFrameStocks(200, 100);


                if (Convert.ToInt16(txtTam.Text) == -3)
                {
                    if (!DONT_DELETE)
                    {
                        LoadDataDaily();
                        LoadDataCommon(60);
                    }
                }


                if ((txtTam.Text == "9" || txtTam.Text == "21" || txtTam.Text == "33" || txtTam.Text == "45" || txtTam.Text == "57" || txtTam.Text == "69"))
                {
                    if (!DONT_DELETE) LoadDataCommon(60);
                    //GetDualTimeFrameStocks(100, 60);

                }
                if (!DONT_DELETE)
                    LoadDataCommon(5);
                if (Convert.ToInt16(txtTam.Text) >= -2)
                {
                    //reason 1 to buy
                    GetDualTimeFrameStocks(60, 5);
                }







                //GetHighProbabiltyLowRiskStock(TokenChannel.GetTimeStamp(Convert.ToInt32(txtTam.Text), CurrentTradingDate));
                /// GetDualTimeFrameStocks(60, 5);
                //GetDualTimeFrameStocks(30, 5);

                //if (Convert.ToInt16(txtTam.Text) % 2 != 0)
                //{
                //    if (!DONT_DELETE) LoadDataCommon(10);
                //    GetDualTimeFrameStocks(60, 10);
                //    GetDualTimeFrameStocks(100, 10);
                //}
                /*
                 if ((txtTam.Text == "0" || txtTam.Text == "3" || txtTam.Text == "6" || txtTam.Text == "9" || txtTam.Text == "12" || txtTam.Text == "15" || txtTam.Text == "12"
                         || txtTam.Text == "15" || txtTam.Text == "18" || txtTam.Text == "21" || txtTam.Text == "24" || txtTam.Text == "27" || txtTam.Text == "30" || txtTam.Text == "33"
                         || txtTam.Text == "36" || txtTam.Text == "39" || txtTam.Text == "42" || txtTam.Text == "45" || txtTam.Text == "48" || txtTam.Text == "51" || txtTam.Text == "54"
                         || txtTam.Text == "57" || txtTam.Text == "60" || txtTam.Text == "63" || txtTam.Text == "66" || txtTam.Text == "69" || txtTam.Text == "72" || txtTam.Text == "75"))
                 {
                     if (!DONT_DELETE)
                     {
                         LoadDataCommon(15);
                         LoadDataCommon(60);
                     }
                     //GetDualTimeFrameStocks(100, 15);
                     //GetDualTimeFrameStocks(60, 15);
                 }
                 if ((txtTam.Text == "3" || txtTam.Text == "9" || txtTam.Text == "15" || txtTam.Text == "21" || txtTam.Text == "27" || txtTam.Text == "33" || txtTam.Text == "39" || txtTam.Text == "45" || txtTam.Text == "51" || txtTam.Text == "57" || txtTam.Text == "63" || txtTam.Text == "69" || txtTam.Text == "75"))
                 {
                     if (!DONT_DELETE) LoadDataCommon(30);
                     // GetDualTimeFrameStocks(100, 30);

                 }

                 if ((txtTam.Text == "9" || txtTam.Text == "21" || txtTam.Text == "33" || txtTam.Text == "45" || txtTam.Text == "57" || txtTam.Text == "69"))
                 {
                     if (!DONT_DELETE) LoadDataCommon(60);
                     //GetDualTimeFrameStocks(100, 60);

                 }
                 //UpdateOrders(30);
                 */
                foreach (var xx in orderDetails)
                {
                    stocksIdentified.Add(xx);
                    orders.Rows.Add(xx);
                    if (!DONT_DELETE)
                    {
                        SoundPlayer snd = new SoundPlayer(NSA.Properties.Resources.ALARM);
                        snd.Play();
                    }


                }
                LogStatus("All orders placed within  " + (DateTime.Now - f1).Seconds);
                orders.AcceptChanges();
                try
                {
                    File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " - Orders Rows Count :- " + orders.Rows.Count);
                    rgvStocks.DataSource = orders;

                }
                catch (Exception ex)
                {
                    File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", Environment.NewLine);
                    File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " - DataSource :- " + ex.Message);
                    File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " - DataSource :- " + ex.StackTrace);
                    File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + "  - DataSource:- " + ex.InnerException);
                }

            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", Environment.NewLine);
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.StackTrace);
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.InnerException);
            }
        }

        public void RemoveFiles()
        {
            try
            {

                if (!ZERODHA)
                {
                    DateTime x = DateTime.Now;
                    System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE");


                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    di = new DirectoryInfo(@"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\backup");
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    di = new DirectoryInfo(@"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\30");
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    di = new DirectoryInfo(@"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\data\15");
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    di = new DirectoryInfo(@"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\data\hourly");
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                }
                else
                {
                    File.Delete(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt");
                    DateTime x = DateTime.Now;
                    System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\");


                    foreach (FileInfo file in di.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        if (file.Name.Contains("instrumetntoken") && file.LastWriteTime > CurrentTradingDate)
                        {
                            continue;
                        }
                        file.Delete();
                    }

                    di = new DirectoryInfo(@"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\");
                    foreach (FileInfo file in di.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        file.Delete();
                    }

                    di = new DirectoryInfo(@"C:\Jai Sri Thakur Ji\Nifty Analysis\PSR\");
                    if (di.GetFiles().Count() > 0 && di.GetFiles().OrderBy(a => a.LastWriteTime).First().LastWriteTime < CurrentTradingDate)
                    {
                        foreach (FileInfo file in di.GetFiles("*.*", SearchOption.AllDirectories))
                        {
                            file.Delete();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }
        public static bool ZERODHA = true;



        List<StockData> sList = new List<StockData>();
        List<string> downlodableScrip = new List<string>();
        bool firstLoad = true;
        bool secondLoad = true;
        bool thirdLoad = true;
        public void LoadData(int period)
        {
            try
            {
                if (txtSwitchMode.Text == string.Empty)
                {
                    downlodableScrip = AllFNO.ToList();
                }
                else
                {
                    foreach (DataRow dr in orders.Rows)
                    {
                        if (!downlodableScrip.Contains(dr["scrip"].ToString()))
                            downlodableScrip.Add(dr["scrip"].ToString());
                    }
                    if (SMAQuanitty + PSAAQuantity + _60MinQuantity + SuperTrendQuanaity > 0)
                        downlodableScrip = AllFNO.ToList();
                }

                Parallel.ForEach(downlodableScrip, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
                {
                    if (period == 60)
                    {
                        if (firstLoad && DONT_DELETE)
                        {
                            CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-7 * period / 5).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, period.ToString() + "minute");

                        }
                        else if (!DONT_DELETE)
                        {
                            CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-7 * period / 5).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, period.ToString() + "minute");
                        }
                    }
                    else if (period == 5)
                    {
                        if (secondLoad && DONT_DELETE)
                        {
                            CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-7 * period / 5).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, period.ToString() + "minute");
                        }
                        else if (!DONT_DELETE)
                        {
                            CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-7 * period / 5).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, period.ToString() + "minute");
                        }


                    }
                    else if (period == 30)
                    {
                        if (thirdLoad && DONT_DELETE)
                        {
                            CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-7 * period / 5).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, period.ToString() + "minute");

                        }
                        else if (!DONT_DELETE)
                        {
                            CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-7 * period / 5).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, period.ToString() + "minute");
                        }
                    }
                });

                if (txtSwitchMode.Text == string.Empty)
                {
                    downlodableScrip = AllFNO.ToList();
                }
                if (period == 60)
                {

                    firstLoad = false;
                }
                else if (period == 5)
                {
                    secondLoad = false;
                }
                else if (period == 30)
                {
                    thirdLoad = false;
                }
            }
            catch
            {
            }
            if (period == 5 && SuperTrendQuanaity > 0)
            {
            }
            if (txtTam.Text == "9" && period == 60)
            {
            }
            else if (period == 30)
            {

                fS1.Clear();
                List<StockData> finalStock = null;


                if (Convert.ToInt32(txtTam.Text) == 15)
                {
                    List<StockData> sGap = new List<StockData>();
                    foreach (var a in allData[period.ToString() + "minute"])
                    {
                        try
                        {
                            var c = a.Value.Where(b => b.TimeStamp.Date == CurrentTradingDate.Date).OrderBy(b => b.TimeStamp).ToArray();
                            Candle tradingCandle = c[2];
                            tradingCandle.PreviousCandle = c[1];
                            tradingCandle.PreviousCandle.PreviousCandle = c[0];
                            if (tradingCandle.Close > 50)
                            {
                                if (((tradingCandle.High - tradingCandle.Close) < ((tradingCandle.Close - tradingCandle.Open) / 2.0)) || ((tradingCandle.Close - tradingCandle.Low) < ((tradingCandle.Open - tradingCandle.Close) / 2.0)))
                                {
                                    if (tradingCandle.CandleType == "G" && tradingCandle.PreviousCandle.CandleType == "R" && tradingCandle.PreviousCandle.PreviousCandle.CandleType == "G")
                                    {
                                        sGap.Add(new StockData
                                        {
                                            Symbol = a.Key,
                                            Open = 0,
                                            Vol = tradingCandle.Volume * tradingCandle.Close,
                                            //dHigh = tradingCandle.High,
                                            //dLow = tradi,
                                            High = tradingCandle.High,
                                            Low = tradingCandle.Low,
                                            Direction = "BM",
                                            stopLoss = tradingCandle.Close - (tradingCandle.High - tradingCandle.Low),
                                            TradingDate = tradingCandle.TimeStamp,
                                            Quantity = Convert.ToInt32(((MaxRisk / 3) / (tradingCandle.High - tradingCandle.Low))),
                                            dClose = ((Math.Abs((double)(tradingCandle.Close - tradingCandle.PreviousCandle.Close)) / tradingCandle.Close) * 100.0),
                                            Close = tradingCandle.Close
                                        });

                                    }
                                    else if (tradingCandle.CandleType == "R" && tradingCandle.PreviousCandle.CandleType == "G" && tradingCandle.PreviousCandle.PreviousCandle.CandleType == "R")
                                    {
                                        sGap.Add(new StockData
                                        {
                                            Symbol = a.Key,
                                            Open = 0,
                                            Vol = tradingCandle.Volume * tradingCandle.Close,
                                            //dHigh = tradingCandle.High,
                                            //dLow = tradi,
                                            High = tradingCandle.High,
                                            Low = tradingCandle.Low,
                                            Direction = "SM",
                                            stopLoss = tradingCandle.Close + (tradingCandle.High - tradingCandle.Low),
                                            TradingDate = tradingCandle.TimeStamp,
                                            Quantity = Convert.ToInt32(((MaxRisk / 3) / (tradingCandle.High - tradingCandle.Low))),
                                            dClose = ((Math.Abs((double)(tradingCandle.Close - tradingCandle.PreviousCandle.Close)) / tradingCandle.Close) * 100.0),
                                            Close = tradingCandle.Close

                                        });

                                    }
                                }
                            }



                        }
                        catch (Exception ex)
                        {
                        }

                    }
                    //var c1 = sGap.OrderBy(a => a.Symbol).ToList();

                    //XmlSerializer xs = new XmlSerializer(typeof(List<StockData>));
                    //using (StreamWriter writer = new StreamWriter(@"C:\Jai Sri Thakur Ji\foo1.xml"))
                    //{
                    //    xs.Serialize(writer, c1);
                    //}
                    finalStock = sGap.OrderByDescending(b => b.Vol).Take(5).ToList().OrderByDescending(a => a.dClose).Take(3).ToList();
                }

                foreach (var s1 in finalStock)
                {
                    if (true || s1.dHigh < s1.High && s1.dLow > s1.Low)
                    {
                        fS1.Add(s1);
                    }
                }
            }

            if (period == 5)
            {
                fS1.Clear();
                List<StockData> finalStock = null;


                if (Convert.ToInt32(txtTam.Text) == -2)
                {
                    List<StockData> sGap = new List<StockData>();

                    Dictionary<Guid, Model.StrategyModel> getTradedStocks = new StockOHLC().GetTopMostSolidGapOpenerDayWise(allData[period.ToString() + "minute"], Common.GetIdeas().Where(a => a.Name == "manual").First(), null);
                    var finalStocks = getTradedStocks.Where(b => b.Value.Date.Date == CurrentTradingDate).ToList();
                    foreach (var a in finalStocks)
                    {
                        try
                        {
                            if (a.Value.Trade == Model.Trade.BUY)
                            {
                                sGap.Add(new StockData
                                {
                                    Symbol = a.Value.Stock,
                                    Open = 0,
                                    Vol = a.Value.Volume,
                                    //dHigh = tradingCandle.High,
                                    //dLow = tradi,
                                    High = a.Value.High,
                                    Low = a.Value.Low,
                                    Direction = "BM",
                                    stopLoss = a.Value.Close - ((MaxTurnOver * 3 / 100) / (MaxTurnOver / a.Value.Close)),
                                    TradingDate = a.Value.Date,
                                    Quantity = Convert.ToInt32(MaxTurnOver / a.Value.Close),
                                    dClose = 0,
                                    Close = a.Value.Close
                                });

                            }
                            else if (a.Value.Trade == Model.Trade.SELL)
                            {
                                sGap.Add(new StockData
                                {
                                    Symbol = a.Value.Stock,
                                    Open = 0,
                                    Vol = a.Value.Volume,
                                    //dHigh = tradingCandle.High,
                                    //dLow = tradi,
                                    High = a.Value.High,
                                    Low = a.Value.Low,
                                    Direction = "SM",
                                    stopLoss = a.Value.Close + ((MaxTurnOver * 3 / 100) / (MaxTurnOver / a.Value.Close)),
                                    TradingDate = a.Value.Date,
                                    Quantity = Convert.ToInt32(MaxTurnOver / a.Value.Close),
                                    dClose = 0,
                                    Close = a.Value.Close

                                });

                            }
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                    //var c1 = sGap.OrderBy(a => a.Symbol).ToList();

                    //XmlSerializer xs = new XmlSerializer(typeof(List<StockData>));
                    //using (StreamWriter writer = new StreamWriter(@"C:\Jai Sri Thakur Ji\foo1.xml"))
                    //{
                    //    xs.Serialize(writer, c1);
                    //}
                    finalStock = sGap.OrderByDescending(b => b.Vol).Take(10).ToList().OrderByDescending(a => a.dClose).Take(5).ToList();
                }

                foreach (var s1 in finalStock)
                {
                    if (true || s1.dHigh < s1.High && s1.dLow > s1.Low)
                    {
                        fS1.Add(s1);
                    }
                }
            }
            else if (period == 60)
            {
            }



            if (period == 30 || period == 5)
            {
                foreach (var s in fS1)
                {
                    NSA.Order o = new NSA.Order() { EntryPrice = s.Close, High = s.High, Low = s.Low, Quantity = s.Quantity, Scrip = s.Symbol, Stoploss = s.stopLoss, Strategy = period == 30 ? "ThirdCandle" : "FirstCandle", TimeStamp = s.TradingDate, TransactionType = s.Direction, Volume = s.Vol };

                    if (o != null)
                    {
                        PlacePartialOrders(o);
                    }

                }
            }

        }

        public void LoadDataForStocksList(int period, List<string> stocksList)
        {
            try
            {

                downlodableScrip = AllFNO.ToList().Intersect<string>(stocksList.ToArray()).ToList();
                Parallel.ForEach(downlodableScrip, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
                {

                    CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-7 * period / 5).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, period.ToString() + "minute", period.ToString());

                });


            }
            catch
            {
                throw;
            }

        }
        public void LoadDataCommon(int period)
        {
            try
            {

                downlodableScrip = AllFNO.ToList();
                Parallel.ForEach(downlodableScrip, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
                {

                    CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-7 * period / 5).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, period.ToString() + "minute", period.ToString());

                });


            }
            catch
            {
                throw;
            }

        }

        public void LoadDataDaily()
        {
            try
            {

                downlodableScrip = AllFNO.ToList();
                Parallel.ForEach(downlodableScrip, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
                {
                    CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-150).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "day", "daily");


                });


            }
            catch
            {
                throw;
            }

        }

        public void LoadDataWeekly()
        {
            try
            {

                downlodableScrip = AllEQ.ToList();
                Parallel.ForEach(downlodableScrip, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
                {
                    CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-1050).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "week", "weekly");


                });


            }
            catch
            {
                throw;
            }

        }



        double GetBody(Candle c)
        {
            return Math.Abs(c.Close - c.Open);
        }

        double GetLowerWick(Candle c)
        {
            if (c.CandleType == "D" || c.CandleType == "R")
            {
                return c.Close - c.Low;
            }
            else
            {
                return c.Open - c.Low;
            }

        }

        double GetUpperWick(Candle c)
        {
            if (c.CandleType == "D" || c.CandleType == "R")
            {
                return c.High - c.Open;
            }
            else
            {
                return c.High - c.Close;
            }

        }




        public void LoadAllDateTillDate()
        {

            if (txtSwitchMode.Text == string.Empty)
            {
                downlodableScrip = AllFNO.ToList();
            }
            else
            {
                foreach (DataRow dr in orders.Rows)
                {
                    if (!downlodableScrip.Contains(dr["scrip"].ToString()))
                        downlodableScrip.Add(dr["scrip"].ToString());
                }
                if (SMAQuanitty + PSAAQuantity + _60MinQuantity + SuperTrendQuanaity > 0)
                    downlodableScrip = AllFNO.ToList();
            }

            bool load30 = Convert.ToBoolean(mySettings["30"]);
            bool load10 = Convert.ToBoolean(mySettings["10"]);
            bool load15 = Convert.ToBoolean(mySettings["15"]);
            bool load60 = Convert.ToBoolean(mySettings["60"]);
            bool load5 = Convert.ToBoolean(mySettings["5"]);
            bool loaddaily = Convert.ToBoolean(mySettings["daily"]);
            string duation = Convert.ToString(mySettings["DLDuration"]);
            int noOfDaysMultiplier = 1;
            if (duation.ToLower() == "short")
            {
                noOfDaysMultiplier = 5;
            }

            Parallel.ForEach(downlodableScrip, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
            {
                if (s != null)
                {
                    if (load60)
                        CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-400 / noOfDaysMultiplier).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "60" + "minute", "60");
                    if (load10)
                        CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-100 / noOfDaysMultiplier).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "10" + "minute", "10");
                    if (load30)
                        CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-200 / noOfDaysMultiplier).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "30" + "minute", "30");
                    if (load15)
                        CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-200 / noOfDaysMultiplier).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "15" + "minute", "15");
                    if (load5)
                        CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-100 / noOfDaysMultiplier).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "5" + "minute", "5");
                    if (loaddaily)
                    {
                        CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-2000 / noOfDaysMultiplier).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "" + "day", "daily");
                        CallWebServiceZerodha(instrToken.Tables[0].Select("tradingsymbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-2000 / noOfDaysMultiplier).ToString("yyyy-MM-dd"), CurrentTradingDate.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "" + "week", "weekly");
                    }
                }

            });

            MessageBox.Show("All data loaded");
        }




        List<StockData> fS1 = new List<StockData>();
        List<string> toptrades = new List<string>();

        List<string> trades = new List<string>();


        public void LoadSectedStocks()
        {
            if (File.Exists(@"C:\Jai Sri Thakur Ji\Nifty Analysis\zerodha\abc.csv"))
            {
                string FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\zerodha\abc.csv";
                string[] s = File.ReadAllLines(@"C:\Jai Sri Thakur Ji\Nifty Analysis\zerodha\abc.csv");



                OleDbConnection conn = new OleDbConnection
                       ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                         Path.GetDirectoryName(FileName) +
                         "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

                conn.Open();

                OleDbDataAdapter adapter = new OleDbDataAdapter
                       ("SELECT * FROM " + Path.GetFileName(FileName), conn);

                DataSet ds = new DataSet("Temp");
                dsSampleTrades.Clear();
                adapter.Fill(dsSampleTrades);
            }
        }

        DataTable finalList = new DataTable();
        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                TestRunMarketWithoutLogin();


            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", Environment.NewLine);
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.StackTrace);
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.InnerException);
            }

        }


        List<DataRow> orderDetails = new List<DataRow>();
        Dictionary<string, StockData> Stocks30 = new Dictionary<string, StockData>();
        Dictionary<string, StockData> Stocks60 = new Dictionary<string, StockData>();
        Dictionary<string, StockData> Stocks5 = new Dictionary<string, StockData>();
        Dictionary<string, StockData> StocksSuperTrend15 = new Dictionary<string, StockData>();
        Dictionary<string, StockData> allStocks = new Dictionary<string, StockData>();
        Dictionary<string, StockData> Stocks15 = new Dictionary<string, StockData>();
        Dictionary<string, StockData> Stocks60Executed = new Dictionary<string, StockData>();
        Dictionary<string, string> Stocks30Executed = new Dictionary<string, string>();
        public void CalculateIndicators(string scrip)
        {
            try
            {

                DataTable dt = null;
                string FileName = string.Empty;
                if (backLiveTest)
                {
                    FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\backup\" + scrip.Replace("-", string.Empty) + ".csv";
                }
                else
                {
                    FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\" + scrip.Replace("-", string.Empty) + ".csv";
                }

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
                if (!backLiveTest)
                {
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
                    Indicators.AddSuperTrendIndicator(ref ds);



                    dt = ds.Tables[0];

                    dt.Columns.Add("20", typeof(double));
                    dt.Columns.Add("50", typeof(double));
                    dt.Columns.Add("200", typeof(double));
                    dt.Columns.Add("macd", typeof(double));
                    dt.Columns.Add("macd9", typeof(double));
                    dt.Columns.Add("RSI14", typeof(double));
                    dt.Columns.Add("BS", typeof(string));
                    dt.Columns.Add("PSR", typeof(string));
                    dt.Columns.Add("AC", typeof(double));
                    dt.Columns.Add("L", typeof(double));
                    dt.Columns.Add("LOffset", typeof(double));
                    dt.Columns.Add("M", typeof(double));
                    dt.Columns.Add("MOffset", typeof(double));
                    dt.Columns.Add("S", typeof(double));
                    dt.Columns.Add("SOffset", typeof(double));
                    dt.Columns.Add("Support", typeof(double));
                    dt.Columns.Add("Resistance", typeof(double));

                    IMovingAverage avg20 = new SimpleMovingAverage(20);
                    IMovingAverage avg50 = new SimpleMovingAverage(50);
                    IMovingAverage avg200 = new SimpleMovingAverage(200);

                    IMovingAverage avg9 = new SimpleMovingAverage(9);
                    IMovingAverage avg12 = new SimpleMovingAverage(12);
                    IMovingAverage avg26 = new SimpleMovingAverage(26);

                    IMovingAverage rsiGain14 = new SimpleMovingAverage(14);
                    IMovingAverage rsiLoss14 = new SimpleMovingAverage(14);

                    double pEMA9 = 0, pEMA12 = 0, pEMA26 = 0, macd = 0;

                    //accelartion oscillator
                    IMovingAverage avg5 = new SimpleMovingAverage(5);
                    IMovingAverage avg34 = new SimpleMovingAverage(34);
                    IMovingAverage avgAO5 = new SimpleMovingAverage(5);

                    //alligator
                    IMovingAverage avg13 = new SimpleMovingAverage(13);
                    IMovingAverage avg8 = new SimpleMovingAverage(8);



                    double smma = 0, sma = 0;
                    double smma8 = 0, sma8 = 0;
                    double smma5 = 0, sma5 = 0;
                    int LLength = 13;

                    int ii = 0;
                    bool prev = false;
                    int prevTrend = -2;
                    foreach (DataRow dr in dt.Rows)
                    {

                        ii++;
                        try
                        {
                            avg20.AddSample((float)Convert.ToDouble(dr["f2"]));
                            avg50.AddSample((float)Convert.ToDouble(dr["f2"]));
                            avg200.AddSample((float)Convert.ToDouble(dr["f2"]));
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
                                dr["LOffset"] = dt.Rows[ii - 9]["L"];
                            }

                            if (ii >= 6)
                            {
                                dr["MOffset"] = dt.Rows[ii - 6]["M"];
                            }

                            if (ii >= 4)
                            {
                                dr["SOffset"] = dt.Rows[ii - 4]["S"];
                            }

                            if (ii > 1)
                            {
                                if (Convert.ToDouble(dr["f2"]) > Convert.ToDouble(dt.Rows[ii - 2]["f2"]))
                                {
                                    rsiGain14.AddSample((float)(Convert.ToDouble(dr["f2"]) - Convert.ToDouble(dt.Rows[ii - 2]["f2"])));
                                    rsiLoss14.AddSample(0);
                                }
                                else if (Convert.ToDouble(dt.Rows[ii - 2]["f2"]) > Convert.ToDouble(dr["f2"]))
                                {
                                    rsiLoss14.AddSample((float)(Convert.ToDouble(dt.Rows[ii - 2]["f2"]) - Convert.ToDouble(dr["f2"])));
                                    rsiGain14.AddSample(0);
                                }
                                else
                                {
                                    rsiGain14.AddSample(0);
                                    rsiLoss14.AddSample(0);
                                }

                                dr["rsi14"] = 100 - (100 / (1 + (rsiGain14.Average / rsiLoss14.Average)));

                            }

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

                            double sma201 = avg20.Average;
                            double sma501 = avg50.Average;
                            double sma2001 = avg200.Average;
                            double maxSma = Math.Max(Math.Max(sma201, sma501), sma2001);
                            double minSma = Math.Min(Math.Min(sma201, sma501), sma2001);
                            int trend = Convert.ToInt16(dr["Trend"]);

                            dr["20"] = sma201;
                            dr["50"] = sma501;
                            dr["200"] = sma2001;
                            dr["macd"] = macd;
                            dr["macd9"] = pEMA9;
                            dr["AC"] = (avg5.Average - avg34.Average) - avgAO5.Average;





                            double diff = 0;


                            double pc1 = Convert.ToDouble(dr["f2"]);
                            double pl1 = Convert.ToDouble(dr["f4"]);
                            double ph1 = Convert.ToDouble(dr["f3"]);
                            double pv1 = Convert.ToDouble(dr["f6"]) * Convert.ToDouble(dr["f2"]);


                            if (pc1 > 99)
                            {
                                double smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;
                                if (scrip != "BANKNIFTY")
                                {
                                    if (pl1 <= sma2001 + 0.2 && ph1 >= sma201 - 0.2 && sma201 >= sma501 && sma501 >= sma2001 && pc1 >= sma201 && trend >= 1)
                                    {
                                        prev = true;
                                        prevTrend = trend;
                                        //dr["BS"] = "BM";
                                    }

                                    smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;
                                    if (ph1 >= sma2001 - 0.2 && pl1 <= sma201 + 0.2 && sma201 <= sma501 && sma501 <= sma2001 && pc1 <= sma201 && trend < 0)
                                    {
                                        prev = true;
                                        prevTrend = trend;
                                        //dr["BS"] = "SM";
                                    }

                                    if (prevTrend != -2 && trend != prevTrend && prev == true)
                                    {
                                        if (trend >= 1 && pc1 > maxSma)
                                            dr["BS"] = "BM";
                                        else if (trend >= 0 && pc1 < minSma)
                                            dr["BS"] = "SM";
                                    }
                                }
                                else
                                {
                                    if (pl1 <= Math.Min(Math.Min(sma2001, sma501), sma201) + 0.2 && ph1 >= Math.Max(Math.Max(sma2001, sma501), sma201) - 0.2)
                                    {
                                        dr["BS"] = "T";
                                    }

                                    smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;
                                    if (ph1 >= Math.Max(Math.Max(sma2001, sma501), sma201) - 0.2 && pl1 <= Math.Min(Math.Min(sma2001, sma501), sma201) + 0.2)
                                    {
                                        dr["BS"] = "T";
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }



                    var rows1 = ds.Tables[0].Select("f2 = f3 and f2=f4 and f2= f5");
                    int count = rows1.Count();
                    foreach (var row in rows1)
                        row.Delete();
                }

                ds.Tables[0].AcceptChanges();

                int startOfTheWeekIndex = 0;

                //need to commented while running

                int cont = 0;
                int backTestDay = BTD;

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
                    else if (Math.Abs(Convert.ToInt32(ds.Tables[0].Rows[i]["period"]) - Convert.ToInt32(ds.Tables[0].Rows[i - 1]["period"])) > 50)
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





                if (backLiveTest || txtSwitchMode.Text == string.Empty)
                {
                    int xL = startOfTheWeekIndex - 1;
                    for (int deleteIndex = startOfTheWeekIndex + Convert.ToInt16(txtTam.Text) + 3; deleteIndex < ds.Tables[0].Rows.Count; deleteIndex++)
                    {
                        ds.Tables[0].Rows[deleteIndex].Delete();
                    }
                    ds.Tables[0].AcceptChanges();
                }


                //if (txtTam.Text == "-2")
                //{
                //    double per = (Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f5"]) - Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex - 1]["f2"])) / Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex - 1]["f2"])) * 100;
                //    Stocks30.Add(scrip, per);
                //}
                if (!string.IsNullOrEmpty(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["PSR"].ToString()))
                {
                    try
                    {
                        string candle = ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["candle"].ToString();
                        double close = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]);
                        double high = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f3"]);
                        double low = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f4"]);
                        double sr = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["support"]);
                        double rr = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["resistance"]);
                        double jaw = ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["Soffset"] == DBNull.Value ? 0 : Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["Soffset"]);
                        double teeth = ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["Moffset"] == DBNull.Value ? 0 : Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["Moffset"]);
                        double lips = ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["Loffset"] == DBNull.Value ? 0 : Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["Loffset"]);
                        double maxAlligator = Math.Max(Math.Max(jaw, lips), teeth);
                        double minAlligator = Math.Min(Math.Min(jaw, lips), teeth);
                        double AC = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["AC"]);
                        double AC1 = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 2]["AC"]);
                        double AC2 = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 3]["AC"]);
                        double AC3 = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 4]["AC"]);
                        double maxAc = Math.Max(AC1, Math.Max(AC, Math.Max(AC2, AC2)));
                        double minAc = Math.Min(AC1, Math.Min(AC, Math.Min(AC2, AC2)));

                    }
                    catch
                    {
                    }
                }

                if (!string.IsNullOrEmpty(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString()))
                {
                    double prevClose = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex - 1]["f2"]);
                    double close = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]);

                    List<double> high = new List<double>();
                    List<double> low = new List<double>();
                    int counter = 1;

                    for (int lastCandle = startOfTheWeekIndex; lastCandle < ds.Tables[0].Rows.Count; lastCandle++)
                    {

                        high.Add(Convert.ToDouble(ds.Tables[0].Rows[lastCandle]["f3"]));
                        low.Add(Convert.ToDouble(ds.Tables[0].Rows[lastCandle]["f4"]));
                        counter++;
                    }

                    //prevClose = Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - counter - 1]["f2"]);



                    if ((ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString().Contains("BM") && ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["candle"].ToString() == "G") || (ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString().Contains("SM") && /*close < prevClose &&*/ ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["candle"].ToString() == "R"))
                    {

                        if (orders.Select("direction='" + ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() + "' and scrip ='" + scrip + "'").Count() == 0)
                        {

                            int lotsize = Convert.ToInt32(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(MaxRisk / (Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min()), 0) : Math.Round(MaxRisk / (high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"])), 0));
                            decimal squareOffValue = Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(3 * (Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min()), 1) : Math.Round(3 * (high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"])), 1));
                            decimal stopLossValue = Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min(), 1) : Math.Round(high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1));

                            if (SMAQuanitty > 0)
                            {
                                double stopL = ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? low.Min() : high.Max();
                                //  PlacePartialOrders(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString(), close, high.Max(), low.Min(), scrip, lotsize, stopL, "5 MIN SMA");
                                xOrders.Add(1);
                                SMAQuanitty--;
                            }
                        }

                    }
                }

                // end of need to be commented


                if (!backLiveTest && txtSwitchMode.Text != string.Empty)
                {
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
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + scrip + Environment.NewLine);
            }
        }

        public void CalculateIndicatorsLoadOnce(string scrip)
        {
            try
            {

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

                dt.Columns.Add("20", typeof(double));
                dt.Columns.Add("50", typeof(double));
                dt.Columns.Add("200", typeof(double));
                dt.Columns.Add("macd", typeof(double));
                dt.Columns.Add("macd9", typeof(double));
                dt.Columns.Add("RSI14", typeof(double));
                dt.Columns.Add("BS", typeof(string));
                dt.Columns.Add("PSR", typeof(string));
                dt.Columns.Add("AC", typeof(double));
                dt.Columns.Add("L", typeof(double));
                dt.Columns.Add("LOffset", typeof(double));
                dt.Columns.Add("M", typeof(double));
                dt.Columns.Add("MOffset", typeof(double));
                dt.Columns.Add("S", typeof(double));
                dt.Columns.Add("SOffset", typeof(double));
                dt.Columns.Add("Support", typeof(double));
                dt.Columns.Add("Resistance", typeof(double));

                IMovingAverage avg20 = new SimpleMovingAverage(20);
                IMovingAverage avg50 = new SimpleMovingAverage(50);
                IMovingAverage avg200 = new SimpleMovingAverage(200);

                IMovingAverage avg9 = new SimpleMovingAverage(9);
                IMovingAverage avg12 = new SimpleMovingAverage(12);
                IMovingAverage avg26 = new SimpleMovingAverage(26);

                IMovingAverage rsiGain14 = new SimpleMovingAverage(14);
                IMovingAverage rsiLoss14 = new SimpleMovingAverage(14);

                double pEMA9 = 0, pEMA12 = 0, pEMA26 = 0, macd = 0;

                //accelartion oscillator
                IMovingAverage avg5 = new SimpleMovingAverage(5);
                IMovingAverage avg34 = new SimpleMovingAverage(34);
                IMovingAverage avgAO5 = new SimpleMovingAverage(5);

                //alligator
                IMovingAverage avg13 = new SimpleMovingAverage(13);
                IMovingAverage avg8 = new SimpleMovingAverage(8);



                double smma = 0, sma = 0;
                double smma8 = 0, sma8 = 0;
                double smma5 = 0, sma5 = 0;
                int LLength = 13;
                bool prev = false;
                int prevTrend = -2;

                int ii = 0;
                foreach (DataRow dr in dt.Rows)
                {

                    ii++;
                    try
                    {
                        avg20.AddSample((float)Convert.ToDouble(dr["f2"]));
                        avg50.AddSample((float)Convert.ToDouble(dr["f2"]));
                        avg200.AddSample((float)Convert.ToDouble(dr["f2"]));
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
                            dr["LOffset"] = dt.Rows[ii - 9]["L"];
                        }

                        if (ii >= 6)
                        {
                            dr["MOffset"] = dt.Rows[ii - 6]["M"];
                        }

                        if (ii >= 4)
                        {
                            dr["SOffset"] = dt.Rows[ii - 4]["S"];
                        }

                        if (ii > 1)
                        {
                            if (Convert.ToDouble(dr["f2"]) > Convert.ToDouble(dt.Rows[ii - 2]["f2"]))
                            {
                                rsiGain14.AddSample((float)(Convert.ToDouble(dr["f2"]) - Convert.ToDouble(dt.Rows[ii - 2]["f2"])));
                                rsiLoss14.AddSample(0);
                            }
                            else if (Convert.ToDouble(dt.Rows[ii - 2]["f2"]) > Convert.ToDouble(dr["f2"]))
                            {
                                rsiLoss14.AddSample((float)(Convert.ToDouble(dt.Rows[ii - 2]["f2"]) - Convert.ToDouble(dr["f2"])));
                                rsiGain14.AddSample(0);
                            }
                            else
                            {
                                rsiGain14.AddSample(0);
                                rsiLoss14.AddSample(0);
                            }

                            dr["rsi14"] = 100 - (100 / (1 + (rsiGain14.Average / rsiLoss14.Average)));

                        }

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

                        double sma201 = avg20.Average;
                        double sma501 = avg50.Average;
                        double sma2001 = avg200.Average;

                        dr["20"] = sma201;
                        dr["50"] = sma501;
                        dr["200"] = sma2001;
                        dr["macd"] = macd;
                        dr["macd9"] = pEMA9;
                        dr["AC"] = (avg5.Average - avg34.Average) - avgAO5.Average;



                        double maxSma = Math.Max(Math.Max(sma201, sma501), sma2001);
                        double minSma = Math.Min(Math.Min(sma201, sma501), sma2001);
                        int trend = Convert.ToInt16(dr["Trend"]);

                        double diff = 0;


                        double pc1 = Convert.ToDouble(dr["f2"]);
                        double pl1 = Convert.ToDouble(dr["f4"]);
                        double ph1 = Convert.ToDouble(dr["f3"]);
                        double pv1 = Convert.ToDouble(dr["f6"]) * Convert.ToDouble(dr["f2"]);


                        double smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;
                        if (scrip != "BANKNIFTY")
                        {
                            if (pl1 <= sma2001 + 0.2 && ph1 >= sma201 - 0.2 && sma201 >= sma501 && sma501 >= sma2001 && pc1 >= sma201 && trend >= 1)
                            {
                                prev = true;
                                prevTrend = trend;
                                //dr["BS"] = "BM";
                            }

                            smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;
                            if (ph1 >= sma2001 - 0.2 && pl1 <= sma201 + 0.2 && sma201 <= sma501 && sma501 <= sma2001 && pc1 <= sma201 && trend < 0)
                            {
                                prev = true;
                                prevTrend = trend;
                                //dr["BS"] = "SM";
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
                        else
                        {
                            if (pl1 <= Math.Min(Math.Min(sma2001, sma501), sma201) + 0.2 && ph1 >= Math.Max(Math.Max(sma2001, sma501), sma201) - 0.2)
                            {
                                dr["BS"] = "T";
                            }

                            smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;
                            if (ph1 >= Math.Max(Math.Max(sma2001, sma501), sma201) - 0.2 && pl1 <= Math.Min(Math.Min(sma2001, sma501), sma201) + 0.2)
                            {
                                dr["BS"] = "T";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
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
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + scrip + Environment.NewLine);
            }
        }

        public void CalculateIndicatorsLoadOnceForSuperTrend(string scrip)
        {
            try
            {

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

                dt.Columns.Add("20", typeof(double));
                dt.Columns.Add("50", typeof(double));
                dt.Columns.Add("200", typeof(double));
                dt.Columns.Add("macd", typeof(double));
                dt.Columns.Add("macd9", typeof(double));
                dt.Columns.Add("RSI14", typeof(double));
                dt.Columns.Add("BS", typeof(string));
                dt.Columns.Add("PSR", typeof(string));
                dt.Columns.Add("AC", typeof(double));
                dt.Columns.Add("L", typeof(double));
                dt.Columns.Add("LOffset", typeof(double));
                dt.Columns.Add("M", typeof(double));
                dt.Columns.Add("MOffset", typeof(double));
                dt.Columns.Add("S", typeof(double));
                dt.Columns.Add("SOffset", typeof(double));
                dt.Columns.Add("Support", typeof(double));
                dt.Columns.Add("Resistance", typeof(double));

                IMovingAverage avg20 = new SimpleMovingAverage(20);
                IMovingAverage avg50 = new SimpleMovingAverage(50);
                IMovingAverage avg200 = new SimpleMovingAverage(200);

                IMovingAverage avg9 = new SimpleMovingAverage(9);
                IMovingAverage avg12 = new SimpleMovingAverage(12);
                IMovingAverage avg26 = new SimpleMovingAverage(26);

                IMovingAverage rsiGain14 = new SimpleMovingAverage(14);
                IMovingAverage rsiLoss14 = new SimpleMovingAverage(14);

                double pEMA9 = 0, pEMA12 = 0, pEMA26 = 0, macd = 0;

                //accelartion oscillator
                IMovingAverage avg5 = new SimpleMovingAverage(5);
                IMovingAverage avg34 = new SimpleMovingAverage(34);
                IMovingAverage avgAO5 = new SimpleMovingAverage(5);

                //alligator
                IMovingAverage avg13 = new SimpleMovingAverage(13);
                IMovingAverage avg8 = new SimpleMovingAverage(8);



                double smma = 0, sma = 0;
                double smma8 = 0, sma8 = 0;
                double smma5 = 0, sma5 = 0;
                int LLength = 13;
                bool prev = false;
                int prevTrend = -2;

                int ii = 0;
                foreach (DataRow dr in dt.Rows)
                {

                    ii++;
                    try
                    {
                        avg20.AddSample((float)Convert.ToDouble(dr["f2"]));
                        avg50.AddSample((float)Convert.ToDouble(dr["f2"]));
                        avg200.AddSample((float)Convert.ToDouble(dr["f2"]));
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
                            dr["LOffset"] = dt.Rows[ii - 9]["L"];
                        }

                        if (ii >= 6)
                        {
                            dr["MOffset"] = dt.Rows[ii - 6]["M"];
                        }

                        if (ii >= 4)
                        {
                            dr["SOffset"] = dt.Rows[ii - 4]["S"];
                        }

                        if (ii > 1)
                        {
                            if (Convert.ToDouble(dr["f2"]) > Convert.ToDouble(dt.Rows[ii - 2]["f2"]))
                            {
                                rsiGain14.AddSample((float)(Convert.ToDouble(dr["f2"]) - Convert.ToDouble(dt.Rows[ii - 2]["f2"])));
                                rsiLoss14.AddSample(0);
                            }
                            else if (Convert.ToDouble(dt.Rows[ii - 2]["f2"]) > Convert.ToDouble(dr["f2"]))
                            {
                                rsiLoss14.AddSample((float)(Convert.ToDouble(dt.Rows[ii - 2]["f2"]) - Convert.ToDouble(dr["f2"])));
                                rsiGain14.AddSample(0);
                            }
                            else
                            {
                                rsiGain14.AddSample(0);
                                rsiLoss14.AddSample(0);
                            }

                            dr["rsi14"] = 100 - (100 / (1 + (rsiGain14.Average / rsiLoss14.Average)));

                        }

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

                        double sma201 = avg20.Average;
                        double sma501 = avg50.Average;
                        double sma2001 = avg200.Average;

                        dr["20"] = sma201;
                        dr["50"] = sma501;
                        dr["200"] = sma2001;
                        dr["macd"] = macd;
                        dr["macd9"] = pEMA9;

                        dr["AC"] = (avg5.Average - avg34.Average) - avgAO5.Average;
                        double maxSma = Math.Max(Math.Max(sma201, sma501), sma2001);
                        double minSma = Math.Min(Math.Min(sma201, sma501), sma2001);

                        int trend = Convert.ToInt16(dr["Trend"]);

                        double diff = 0;
                        double pc1 = Convert.ToDouble(dr["f2"]);
                        double pl1 = Convert.ToDouble(dr["f4"]);
                        double ph1 = Convert.ToDouble(dr["f3"]);
                        double pv1 = Convert.ToDouble(dr["f6"]) * Convert.ToDouble(dr["f2"]);


                        double smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;
                        if (scrip != "BANKNIFTY")
                        {
                            if (pl1 <= sma2001 + 0.2 && ph1 >= sma201 - 0.2 && sma201 >= sma501 && sma501 >= sma2001 && pc1 >= sma201 && trend >= 1)
                            {
                                prev = true;
                                prevTrend = trend;
                                //dr["BS"] = "BM";
                            }

                            smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;
                            if (ph1 >= sma2001 - 0.2 && pl1 <= sma201 + 0.2 && sma201 <= sma501 && sma501 <= sma2001 && pc1 <= sma201 && trend < 0)
                            {
                                prev = true;
                                prevTrend = trend;
                                //dr["BS"] = "SM";
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
                        else
                        {
                            if (pl1 <= Math.Min(Math.Min(sma2001, sma501), sma201) + 0.2 && ph1 >= Math.Max(Math.Max(sma2001, sma501), sma201) - 0.2)
                            {
                                dr["BS"] = "T";
                            }

                            smaDiff = ((((sma501 - sma2001) / sma2001) * 100) + (((sma201 - sma501) / sma501) * 100)) / 2;
                            if (ph1 >= Math.Max(Math.Max(sma2001, sma501), sma201) - 0.2 && pl1 <= Math.Min(Math.Min(sma2001, sma501), sma201) + 0.2)
                            {
                                dr["BS"] = "T";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
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
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + scrip + Environment.NewLine);
            }
        }

        static object orderPlacement = new object();

        public bool PlacePartialOrders(NSA.Order o)
        {


            bool orderplaced = false;


            //return false;
            lock (orderPlacement)
            {

                string direction = o.TransactionType;
                double close = o.EntryPrice;
                double high = o.High;
                double low = o.Low;
                string scrip = o.Scrip;
                int quantity = o.Quantity;
                double stopLoss = o.Stoploss;
                string desc = o.Strategy;

                double dayDiff = 0;


                DataRow drOrderLeg1 = orders.NewRow();
                DataRow drOrderLeg2 = orders.NewRow();
                DataRow drOrderLeg3 = orders.NewRow();

                int lotSize = quantity;
                double stoplossOrder = direction == "BM" ? stopLoss - (stopLoss * 0.11 / 100) : stopLoss + (stopLoss * 0.11 / 100);


                decimal stopLossValue = Convert.ToDecimal(direction == "BM" ? Math.Round(close - stoplossOrder, 1) : Math.Round(stoplossOrder - close, 1));
                decimal squareOffValue1 = stopLossValue;
                decimal squareOffValue2 = stopLossValue * 2;
                decimal squareOffValue3 = stopLossValue * 10;

                double stopLossCoverOrder = Convert.ToDouble(direction == "BM" ? Math.Round((decimal)close - stopLossValue, 1) : Math.Round((decimal)close + stopLossValue, 1));
                double diffBS = 0;


                drOrderLeg1.ItemArray = new object[] { scrip, close, high, low, lotSize, squareOffValue1, stopLossCoverOrder, o.TimeStamp, direction, (double)0.0, (double)0.0, o.Strategy + "LEG 1", close, (double)0.0, "Leg 1", (double)0.0, lotSize, "" };
                orderDetails.Add(drOrderLeg1);



                //drOrderLeg2.ItemArray = new object[] { scrip, close, high, low, lotSize, squareOffValue2, stopLossCoverOrder, o.TimeStamp, direction, (double)0.0, (double)0.0, o.Strategy + "LEG 2", close, (double)0.0, "Leg 2", (double)0.0, lotSize, "" };
                //orderDetails.Add(drOrderLeg2);


                //drOrderLeg3.ItemArray = new object[] { scrip, close, high, low, lotSize, squareOffValue3, stopLossCoverOrder, o.TimeStamp, direction, (double)0.0, (double)0.0, o.Strategy + "LEG 3", close, (double)0.0, "Leg 3", (double)0.0, lotSize, "" };
                //orderDetails.Add(drOrderLeg3);


                orderplaced = true;
                IncrementDecrement(o.Strategy, -1);
                /*
                if (txtSwitchMode.Text != string.Empty)
                {
                    //return orderplaced;
                    if (!backLiveTest)
                    {
                        foreach (Kite kiteUser in kUsers)
                        {
                            try
                            {
                                OrderPlacingMode = Constants.VARIETY_BO;
                                Dictionary<string, dynamic> response = kiteUser.PlaceOrder(
           Exchange: Constants.EXCHANGE_NSE,
           TradingSymbol: scrip,
           TransactionType: direction == "BM" ? Constants.TRANSACTION_TYPE_BUY : Constants.TRANSACTION_TYPE_SELL,
           Quantity: lotSize,
           //Price: Convert.ToDecimal(Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
           OrderType: Constants.ORDER_TYPE_LIMIT,
           Product: Constants.PRODUCT_MIS,
           //StoplossValue: Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min(), 1) : Math.Round(high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
           //TriggerPrice: direction == "BM" ? Math.Round((decimal)close - stopLossValue, 1) : Math.Round((decimal)close + stopLossValue, 1),//
           StoplossValue: stopLossValue,
           SquareOffValue: squareOffValue1,
           Price: (decimal)(direction == "BM" ? Math.Round(close + close * 0.0011, 1) : Math.Round(close - close * 0.0011, 1)),

           //        SquareOffValue: squareOffValue,
           Validity: Constants.VALIDITY_DAY,
           Variety: Constants.VARIETY_BO//,,


           );
                                response = kiteUser.PlaceOrder(
          Exchange: Constants.EXCHANGE_NSE,
          TradingSymbol: scrip,
          TransactionType: direction == "BM" ? Constants.TRANSACTION_TYPE_BUY : Constants.TRANSACTION_TYPE_SELL,
          Quantity: lotSize,
          //Price: Convert.ToDecimal(Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
          OrderType: Constants.ORDER_TYPE_LIMIT,
          Product: Constants.PRODUCT_MIS,
          //StoplossValue: Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min(), 1) : Math.Round(high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
          //TriggerPrice: direction == "BM" ? Math.Round((decimal)close - stopLossValue, 1) : Math.Round((decimal)close + stopLossValue, 1),//
          StoplossValue: stopLossValue,
          SquareOffValue: squareOffValue2,
          Price: (decimal)(direction == "BM" ? Math.Round(close + close * 0.0011, 1) : Math.Round(close - close * 0.0011, 1)),

          //        SquareOffValue: squareOffValue,
          Validity: Constants.VALIDITY_DAY,
          Variety: Constants.VARIETY_BO//,,


          );
                                response = kiteUser.PlaceOrder(
           Exchange: Constants.EXCHANGE_NSE,
           TradingSymbol: scrip,
           TransactionType: direction == "BM" ? Constants.TRANSACTION_TYPE_BUY : Constants.TRANSACTION_TYPE_SELL,
           Quantity: lotSize,
           //Price: Convert.ToDecimal(Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
           OrderType: Constants.ORDER_TYPE_LIMIT,
           Product: Constants.PRODUCT_MIS,
           //StoplossValue: Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min(), 1) : Math.Round(high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
           //TriggerPrice: direction == "BM" ? Math.Round((decimal)close - stopLossValue, 1) : Math.Round((decimal)close + stopLossValue, 1),//
           StoplossValue: stopLossValue,
           SquareOffValue: squareOffValue3,
           Price: (decimal)(direction == "BM" ? Math.Round(close + close * 0.0011, 1) : Math.Round(close - close * 0.0011, 1)),

           //        SquareOffValue: squareOffValue,
           Validity: Constants.VALIDITY_DAY,
           Variety: Constants.VARIETY_BO//,,


           );
                            }
                            catch (Exception ex)
                            {
                                //  MessageBox.Show(ex.Message);
                                if (true)//MessageBox.Show("Do you want to place CO order?", "BO- Failed", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    try
                                    {
                                        OrderPlacingMode = Constants.VARIETY_CO;
                                        Dictionary<string, dynamic> response = kiteUser.PlaceOrder(
Exchange: Constants.EXCHANGE_NSE,
TradingSymbol: scrip,
TransactionType: direction == "BM" ? Constants.TRANSACTION_TYPE_BUY : Constants.TRANSACTION_TYPE_SELL,
Quantity: lotSize,
//Price: Convert.ToDecimal(Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
OrderType: Constants.ORDER_TYPE_LIMIT,
Product: Constants.PRODUCT_MIS,
//StoplossValue: Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min(), 1) : Math.Round(high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
TriggerPrice: Math.Round((decimal)stoplossOrder, 1),
//StoplossValue: stopLossValue,
//SquareOffValue: squareOffValue1,
Price: (decimal)(direction == "BM" ? Math.Round(close + close * 0.0011, 1) : Math.Round(close - close * 0.0011, 1)),

//        SquareOffValue: squareOffValue,
Validity: Constants.VALIDITY_DAY,
Variety: Constants.VARIETY_CO//,,


);
                                        response = kiteUser.PlaceOrder(
                  Exchange: Constants.EXCHANGE_NSE,
                  TradingSymbol: scrip,
                  TransactionType: direction == "BM" ? Constants.TRANSACTION_TYPE_BUY : Constants.TRANSACTION_TYPE_SELL,
                  Quantity: lotSize,
                  //Price: Convert.ToDecimal(Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
                  OrderType: Constants.ORDER_TYPE_LIMIT,
                  Product: Constants.PRODUCT_MIS,
                  //StoplossValue: Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min(), 1) : Math.Round(high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
                  TriggerPrice: Math.Round((decimal)stoplossOrder, 1),//
                                                                      //StoplossValue: stopLossValue,
                                                                      //SquareOffValue: squareOffValue2,
                  Price: (decimal)(direction == "BM" ? Math.Round(close + close * 0.0011, 1) : Math.Round(close - close * 0.0011, 1)),

                  //        SquareOffValue: squareOffValue,
                  Validity: Constants.VALIDITY_DAY,
                  Variety: Constants.VARIETY_CO//,,


                  );
                                        response = kiteUser.PlaceOrder(
                   Exchange: Constants.EXCHANGE_NSE,
                   TradingSymbol: scrip,
                   TransactionType: direction == "BM" ? Constants.TRANSACTION_TYPE_BUY : Constants.TRANSACTION_TYPE_SELL,
                   Quantity: lotSize,
                   //Price: Convert.ToDecimal(Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
                   OrderType: Constants.ORDER_TYPE_LIMIT,
                   Product: Constants.PRODUCT_MIS,
                   //StoplossValue: Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min(), 1) : Math.Round(high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
                   TriggerPrice: Math.Round((decimal)stoplossOrder, 1),//
                                                                       //StoplossValue: stopLossValue,
                                                                       //SquareOffValue: squareOffValue3,
                   Price: (decimal)(direction == "BM" ? Math.Round(close + close * 0.0011, 1) : Math.Round(close - close * 0.0011, 1)),

                   //        SquareOffValue: squareOffValue,
                   Validity: Constants.VALIDITY_DAY,
                   Variety: Constants.VARIETY_CO//,,


                   );
                                    }
                                    catch (Exception exCo)
                                    {
                                        try
                                        {
                                            OrderPlacingMode = Constants.VARIETY_REGULAR;
                                            Dictionary<string, dynamic> response = kiteUser.PlaceOrder(
    Exchange: Constants.EXCHANGE_NSE,
    TradingSymbol: scrip,
    TransactionType: direction == "BM" ? Constants.TRANSACTION_TYPE_BUY : Constants.TRANSACTION_TYPE_SELL,
    Quantity: lotSize,
    //Price: Convert.ToDecimal(Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
    OrderType: Constants.ORDER_TYPE_LIMIT,
    Product: Constants.PRODUCT_MIS,
    //StoplossValue: Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min(), 1) : Math.Round(high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
    //TriggerPrice: Math.Round((decimal)stoplossOrder, 1),
    //StoplossValue: stopLossValue,
    //SquareOffValue: squareOffValue1,
    Price: (decimal)(direction == "BM" ? Math.Round(close + close * 0.0011, 1) : Math.Round(close - close * 0.0011, 1)),

    //        SquareOffValue: squareOffValue,
    Validity: Constants.VALIDITY_DAY,
    Variety: Constants.VARIETY_REGULAR//,,

    );
                                            response = kiteUser.PlaceOrder(
                      Exchange: Constants.EXCHANGE_NSE,
                      TradingSymbol: scrip,
                      TransactionType: direction == "BM" ? Constants.TRANSACTION_TYPE_BUY : Constants.TRANSACTION_TYPE_SELL,
                      Quantity: lotSize,
                      //Price: Convert.ToDecimal(Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
                      OrderType: Constants.ORDER_TYPE_LIMIT,
                      Product: Constants.PRODUCT_MIS,
                      //StoplossValue: Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min(), 1) : Math.Round(high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
                      //TriggerPrice: Math.Round((decimal)stoplossOrder, 1),//
                      //StoplossValue: stopLossValue,
                      //SquareOffValue: squareOffValue2,
                      Price: (decimal)(direction == "BM" ? Math.Round(close + close * 0.0011, 1) : Math.Round(close - close * 0.0011, 1)),

                      //        SquareOffValue: squareOffValue,
                      Validity: Constants.VALIDITY_DAY,
                      Variety: Constants.VARIETY_REGULAR//,,


                      );
                                            response = kiteUser.PlaceOrder(
                       Exchange: Constants.EXCHANGE_NSE,
                       TradingSymbol: scrip,
                       TransactionType: direction == "BM" ? Constants.TRANSACTION_TYPE_BUY : Constants.TRANSACTION_TYPE_SELL,
                       Quantity: lotSize,
                       //Price: Convert.ToDecimal(Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
                       OrderType: Constants.ORDER_TYPE_LIMIT,
                       Product: Constants.PRODUCT_MIS,
                       //StoplossValue: Convert.ToDecimal(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["BS"].ToString() == "BM" ? Math.Round(Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]) - low.Min(), 1) : Math.Round(high.Max() - Convert.ToDouble(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["f2"]), 1)),
                       //TriggerPrice: Math.Round((decimal)stoplossOrder, 1),//
                       //StoplossValue: stopLossValue,
                       //SquareOffValue: squareOffValue3,
                       Price: (decimal)(direction == "BM" ? Math.Round(close + close * 0.0011, 1) : Math.Round(close - close * 0.0011, 1)),

                       //        SquareOffValue: squareOffValue,
                       Validity: Constants.VALIDITY_DAY,
                       Variety: Constants.VARIETY_REGULAR//,,


                       );
                                        }
                                        catch (Exception exMIS)
                                        {
                                            MessageBox.Show(exMIS.Message);
                                        }
                                    }
                                }


                            }
                        }
                    }
                }
                */
            }





            return orderplaced;
        }




        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }

            return objectOut;
        }
        public void CalculateIndicators30(string scrip)
        {
            try
            {
                DataTable dt = null;
                string FileName = string.Empty;
                if (backLiveTest)
                {
                    FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\30\" + scrip.Replace("-", string.Empty) + "30.csv";
                }
                else
                {
                    FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\30\" + scrip.Replace("-", string.Empty) + "30.csv";
                }

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

                IMovingAverage avg20 = new SimpleMovingAverage(20);
                IMovingAverage avg50 = new SimpleMovingAverage(50);
                IMovingAverage avg200 = new SimpleMovingAverage(200);

                IMovingAverage avg9 = new SimpleMovingAverage(9);
                IMovingAverage avg12 = new SimpleMovingAverage(12);
                IMovingAverage avg26 = new SimpleMovingAverage(26);

                IMovingAverage rsiGain14 = new SimpleMovingAverage(14);
                IMovingAverage rsiLoss14 = new SimpleMovingAverage(14);

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
                        if (0 == cont)
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
                        if (0 == cont)
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
                        if (0 == cont)
                        {
                            break;
                        }
                        else
                        {
                            cont++;
                        }
                    }
                }


                //if (mySettings["BackLiveTest"] == "True")
                //{
                //    int xL = startOfTheWeekIndex - 1;
                //    for (int deleteIndex = xL + goTimeCount; deleteIndex < ds.Tables[0].Rows.Count; deleteIndex++)
                //    {
                //        ds.Tables[0].Rows[deleteIndex].Delete();
                //    }
                //    ds.Tables[0].AcceptChanges();
                //}

                if (txtTam.Text == "0")
                {
                    //double per = (Math.Abs(Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f5"]) - Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex - 1]["f2"])) / Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex - 1]["f2"])) * 100;
                    //if (!Stocks30.ContainsKey(scrip))
                    //    Stocks30.Add(scrip, per);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
            }
        }



        public void CalculateIndicators60(string scrip)
        {
            try
            {
                DataTable dt = null;
                string FileName = string.Empty;
                if (backLiveTest)
                {
                    FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\Data\Hourly\" + scrip.Replace("-", string.Empty) + ".csv";
                }
                else
                {
                    FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\Data\Hourly\" + scrip.Replace("-", string.Empty) + ".csv";
                }

                List<StockData> allLevels = DeSerializeObject<List<StockData>>(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Anti\" + scrip + ".xml");
                DateTime currentTradingDate = Convert.ToDateTime(dateMapping.Tables[0].Rows[dateMapping.Tables[0].Rows.Count - 1 - BTD]["Date"]);
                StockData todaysLevel = null;

                if (allLevels.Where(a => a.TradingDate == currentTradingDate).Count() > 0)
                {
                    todaysLevel = allLevels.Where(a => a.TradingDate == currentTradingDate).First();
                }
                else
                {
                    todaysLevel = allLevels[allLevels.Count - 1];
                }

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

                int backTestDay = BTD;
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


                //if (mySettings["BackLiveTest"] == "True")
                //{
                //    int xL = startOfTheWeekIndex - 1;
                //    for (int deleteIndex = xL + goTimeCount; deleteIndex < ds.Tables[0].Rows.Count; deleteIndex++)
                //    {
                //        ds.Tables[0].Rows[deleteIndex].Delete();
                //    }
                //    ds.Tables[0].AcceptChanges();
                //}

                var pObj = pList.Where(a => a.scrip == scrip).ToList().FirstOrDefault();
                todaysLevel.dR2 = pObj.r2;
                todaysLevel.dR3 = pObj.r3;
                todaysLevel.dR1 = pObj.r1;
                todaysLevel.dPP = pObj.pivot;
                todaysLevel.dS2 = pObj.s2;
                todaysLevel.dS1 = pObj.s1;
                todaysLevel.dS3 = pObj.s3;


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
                int numberOfStocks = Convert.ToInt32(MaxTurnOver / close);
                double risk = numberOfStocks * (close - low);
                allStocks.Add(scrip, new StockData() { Direction = "BM", Quantity = 0, Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = 0, Levels = listX });
                if ((high - close) < (close - open) && list.Count(a => a.SupportOrResistance == "S") > list.Count(a => a.SupportOrResistance == "R") && list.Where(a => a.LevelName.Contains("w")).Count() > 0 && close > list.Where(a => a.LevelName.Contains("w")).First().price && open < list.Where(a => a.LevelName.Contains("w")).First().price && close > open)
                {


                    if (!Stocks60.ContainsKey(scrip))
                    {
                        foreach (SR s in list)
                        {
                            if (high >= s.price && close < s.price)
                            {
                                return;
                            }
                        }
                        Stocks60.Add(scrip, new StockData() { Direction = "BM", Quantity = Convert.ToInt32(numberOfStocks), Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("w")).First().price, Levels = listX });
                    }
                }
                else if ((close - low) < (open - close) && list.Count(a => a.SupportOrResistance == "R") > list.Count(a => a.SupportOrResistance == "S") && list.Where(a => a.LevelName.Contains("w")).Count() > 0 && open > list.Where(a => a.LevelName.Contains("w")).First().price && close < list.Where(a => a.LevelName.Contains("w")).First().price && open > close)
                {
                    foreach (SR s in list)
                    {
                        if (low <= s.price && close > s.price)
                        {
                            return;
                        }
                    }



                    if (!Stocks60.ContainsKey(scrip))
                    {
                        Stocks60.Add(scrip, new StockData() { Direction = "SM", Quantity = Convert.ToInt32(numberOfStocks), Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("w")).First().price, Levels = listX });
                    }

                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
            }

            rXs.StocksIdentified = allStocks.OrderBy(a => a.Value.Risk).ToDictionary(a => a.Key, b => b.Value);

        }
        Receipt rXs = new Receipt();
        public void CalculateIndicators15(string scrip)
        {
            try
            {
                DataTable dt = null;
                string FileName = string.Empty;
                if (backLiveTest)
                {
                    FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\Data\15\" + scrip.Replace("-", string.Empty) + ".csv";
                }
                else
                {
                    FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\Data\15\" + scrip.Replace("-", string.Empty) + ".csv";
                }

                List<StockData> allLevels = DeSerializeObject<List<StockData>>(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Anti\" + scrip + ".xml");
                DateTime currentTradingDate = Convert.ToDateTime(dateMapping.Tables[0].Rows[dateMapping.Tables[0].Rows.Count - 1 - BTD]["Date"]);
                StockData todaysLevel = null;

                if (allLevels.Where(a => a.TradingDate == currentTradingDate).Count() > 0)
                {
                    todaysLevel = allLevels.Where(a => a.TradingDate == currentTradingDate).First();
                }
                else
                {
                    todaysLevel = allLevels[allLevels.Count - 1];
                }

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


                int backTestDay = BTD;
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


                //if (mySettings["BackLiveTest"] == "True")
                //{
                //    int xL = startOfTheWeekIndex - 1;
                //    for (int deleteIndex = xL + goTimeCount; deleteIndex < ds.Tables[0].Rows.Count; deleteIndex++)
                //    {
                //        ds.Tables[0].Rows[deleteIndex].Delete();
                //    }
                //    ds.Tables[0].AcceptChanges();
                //}

                //list.Add(new SR { price = 0, LevelName = "D20MA" });
                startOfTheWeekIndex = startOfTheWeekIndex + Convert.ToInt32(txtTam.Text) / 3;

                var pObj = pList.Where(a => a.scrip == scrip).ToList().FirstOrDefault();
                todaysLevel.dR2 = pObj.r2;
                todaysLevel.dR3 = pObj.r3;
                todaysLevel.dR1 = pObj.r1;
                todaysLevel.dPP = pObj.pivot;
                todaysLevel.dS2 = pObj.s2;
                todaysLevel.dS1 = pObj.s1;
                todaysLevel.dS3 = pObj.s3;


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
                list.Add(new SR { price = Math.Round(Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["SuperTrend"]), 2), LevelName = "ST15" });

                double low = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f4"]);
                double high = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f3"]);
                double close = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f2"]);
                double open = Convert.ToDouble(ds.Tables[0].Rows[startOfTheWeekIndex]["f5"]);

                list = WildAnalysis(Math.Round(low, 1), Math.Round(high, 1), Math.Round(close, 1), 0, list);


                if (list.Where(a => a.LevelName.Contains("ST15")).Count() > 0 && close > list.Where(a => a.LevelName.Contains("ST15")).First().price && close > open)
                {
                    int numberOfStocks = Convert.ToInt32(MaxTurnOver / close);
                    double risk = numberOfStocks * (close - low);
                    if (!Stocks15.ContainsKey(scrip))
                    {
                        Stocks15.Add(scrip, new StockData() { Direction = "BM", Quantity = Convert.ToInt32(numberOfStocks), Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("ST15")).First().price });
                    }
                }
                else if (list.Where(a => a.LevelName.Contains("ST15")).Count() > 0 && close < list.Where(a => a.LevelName.Contains("ST15")).First().price && open > close)
                {
                    int numberOfStocks = Convert.ToInt32(MaxTurnOver / close);
                    double risk = numberOfStocks * (high - close);
                    if (!Stocks15.ContainsKey(scrip))
                    {
                        Stocks15.Add(scrip, new StockData() { Direction = "SM", Quantity = Convert.ToInt32(numberOfStocks), Risk = risk, Low = low, High = high, Close = close, Open = open, stopLoss = list.Where(a => a.LevelName.Contains("ST15")).First().price });
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
            }
        }

        public List<SR> WildAnalysis(double low, double high, double close, double open, List<SR> fullList)
        {
            low = low - (low * 0.001);
            high = high + (high * 0.001);
            List<SR> resultSet = new List<SR>();

            foreach (SR srObje in fullList)
            {
                if (close < srObje.price && high >= srObje.price && open <= srObje.price)
                {
                    SR x = new SR();
                    x.price = srObje.price;
                    x.LevelName = srObje.LevelName;
                    x.SupportOrResistance = "R";
                    resultSet.Add(x);
                }
                else if (close > srObje.price && low <= srObje.price && open>=srObje.price)
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



        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (goLiveTimer.Interval == 60001)
            {
                //  ifCalculator = new _10MCE();
            }
            else
            {
                // ifCalculator = new _10MCE();
            }

            if (final.Columns.Count == 0)
            {
                final.Columns.Add("stock", typeof(string));
                final.Columns.Add("direction", typeof(string));
                final.Columns.Add("per", typeof(double));
                final.Columns.Add("high", typeof(double));
                final.Columns.Add("low", typeof(double));
                final.Columns.Add("isBreakOut", typeof(bool));
            }
            else
            {
                final.Rows.Clear();
                dtStocksBuy.Tables.Clear();
                dtStocksBuy2.Tables.Clear();
                dtStocksSell.Tables.Clear();
                dtStocksSell2.Tables.Clear();
                if (!appendStock)
                    sortedDT.Rows.Clear();
            }

            bool x = false;
            DataTable scanResult = ifCalculator.ScanStocks(Convert.ToInt32(txtTam.Text), BTD, Convert.ToInt16(txtTam.Text) + 2);

            //if (Convert.ToInt32(txtTam.Text) == -2 && !x)
            //{
            //    ifCalculator = new __BankNifty();
            //    scanResult.Merge(ifCalculator.ScanStocks(Convert.ToInt32(txtTam.Text), Convert.ToInt32(txtBTD.Text), goTimeCount));
            //    x = true;
            //}
            appendStock = false;
            if (appendStock)
            {
                if (sortedDT == null)
                {
                    sortedDT = scanResult.Copy();
                }
                else
                {
                    foreach (DataRow dr in scanResult.Rows)
                    {
                        sortedDT.Rows.Add(dr.ItemArray);
                    }
                }

            }
            else
            {
                sortedDT = scanResult.Copy();
            }

            rgvStocks.DataSource = sortedDT;

        }

        Dictionary<string, Dictionary<string, List<Candle>>> allData = new Dictionary<string, Dictionary<string, List<Candle>>>();


        private void LoadInstruments(string apiKey, string accessToken)
        {
            try
            {

                string URL_ADDRESS = string.Format("https://api.kite.trade/instruments?api_key={0}&access_token={1}", apiKey, accessToken);
                // ===== You shoudn't need to edit the lines below =====

                // Create the web request
                HttpWebRequest request = WebRequest.Create(new Uri(URL_ADDRESS)) as HttpWebRequest;

                // Set type to POST
                request.Method = "GET";
                request.ContentType = "application/json";

                // Create the data we want to send
                string result = string.Empty;
                // Get response and return it
                XmlDocument xmlResult = new XmlDocument();

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    result = reader.ReadToEnd();
                    reader.Close();

                }

                File.WriteAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\ZERODHA\InstrumentToken\instrumenttoken.csv", result);

            }
            catch (Exception ex)
            {

                LogStatus("Failed to download instruments" + ex.Message);


            }

        }
        StockOHLC ohlcObj = new StockOHLC();
        private void CallWebServiceZerodha(string InstrumentToken, string SymbolName, string dateFrom, string dateTo, string apiKey, string accessToken, string interval, string period = "30")
        {
            // File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\Instr.txt", InstrumentToken.ToString() + ",");

            try
            {
                if (allData.ContainsKey(interval))
                {
                    if (allData[interval].ContainsKey(SymbolName))
                    {
                        allData[interval].Remove(SymbolName);
                    }
                }
                else
                {
                    allData.Add(interval, new Dictionary<string, List<Candle>>());
                }
                List<Candle> ls = new List<Candle>();

                if (!DONT_DELETE)//!DONT_DELETE)
                {

                    string URL_ADDRESS = string.Format("https://api.kite.trade/instruments/historical/{0}/{5}?from={1}&to={2}&api_key={3}&access_token={4}", InstrumentToken, dateFrom, dateTo, apiKey, accessToken, interval);
                    // ===== You shoudn't need to edit the lines below =====

                    // Create the web request
                    HttpWebRequest request = WebRequest.Create(new Uri(URL_ADDRESS)) as HttpWebRequest;

                    // Set type to POST
                    request.Method = "GET";
                    request.ContentType = "application/json";

                    // Create the data we want to send
                    string result = string.Empty;
                    // Get response and return it
                    XmlDocument xmlResult = new XmlDocument();

                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        result = reader.ReadToEnd();
                        reader.Close();
                    }
                    File.WriteAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\ZERODHA\" + period + "\\" + SymbolName + ".json", result);
                    ls = TokenChannel.ConvertToJason(result, SymbolName);
                }
                else
                {
                    ls = TokenChannel.ConvertToJason(File.ReadAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\ZERODHA\" + period + "\\" + SymbolName + ".json"), SymbolName);

                }
                allData[interval].Add(SymbolName, ls);

            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message + " Download error!!");
            }
            
        }

        //private DataSet GetQuotesZerodha(string apiKey, string accessToken)
        //{
        //    if (txtSwitchMode.Text != "09:05:13")
        //    {
        //        string json = File.ReadAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Quotes\AllQuotes" + txtBTD.Text + ".txt").Replace("]}}", string.Empty).Replace("\"", "").Replace("T", " ").Replace("+", " ");
        //        DataSet ds = TokenChannel.ConvertToJasonQuotes(json);
        //        return ds;
        //    }
        //    else
        //    {
        //        try
        //        {
        //            StringBuilder instruments = new StringBuilder();
        //            foreach (string s in AllFNO)
        //            {
        //                instruments.Append("i=");
        //                instruments.Append("NSE:");
        //                instruments.Append(s);
        //                instruments.Append("&");

        //            }
        //            string URL_ADDRESS = string.Format("https://api.kite.trade/quote?{2}api_key={0}&access_token={1}", apiKey, accessToken, instruments.ToString());
        //            // ===== You shoudn't need to edit the lines below =====

        //            // Create the web request
        //            HttpWebRequest request = WebRequest.Create(new Uri(URL_ADDRESS)) as HttpWebRequest;

        //            // Set type to POST
        //            request.Method = "GET";
        //            request.ContentType = "application/json";

        //            // Create the data we want to send
        //            string result = string.Empty;
        //            // Get response and return it
        //            XmlDocument xmlResult = new XmlDocument();

        //            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //            {
        //                StreamReader reader = new StreamReader(response.GetResponseStream());
        //                result = reader.ReadToEnd();
        //                reader.Close();

        //                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\Quotes\quotes.txt", result.ToString());

        //            }


        //            string json = result.Replace("]}}", string.Empty).Replace("\"", "").Replace("T", " ").Replace("+", " ");
        //            DataSet ds = TokenChannel.ConvertToJasonQuotes(json);
        //            return ds;

        //        }
        //        catch (Exception ex)
        //        {

        //            try
        //            {

        //                Thread.Sleep(5000);
        //                StringBuilder instruments = new StringBuilder();
        //                foreach (string s in AllFNO)
        //                {
        //                    instruments.Append("i=");
        //                    instruments.Append("NSE:");
        //                    instruments.Append(s);
        //                    instruments.Append("&");
        //                }
        //                string URL_ADDRESS = string.Format("https://api.kite.trade/quote?{2}api_key={0}&access_token={1}", apiKey, accessToken, instruments.ToString());
        //                // ===== You shoudn't need to edit the lines below =====

        //                // Create the web request
        //                HttpWebRequest request = WebRequest.Create(new Uri(URL_ADDRESS)) as HttpWebRequest;

        //                // Set type to POST
        //                request.Method = "GET";
        //                request.ContentType = "application/json";

        //                // Create the data we want to send
        //                string result = string.Empty;
        //                // Get response and return it
        //                XmlDocument xmlResult = new XmlDocument();

        //                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //                {
        //                    StreamReader reader = new StreamReader(response.GetResponseStream());
        //                    result = reader.ReadToEnd();
        //                    reader.Close();
        //                    File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\Quotes\quotes.txt", result.ToString());
        //                }

        //                string json = result.Replace("]}}", string.Empty).Replace("\"", "").Replace("T", " ").Replace("+", " ");
        //                DataSet ds = TokenChannel.ConvertToJasonQuotes(json);
        //                return ds;
        //            }
        //            catch (Exception ex1)
        //            {
        //                LogStatus("Failed to get quotes");
        //            }
        //        }
        //    }
        //    return null;

        //}

        private DataSet WritesQuotesZerodha(string apiKey, string accessToken)
        {
            try
            {
                StringBuilder instruments = new StringBuilder();
                foreach (string s in AllFNO)
                {
                    instruments.Append("i=");
                    instruments.Append("NSE:");
                    instruments.Append(s);
                    instruments.Append("&");

                }
                string URL_ADDRESS = string.Format("https://api.kite.trade/quote?{2}api_key={0}&access_token={1}", apiKey, accessToken, instruments.ToString());
                // ===== You shoudn't need to edit the lines below =====

                // Create the web request
                HttpWebRequest request = WebRequest.Create(new Uri(URL_ADDRESS)) as HttpWebRequest;

                // Set type to POST
                request.Method = "GET";
                request.ContentType = "application/json";

                // Create the data we want to send
                string result = string.Empty;
                // Get response and return it
                XmlDocument xmlResult = new XmlDocument();

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    result = reader.ReadToEnd();
                    reader.Close();

                    File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\Quotes\AllQuotes.txt", result.ToString());

                }
            }
            catch (Exception ex)
            {
                try
                {
                    Thread.Sleep(5000);
                    StringBuilder instruments = new StringBuilder();
                    foreach (string s in AllFNO)
                    {
                        instruments.Append("i=");
                        instruments.Append("NSE:");
                        instruments.Append(s);
                        instruments.Append("&");
                    }
                    string URL_ADDRESS = string.Format("https://api.kite.trade/quote?{2}api_key={0}&access_token={1}", apiKey, accessToken, instruments.ToString());
                    // ===== You shoudn't need to edit the lines below =====

                    // Create the web request
                    HttpWebRequest request = WebRequest.Create(new Uri(URL_ADDRESS)) as HttpWebRequest;

                    // Set type to POST
                    request.Method = "GET";
                    request.ContentType = "application/json";

                    // Create the data we want to send
                    string result = string.Empty;
                    // Get response and return it
                    XmlDocument xmlResult = new XmlDocument();

                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        result = reader.ReadToEnd();
                        reader.Close();
                        File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\Quotes\AllQuotes.txt", result.ToString());
                    }
                }
                catch (Exception ex1)
                {
                    LogStatus("Failed to get quotes");
                }
            }
            return null;

        }

        public static void Read(RegistryKey root)
        {
            try
            {


                foreach (var child in root.GetSubKeyNames())
                {
                    using (var childKey = root.OpenSubKey(child))
                    {
                        Read(childKey);
                    }
                }

                foreach (var value in root.GetValueNames())
                {

                    string log = " Key :" + string.Format("{0}\\{1}", root, value) + " Type : " + root.GetValueKind(value) + "  value: " + (root.GetValue(value) ?? "").ToString();
                    //LogFile.Reference.WriteLogFile("laod last in  : ", log);
                }
            }
            catch (Exception)
            {


            }
        }

        public void GridEntryInvoke()
        {

        }








        public int ConvertDoubleToInt(object double1)
        {
            int triggerPrice = 0;
            double triggerPriceD = 0;

            triggerPriceD = Convert.ToDouble(double1) * 100;
            triggerPrice = Convert.ToInt32(triggerPriceD);


            return triggerPrice;
        }







        public static int GetCount()
        {
            return Interlocked.Increment(ref IncrementCounter);
        }



        public string DataTableToCSV(DataTable datatable, char seperator)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < datatable.Columns.Count; i++)
                {
                    sb.Append(datatable.Columns[i]);
                    if (i < datatable.Columns.Count - 1)
                        sb.Append(seperator);
                }
                sb.AppendLine();
                foreach (DataRow dr in datatable.Rows)
                {
                    for (int i = 0; i < datatable.Columns.Count; i++)
                    {
                        sb.Append(dr[i].ToString());

                        if (i < datatable.Columns.Count - 1)
                            sb.Append(seperator);
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
            }
            return string.Empty;
        }

        Dictionary<string, List<StockData>> pivotList = new Dictionary<string, List<StockData>>();

        public void LoadDailyNPivotsDataZerodha()
        {
            try
            {

                Parallel.ForEach(AllFNO, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
                {

                    var prevDayCandle = allData["day"][s].Where(a => a.TimeStamp.Date < CurrentTradingDate.Date).Last();

                    var lastMonthData = allData["day"][s].Where(a => a.TimeStamp.Month == CurrentTradingDate.AddMonths(-1).Month && a.TimeStamp.Year== CurrentTradingDate.AddMonths(-1).Year);
                    var monthlyCandle = MyCandle(lastMonthData.ToList());

                    var mPP = Math.Round((monthlyCandle.Close + monthlyCandle.High + monthlyCandle.Low) / 3, 2);
                    var mR1 = Math.Round((2 * mPP) - monthlyCandle.Low, 2);
                    var mS1 = Math.Round((2 * mPP) - monthlyCandle.High, 2);
                    var mR2 = Math.Round(mPP + (monthlyCandle.High - monthlyCandle.Low), 2);
                    var mS2 = Math.Round(mPP - (monthlyCandle.High - monthlyCandle.Low), 2);
                    var mR3 = Math.Round(mPP + 2 * (monthlyCandle.High - monthlyCandle.Low), 2);
                    var mS3 = Math.Round(mPP - 2 * (monthlyCandle.High - monthlyCandle.Low), 2);

                    double prevDayClose = allData["5minute"][s].Where(a => a.TimeStamp.Date < CurrentTradingDate.Date).Last().Close;


                    double prevDayOpen = prevDayCandle.Open;
                    double prevDayHigh = prevDayCandle.High;
                    double prevDayLow = prevDayCandle.Low;
                    double dailyPivot = (prevDayClose + prevDayHigh + prevDayLow) / 3;
                    var dPP = Math.Round(dailyPivot, 2);
                    var dR1 = Math.Round((2 * dailyPivot) - prevDayLow, 2);
                    var dS1 = Math.Round((2 * dailyPivot) - prevDayHigh, 2);
                    var dR2 = Math.Round(dailyPivot + (prevDayHigh - prevDayLow), 2);
                    var dS2 = Math.Round(dailyPivot - (prevDayHigh - prevDayLow), 2);
                    var dR3 = Math.Round(dailyPivot + 2 * (prevDayHigh - prevDayLow), 2);
                    var dS3 = Math.Round(dailyPivot - 2 * (prevDayHigh - prevDayLow), 2);
                    var dClose = prevDayClose;
                    var dHigh = prevDayHigh;
                    var dLow = prevDayLow;
                    var dOpen = prevDayOpen;

                    Parallel.ForEach(allData["5minute"][s].Where(a => a.TimeStamp.Date == CurrentTradingDate), (a) =>
                         {
                             a.dPP = dPP;
                             a.dR1 = dR1;
                             a.dS1 = dS1;
                             a.dS2 = dS2;
                             a.dS3 = dS3;
                             a.dR2 = dR2;
                             a.dR3 = dR3;
                             a.mPP = mPP;
                             a.mR1 = mR1;
                             a.mR2 = mR2;
                             a.mR3 = mR3;
                             a.mS1 = mS1;
                             a.mS2 = mS2;
                             a.mS3 = mS3;

                         });

                });

                LogStatus("Pivot points are loaded successfully.");

            }
            catch (Exception ex)
            {
                LogStatus("Pivot points are loaded successfully.exception occured");

            }
        }
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }
        }


        StockOHLC o;
        private void btnLive_Click(object sender, EventArgs e)
        {
            LiveRunMarket();
        }

        private void UploadData(string v, string InstrumentToken)
        {
            string URL_ADDRESS = string.Format("https://api.kite.trade/instruments/historical/{0}/{5}?from={1}&to={2}&api_key={3}&access_token={4}", InstrumentToken, new DateTime(2019, 05, 25).ToString("yyyy-MM-dd"), DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, v);
            // ===== You shoudn't need to edit the lines below =====

            // Create the web request
            HttpWebRequest request = WebRequest.Create(new Uri(URL_ADDRESS)) as HttpWebRequest;

            // Set type to POST
            request.Method = "GET";
            request.ContentType = "application/json";

            // Create the data we want to send
            string result = string.Empty;
            // Get response and return it
            XmlDocument xmlResult = new XmlDocument();

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                result = reader.ReadToEnd();
                reader.Close();
            }
            int p = 0;
            int.TryParse(v.Replace("minute", string.Empty), out p);
            o.InsertHistory(InstrumentToken, p, result);

        }

        static async void DoSomethingAsync(string collectionName, string databaseName)
        {
            const string connectionString = "mongodb://localhost:27017";

            // Create a MongoClient object by using the connection string
            var client = new MongoClient(connectionString);

            //Use the MongoClient to access the server
            var database = client.GetDatabase(databaseName);

            //get mongodb collection
            //var collection = database.GetCollection(collectionName);
            //await collection.InsertOneAsync(new Entity { Name = "Jack" });
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                if (BTD.ToString() != "-1")
                {
                    if (txtTam.Text == "-2" && sortedDT != null)
                    {
                        sortedDT.Rows.Clear();
                    }
                    appendStock = true;

                    btnSelect_Click(this, System.EventArgs.Empty);
                    timer1.Start();
                    timer1.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            timer1.Enabled = false;

            timer2.Start();
            timer2.Enabled = true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
            timer2.Enabled = false;

            timer3.Start();
            timer3.Enabled = true;

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            timer3.Stop();
            timer3.Enabled = false;
            //if (appendStock)
            //    return;
            if (backTestStatus)
            {
                double sum = 0; double totallost = 0, totalWon = 0;
                if (backTestStatus)// && txtTam.Text == "-2")
                {

                    for (int i = 0; i < this.rgvStocks.Rows.Count; i++)
                    {
                        if (rgvStocks.Rows[i].Cells["profitloss"].Value != string.Empty)
                        {
                            if (Convert.ToDouble(rgvStocks.Rows[i].Cells["profitloss"].Value) > 0)
                            {
                                totalWon++;
                            }
                            else if (Convert.ToDouble(rgvStocks.Rows[i].Cells["profitloss"].Value) < 0)
                            {
                                totallost++;
                            }
                            sum += Convert.ToDouble(rgvStocks.Rows[i].Cells["profitloss"].Value);
                        }
                    }
                    if (rgvStocks.RowCount > 0)
                        File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\profitloss.txt", sum.ToString() + "," + totalWon.ToString() + "," + totallost.ToString() + "," + rgvStocks.Rows[0].Cells["stock"].Value + Environment.NewLine);
                    txtBTD.Text = Convert.ToString(Convert.ToInt32(txtBTD.Text) - 1);
                    txtTam.Text = "-2";
                }
                //else if (txtTam.Text == "0")
                //{
                //    txtTam.Text = "1";
                //}
                //else if (txtTam.Text == "-2")
                //{
                //    txtTam.Text = "0";

                //}

                btnTest_Click(this, System.EventArgs.Empty);
            }
        }

        private void txtTam_TextChanged(object sender, EventArgs e)
        {
            //orders.Rows.Clear();
            if (!string.IsNullOrEmpty(txtTam.Text))
            {
                switch (Convert.ToInt32(txtTam.Text))
                {
                    case -2:
                        //  ifCalculator = new _5MCE();
                        break;
                    case 0:
                        // ifCalculator = new __15MCE();
                        break;
                    case 1:
                        // ifCalculator = new _20MCE();
                        break;
                }
            }
        }

        private void radMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void radMenuItem4_Click(object sender, EventArgs e)
        {

        }

        private void radMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void radMenuItem5_Click(object sender, EventArgs e)
        {

            RadMenuItem menuItem = (RadMenuItem)sender;

            foreach (RadMenuItem sibling in menuItem.HierarchyParent.Items)
            {
                sibling.IsChecked = false;
            }

            menuItem.IsChecked = true;

            string themeName = (string)(menuItem).Text;
            ModuleCommon.ChangeThemeName(this, themeName);
        }




        private void rgvStocks_CellClick(object sender, GridViewCellEventArgs e)
        {
            try
            {

                //List<StockData> allLevels = DeSerializeObject<List<StockData>>(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Anti\" + scrip + ".xml");
                //DateTime currentTradingDate = Convert.ToDateTime(dateMapping.Tables[0].Rows[dateMapping.Tables[0].Rows.Count - 1 - Convert.ToInt16(txtBTD.Text)]["Date"]);
                //StockData todaysLevel = null;

                //if (allLevels.Where(a => a.TradingDate == currentTradingDate).Count() > 0)
                //{
                //    todaysLevel = allLevels.Where(a => a.TradingDate == currentTradingDate).First();
                //}
                //else
                //{
                //    todaysLevel = allLevels[allLevels.Count - 1];
                //}

                //if (e.ColumnIndex == 0)
                //{
                //    string FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\15MCE\" + rgvStocks.Rows[e.RowIndex].Cells["stock"].Value.ToString() + ".csv";
                //    if (rgvStocks.Rows[e.RowIndex].Cells["stock"].Value.ToString() == "BANKNIFTY")
                //    {
                //        FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\BANKNIFTY\" + rgvStocks.Rows[e.RowIndex].Cells["stock"].Value.ToString() + ".csv";
                //    }
                //    OleDbConnection conn = new OleDbConnection
                //           ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                //             Path.GetDirectoryName(FileName) +
                //             "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

                //    conn.Open();

                //    OleDbDataAdapter adapter = new OleDbDataAdapter
                //           ("SELECT * FROM " + Path.GetFileName(FileName), conn);

                //    DataSet ds = new DataSet("Temp");
                //    adapter.Fill(ds);
                //    ds.Tables[0].Rows.RemoveAt(0);
                //    ds.Tables[0].Rows.RemoveAt(0);
                //    ds.Tables[0].Rows.RemoveAt(0);
                //    ds.Tables[0].Rows.RemoveAt(0);
                //    ds.Tables[0].Rows.RemoveAt(0);
                //    ds.Tables[0].Rows.RemoveAt(0);
                //    ds.Tables[0].Columns[0].ColumnName = "Period";
                //    ds.Tables[0].Columns[0].Caption = "Period";
                //    ds.Tables[0].AcceptChanges();
                //    ds.Tables[0].Columns.Add("Candle", typeof(string), "IIF([f2] > [f5],'G',IIF([f2] = [f5],'D','R'))");
                //    conn.Close();

                //    int startOfTheWeekIndex = 0;

                //    var rows1 = ds.Tables[0].Select("f2 = f3 and f2=f4 and f2= f5");
                //    int count = rows1.Count();
                //    foreach (var row in rows1)
                //        row.Delete();

                //    ds.Tables[0].AcceptChanges();

                //    int backTestDay = Convert.ToInt32(txtBTD.Text);
                //    int cont = 0;

                //    for (int i = ds.Tables[0].Rows.Count - 1; i > ds.Tables[0].Rows.Count - 10000; i--)
                //    {
                //        if (string.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[i][0])))
                //        {
                //            startOfTheWeekIndex = i;
                //            if (backTestDay == cont)
                //            {
                //                break;
                //            }
                //            else
                //            {
                //                cont++;
                //            }
                //        }
                //        else if (string.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[i - 1][0])))
                //        {
                //            startOfTheWeekIndex = i - 1;
                //            if (backTestDay == cont)
                //            {

                //                break;
                //            }
                //            else
                //            {
                //                cont++;
                //                i = i - 1;
                //            }

                //        }
                //        else if (Math.Abs(Convert.ToInt32(ds.Tables[0].Rows[i]["period"]) - Convert.ToInt32(ds.Tables[0].Rows[i - 1]["period"])) > 50)
                //        {
                //            startOfTheWeekIndex = i;
                //            if (backTestDay == cont)
                //            {
                //                break;
                //            }
                //            else
                //            {
                //                cont++;
                //            }
                //        }
                //    }

                //    int testAtMinute = Convert.ToInt32(txtTam.Text);

                //    ds.Tables[0].Rows[startOfTheWeekIndex - 1 + testAtMinute]["period"] = 99999;
                //    ds.Tables[0].Rows[startOfTheWeekIndex + testAtMinute]["period"] = 100001;
                //    ds.Tables[0].Rows[startOfTheWeekIndex + 1 + testAtMinute]["period"] = 100002;
                //    ds.Tables[0].Rows[startOfTheWeekIndex + 2 + testAtMinute]["period"] = 100003;


                //    DataTable dtChart = new DataTable();
                //    dtChart.Columns.Add("index", typeof(double));
                //    dtChart.Columns.Add("close", typeof(double));
                //    dtChart.Columns.Add("high", typeof(double));
                //    dtChart.Columns.Add("low", typeof(double));
                //    dtChart.Columns.Add("open", typeof(double));
                //    for (int i = startOfTheWeekIndex - 225; i < startOfTheWeekIndex + 75; i++)
                //    {
                //        dtChart.Rows.Add(i - startOfTheWeekIndex - 225, ds.Tables[0].Rows[i]["f2"], ds.Tables[0].Rows[i]["f3"], ds.Tables[0].Rows[i]["f4"], ds.Tables[0].Rows[i]["f5"]);
                //    }


                //    DataSet dsPivots = new DataSet();
                //    string pFileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\PSR\" + rgvStocks.Rows[e.RowIndex].Cells["stock"].Value.ToString() + "pivot.csv";
                //    OleDbConnection pconn = new OleDbConnection
                //           ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                //             Path.GetDirectoryName(pFileName) +
                //             "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

                //    pconn.Open();

                //    OleDbDataAdapter padapter = new OleDbDataAdapter
                //           ("SELECT * FROM " + Path.GetFileName(pFileName), pconn);


                //    padapter.Fill(dsPivots);
                //    int index = Convert.ToInt32(txtBTD.Text);


                //    NSA.TChart c = new NSA.TChart(dtChart, dsPivots, index);
                //    c.Show();
                //}

            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
            }
        }

        Dictionary<string, Quote> latestQuoteforStocks = new Dictionary<string, Quote>();

        public int OriginalTradeCount
        {
            get
            {
                return orders.AsEnumerable().Count();
            }
            set
            {
            }
        }

        public double TodaysRisk
        {
            get
            {
                return HighestProfitToday + (-MaxRisk * OriginalTradeCount * .8);
            }
            set
            {
            }
        }

        public double TodaysTarget
        {
            get
            {
                return MaxRisk * 5;
            }
            set
            {
            }
        }


        Dictionary<string, double> myLIst = new Dictionary<string, double>();

        List<VolumeFilter> vList = new List<VolumeFilter>();
        private void goLiveTimer_Tick(object sender, EventArgs e)
        {
            //goLiveTimer.Stop();
            RefreshData();
            radLabelElement1.Text = $"Last 5 minute tick :{ TokenChannel.GetTimeStamp(Convert.ToInt32(txtTam.Text), CurrentTradingDate).ToString()}";
            txtTam.Text = Convert.ToString(Convert.ToInt32(txtTam.Text) + 1);


        }

        private static System.TimeZoneInfo INDIAN_ZONE = System.TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        bool tryLogin = true;
        private void tmrClock_Tick(object sender, EventArgs e)
        {
            if (OrderPlacingMode == Constants.VARIETY_CO && DateTime.Now.Second % 12 == 0)
            {
                foreach (Kite kiteUser in kUsers)
                {
                    List<KiteConnect.Order> allOrders = kiteUser.GetOrders();
                    var positions = kiteUser.GetPositions().Day;


                    List<KiteConnect.Order> pendingOrders = allOrders.Where(a => a.Status == "TRIGGER PENDING").ToList();
                    List<KiteConnect.Order> completedOrders = allOrders.Where(a => a.Status == "COMPLETE").ToList();
                    if (DateTime.Now.Hour == 15 && DateTime.Now.Minute >= 15)
                    {
                        foreach (var oc in pendingOrders)
                        {
                            kiteUser.CancelOrder(oc.OrderId, Constants.VARIETY_CO, oc.ParentOrderId);
                        }
                    }
                    var ordersFromGrid = orders.AsEnumerable().ToList();

                    foreach (var stock in completedOrders.GroupBy(b => b.Tradingsymbol))
                    {

                        int stoplossOrdersCount = pendingOrders.Where(a => a.Tradingsymbol == stock.Key).Count();
                        if (stoplossOrdersCount >= 2)
                        {

                            double entryPrice = 0;
                            double stoplossPrice = 0;


                            if (stoplossOrdersCount == 3 && positions.Where(a => a.TradingSymbol == stock.Key && a.PNL >= (decimal)MaxRisk + 600).Count() > 0)
                            {
                                var orderTobeCancelled = pendingOrders.Where(g => g.Tradingsymbol == stock.Key).First();
                                var orderTobeUpdated = pendingOrders.Where(g => g.Tradingsymbol == stock.Key && g.OrderId != orderTobeCancelled.OrderId);
                                kiteUser.CancelOrder(orderTobeCancelled.OrderId, Constants.VARIETY_CO, orderTobeCancelled.ParentOrderId);
                                foreach (var o in orderTobeUpdated)
                                {
                                    kiteUser.ModifyOrder(o.OrderId, o.ParentOrderId, o.Exchange, o.Tradingsymbol, o.TransactionType, Convert.ToString(o.Quantity), o.Price, o.Product, o.OrderType, o.Validity, o.DisclosedQuantity, Math.Round(positions.Where(a => a.TradingSymbol == stock.Key).First().AveragePrice, 1), o.Variety);
                                }
                            }
                            if (stoplossOrdersCount == 2 && positions.Where(a => a.TradingSymbol == stock.Key && a.PNL >= (decimal)((MaxRisk / 3) + (MaxRisk / 3) * 4) + 200).Count() > 0)
                            {
                                var orderTobeCancelled = pendingOrders.Where(g => g.Tradingsymbol == stock.Key).First();
                                kiteUser.CancelOrder(orderTobeCancelled.OrderId, Constants.VARIETY_CO, orderTobeCancelled.ParentOrderId);
                                var orderTobeUpdated = pendingOrders.Where(g => g.Tradingsymbol == stock.Key && g.OrderId != orderTobeCancelled.OrderId);
                                foreach (var o in orderTobeUpdated)
                                {
                                    kiteUser.ModifyOrder(o.OrderId, o.ParentOrderId, o.Exchange, o.Tradingsymbol, o.TransactionType, Convert.ToString(o.Quantity), o.Price, o.Product, o.OrderType, o.Validity, o.DisclosedQuantity, Math.Round(o.TransactionType == "SELL" ? positions.Where(a => a.TradingSymbol == stock.Key).First().AveragePrice + (decimal)((MaxRisk / 3) / o.Quantity) : positions.Where(a => a.TradingSymbol == stock.Key).First().AveragePrice - (decimal)((MaxRisk / 3) / o.Quantity), 1), o.Variety);
                                }
                            }
                        }
                    }
                }
            }
            //if (DONT_DELETE && tryLogin)
            //{
            //    tryLogin = false;
            //    panel1.Visible = false;
            //    txtSwitchMode.Enabled = true;
            //    LoadInstruments();
            //}
            try
            {
                if (tryLogin)
                {
                    if (!isLogin && !rdoLive.IsChecked)
                    {
                        rdoLive.IsChecked = true;
                        Login();

                    }
                    if (isLogin)
                    {
                        if (!isPivotLoaded)
                        {
                            LoadInstruments();
                            txtSwitchMode.Enabled = true;

                            //LoadDailyNPivotsDataZerodha();
                        }
                        else if (isPivotLoaded && instrumentNamebyKey.Count == 0)
                        {
                            LoadInstruments();
                            txtSwitchMode.Enabled = true;
                        }
                        /*
                        DateTime x = System.TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE); ;
                        if (DONT_DELETE == false && DateTime.Today == CurrentTradingDate && x.Second >= 6 && x.Minute % 5 == 0 && !goLiveTimer.Enabled && txtSwitchMode.Text != string.Empty)
                        {
                            orders.Clear();
                            DateTime indianTime = System.TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                            if (indianTime > Convert.ToDateTime(CurrentTradingDate.Date.ToShortDateString() + " 09:15:00"))
                            {
                                Restore();

                                txtTam.Text = Convert.ToString(TokenChannel.GetMinuteNumber(CurrentTradingDate) - 1);
                                tmrQuote.Start();
                                tmrQuote.Enabled = true;
                                LogStatus("Quotes Ticker is started.");
                                backTestStatus = false;
                                radLabelElement1.Text = "Live Mode Started";
                                backTestStatus = false;
                                if (string.IsNullOrEmpty(txtSwitchMode.Text))
                                {
                                    goLiveTimer.Interval = 60000;
                                }
                                else
                                {
                                    goLiveTimer.Interval = 60000;
                                }
                                goLiveTimer.Start();
                                goLiveTimer.Enabled = true;
                                radLabelElement1.Text = "Live Mode Started";
                                //CloseOrder();
                                LogStatus("Market is stared now...");
                                goLiveTimer_Tick(this, System.EventArgs.Empty);
                                txtTam.Text = TokenChannel.GetMinuteNumber(CurrentTradingDate).ToString();
                            }
                        }*/

                    }

                    int seconds = DateTime.Now.Second;
                    radLabel4.Text = System.TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE).ToString("hh:mm:ss");
                    if (radLabel4.Text == txtSwitchMode.Text || string.IsNullOrEmpty(txtSwitchMode.Text))
                    {
                        rdoLive.IsChecked = true;
                    }
                    if (doLogin)
                    {
                        //checkLogin();
                    }
                    if (!backLiveTest)
                    {
                        if (radLabel4.Text == txtPivotLoad.Text)
                        {
                            //LoadDailyNPivotsData();
                        }
                        else if (radLabel4.Text == txtQuoteStart.Text)
                        {
                            tmrQuote.Start();
                            tmrQuote.Enabled = true;
                            LogStatus("Quotes Ticker is started.");
                            backTestStatus = false;
                            radLabelElement1.Text = "Live Mode Started";
                            //CloseOrder();
                        }

                        else if (radLabel4.Text == txtMarketStart.Text)
                        {
                            /*
                        backTestStatus = false;
                        if (string.IsNullOrEmpty(txtSwitchMode.Text))
                        {
                            goLiveTimer.Interval = 60000;
                        }
                        else
                        {
                            goLiveTimer.Interval = 1800000;
                        }
                        goLiveTimer.Start();
                        goLiveTimer.Enabled = true;
                        radLabelElement1.Text = "Live Mode Started";
                        //CloseOrder();
                        LogStatus("Market is stared now...");
                        RefreshData();
                            */
                        }


                    }
                }
                else
                {
                    panel1.Visible = false;
                    txtSwitchMode.Enabled = true;

                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
            }
        }

        public void TestRunMarketWithoutLogin()
        {
            backTestStatus = false;
            goLiveTimer.Interval = 15000;
            DONT_DELETE = true;
            goLiveTimer.Start();
            goLiveTimer.Enabled = true;
            radLabelElement1.Text = "Test Mode Started";
            //CloseOrder();
            LogStatus("Test mode started  now...");
        }
        public void TestRunMarket()
        {
            if (DONT_DELETE)
            {
                LoadInstruments();
            }
            //LoadDataWeekly();
            //LoadDataDaily();
            LoadAllDateTillDate();

            LoadDailyNPivotsDataZerodha();
            backTestStatus = false;
            goLiveTimer.Interval = 5000;
            DONT_DELETE = true;
            goLiveTimer.Start();
            goLiveTimer.Enabled = true;
            radLabelElement1.Text = "Test Mode Started";
            //CloseOrder();
            LogStatus("Test mode started  now...");
            //RefreshData();
        }

        public void LiveRunMarket()
        {
            backTestStatus = false;

            //txtTam.Text = TokenChannel.GetMinuteNumber(CurrentTradingDate).ToString();
            DONT_DELETE = false;
            goLiveTimer.Interval = 300000;

            goLiveTimer.Start();
            goLiveTimer.Enabled = true;
            radLabelElement1.Text = "Live Mode Started";
            //CloseOrder();
            LogStatus("Market is stared now...");
            RefreshData();
        }




        public void LoadInstruments()
        {
            foreach (Kite kiteUser in kUsers)
            {
                if (!File.Exists(@"C:\Jai Sri Thakur Ji\Nifty Analysis\ZERODHA\InstrumentToken\instrumenttoken.csv"))
                {

                    LoadInstruments(APIKEY, ACCESSTOKEN);
                    break;
                }
            }

            if (File.Exists(@"C:\Jai Sri Thakur Ji\Nifty Analysis\ZERODHA\InstrumentToken\instrumenttoken.csv"))
            {
                OleDbConnection dconnn = new OleDbConnection
                       ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                         Path.GetDirectoryName(@"C:\Jai Sri Thakur Ji\Nifty Analysis\ZERODHA\InstrumentToken\instrumenttoken.csv") +
                         "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

                dconnn.Open();

                OleDbDataAdapter dadapter1 = new OleDbDataAdapter
                       ("SELECT * FROM " + Path.GetFileName(@"C:\Jai Sri Thakur Ji\Nifty Analysis\ZERODHA\InstrumentToken\instrumenttoken.csv") + " where instrument_type='EQ' and exchange='NSE'", dconnn);


                dadapter1.Fill(instrToken);

                dconnn.Close();

                foreach (DataRow dr in instrToken.Tables[0].Rows)
                {
                    if (!instrumentNamebyKey.ContainsKey(Convert.ToDouble(dr["instrument_token"])))
                    {
                        if (dr["exchange"].ToString() == "NSE" && dr["instrument_type"].ToString() == "EQ")
                            instrumentNamebyKey.Add(Convert.ToDouble(dr["instrument_token"]), Convert.ToString(dr["tradingsymbol"]));
                    }
                }
            }
            isPivotLoaded = true;
        }

        Dictionary<double, string> instrumentNamebyKey = new Dictionary<double, string>();


        public bool isPivotLoaded
        {
            get;
            set;
        }
        private void radButton2_Click(object sender, EventArgs e)
        {

            try

            {
                Login();
                int i = 0;
                XDocument doc = XDocument.Load(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Users\users.xml");
                var Users = doc.Descendants("user");

                foreach (Kite kiteUser in kUsers)
                {
                    string apiSecret = mySecret;
                    string requestToken = myRequestToken;
                    User Kuser = kiteUser.GenerateSession(requestToken, apiSecret);
                    ACCESSTOKEN = Kuser.AccessToken;
                    string userAccessToken = Kuser.AccessToken;
                    string userPublicToken = Kuser.PublicToken;
                    kiteUser.SetAccessToken(userAccessToken);
                    kiteUser.SetSessionExpiryHook(() => isLogin = false);
                    isLogin = true;
                    i++;
                }
                LoadInstruments();
                txtSwitchMode.Enabled = true;

            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message); ;
            }

        }

        public void LogStatus(string logText)
        {
            try
            {
                txtLog.Text += logText + Environment.NewLine;
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
            }
        }

        List<KiteConnect.Order> stoplossOrders = null;
        public void UpdateOrders(int period)
        {
            try
            {
                List<string> scripList = new List<string>();
                for (int counter = 0; counter < orders.Rows.Count; counter++)
                {
                    if (true || Convert.ToDateTime(TokenChannel.GetTimeStamp(Convert.ToInt16(txtTam.Text), CurrentTradingDate)) > Convert.ToDateTime(orders.Rows[counter]["candle"]))
                    {
                        if (string.IsNullOrEmpty(orders.Rows[counter]["Aexit"].ToString().Replace("0", string.Empty)) && orders.Rows[counter]["strategy"].ToString().Contains("LEG"))
                        {
                            scripList.Add("NSE:" + orders.Rows[counter]["scrip"].ToString());


                            double pnl = 0;
                            LTPValues ltpObj = null;


                            ltpObj = CalculateExit(orders.Rows[counter]["scrip"].ToString(), orders.Rows[counter]["direction"].ToString(), Convert.ToInt16(txtTam.Text), BTD, Convert.ToInt16(txtTam.Text) + 2, Convert.ToDouble(orders.Rows[counter]["high"]), Convert.ToDouble(orders.Rows[counter]["low"]), out pnl, Convert.ToDouble(orders.Rows[counter]["entry"]), Convert.ToInt32(orders.Rows[counter]["quantity"]), Convert.ToDouble(orders.Rows[counter]["stoploss"]), Convert.ToDateTime(orders.Rows[counter]["candle"]), period, Convert.ToDouble(orders.Rows[counter]["Aentry"].ToString()), orders.Rows[counter]["strategy"].ToString());

                            if (!ltpObj.IsExit && pnl >= MaxRisk / 3)
                            {
                                foreach (DataRow dr in orders.Select("Scrip='" + orders.Rows[counter]["scrip"].ToString() + "'"))
                                {
                                    if (Convert.ToInt16(dr["AExit"]) == 0)
                                    {
                                        if (dr["strategy"].ToString().Contains("LEG 2"))
                                        {


                                            BookPartialProfit(100, dr, ltpObj.LtpClose, ltpObj.TimeStamp, MaxRisk / 3);
                                            //orders.Select("scrip = '" + orders.Rows[counter]["scrip"] + "' and (strategy like '%LEG 1%')")[0]["stoploss"] = dr["entry"];
                                            //orders.Select("scrip = '" + orders.Rows[counter]["scrip"] + "' and (strategy like '%LEG 3%')")[0]["stoploss"] = dr["entry"];
                                            ltpObj.trailingStopLoss = Convert.ToDouble(dr["entry"]);
                                            dr["stoploss"] = Convert.ToDouble(dr["entry"]);


                                        }
                                        else if (dr["strategy"].ToString().Contains("Strength") && pnl >= MaxRisk)
                                        {


                                            //  BookPartialProfit(100, dr, ltpObj.LtpClose, ltpObj.TimeStamp, MaxRisk);
                                            //IncrementDecrement(orders.Rows[counter]["strategy"].ToString(), 1);

                                        }

                                    }
                                }

                            }

                            if (!ltpObj.IsExit && pnl >= (MaxRisk * 2) / 3)
                            {
                                foreach (DataRow dr in orders.Select("Scrip='" + orders.Rows[counter]["scrip"].ToString() + "'"))
                                {
                                    if (dr["strategy"].ToString().Contains("LEG 1"))
                                    {
                                        if (Convert.ToInt16(dr["AExit"]) == 0)
                                        {
                                            BookPartialProfit(100, dr, ltpObj.LtpClose, ltpObj.TimeStamp, (MaxRisk * 2) / 3);
                                        }
                                    }
                                }

                            }
                            else if (ltpObj.IsExit)
                            {
                                BookPartialProfit(100, orders.Rows[counter], ltpObj.trailingStopLoss, ltpObj.TimeStamp, 0);
                                //if (!orders.Rows[counter]["strategy"].ToString().Contains("Strength"))
                                //IncrementDecrement(orders.Rows[counter]["strategy"].ToString(), 1);

                                if (orders.Rows[counter]["strategy"].ToString().Contains("LEG 1"))
                                {
                                    foreach (DataRow dr in orders.Select("scrip = '" + orders.Rows[counter]["scrip"] + "' and (strategy like '%LEG 2%' or strategy like '%LEG 3%')"))
                                    {
                                        if (Convert.ToInt16(dr["AExit"]) == 0)
                                        {
                                            BookPartialProfit(100, dr, ltpObj.trailingStopLoss, ltpObj.TimeStamp, 0);
                                        }
                                    }
                                }

                            }
                            if (!ltpObj.IsExit)
                            {


                            }
                            else
                            {
                                orders.Rows[counter]["exlevel"] = ltpObj.ExitLevels;
                            }

                            orders.Rows[counter]["ltp"] = ltpObj.LtpClose;
                            //orders.Rows[counter]["target"] = ltpObj.PNL;
                        }

                    }


                }

                List<string> s = new List<string>();
                s = orders.AsEnumerable().Select(a => a.Field<string>("scrip")).Distinct().ToList();
                foreach (string scrip in s)
                {
                    string d = orders.AsEnumerable().Where(b => b.Field<string>("scrip") == scrip).First().Field<string>("Direction");
                    double AEntry = orders.AsEnumerable().Where(b => b.Field<string>("scrip") == scrip).Max(b => b.Field<double>("Aentry"));
                    double sl = 0;
                    if (d == "BM")
                    {
                        sl = orders.AsEnumerable().Where(b => b.Field<string>("scrip") == scrip).Min(a => a.Field<double>("stoploss"));
                    }
                    else
                    {
                        sl = orders.AsEnumerable().Where(b => b.Field<string>("scrip") == scrip).Max(a => a.Field<double>("stoploss"));
                    }
                    double ltpClose = orders.AsEnumerable().Where(b => b.Field<double>("Aexit") == 0).First().Field<double>("ltp");
                    UpdateStopLossOrder(scrip, d, sl, string.Empty, AEntry, ltpClose);
                }

            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
            }
        }

        public void IncrementDecrement(string strategy, int value)
        {
            if (strategy.Contains("Continuation"))
            {
                SMAQuanitty = SMAQuanitty + value;
            }
            else if (strategy.Contains("Failure"))
            {
                PSAAQuantity = PSAAQuantity + value;
            }
            else if (strategy.Contains("Strength"))
            {
                SuperTrendQuanaity = SuperTrendQuanaity + value;
            }
            else if (strategy.Contains("AllCross"))
            {
                _30MinQuantity = _30MinQuantity + value;
            }
            else if (strategy.Contains("Gap"))
            {
                _60MinQuantity = _60MinQuantity + value;
            }
        }

        public void BookPartialProfit(double per, DataRow order, double closingPrice, DateTime closingTime, double pnl)
        {
            //UpdateStopLossOrder(order["scrip"].ToString(), order["direction"].ToString(), Convert.ToDouble(order["ltp"]));
            CloseOrder(order["scrip"].ToString(), Convert.ToInt64(order["quantity"]), order["direction"].ToString());
            if (pnl == 0)
            {
                pnl = order["direction"].ToString() == "BM" ? (closingPrice - Convert.ToDouble(order["entry"])) * Convert.ToDouble(order["quantity"]) : (Convert.ToDouble(order["entry"]) - closingPrice) * Convert.ToDouble(order["quantity"]);
            }
            if (order["BP"].ToString().Trim() == "")
                order["BP"] = 0;
            order["BP"] = Math.Round(Convert.ToDouble(order["BP"]) + (pnl * per / 100));
            order["quantity"] = Convert.ToInt32(order["quantity"]) - (Convert.ToInt32(order["quantity"]) * per / 100);
            order["ec"] = Convert.ToInt32(txtTam.Text) + 2;
            order["Aexit"] = closingPrice;
            order["target"] = "0";
            order["ClosingTime"] = closingTime.ToShortTimeString();
            LogStatus("Order closed on hitting target : " + order["scrip"].ToString());



        }

        public void CloseOrder(string scrip, Int64 lotSize, string direction)
        {
            try
            {
                if (txtSwitchMode.Text != string.Empty)
                {
                    //foreach (Kite kiteUser in kUsers)
                    //{
                    //    List<KiteConnect.Order> allOrders = kiteUser.GetOrders();
                    //    //filter all the stoploss orders            
                    //    stoplossOrders = allOrders.Where(a => a.Tradingsymbol == scrip && a.Status == "TRIGGER PENDING").ToList();
                    //    foreach (KiteConnect.Order o in stoplossOrders)
                    //    {
                    //        kiteUser.CancelOrder(o.OrderId, Constants.VARIETY_CO, o.ParentOrderId);
                    //        LogStatus("Close - Order " + o.Tradingsymbol + " Quantity " + o.Quantity + " Order Id : " + o.OrderId);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + Environment.NewLine);
            }

        }

        public void CloseAllOrder()
        {
            try
            {
                if (txtSwitchMode.Text != string.Empty)
                {
                    foreach (Kite kiteUser in kUsers)
                    {
                        List<KiteConnect.Order> allOrders = kiteUser.GetOrders();
                        //filter all the stoploss orders            
                        stoplossOrders = allOrders.Where(a => a.Status == "TRIGGER PENDING").ToList();
                        foreach (KiteConnect.Order o in stoplossOrders)
                        {
                            kiteUser.CancelOrder(o.OrderId, Constants.VARIETY_CO, o.ParentOrderId);
                            LogStatus("Close - Order " + o.Tradingsymbol + " Quantity " + o.Quantity + " Order Id : " + o.OrderId);
                        }
                    }
                }

                foreach (DataRow dr in orders.Rows)
                {
                    if (Convert.ToDouble(dr["Aexit"]) == 0)
                    {
                        if (dr["BP"].ToString().Trim() == "")
                            dr["BP"] = 0;
                        dr["BP"] = Math.Round(dr["direction"].ToString() == "BM" ? (Convert.ToInt32(dr["quantity"]) * (Convert.ToDouble(dr["ltp"]) - Convert.ToDouble(dr["entry"]))) : (Convert.ToInt32(dr["quantity"]) * (Convert.ToDouble(dr["entry"]) - Convert.ToDouble(dr["ltp"]))), 2);
                        dr["quantity"] = Convert.ToInt32(dr["quantity"]) - (Convert.ToInt32(dr["quantity"]) * 100 / 100);
                        dr["ec"] = Convert.ToInt32(txtTam.Text) + 2;
                        dr["Aexit"] = dr["ltp"].ToString();
                        dr["target"] = "0";
                        LogStatus("Order closed on revresal");
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + Environment.NewLine);
            }
        }



        public void UpdateStopLossOrder(string scrip, string direction, double trailingStoploss, string exitlevels, double highestPNL, double close)
        {
            try
            {
                trailingStoploss = direction == "BM" ? Math.Round(trailingStoploss, 1) : Math.Round(trailingStoploss, 1);

                foreach (DataRow dr in orders.Select("scrip='" + scrip + "' and direction='" + direction + "'"))
                {

                    if (string.IsNullOrEmpty(dr["Aexit"].ToString().Replace("0", string.Empty)))
                    {
                        dr["ltp"] = close;
                        dr["Aentry"] = highestPNL;
                        dr["exlevel"] = exitlevels;
                        dr["stoploss"] = trailingStoploss;
                        dr["target"] = direction == "BM" ? ((close - Convert.ToDouble(dr["entry"])) * Convert.ToDouble(dr["quantity"])) : ((Convert.ToDouble(dr["entry"]) - close) * Convert.ToDouble(dr["quantity"]));
                    }

                }
                if (txtSwitchMode.Text != string.Empty)
                {
                    //foreach (Kite kiteUser in kUsers)
                    //{
                    //    List<KiteConnect.Order> allOrders = kiteUser.GetOrders();
                    //    //filter all the stoploss orders            
                    //    stoplossOrders = allOrders.Where(a => a.Tradingsymbol == scrip && a.ParentOrderId != null && a.Status == "TRIGGER PENDING" && a.TransactionType == (direction == "BM" ? "SELL" : "BUY")).ToList();
                    //    foreach (KiteConnect.Order o in stoplossOrders)
                    //    {
                    //        kiteUser.ModifyOrder(o.OrderId, o.ParentOrderId, o.Exchange, o.Tradingsymbol, o.TransactionType, "0", 0, o.Product, o.OrderType, o.Validity, o.DisclosedQuantity, (decimal)Math.Round(trailingStoploss, 1), o.Variety);
                    //        LogStatus("Modify - Order " + o.Tradingsymbol + " stop loss" + trailingStoploss);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + Environment.NewLine);
            }
        }


        public LTPValues CalculateExit(string scrip, string direction, int testAtMinute, int backTestDay, int goTimeCount, double bHigh, double bLow, out double pnl, double avgPrice, int quantity, double stopLossValue, DateTime entryCandle, int period, double exitingPNL, string strat)
        {
            try
            {

                string comment = "";

                string FileName = string.Empty;


                List<Candle> data = allData[period.ToString() + "minute"][scrip];

                int exitCheck = 0;
                bool wasInProfit = false;
                double profit = 0, stoploss = 0;
                double directionBreakout = direction == "BM" ? bHigh : bLow;
                double trailingStoploss = stopLossValue;

                double target = risk;
                double swingLow = 999999;
                double swingValue = direction == "BM" ? 99999 : 0;

                double lowest = 99999;
                double highest = 0;


                Candle drCurrent = null;
                Candle drCurrentPrev = null;
                switch (period)
                {

                    case 60:
                        drCurrent = data.Where(a => a.TimeStamp == TokenChannel.GetTimeStamp60(testAtMinute, CurrentTradingDate)).First();

                        break;
                    case 15:
                        drCurrent = data.Where(a => a.TimeStamp == TokenChannel.GetTimeStamp15(testAtMinute, CurrentTradingDate)).First();
                        break;
                    case 30:
                        drCurrent = data.Where(a => a.TimeStamp == TokenChannel.GetTimeStamp30(testAtMinute, CurrentTradingDate)).First();
                        break;
                    case 5:
                        drCurrent = data.Where(a => a.TimeStamp == TokenChannel.GetTimeStamp(testAtMinute, CurrentTradingDate)).First();
                        break;
                }
                //drCurrent.Low = Math.Min(drCurrent.Low, drCurrentPrev.Low);
                //drCurrent.High = Math.Max(drCurrent.High, drCurrentPrev.High);
                LTPValues ltpObj = new LTPValues();
                ltpObj.TimeStamp = TokenChannel.GetTimeStamp60(testAtMinute, CurrentTradingDate);
                pnl = direction == "BM" ? (drCurrent.High - avgPrice) * quantity : (avgPrice - drCurrent.Low) * quantity;

                double pnl1 = direction == "BM" ? (drCurrent.High - avgPrice) * quantity : (avgPrice - drCurrent.Low) * quantity;
                double currentValue = 0;
                if (pnl1 > exitingPNL)
                {
                    currentValue = pnl1;
                }
                else
                {
                    currentValue = exitingPNL;
                }

                #region "PRIORITY 1 : TRAILING STOP LOSS HIT CHECK"
                if (direction == "BM")
                {


                    if (drCurrent.Low < trailingStoploss)
                    {
                        ltpObj.IsExit = true;
                        comment = "Exited - Trailing Stoploss hit - " + trailingStoploss.ToString();
                        pnl = direction == "BM" ? (trailingStoploss - avgPrice) * quantity : (avgPrice - trailingStoploss) * quantity;
                        ltpObj.LtpClose = trailingStoploss;
                        ltpObj.LtpHigh = drCurrent.High;
                        ltpObj.LtpLow = drCurrent.Low;
                        ltpObj.LtpOpen = drCurrent.Open;
                        ltpObj.trailingStopLoss = trailingStoploss;
                        ltpObj.PNL = pnl;
                        ltpObj.ExitCandle = testAtMinute + 3;
                        ltpObj.ExitLevels = comment;
                        ltpObj.HighestPNL = currentValue;
                        return ltpObj;
                    }


                }
                else if (direction == "SM")
                {
                    if (drCurrent.High > trailingStoploss)
                    {
                        ltpObj.IsExit = true;
                        comment = "Exited - Trailing Stoploss hit - " + trailingStoploss.ToString();
                        pnl = direction == "BM" ? (trailingStoploss - avgPrice) * quantity : (avgPrice - trailingStoploss) * quantity;
                        ltpObj.LtpClose = trailingStoploss;
                        ltpObj.LtpHigh = drCurrent.High;
                        ltpObj.LtpLow = drCurrent.Low;
                        ltpObj.LtpOpen = drCurrent.Open;
                        ltpObj.trailingStopLoss = trailingStoploss;
                        ltpObj.PNL = pnl;
                        ltpObj.ExitCandle = testAtMinute + 3;
                        ltpObj.ExitLevels = comment;
                        ltpObj.HighestPNL = currentValue;
                        return ltpObj;
                    }

                }


                #endregion

                ltpObj.LtpClose = drCurrent.Close;
                ltpObj.LtpHigh = drCurrent.High;
                ltpObj.LtpLow = drCurrent.Low;
                ltpObj.LtpOpen = drCurrent.Open;
                ltpObj.trailingStopLoss = trailingStoploss;
                ltpObj.PNL = pnl;
                ltpObj.HighestPNL = currentValue;
                //if (pnl < MaxRisk / 2 && pnl > 200 && pnl1 > exitingPNL)
                //{
                //    ltpObj.trailingStopLoss = direction == "BM" ? ltpObj.LtpHigh - (MaxRisk / quantity) : ltpObj.LtpLow + (MaxRisk / quantity);
                //}
                //else if (pnl >= MaxRisk / 2 && pnl > 200 && pnl1 > exitingPNL)
                //{
                //    ltpObj.trailingStopLoss = direction == "BM" ? ltpObj.LtpHigh - ((MaxRisk / 3) / quantity) : ltpObj.LtpLow + ((MaxRisk / 3) / quantity);
                //}
                return ltpObj;
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + ex.StackTrace + Environment.NewLine);
            }
            pnl = 0;
            return null;
        }

        public LTPValues CalculateExitFast(string scrip, string direction, int testAtMinute, int backTestDay, int goTimeCount, double bHigh, double bLow, out double pnl, double avgPrice, int quantity, double stopLossValue, DateTime entryCandle, int period, double exitingPNL, string strat)
        {
            try
            {

                string comment = "";

                string FileName = string.Empty;


                List<Candle> data = allData[period.ToString() + "minute"][scrip];

                int exitCheck = 0;
                bool wasInProfit = false;
                double profit = 0, stoploss = 0;
                double directionBreakout = direction == "BM" ? bHigh : bLow;
                double trailingStoploss = stopLossValue;

                double target = risk;
                double swingLow = 999999;
                double swingValue = direction == "BM" ? 99999 : 0;

                double lowest = 99999;
                double highest = 0;


                Candle drCurrent = null;
                Candle drCurrentPrev = null;
                switch (period)
                {

                    case 60:
                        drCurrent = data.Where(a => a.TimeStamp == TokenChannel.GetTimeStamp60(testAtMinute, CurrentTradingDate)).First();
                        break;
                    case 15:
                        drCurrent = data.Where(a => a.TimeStamp == TokenChannel.GetTimeStamp15(testAtMinute, CurrentTradingDate)).First();
                        break;
                    case 30:
                        drCurrent = data.Where(a => a.TimeStamp == TokenChannel.GetTimeStamp30(testAtMinute, CurrentTradingDate)).First();
                        break;
                    case 5:
                        drCurrent = data.Where(a => a.TimeStamp == TokenChannel.GetTimeStamp(testAtMinute, CurrentTradingDate)).First();
                        break;
                }
                //drCurrent.Low = Math.Min(drCurrent.Low, drCurrentPrev.Low);
                //drCurrent.High = Math.Max(drCurrent.High, drCurrentPrev.High);
                LTPValues ltpObj = new LTPValues();
                ltpObj.TimeStamp = TokenChannel.GetTimeStamp(testAtMinute, CurrentTradingDate);
                pnl = direction == "BM" ? (drCurrent.High - avgPrice) * quantity : (avgPrice - drCurrent.Low) * quantity;

                double pnl1 = direction == "BM" ? (drCurrent.High - avgPrice) * quantity : (avgPrice - drCurrent.Low) * quantity;
                DateTime entryTime = entryCandle;
                DateTime exitTime = TokenChannel.GetTimeStamp(71, CurrentTradingDate);
                double bookProfit = direction == "BM" ? avgPrice + (MaxRisk * 2) / quantity : avgPrice - (MaxRisk * 2) / quantity;

                List<Candle> checkData = data.Where(a => a.TimeStamp < exitTime && a.TimeStamp > entryTime).ToList();
                DateTime bookProfitHit = DateTime.MaxValue;
                DateTime stoplossHit = DateTime.MaxValue;
                var targetHit = checkData.Where(a => a.High >= bookProfit && a.Low <= bookProfit);
                var stopHit = checkData.Where(a => a.High >= trailingStoploss && a.Low <= trailingStoploss);

                if (targetHit.Count() > 0)
                {
                    bookProfitHit = targetHit.First().TimeStamp;
                }
                if (stopHit.Count() > 0)
                {
                    stoplossHit = stopHit.FirstOrDefault().TimeStamp;
                }

                if (bookProfitHit < stoplossHit)
                {
                    ltpObj.IsExit = true;
                    trailingStoploss = checkData.Last().Close;
                    comment = "Exited - Target Hit";
                    pnl = direction == "BM" ? (bookProfit - avgPrice) * quantity : (avgPrice - bookProfit) * quantity;
                    ltpObj.LtpClose = bookProfit;
                    ltpObj.LtpHigh = drCurrent.High;
                    ltpObj.LtpLow = drCurrent.Low;
                    ltpObj.LtpOpen = drCurrent.Open;
                    ltpObj.trailingStopLoss = bookProfit;
                    ltpObj.PNL = pnl;
                    ltpObj.ExitCandle = TokenChannel.GetMinuteNumber(CurrentTradingDate, bookProfitHit);
                    ltpObj.ExitLevels = comment;
                    ltpObj.HighestPNL = 0;
                    return ltpObj;

                }
                else if (stoplossHit < bookProfitHit)
                {
                    ltpObj.IsExit = true;
                    comment = "Exited - Trailing Stoploss hit - " + trailingStoploss.ToString();
                    pnl = direction == "BM" ? (trailingStoploss - avgPrice) * quantity : (avgPrice - trailingStoploss) * quantity;
                    ltpObj.LtpClose = trailingStoploss;
                    ltpObj.LtpHigh = drCurrent.High;
                    ltpObj.LtpLow = drCurrent.Low;
                    ltpObj.LtpOpen = drCurrent.Open;
                    ltpObj.trailingStopLoss = trailingStoploss;
                    ltpObj.PNL = pnl;
                    ltpObj.ExitCandle = TokenChannel.GetMinuteNumber(CurrentTradingDate, stoplossHit);
                    ltpObj.ExitLevels = comment;
                    ltpObj.HighestPNL = 0;
                    return ltpObj;
                }
                else if (stoplossHit == bookProfitHit && stoplossHit == DateTime.MaxValue)
                {
                    ltpObj.IsExit = true;
                    trailingStoploss = checkData.Last().Close;
                    comment = "Exited - Day End Close";
                    pnl = direction == "BM" ? (trailingStoploss - avgPrice) * quantity : (avgPrice - trailingStoploss) * quantity;
                    ltpObj.LtpClose = trailingStoploss;
                    ltpObj.LtpHigh = drCurrent.High;
                    ltpObj.LtpLow = drCurrent.Low;
                    ltpObj.LtpOpen = drCurrent.Open;
                    ltpObj.trailingStopLoss = trailingStoploss;
                    ltpObj.PNL = pnl;
                    ltpObj.ExitCandle = TokenChannel.GetMinuteNumber(CurrentTradingDate, checkData.Last().TimeStamp);
                    ltpObj.ExitLevels = comment;
                    ltpObj.HighestPNL = 0;
                    return ltpObj;
                }
                else if (stoplossHit == bookProfitHit)
                {
                    ltpObj.IsExit = true;
                    comment = "Exited - Trailing Stoploss hit - " + trailingStoploss.ToString();
                    pnl = direction == "BM" ? (trailingStoploss - avgPrice) * quantity : (avgPrice - trailingStoploss) * quantity;
                    ltpObj.LtpClose = trailingStoploss;
                    ltpObj.LtpHigh = drCurrent.High;
                    ltpObj.LtpLow = drCurrent.Low;
                    ltpObj.LtpOpen = drCurrent.Open;
                    ltpObj.trailingStopLoss = trailingStoploss;
                    ltpObj.PNL = pnl;
                    ltpObj.ExitCandle = TokenChannel.GetMinuteNumber(CurrentTradingDate, stoplossHit);
                    ltpObj.ExitLevels = comment;
                    ltpObj.HighestPNL = 0;
                    return ltpObj;
                }





                if (direction == "BM")
                {

                }
                else if (direction == "SM")
                {

                }


                double currentValue = 0;
                if (pnl1 > exitingPNL)
                {
                    currentValue = pnl1;
                }
                else
                {
                    currentValue = exitingPNL;
                }

                #region "PRIORITY 1 : TRAILING STOP LOSS HIT CHECK"
                if (direction == "BM")
                {


                    if (drCurrent.Low <= trailingStoploss)
                    {
                        ltpObj.IsExit = true;
                        comment = "Exited - Trailing Stoploss hit - " + trailingStoploss.ToString();
                        pnl = direction == "BM" ? (trailingStoploss - avgPrice) * quantity : (avgPrice - trailingStoploss) * quantity;
                        ltpObj.LtpClose = trailingStoploss;
                        ltpObj.LtpHigh = drCurrent.High;
                        ltpObj.LtpLow = drCurrent.Low;
                        ltpObj.LtpOpen = drCurrent.Open;
                        ltpObj.trailingStopLoss = trailingStoploss;
                        ltpObj.PNL = pnl;
                        ltpObj.ExitCandle = testAtMinute + 3;
                        ltpObj.ExitLevels = comment;
                        ltpObj.HighestPNL = currentValue;
                        return ltpObj;
                    }


                }
                else if (direction == "SM")
                {
                    if (drCurrent.High > trailingStoploss)
                    {
                        ltpObj.IsExit = true;
                        comment = "Exited - Trailing Stoploss hit - " + trailingStoploss.ToString();
                        pnl = direction == "BM" ? (trailingStoploss - avgPrice) * quantity : (avgPrice - trailingStoploss) * quantity;
                        ltpObj.LtpClose = trailingStoploss;
                        ltpObj.LtpHigh = drCurrent.High;
                        ltpObj.LtpLow = drCurrent.Low;
                        ltpObj.LtpOpen = drCurrent.Open;
                        ltpObj.trailingStopLoss = trailingStoploss;
                        ltpObj.PNL = pnl;
                        ltpObj.ExitCandle = testAtMinute + 3;
                        ltpObj.ExitLevels = comment;
                        ltpObj.HighestPNL = currentValue;
                        return ltpObj;
                    }

                }


                #endregion

                ltpObj.LtpClose = drCurrent.Close;
                ltpObj.LtpHigh = drCurrent.High;
                ltpObj.LtpLow = drCurrent.Low;
                ltpObj.LtpOpen = drCurrent.Open;
                ltpObj.trailingStopLoss = trailingStoploss;
                ltpObj.PNL = pnl;
                ltpObj.HighestPNL = currentValue;
                //if (pnl < MaxRisk / 2 && pnl > 200 && pnl1 > exitingPNL)
                //{
                //    ltpObj.trailingStopLoss = direction == "BM" ? ltpObj.LtpHigh - (MaxRisk / quantity) : ltpObj.LtpLow + (MaxRisk / quantity);
                //}
                //else if (pnl >= MaxRisk / 2 && pnl > 200 && pnl1 > exitingPNL)
                //{
                //    ltpObj.trailingStopLoss = direction == "BM" ? ltpObj.LtpHigh - ((MaxRisk / 3) / quantity) : ltpObj.LtpLow + ((MaxRisk / 3) / quantity);
                //}
                return ltpObj;
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message + " : " + ex.StackTrace + Environment.NewLine);
            }
            pnl = 0;
            return null;
        }
        List<string> orderLevel = new List<string>();


        private void tmrQuote_Tick(object sender, EventArgs e)
        {
            WritesQuotesZerodha(APIKEY, ACCESSTOKEN);
        }

        private void rdoLive_CheckStateChanged(object sender, EventArgs e)
        {
            if (rdoLive.IsChecked)
            {
                backLiveTest = false;
                //  panel1.Visible = true;
                panel1.Height = 700;
                panel1.Refresh();
                LogStatus("Application Mode is changed from Simulation to Live.");
            }
            else if (rdoSimulation.IsChecked)
            {
                backLiveTest = true;
                // panel1.Visible = false;
                LogStatus("Application Mode is changed from Live to Simulation.");

            }
        }

        private void rdoSimulation_CheckStateChanged(object sender, EventArgs e)
        {
            if (rdoLive.IsChecked)
            {
                backLiveTest = false;
                LogStatus("Application Mode is changed from Simulation to Live.");
            }
            else if (rdoSimulation.IsChecked)
            {
                backLiveTest = true;
                LogStatus("Application Mode is changed from Live to Simulation.");
            }
        }

        private void w_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        public string APIKEY
        {
            get;
            set;
        }

        public string ACCESSTOKEN
        {
            get;
            set;
        }
        List<Kite> kUsers = new List<Kite>();
        Dictionary<string, string> userLoginValues = new Dictionary<string, string>();
        bool doLogin = false;
        public void Login()
        {
            try
            {


                XDocument doc = XDocument.Load(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Users\users.xml");
                var Users = doc.Descendants("user");
                File.WriteAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Users\LoginUrl.txt", string.Empty);

                foreach (var u in Users)
                {
                    string uid = u.Attributes("id").First().Value;
                    string password = u.Attributes("password").First().Value;
                    string Q1 = u.Attributes("Q1").First().Value;

                    string A1 = u.Attributes("A1").First().Value;

                    userLoginValues.Add(uid, password);
                    userLoginValues.Add(Q1 + uid, A1);

                    string apiKey = u.Descendants("apikey").First().Value;
                    APIKEY = apiKey;
                    string apiSecret = u.Descendants("apisecret").First().Value;
                    Kite k = new Kite(apiKey, Debug: true);
                    kUsers.Add(k);
                    string lUrl = k.GetLoginURL();
                    File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Users\LoginUrl.txt", lUrl + Environment.NewLine);


                    System.Windows.Forms.WebBrowser w = new System.Windows.Forms.WebBrowser();
                    //w.Url = new Uri(lUrl);
                    //w.Dock = DockStyle.Fill;
                    //w.Name = uid;
                    //w.DocumentCompleted += w_DocumentCompleted;
                    //panel1.Controls.Add(w);
                }
            }
            catch (Exception ex)
            {
            }
            doLogin = true;
        }

        public void checkLogin()
        {

            if (panel1.Visible)
            {
                int remainingLogins = 0;
                for (int i = 0; i < panel1.Controls.Count; i++)
                {
                    if (panel1.Controls[i].Visible)
                    {
                        remainingLogins = remainingLogins + 1;
                    }

                }
                isLogin = true;
                if (remainingLogins == 0)
                {
                    panel1.Visible = false;
                    LogStatus("Logged in Successfully to Zerodha.");

                    if (string.IsNullOrEmpty(txtSwitchMode.Text))
                    {
                        backTestStatus = false;
                        goLiveTimer.Interval = 2500;
                        goLiveTimer.Start();
                        goLiveTimer.Enabled = true;
                        LogStatus("Market is stared now...");
                    }
                    radButton2_Click(this, EventArgs.Empty);
                    return;
                }

                /* foreach (Control c in panel1.Controls)
                 {
                     XDocument doc = XDocument.Load(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Users\users.xml");
                     var Users = doc.Descendants("user");

                     try
                     {
                         bool detailsfilled = true;
                         System.Windows.Forms.WebBrowser x = (System.Windows.Forms.WebBrowser)c;
                         string param1 = HttpUtility.ParseQueryString(x.Url.Query).Get("request_token");

                         XElement user = (from el in doc.Root.Elements("user")
                                          where (string)el.Attribute("id") == c.Name.ToString()
                                          select el).First();

                         if (!string.IsNullOrEmpty(param1) && x.Visible)
                         {

                             user.Descendants("requestToken").First().Value = param1;
                             doc.Save(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Users\users.xml");
                             x.Visible = false;
                         }
                         else if (x.Visible)
                         {

                             foreach (HtmlElement he in x.Document.GetElementsByTagName("input"))
                             {
                                 if (he.GetAttribute("value") == "")
                                     detailsfilled = false;

                                 if (he.GetAttribute("type") == "text" && he.GetAttribute("maxlength") == "6" && he.GetAttribute("value") == "")
                                 {
                                     he.SetAttribute("value", x.Name);
                                     he.SetAttribute("innerText", x.Name);
                                 }

                                 if (he.GetAttribute("type") == "password" && he.GetAttribute("maxlength") == "30" && he.GetAttribute("value") == "" && he.GetAttribute("label") == "")
                                 {
                                     he.SetAttribute("value", userLoginValues[x.Name]);
                                     he.SetAttribute("innerText", userLoginValues[x.Name]);
                                 }

                                 if (he.GetAttribute("type") == "password" && he.GetAttribute("label") == "PIN")
                                 {
                                     string ques = he.GetAttribute("label");
                                     try
                                     {
                                         he.SetAttribute("value", userLoginValues.Last().Value);
                                         he.SetAttribute("innerText", userLoginValues.Last().Value);
                                     }

                                     catch
                                     {

                                     }

                                 }

                             }


                             if (detailsfilled)
                             {
                                 x.Document.GetElementsByTagName("button")[0].InvokeMember("click");
                             }
                         }
                     }
                     catch
                     {
                     }
                 }*/
            }

        }

        private void rdoLive_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {

        }

        private void rgvStocks_CellFormatting(object sender, CellFormattingEventArgs e)
        {
            if (e.Column.HeaderText == "MTM"
        && !string.IsNullOrEmpty(e.Row.Cells["target"].Value.ToString()) && Convert.ToDouble(e.Row.Cells["target"].Value) < 0)
            {
                e.CellElement.DrawFill = true;
                e.CellElement.BackColor = Color.Red;
                e.CellElement.ForeColor = Color.White;
                e.CellElement.NumberOfColors = 1;
            }
            else if (e.Column.HeaderText == "MTM"
        && !string.IsNullOrEmpty(e.Row.Cells["target"].Value.ToString()) && Convert.ToDouble(e.Row.Cells["target"].Value) > 0)
            {
                e.CellElement.DrawFill = true;
                e.CellElement.BackColor = Color.Green;
                e.CellElement.ForeColor = Color.White;
                e.CellElement.NumberOfColors = 1;

            }
            else
            {
                e.CellElement.ResetValue(LightVisualElement.DrawFillProperty, ValueResetFlags.Local);
                e.CellElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
                e.CellElement.ResetValue(LightVisualElement.NumberOfColorsProperty, ValueResetFlags.Local);
            }
        }

        private void rgvStocks_Click(object sender, EventArgs e)
        {

        }

        Dictionary<string, StockData> scripTodaysLevels = new Dictionary<string, StockData>();

        private void txtBTD_TextChanged(object sender, EventArgs e)
        {
            DataSet dateMapping = new DataSet();
            string currentFileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\Users\Dates.csv";
            OleDbConnection dconn = new OleDbConnection
                   ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                     Path.GetDirectoryName(currentFileName) +
                     "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

            dconn.Open();

            OleDbDataAdapter dadapter = new OleDbDataAdapter
                   ("SELECT * FROM " + Path.GetFileName(currentFileName), dconn);


            dadapter.Fill(dateMapping);

            dconn.Close();

            CurrentTradingDate = Convert.ToDateTime(dateMapping.Tables[0].Rows[dateMapping.Tables[0].Rows.Count - 1 - BTD]["Date"]);
            radLabel9.Text = CurrentTradingDate.ToString("dd-MMM-yyyy");



            pList.Clear();
            int index = BTD;
            if (scripTodaysLevels != null)
                scripTodaysLevels.Clear();

            //LoadDailyNPivotsData();

        }
        string htmlReceipt = "<!DOCTYPE HTML><html><head></head><body>{0}</body></html>";
        StringBuilder sXs = new StringBuilder();

        private void AppendDataToReceipt(StringBuilder header)
        {
            header.Insert(0, "<table border=1><tr>");
            header.Append("</tr>");

            if (orders != null && orders.Rows.Count > 0)
            {
                foreach (DataColumn c in orders.Columns)
                {
                    if (reportColumns.Contains(c.ColumnName))
                        header.Append("<th>" + c.ColumnName + "</th>");
                }

                foreach (DataRow d in orders.Rows)
                {
                    header.Append("<tr>");
                    foreach (DataColumn c in orders.Columns)
                    {
                        if (reportColumns.Contains(c.ColumnName))
                            header.Append("<td>" + d[c].ToString() + "</td>");
                    }
                    header.Append("</tr>");
                }
            }
            header.Append("</table>");
            sXs.Append(header);
        }
        private void btnReceipt_Click(object sender, EventArgs e)
        {
            File.WriteAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\receipt" + radLabel9.Text + ".html", string.Format(htmlReceipt, sXs));
        }

        private void txtSwitchMode_TextChanged(object sender, EventArgs e)
        {
            if (txtSwitchMode.Text != string.Empty)
            {
                if (MessageBox.Show("To run realtime Enter 09:05:13 as Start Time. Are you ok to proceed with current config??", "Start Time Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
                if (txtSwitchMode.Text == "09:14:13" || txtSwitchMode.Text != string.Empty)
                {
                    if (MessageBox.Show("Confirm Your Current Trading Date!!", "Date Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        if (MessageBox.Show("Confirm Your Risk!!", "Risk Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            if (MessageBox.Show("Confirm Minute Pointer(30-3,60-9)!!", "Minute Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                DateTime t = Convert.ToDateTime(Convert.ToDateTime(txtSwitchMode.Text).ToString("hh:mm:ss"));

                                txtQuoteStart.Text = t.AddMinutes(5).AddSeconds(-10).ToString("hh:mm:ss");
                                txtLogin.Text = t.AddMinutes(0).AddSeconds(5).ToString("hh:mm:ss");
                                txtMarketStart.Text = t.AddMinutes(1).AddSeconds(-9).ToString("hh:mm:ss");

                                LogStatus("Quote Time : " + txtQuoteStart.Text);
                                LogStatus("Login Time : " + txtLogin.Text);
                                LogStatus("Market Time : " + txtMarketStart.Text);
                            }
                        }
                    }

                }
                else
                {
                    DateTime t = Convert.ToDateTime(Convert.ToDateTime(txtSwitchMode.Text).ToString("hh:mm:ss"));
                    txtQuoteStart.Text = t.AddMinutes(1).AddSeconds(5).ToString("hh:mm:ss");
                    txtLogin.Text = t.AddMinutes(1).AddSeconds(10).ToString("hh:mm:ss");
                    txtMarketStart.Text = t.AddMinutes(2).AddSeconds(10).ToString("hh:mm:ss");
                    LogStatus("Quote Time : " + txtQuoteStart.Text);
                    LogStatus("Login Time : " + txtLogin.Text);
                    LogStatus("Market Time : " + txtMarketStart.Text);
                }
            }
            else if (txtSwitchMode.Text == string.Empty)
            {
                LoadAllDateTillDate();
                goLiveTimer.Interval = 5000;
                goLiveTimer.Start();
                return;
            }

        }

        public void ResetScreen(double currentValue)
        {
            if (orders != null && orders.Rows.Count > 0)
            {


                int totalWrong = orders.AsEnumerable()
          .GroupBy(r => r.Field<string>("scrip"))
          .Select(g => new
          {
              NAME = g.Key,
              SUM = g.Sum(r => r.Field<double>("bp"))
          }).Count(b => b.SUM < 0);

                int totalWright = orders.AsEnumerable()
          .GroupBy(r => r.Field<string>("scrip"))
          .Select(g => new
          {
              NAME = g.Key,
              SUM = g.Sum(r => r.Field<double>("bp"))
          }).Count(b => b.SUM > 0);
                double increaseTrade = 0;
                var z = orders.AsEnumerable()
          .GroupBy(r => r.Field<string>("strategy"))
          .Select(g => new
          {
              NAME = g.Key,
              SUM = g.Sum(r => r.Field<double>("bp"))
          }).ToList();
                if (z.Where(a => a.NAME == "Increase..").Count() > 0)
                {

                    increaseTrade = z.Where(a => a.NAME == "Increase..").First().SUM;
                }

                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\NEWPL.txt", CurrentTradingDate.ToString() + "," + currentValue.ToString() + "," + totalWrong.ToString() + "," + totalWright.ToString() + "," + increaseTrade.ToString() + Environment.NewLine);
                StringBuilder sb = new StringBuilder();
                sb.Append("<th>Date : " + CurrentTradingDate.ToString() + "</th>");
                sb.Append("<th>PNL : " + currentValue.ToString() + "</th>");
                sb.Append("<th>IT : " + totalWrong.ToString() + "</th>");
                sb.Append("<th>CT : " + totalWright.ToString() + "</th>");
                sb.Append("<th>AT : " + increaseTrade.ToString() + "</th>");
                AppendDataToReceipt(sb);



            }

            tmrClock.Enabled = false;
            tmrClock.Stop();
            goLiveTimer.Stop();
            goLiveTimer.Enabled = false;
            txtSMA.Text = "3";
            txtPSAA.Text = "0";
            txt60Min.Text = "1";
            txt30Min.Text = "1";
            txtSuperTrend.Text = "1";
            lowestTotalToday = 0;
            HighestProfitToday = 0;
            txtBTD.Text = Convert.ToString(Convert.ToInt32(txtBTD.Text) + 1);
            //  LoadDailyNPivotsDataZerodha();
            firstLoad = true;
            secondLoad = true;
            thirdLoad = true;
            allData.Clear();
            //if (currentValue < 0)
            //{
            //    MaxRisk = MaxRisk + 2000;
            //}
            //else if (currentValue > 0)
            //{
            //    MaxRisk = 2000;
            //}
            txtTam.Text = "-3";

            orders.Rows.Clear();
            rgvStocks.DataSource = orders;

            dateMapping = new DataSet();
            string currentFileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\Users\Dates.csv";
            OleDbConnection dconn = new OleDbConnection
                   ("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                     Path.GetDirectoryName(currentFileName) +
                     "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

            dconn.Open();

            OleDbDataAdapter dadapter = new OleDbDataAdapter
                   ("SELECT * FROM " + Path.GetFileName(currentFileName), dconn);


            dadapter.Fill(dateMapping);

            dconn.Close();
            if (File.Exists(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\ABC.csv"))
            {
                File.Delete(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\ABC.csv");

                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\ABC.csv", "Name" + "," + "Timestamp" + "," + "BS" + "," + "Quantity" + "," + "close" + "," + "stoploss" + "," + "high" + "," + "low" + "," + "volume" + "," + "pattern" + "," + "level" + Environment.NewLine);
            }
            CurrentTradingDate = Convert.ToDateTime(dateMapping.Tables[0].Rows[dateMapping.Tables[0].Rows.Count - 1 - BTD]["Date"]).Date;
            radLabel9.Text = CurrentTradingDate.ToString("dd-MMM-yyyy");
            rgvStocks.DataSource = orders;
            tmrClock.Enabled = true;
            tmrClock.Start();
            goLiveTimer.Enabled = true;
            goLiveTimer.Start();
        }

        DataSet dsSampleTrades = new DataSet();

        private void txtLDF_TextChanged(object sender, EventArgs e)
        {
            //MessageBox.Show(TokenChannel.RoundDown(Convert.ToInt32(txtLDF.Text)).ToString());
            //MessageBox.Show(TokenChannel.RoundUp(Convert.ToInt32(txtLDF.Text)).ToString());
        }

        double lowestTotalToday = 0;

        private void rgvStocks_GroupSummaryEvaluate(object sender, GroupSummaryEvaluationEventArgs e)
        {
            if (e.SummaryItem.Name == "total")
            {


            }

        }

        public void Restore()
        {
            List<SyncData> lsd = DeSerializeObject<List<SyncData>>(@"C:\Jai Sri Thakur Ji\Sync\Sync.xml");
            SyncData sd = lsd[0];
            HighestProfitToday = sd.HighestProfitToday;
            lowestTotalToday = sd.LowestRiskToday;
            txtTam.Text = sd.Minute.ToString();
            DataSet dsj = new DataSet();
            dsj.ReadXml(new StringReader(sd.orders));
            DataTable c = dsj.Tables[0];

            foreach (DataRow dr in c.Rows)
            {
                orders.Rows.Add(dr.ItemArray);
            }


            rgvStocks.DataSource = orders;
            SMAQuanitty = 0;
            PSAAQuantity = 0;
            _60MinQuantity = 0;

        }

        private void txtBackupNSync_Click(object sender, EventArgs e)
        {
            TestRunMarket();
        }
        static List<Candle> analysisData = new List<Candle>();
        private void radButton1_Click_1(object sender, EventArgs e)
        {

            foreach (Kite kiteUser in kUsers)
            {
                List<KiteConnect.Order> allOrders = kiteUser.GetOrders();
                List<KiteConnect.Order> completedOrders = allOrders.Where(a => a.Status == "COMPLETE" || a.Status == "TRIGGER PENDING").ToList();
                double brokerage = completedOrders.Count() * 20;

                double totalSellValue = Convert.ToDouble(completedOrders.Where(a => a.TransactionType == "SELL").Select(a => a.Quantity * a.AveragePrice).Sum());
                double totalBuyValue = Convert.ToDouble(completedOrders.Where(a => a.TransactionType == "BUY").Select(a => a.Quantity * a.AveragePrice).Sum());
                double STT = totalSellValue * 0.00025;
                double Ett = (totalBuyValue * 0.0000325) + (totalSellValue * 0.0000325);
                double GST = (brokerage + Ett) * 0.018;
                double sebiCharges = (totalBuyValue + totalSellValue) * 0.0000015;
                double stampDuty = (totalBuyValue + totalSellValue) * 0.00002;

                if (stampDuty > 1000)
                {
                    stampDuty = 1000;
                }
                double total = brokerage + STT + Ett + GST + sebiCharges + stampDuty;

                LogStatus("Brokerage : " + brokerage.ToString());
                LogStatus("STT : " + STT.ToString());
                LogStatus("Exchange : " + Ett.ToString());
                LogStatus("GST : " + GST.ToString());
                LogStatus("SEBI : " + sebiCharges.ToString());
                LogStatus("STAMP DUTY  : " + stampDuty.ToString());
                LogStatus("Grand Total :" + total.ToString());


            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void txt30Min_TextChanged(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            LongTermInvestment();
            List<double> profitList = new List<double>();

            int i = 0;
            for (i = 0; i < 500; i++)
            {
                List<StockData> trades = new List<StockData>();
                foreach (var stock in allData["day"])
                {
                    List<Candle> hstData = stock.Value;
                    if (hstData.Count < 500)
                        continue;
                    int count = hstData.Count - i;
                    string stockName = stock.Key;
                    Candle CurrentDayCandle = hstData[count - 1];
                    Candle PreviousDayCandle = hstData[count - 2];
                    Candle GreatPreviousDayCandle = hstData[count - 3];
                    if (PreviousDayCandle.STrend.Trend == -1 && CurrentDayCandle.STrend.Trend == 1)
                    {
                        trades.Add(new StockData { Symbol = stockName, Direction = "BM", Vol = PreviousDayCandle.Close * PreviousDayCandle.Volume, dClose = PreviousDayCandle.Close, dOpen = CurrentDayCandle.Open, Close = CurrentDayCandle.Close });
                    }


                }
                trades = trades.OrderByDescending(a => a.Vol).ToList();

                trades.Clear();
            }

            double finalProfit = profitList.Sum();

        }
    }


}
