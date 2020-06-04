using BAL;
using CommonFeatures;
using KiteConnect;
using Model;
using Newtonsoft.Json;
using NSA;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Telerik.WinControls.UI;

namespace _15MCE
{

    public partial class IdeaCreator : Telerik.WinControls.UI.RadForm
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
        List<CIndexes> listIndex = new List<CIndexes>();
        public IdeaCreator()
        {
            InitializeComponent();
        }
        List<Pivots> pList = new List<Pivots>();
        private void radGroupBox1_Click(object sender, EventArgs e)
        {

        }

        bool backLiveTest = false;
    
        //bool calculateBrokerage = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["Brokerage"]);
        //int sma = Convert.ToInt16(System.Configuration.ConfigurationSettings.AppSettings["SMA"]);

        DataSet instrToken = new DataSet();
     

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
            radDropDownList1.SelectedIndex = 0;
            radCandType.SelectedIndex = 1;
            ddlRange.SelectedIndex = 1;
            ddlSorting.SelectedIndex = 0;
            ddlOrder.SelectedIndex = 0;
            ddlBookProfit.SelectedIndex = 3;


        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            List<Model.Idea> ideas = Common.GetIdeas();

            List<Model.Technical> ti = new List<Model.Technical>();
            foreach (var item in radCheckedListBox1.Items)
            {
                if (item.CheckState == Telerik.WinControls.Enumerations.ToggleState.On)
                {
                    switch (item.Text)
                    {
                        case "SuperTrend":
                            ti.Add(Model.Technical.SuperTrend);
                            break;
                        case "Simple Moving Average":
                            ti.Add(Model.Technical.SimpleMovingAverage);
                            break;
                        case "MACD":
                            ti.Add(Model.Technical.MACD);
                            break;
                    }
                }
            }
            Model.Idea newIdea = new Model.Idea
            {
                Interval = Convert.ToInt32(txtInterval.Text),
                Name = txtIdeaName.Text,
                Stoploss = radDropDownList1.SelectedIndex,
                TI = ti,
                EntryStartCandle = Convert.ToInt16(txtEntryStartCandle.Text),
                EntryFinishCandle = Convert.ToInt16(txtEntryFinishCandle.Text),
                TradePerSession = Convert.ToInt16(txtTradePerSession.Text),
                FilterByVolume = Convert.ToInt16(txtFilterByVolume.Text),
                CandleType = (Model.CandleType)radCandType.SelectedIndex,
                Range = (Model.Range)ddlRange.SelectedIndex,
                Sorting = (Model.Sorting)ddlSorting.SelectedIndex,
                OrderMultiples = (Model.OrderMultiples)ddlOrder.SelectedIndex,
                Risk = Convert.ToDouble(txtRisk.Text)
            };

            if (ideas.Where(a => a.Name == newIdea.Name).Count() == 0)
                ideas.Add(newIdea);

            string json = JsonConvert.SerializeObject(ideas.ToArray());
            File.WriteAllText(Common.strategyFileName, json);

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

        Dictionary<string, List<StockData>> pivotList = new Dictionary<string, List<StockData>>();

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

        private void RadLabel1_Click(object sender, EventArgs e)
        {

        }

        List<Kite> kUsers = new List<Kite>();
        Dictionary<string, string> userLoginValues = new Dictionary<string, string>();


        Dictionary<string, StockData> scripTodaysLevels = new Dictionary<string, StockData>();

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
            if (e.SummaryItem.Name == "total")
            {


            }

        }


    }


}
