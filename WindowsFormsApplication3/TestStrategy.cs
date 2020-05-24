using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Telerik.WinControls.UI;

using NSA;
using System.Linq;

using System.Threading.Tasks;
using KiteConnect;
using System.Xml.Linq;
using System.Xml.Serialization;
using CommonFeatures;
using BAL;
using Telerik.WinControls.Export;
using System.Windows.Forms;
using System.Collections.Concurrent;
using Model;

namespace _15MCE
{


    public partial class TestStrategy : Telerik.WinControls.UI.RadForm
    {
        DataTable orders = null;

        DataTable pivotPointsCurrent = new DataTable();
        DataTable pivotPointsAll = new DataTable();
        DataTable dma = new DataTable();
        DataSet dsLots = new DataSet();
        public static int IncrementCounter = 3;



        public string[] AllFNO
        {
            get
            {
                return Common.GetStocks().Select(a => a.StockName).ToArray();
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

        public DateTime CurrentTradingDate
        {
            get;
            set;
        }

        public TestStrategy()
        {
            InitializeComponent();
        }
        List<Pivots> pList = new List<Pivots>();
        private void radGroupBox1_Click(object sender, EventArgs e)
        {

        }

        bool backLiveTest = false;
        int psa = Convert.ToInt16(System.Configuration.ConfigurationSettings.AppSettings["PSA"]);
        int sma = Convert.ToInt16(System.Configuration.ConfigurationSettings.AppSettings["SMA"]);
        //bool calculateBrokerage = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["Brokerage"]);
        //int sma = Convert.ToInt16(System.Configuration.ConfigurationSettings.AppSettings["SMA"]);

        DataSet instrToken = new DataSet();
        public static bool DONT_DELETE = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["DoNotRemove"]);
        public static bool takeBackupOfFiles = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["TakeBackupOfFilesAfterPlacingOrders"]);

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        List<string> reportColumns = new List<string>();
        private void TestStrategy_Load(object sender, EventArgs e)
        {
            List<Model.Idea> ideas = Common.GetIdeas();
            foreach (var i in ideas)
            {
                radDropDownList1.Items.Add(i.Name);
                radDropDownList1.SelectedIndex = 0;
            }

            ddlStartDate.SelectedIndex = 0;
            ddlStartMonth.SelectedIndex = 0;
            ddlStartYear.SelectedIndex = 0;
            ddlEndDate.SelectedIndex = 0;
            ddlEndMonth.SelectedIndex = 0;
            ddlEndYear.SelectedIndex = 0;


        }

        public static bool ZERODHA = true;





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




        public void ShowMyProgress(string status)
        {
            SetText(status);
        }

        delegate void SetTextCallback(string text);
        private void SetText(string text)
        {
            if (this.radProgres.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.radProgres.Text = text;
            }
        }

        delegate void SetDataSourceCallback(List<Model.PNL> outcome);
        private void SetDataSource(List<Model.PNL> outcome)
        {

            List<Model.PNL> x = new List<Model.PNL>();
            int cc = 0;
            foreach (var b in outcome.OrderBy(c => c.Date))
            {
                x.Add(b);
                continue;
                if (cc >= 3)
                {

                    cc++;
                    x.Add(b);
                    if (b.Amount > 0)
                    {
                        cc = 0;
                    }
                }
                if (b.Amount <= 0)
                {
                    cc++;

                }
                else
                {
                    cc = 0;
                }


            }
            if (rgvStocks.DataSource as List<PNL> != null)
            {
                x.AddRange(rgvStocks.DataSource as List<PNL>);
            }

            if (this.rgvStocks.InvokeRequired)
            {
                SetDataSourceCallback d = new SetDataSourceCallback(SetDataSource);
                this.Invoke(d, new object[] { outcome });
            }
            else
            {

                this.rgvStocks.DataSource = x;
            }
        }


        DataTable finalList = new DataTable();
        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                ProgressDelegate myProgres = ShowMyProgress;
                List<Model.Idea> myideas = null;
                if (checkBox1.Checked)
                {
                    myideas = Common.GetIdeas();
                }
                else
                {
                    myideas = Common.GetIdeas().Where(a => a.Name == radDropDownList1.SelectedItem.Text).ToList();
                }

                //Model.Idea selectedIdea = myideas.Where(a => a.Name == radDropDownList1.SelectedItem.Text).First();
                foreach (var selectedIdea in myideas)
                {
                    StockOHLC stockOHLC = new StockOHLC();

                    //Load Data
                    Task<Dictionary<string, List<Model.Candle>>> loadmydata = Task.Run<Dictionary<string, List<Model.Candle>>>(() => stockOHLC.GetOHLC(new DateTime(Convert.ToInt32(ddlStartYear.SelectedItem.Text), Convert.ToInt32(ddlStartMonth.SelectedItem.Text), Convert.ToInt32(ddlStartDate.SelectedItem.Text)), new DateTime(Convert.ToInt32(ddlEndYear.SelectedItem.Text), Convert.ToInt32(ddlEndMonth.SelectedItem.Text), Convert.ToInt32(ddlEndDate.SelectedItem.Text)), selectedIdea.Interval, myProgres));

                    //Apply indicators
                    loadmydata.ContinueWith((t0) =>
                    {
                        SetText("Applying indicators");
                        Task<Dictionary<string, List<Model.Candle>>> withIndicators = Task.Run<Dictionary<string, List<Model.Candle>>>(() => TechnicalIndicators.AddIndicators(t0.Result, selectedIdea.TI));
                        Task getTradingStocks = withIndicators.ContinueWith((t1) =>
                        {
                            Task<Dictionary<Guid, Model.StrategyModel>> getTradedStocks = Task.Run<Dictionary<Guid, Model.StrategyModel>>(() => stockOHLC.GetTopMostSolidGapOpenerDayWise(t1.Result, selectedIdea, myProgres));
                            Task tradeMyStocks = getTradedStocks.ContinueWith((t2) =>
                            {
                                Task<List<Model.PNL>> calculation = Task<List<Model.PNL>>.Run(() => stockOHLC.TradeStocks(t2.Result, t1.Result, selectedIdea, myProgres));
                                calculation.ContinueWith((t3) =>
                                {
                                    SetDataSource(t3.Result);
                                    SetText("Idea ran successfully");
                                });
                            });
                        });
                    });
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
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
            }
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



        //public void LoadDailyNPivotsDataZerodha()
        //{
        //    try
        //    {
        //        //scrips = new string[] { "ITC" };
        //        if (new DirectoryInfo(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\pivotpoints\").GetFiles().Count() == 0)
        //        {
        //            Parallel.ForEach(AllFNO, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
        //            {
        //                CallWebServiceZerodha(instrToken.Tables[0].Select("symbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-50).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "day");

        //            });

        //            Parallel.ForEach(AllFNO, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
        //            {
        //                CallWebServiceZerodha(instrToken.Tables[0].Select("symbol='" + s + "'")[0][0].ToString(), s, CurrentTradingDate.AddDays(-50).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"), APIKEY, ACCESSTOKEN, "5minute");

        //            });

        //            Parallel.ForEach(AllFNO, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (s) =>
        //            {
        //                List<StockData> lstStockData = new List<StockData>();
        //                string FileName = string.Empty;
        //                FileName = @"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\day\" + s.Replace("-", string.Empty) + ".txt";
        //                string json = File.ReadAllText(FileName).Replace("]}}", string.Empty).Replace("\"", "").Replace("T", " ").Replace("+", " ");
        //                DataSet ds = TokenChannel.ConvertToJason(json);
        //                DataTable dtDataHistory = ds.Tables[0];



        //                Indicator ind1 = new Indicator();
        //                ind1.IndicatorName = "SuperTrend";

        //                Indicator ind2 = new Indicator();
        //                ind2.IndicatorName = "SMA20";

        //                Indicator ind3 = new Indicator();
        //                ind3.IndicatorName = "SMA50";

        //                Indicator ind4 = new Indicator();
        //                ind4.IndicatorName = "SMA200";

        //                Indicator ind5 = new Indicator();
        //                ind5.IndicatorName = "MACD";


        //                List<Indicator> xInd = new List<Indicator>();
        //                xInd.Add(ind1);
        //                xInd.Add(ind2);
        //                xInd.Add(ind3);
        //                xInd.Add(ind4);
        //                xInd.Add(ind5);

        //                //Indicator ind2 = new Indicator();
        //                //ind2.IndicatorName = "RSI";              
        //                //xInd.Add(ind2);

        //                Indicators.AddIndicators(ref ds, xInd);


        //                foreach (DataRow dr in dtDataHistory.Rows)
        //                {
        //                    lstStockData.Add(new StockData { TradingDate = Convert.ToDateTime(dr["Timestamp"]).Date, Symbol = s, High = Convert.ToDouble(dr["f3"]), Open = Convert.ToDouble(dr["f5"]), Close = Convert.ToDouble(dr["f2"]), Low = Convert.ToDouble(dr["f4"]), MACD = Convert.ToDouble(dr["MACD"]), MACD9 = Convert.ToDouble(dr["MACD9"]), HISTOGRAM = Convert.ToDouble(dr["HISTOGRAM"]), SuperTrend = Convert.ToDouble(dr["SuperTrend"]), SMA20 = Convert.ToDouble(dr["20"]), SMA50 = Convert.ToDouble(dr["50"]), SMA200 = Convert.ToDouble(dr["200"]) });
        //                }
        //                if (lstStockData.Where(a => a.TradingDate == CurrentTradingDate).Count() == 0)
        //                {
        //                    lstStockData.Add(new StockData { TradingDate = CurrentTradingDate, Symbol = s, High = 0, Open = 0, Close = 0, Low = 0, MACD = 0, MACD9 = 0, HISTOGRAM = 0, SMA20 = 0, SMA200 = 0, SMA50 = 0, SuperTrend = 0 });
        //                }

        //                // pivot calculation for 5,10,15 Min & 60 Min chart - range is 1 Day & for last 2 years
        //                lstStockData = lstStockData.OrderBy(a => a.TradingDate).ToList();
        //                int i = 0;
        //                foreach (StockData sd in lstStockData)
        //                {
        //                    if (i - 1 >= 0)
        //                    {
        //                        DateTime currentDay = sd.TradingDate;
        //                        DateTime prevDate = lstStockData[i - 1].TradingDate;
        //                        double prevDayClose = lstStockData.Where(a => a.TradingDate == prevDate).First().Close;
        //                        // get last price from 5 minutes tick
        //                        string FileNameforLastPrice = string.Empty;
        //                        FileNameforLastPrice = @"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\5\" + s.Replace("-", string.Empty) + ".txt";
        //                        string jsonforLastPrice = File.ReadAllText(FileNameforLastPrice).Replace("]}}", string.Empty).Replace("\"", "").Replace("T", " ").Replace("+", " ");

        //                        DataSet dsforLastPrice = TokenChannel.ConvertToJason(jsonforLastPrice);

        //                        DataTable dtforLastPrice = dsforLastPrice.Tables[0];
        //                        string timeStamp = TokenChannel.GetTimeStamp(-2, prevDate.Date);
        //                        string timeStampLast = TokenChannel.GetTimeStamp(-2, prevDate.Date.AddDays(1));
        //                        var data = dsforLastPrice.Tables[0].AsEnumerable().Where(a => Convert.ToDateTime(a.Field<String>("Timestamp")) >= Convert.ToDateTime(timeStamp) && Convert.ToDateTime(a.Field<String>("Timestamp")) < Convert.ToDateTime(timeStampLast)).OrderByDescending(a => Convert.ToDateTime(a.Field<String>("Timestamp"))).ToList();
        //                        if (data.Count > 0)
        //                        {
        //                            prevDayClose = Convert.ToDouble(data[0].Field<double>("f2"));
        //                        }

        //                        double prevDayOpen = lstStockData.Where(a => a.TradingDate == prevDate).First().Open;
        //                        double prevDayHigh = lstStockData.Where(a => a.TradingDate == prevDate).First().High;
        //                        double prevDayLow = lstStockData.Where(a => a.TradingDate == prevDate).First().Low;
        //                        double dailyPivot = (prevDayClose + prevDayHigh + prevDayLow) / 3;
        //                        sd.dPP = Math.Round(dailyPivot, 2);
        //                        sd.dR1 = Math.Round((2 * dailyPivot) - prevDayLow, 2);
        //                        sd.dS1 = Math.Round((2 * dailyPivot) - prevDayHigh, 2);
        //                        sd.dR2 = Math.Round(dailyPivot + (prevDayHigh - prevDayLow), 2);
        //                        sd.dS2 = Math.Round(dailyPivot - (prevDayHigh - prevDayLow), 2);
        //                        sd.dR3 = Math.Round(dailyPivot + 2 * (prevDayHigh - prevDayLow), 2);
        //                        sd.dS3 = Math.Round(dailyPivot - 2 * (prevDayHigh - prevDayLow), 2);
        //                        sd.dClose = prevDayClose;
        //                        sd.dHigh = prevDayHigh;
        //                        sd.dLow = prevDayLow;
        //                        sd.dOpen = prevDayOpen;


        //                        // pivot calculation for monthly & weekly chart - range is one year
        //                        int currentYear = sd.TradingDate.Year;
        //                        int lastYear = currentYear - 1;
        //                        if (lstStockData.Where(a => a.TradingDate.Year == lastYear).Count() > 0)
        //                        {
        //                            double lastYearClose = lstStockData.Where(a => a.TradingDate.Year == lastYear).OrderBy(a => a.TradingDate).Last().Close;
        //                            double lastYearHigh = lstStockData.Where(a => a.TradingDate.Year == lastYear).Max(a => a.High);
        //                            double lastYearLow = lstStockData.Where(a => a.TradingDate.Year == lastYear).Min(a => a.Low);
        //                            double lastYearOpen = lstStockData.Where(a => a.TradingDate.Year == lastYear).First().Open;
        //                            double yearPivot = (lastYearClose + lastYearHigh + lastYearLow) / 3;
        //                            sd.yPP = Math.Round(yearPivot, 2);
        //                            sd.yR1 = Math.Round((2 * yearPivot) - lastYearLow, 2);
        //                            sd.yS1 = Math.Round((2 * yearPivot) - lastYearHigh, 2);
        //                            sd.yR2 = Math.Round(yearPivot + (lastYearHigh - lastYearLow), 2);
        //                            sd.yS2 = Math.Round(yearPivot - (lastYearHigh - lastYearLow), 2);
        //                            sd.yR3 = Math.Round(yearPivot + 2 * (lastYearHigh - lastYearLow), 2);
        //                            sd.yS3 = Math.Round(yearPivot - 2 * (lastYearHigh - lastYearLow), 2);
        //                            sd.yClose = lastYearClose;
        //                            sd.yHigh = lastYearHigh;
        //                            sd.yLow = lastYearLow;
        //                            sd.yOpen = lastYearOpen;
        //                            sd.YearStartDate = lstStockData.Where(a => a.TradingDate.Year == lastYear).OrderBy(a => a.TradingDate).First().TradingDate;
        //                            sd.YearEndDate = lstStockData.Where(a => a.TradingDate.Year == lastYear).OrderBy(a => a.TradingDate).Last().TradingDate;
        //                        }

        //                        // pivot calculation for daily chart - range is one month                
        //                        int currentMonth = sd.TradingDate.Month;
        //                        int lastMonth = currentMonth - 1;
        //                        if (lastMonth == 0)
        //                        {
        //                            lastMonth = 12;
        //                            currentYear = currentYear - 1;
        //                        }
        //                        if (lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == lastMonth).Count() > 0)
        //                        {
        //                            double lastMonthClose = lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == lastMonth).OrderBy(a => a.TradingDate).Last().Close;
        //                            double lastMonthHigh = lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == lastMonth).Max(a => a.High);
        //                            double lastMonthLow = lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == lastMonth).Min(a => a.Low);
        //                            double lastMonthOpen = lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == lastMonth).First().Open;
        //                            double monthPivot = (lastMonthClose + lastMonthHigh + lastMonthLow) / 3;
        //                            sd.mPP = Math.Round(monthPivot, 2);
        //                            sd.mR1 = Math.Round((2 * monthPivot) - lastMonthLow, 2);
        //                            sd.mS1 = Math.Round((2 * monthPivot) - lastMonthHigh, 2);
        //                            sd.mR2 = Math.Round(monthPivot + (lastMonthHigh - lastMonthLow), 2);
        //                            sd.mS2 = Math.Round(monthPivot - (lastMonthHigh - lastMonthLow), 2);
        //                            sd.mR3 = Math.Round(monthPivot + 2 * (lastMonthHigh - lastMonthLow), 2);
        //                            sd.mS3 = Math.Round(monthPivot - 2 * (lastMonthHigh - lastMonthLow), 2);
        //                            sd.mClose = lastMonthClose;
        //                            sd.mHigh = lastMonthHigh;
        //                            sd.mLow = lastMonthLow;
        //                            sd.mOpen = lastMonthOpen;
        //                            sd.MonthStartDate = lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == lastMonth).OrderBy(a => a.TradingDate).First().TradingDate;
        //                            sd.MonthEndDate = lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == lastMonth).OrderBy(a => a.TradingDate).Last().TradingDate;
        //                            try
        //                            {
        //                                sd.curMonthClose = lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == sd.TradingDate.Month).OrderBy(a => a.TradingDate).Last().Close;
        //                                sd.curMonthHigh = lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == sd.TradingDate.Month).Max(a => a.High);
        //                                sd.curMonthLow = lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == sd.TradingDate.Month).Min(a => a.Low);
        //                                sd.curMonthOpen = lstStockData.Where(a => a.TradingDate.Year == currentYear && a.TradingDate.Month == sd.TradingDate.Month).First().Open;
        //                            }
        //                            catch
        //                            {
        //                            }
        //                        }
        //                        currentYear = sd.TradingDate.Year;

        //                        // pivot calculation for 30 Min & 60 Min chart - range is 1 week
        //                        int curretnWeek = TokenChannel.GetWeekOfMonth(sd.TradingDate);
        //                        int lastWeek = curretnWeek - 1;
        //                        if (lastWeek == 0)
        //                        {
        //                            if (currentMonth == 1 && curretnWeek == 1)
        //                            {
        //                                currentYear = currentYear - 1;
        //                                currentMonth = 12;
        //                                curretnWeek = TokenChannel.GetWeekOfMonth(new DateTime(currentYear, currentMonth, 27));
        //                            }
        //                            else if (curretnWeek == 1)
        //                            {
        //                                currentMonth = 12;
        //                                curretnWeek = TokenChannel.GetWeekOfMonth(new DateTime(currentYear, currentMonth, 27));
        //                            }
        //                        }

        //                        DateTime thisWeekMonday = sd.TradingDate.StartOfWeek(DayOfWeek.Monday).AddDays(-7);
        //                        DateTime thisWeekTuesday = thisWeekMonday.AddDays(1);
        //                        DateTime thisWeekWednesday = thisWeekMonday.AddDays(2);
        //                        DateTime thisWeekThursday = thisWeekMonday.AddDays(3);
        //                        DateTime thisWeekFriday = thisWeekMonday.AddDays(4);
        //                        List<DateTime> weekDayList = new List<DateTime>();
        //                        weekDayList.Add(thisWeekMonday);
        //                        weekDayList.Add(thisWeekTuesday);
        //                        weekDayList.Add(thisWeekWednesday);
        //                        weekDayList.Add(thisWeekThursday);
        //                        weekDayList.Add(thisWeekFriday);

        //                        var lstWeekData = lstStockData.Where(a => weekDayList.Contains(a.TradingDate)).OrderBy(a => a.TradingDate).ToList();


        //                        DateTime curWeekMonday = sd.TradingDate.StartOfWeek(DayOfWeek.Monday);
        //                        DateTime curWeekTuesday = curWeekMonday.AddDays(1);
        //                        DateTime curWeekWednesday = curWeekMonday.AddDays(2);
        //                        DateTime curWeekThursday = curWeekMonday.AddDays(3);
        //                        DateTime curWeekFriday = curWeekMonday.AddDays(4);
        //                        List<DateTime> curweekDayList = new List<DateTime>();
        //                        curweekDayList.Add(curWeekMonday);
        //                        curweekDayList.Add(curWeekTuesday);
        //                        curweekDayList.Add(curWeekWednesday);
        //                        curweekDayList.Add(curWeekThursday);
        //                        curweekDayList.Add(curWeekFriday);

        //                        var curWeekData = lstStockData.Where(a => curweekDayList.Contains(a.TradingDate)).OrderBy(a => a.TradingDate).ToList();
        //                        if (curWeekData.Count() > 0)
        //                        {
        //                            sd.curWeekOpen = curWeekData.First().Open;
        //                            sd.curWeekHigh = curWeekData.Max(a => a.High);
        //                            sd.curWeekLow = curWeekData.Min(a => a.Low);
        //                            sd.curWeekClose = curWeekData.Last().Close;
        //                        }

        //                        if (lstWeekData.Count() > 0)
        //                        {

        //                            double lastWeekclose = lstWeekData.Last().Close;
        //                            double lastWeekHigh = lstWeekData.Max(a => a.High);
        //                            double lastWeekLow = lstWeekData.Min(a => a.Low);
        //                            double lastWeekOpen = lstWeekData.First().Open;
        //                            double weekPivot = (lastWeekclose + lastWeekHigh + lastWeekLow) / 3;
        //                            sd.wPP = Math.Round(weekPivot, 2);
        //                            sd.wR1 = Math.Round((2 * weekPivot) - lastWeekLow, 2);
        //                            sd.wS1 = Math.Round((2 * weekPivot) - lastWeekHigh, 2);
        //                            sd.wR2 = Math.Round(weekPivot + (lastWeekHigh - lastWeekLow), 2);
        //                            sd.wS2 = Math.Round(weekPivot - (lastWeekHigh - lastWeekLow), 2);
        //                            sd.wR3 = Math.Round(weekPivot + 2 * (lastWeekHigh - lastWeekLow), 2);
        //                            sd.wS3 = Math.Round(weekPivot - 2 * (lastWeekHigh - lastWeekLow), 2);
        //                            sd.wClose = lastWeekclose;
        //                            sd.wHigh = lastWeekHigh;
        //                            sd.wLow = lastWeekLow;
        //                            sd.wOpen = lastWeekOpen;
        //                            sd.WeekStartDate = lstWeekData.First().TradingDate;
        //                            sd.WeekEndDate = lstWeekData.Last().TradingDate;

        //                        }
        //                    }
        //                    i++;
        //                }


        //                if (!pivotList.ContainsKey(s))
        //                    pivotList.Add(s, lstStockData);
        //            });



        //            foreach (var dx in pivotList)
        //            {
        //                SerializeObject(dx.Value, @"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\PivotPoints\" + dx.Key + ".xml");
        //            }
        //        }


        //        pList = null;
        //        pList = new List<Pivots>();
        //        if (scripTodaysLevels != null)
        //        {
        //            scripTodaysLevels.Clear();
        //        }
        //        foreach (string s in AllFNO)
        //        {
        //            List<StockData> allLevels = DeSerializeObject<List<StockData>>(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Zerodha\PivotPoints\" + s + ".xml");
        //            StockData todaysLevel = null;
        //            todaysLevel = allLevels.Where(a => a.TradingDate == CurrentTradingDate).First();
        //            scripTodaysLevels.Add(s, todaysLevel);
        //        }
        //        LogStatus("Pivot points are loaded successfully.");
        //        txtSwitchMode.Enabled = true;
        //        isPivotLoaded = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogStatus("Pivot points are loaded successfully.");
        //        txtSwitchMode.Enabled = true;
        //        isPivotLoaded = true;
        //        File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
        //    }
        //}
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

        Dictionary<string, double> myLIst = new Dictionary<string, double>();

        List<VolumeFilter> vList = new List<VolumeFilter>();






        private static System.TimeZoneInfo INDIAN_ZONE = System.TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");






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
                int i = 0;
                XDocument doc = XDocument.Load(@"C:\Jai Sri Thakur Ji\Nifty Analysis\Users\users.xml");
                var Users = doc.Descendants("user");

                foreach (Kite kiteUser in kUsers)
                {
                    string apiSecret = Users.ToList()[i].Descendants("apisecret").First().Value;
                    string requestToken = Users.ToList()[i].Descendants("requestToken").First().Value;
                    User Kuser = kiteUser.GenerateSession(requestToken, apiSecret);
                    ACCESSTOKEN = Kuser.AccessToken;
                    string userAccessToken = Kuser.AccessToken;
                    string userPublicToken = Kuser.PublicToken;
                    kiteUser.SetAccessToken(userAccessToken);
                    kiteUser.SetSessionExpiryHook(() => isLogin = false);
                    isLogin = true;
                    i++;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message); ;
            }

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

        private void RadButton1_Click(object sender, EventArgs e)
        {
            IdeaCreator iObj = new IdeaCreator();
            iObj.Show();

        }

        private void RadProgres_Click(object sender, EventArgs e)
        {

        }

        List<Kite> kUsers = new List<Kite>();
        Dictionary<string, string> userLoginValues = new Dictionary<string, string>();



        private void btnReceipt_Click(object sender, EventArgs e)
        {
            StockSelection s = new StockSelection();
            s.Show();
        }




        DataSet dsSampleTrades = new DataSet();


        private void txtLDF_TextChanged(object sender, EventArgs e)
        {
            //MessageBox.Show(TokenChannel.RoundDown(Convert.ToInt32(txtLDF.Text)).ToString());
            //MessageBox.Show(TokenChannel.RoundUp(Convert.ToInt32(txtLDF.Text)).ToString());
        }

        private void rgvStocks_GroupSummaryEvaluate(object sender, GroupSummaryEvaluationEventArgs e)
        {

        }

        private void BtnExports_Click(object sender, EventArgs e)
        {
            List<Model.PNL> pnlObj = rgvStocks.DataSource as List<Model.PNL>;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Monthly");
            foreach (var p in pnlObj.GroupBy(a => new { a.Date.Month, a.Date.Year }))
            {
                sb.AppendLine(p.Key.Month + "/" + p.Key.Year + " : " + p.Sum(b => b.Amount));
            }

            sb.AppendLine("Daily");
            foreach (var p in pnlObj.GroupBy(a => new { a.Date }))
            {
                sb.AppendLine(p.Key.Date + " : " + p.Sum(b => b.Amount));
            }
            sb.AppendLine("Summary");

            sb.AppendLine("Total Trades : " + pnlObj.Count());
            sb.AppendLine("Profitable Trades : " + pnlObj.Count(b => b.Amount > 0));
            sb.AppendLine("Negative Trades : " + pnlObj.Count(b => b.Amount < 0));
            sb.AppendLine("Total Profit : " + pnlObj.Sum(b => b.Amount));
            sb.AppendLine("Total No of Days : " + pnlObj.GroupBy(b => b.Date).Count());
            sb.AppendLine("Max Profit : " + pnlObj.GroupBy(b => b.Date).Max(a => a.Sum(c => c.Amount)));
            sb.AppendLine("Max Loss : " + pnlObj.GroupBy(b => b.Date).Min(a => a.Sum(c => c.Amount)));
            sb.AppendLine("Avg Profit : " + pnlObj.Average(b => b.Amount));
            sb.AppendLine("Max Trade a Single Day : " + pnlObj.GroupBy(b => b.Date).Max(a => a.Count()));

            File.WriteAllText(@"C:\Jai Sri Thakur Ji\Summary.txt", sb.ToString());

        }
        ConcurrentBag<List<Model.PNL>> consolidated = new ConcurrentBag<List<Model.PNL>>();
        private void RadButton2_Click_1(object sender, EventArgs e)
        {
            // ConcurrentBag<List<Model.PNL>> consolidated = new ConcurrentBag<List<Model.PNL>>();
            try
            {
                Parallel.ForEach((radDropDownList1.Items), (x) =>
                {
                    ProgressDelegate myProgres = ShowMyProgress;
                    List<Model.Idea> myideas = Common.GetIdeas();

                    Model.Idea selectedIdea = myideas.Where(a => a.Name == x.Text).First();
                    StockOHLC stockOHLC = new StockOHLC();
                    int year = 2019;
                    //for (int year = 2015; year <= 2019; year++)
                    //{
                    //Load Data
                    Task<Dictionary<string, List<Model.Candle>>> loadmydata = Task.Run<Dictionary<string, List<Model.Candle>>>(() => stockOHLC.GetOHLC(new DateTime(year, Convert.ToInt32(ddlStartMonth.SelectedItem.Text), Convert.ToInt32(ddlStartDate.SelectedItem.Text)), new DateTime(year + 1, Convert.ToInt32(ddlEndMonth.SelectedItem.Text), Convert.ToInt32(ddlEndDate.SelectedItem.Text)), selectedIdea.Interval, myProgres));

                    //Apply indicators

                    loadmydata.ContinueWith((t0) =>
                    {
                        SetText("Applying indicators");
                        Task<Dictionary<string, List<Model.Candle>>> withIndicators = Task.Run<Dictionary<string, List<Model.Candle>>>(() => TechnicalIndicators.AddIndicators(t0.Result, selectedIdea.TI));
                        Task getTradingStocks = withIndicators.ContinueWith((t1) =>
                        {

                            Task<Dictionary<Guid, Model.StrategyModel>> getTradedStocks = Task.Run<Dictionary<Guid, Model.StrategyModel>>(() => stockOHLC.GetTopMostSolidGapOpenerDayWise(t1.Result, selectedIdea, myProgres));
                            Task tradeMyStocks = getTradedStocks.ContinueWith((t2) =>
                            {
                                Task<List<Model.PNL>> calculation = Task<List<Model.PNL>>.Run(() => stockOHLC.TradeStocks(t2.Result, t1.Result, selectedIdea, myProgres));
                                calculation.ContinueWith((t3) =>
                                {
                                    consolidated.Add(t3.Result);
                                    //SetDataSource(t3.Result);
                                    //SetText("Idea ran successfully");
                                });
                            });
                        });
                    });
                    //}
                });
                //while(consolidated.Count()< radDropDownList1.Items.Count)
                //{
                //    Thread.Sleep(10000);
                //}

            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", Environment.NewLine);
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.Message);
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.StackTrace);
                File.AppendAllText(@"C:\Jai Sri Thakur Ji\Nifty Analysis\errors.txt", System.Reflection.MethodBase.GetCurrentMethod() + " :- " + ex.InnerException);
            }
        }

        private void RadButton3_Click(object sender, EventArgs e)
        {

        }

        private void rgvStocks_CellDoubleClick(object sender, GridViewCellEventArgs e)
        {
            List<Candle> cd = (rgvStocks.DataSource as List<Model.PNL>)[e.RowIndex].ChartData;
            double low = cd.Min(a => a.Low);
            //jsfidler
            StringBuilder cdTransformation = new StringBuilder();
            foreach (var c in cd)
            {
                if (c.CandleType == "G")
                {
                    cdTransformation.Append("[");
                    cdTransformation.Append("'");
                    cdTransformation.Append(c.TimeStamp);
                    cdTransformation.Append("'");
                    cdTransformation.Append(",");
                    if (c.AllIndicators.SuperTrend != null)
                        cdTransformation.Append(c.AllIndicators.SuperTrend.SuperTrendValue);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators.SMA20 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA20);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators.SMA50 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA50);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators.SMA200 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA200);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    cdTransformation.Append(c.Low);
                    cdTransformation.Append(",");
                    cdTransformation.Append(c.Open);
                    cdTransformation.Append(",");
                    cdTransformation.Append(c.Close);
                    cdTransformation.Append(",");
                    cdTransformation.Append(c.High);
                    cdTransformation.Append("]");
                    cdTransformation.Append(",");
                }
                else
                {
                    cdTransformation.Append("[");
                    cdTransformation.Append("'");
                    cdTransformation.Append(c.TimeStamp);
                    cdTransformation.Append("'");
                    cdTransformation.Append(",");
                    if (c.AllIndicators.SuperTrend != null)
                        cdTransformation.Append(c.AllIndicators.SuperTrend.SuperTrendValue);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators.SMA20 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA20);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators.SMA50 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA50);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators.SMA200 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA200);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    cdTransformation.Append(c.High);
                    cdTransformation.Append(",");
                    cdTransformation.Append(c.Open);
                    cdTransformation.Append(",");
                    cdTransformation.Append(c.Close);
                    cdTransformation.Append(",");
                    cdTransformation.Append(c.Low);
                    cdTransformation.Append("]");
                    cdTransformation.Append(",");
                }
            }

            string s = File.ReadAllText(@"C:\Jai Sri Thakur Ji\Chart.html");
            s = s.Replace("__chartdata", cdTransformation.ToString());
            string fileName = Guid.NewGuid().ToString();

            File.WriteAllText(@"C:\Jai Sri Thakur Ji\" + fileName + ".html", s);
            System.Diagnostics.Process.Start(@"file:///C:/Jai%20Sri%20Thakur%20Ji/" + fileName + ".html");
        }

        private void radButton4_Click(object sender, EventArgs e)
        {
            rgvStocks.DataSource = null;
        }
    }
}
