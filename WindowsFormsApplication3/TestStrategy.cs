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

        int psa = 0;
        int sma = 0;
        bool DONT_DELETE = false;
        bool takeBackupOfFiles = false;
        public TestStrategy(Dictionary<string, string> setting)
        {
            mySettings = setting;
            InitializeComponent();
            psa = Convert.ToInt16(mySettings["PSA"]);
            sma = Convert.ToInt16(mySettings["SMA"]);
            DONT_DELETE = Convert.ToBoolean(mySettings["DoNotRemove"]);
            takeBackupOfFiles = Convert.ToBoolean(mySettings["TakeBackupOfFilesAfterPlacingOrders"]);
        }
        List<Pivots> pList = new List<Pivots>();
        Dictionary<string, string> mySettings = new Dictionary<string, string>();
        private void radGroupBox1_Click(object sender, EventArgs e)
        {

        }

        bool backLiveTest = false;

        //bool calculateBrokerage = Convert.ToBoolean(mySettings["Brokerage"]);
        //int sma = Convert.ToInt16(mySettings["SMA"]);

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

        delegate void SetDataSourceCallback(List<Model.PNL> outcome, Idea selectedIdea);
        private void SetDataSource(List<Model.PNL> outcome, Idea selectedIdea)
        {

            List<Model.PNL> x = new List<Model.PNL>();
            int cc = 0;
            foreach (var b in outcome.OrderBy(c => c.Date))
            {
                //x.Add(b);
                //continue;
                if (cc >= selectedIdea.TryAfterContinuosError)
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
            x = outcome;
            //if (rgvStocks.DataSource as List<PNL> != null && (rgvStocks.DataSource as List<PNL>).Count() > 0 && outcome.Count > 0)
            //{
            //    PNL[] cc2 = new PNL[rgvStocks.Rows.Count];
            //    (rgvStocks.DataSource as List<PNL>).CopyTo(cc2);
            //    var cc1 = cc2.ToList();
            //    for (int i = 0; i < cc2.Count(); i++)
            //    {
            //        var d = cc2[i];
            //        if (outcome.Where(a => a.Stock == d.Stock).Count() == 0)
            //        {
            //            cc1.Remove(d);
            //        }

            //    }
            //    if (cc1.Count > 0)
            //    {
            //        x = cc1;
            //    }
            //    else
            //    {
            //        x = rgvStocks.DataSource as List<PNL>;
            //    }
            //    // outcome = x;
            //}

            if (this.rgvStocks.InvokeRequired)
            {
                SetDataSourceCallback d = new SetDataSourceCallback(Lund);
                // this.Invoke(d, new object[] { null, selectedIdea });
                this.Invoke(d, new object[] { x, selectedIdea });
            }
            else
            {

                this.rgvStocks.DataSource = x;
            }

            foreach (var x1 in this.rgvStocks.Rows)
            {
                x1.Cells[0].ReadOnly = true;
            }
        }

        public void Lund(List<PNL> final, Idea selectedIda)
        {
            this.rgvStocks.DataSource = null;
            this.rgvStocks.DataSource = final;
        }


        DataTable finalList = new DataTable();
        static List<CustomizedPNL> l = new List<CustomizedPNL>();

        private void btnLoad_Click(object sender, EventArgs e)
        {
            l.Clear();
            //rgvStocks.DataSource = null;
            try
            {
                ProgressDelegate myProgres = ShowMyProgress;
                List<Model.Idea> myideas = null;
                if (checkBox1.Checked)
                {
                    myideas = Common.GetIdeas().OrderBy(a => a.runOrder).ToList();
                }
                else
                {
                    myideas = Common.GetIdeas().Where(a => a.Name == radDropDownList1.SelectedItem.Text).ToList();
                }

                List<Task> allTask = new List<Task>();
                //Model.Idea selectedIdea = myideas.Where(a => a.Name == radDropDownList1.SelectedItem.Text).First();
                foreach (var selectedIdea in myideas.OrderBy(a => a.runOrder))
                {
                    if (selectedIdea.Name == "Dual_Time_Frame_Momentum")
                    {
                        StockOHLC stockOHLC = new StockOHLC();

                        //Load Data
                        Task<Dictionary<string, List<Model.Candle>>> loadmydata = Task.Run<Dictionary<string, List<Model.Candle>>>(() => stockOHLC.GetOHLC(new DateTime(Convert.ToInt32(ddlStartYear.SelectedItem.Text), Convert.ToInt32(ddlStartMonth.SelectedItem.Text), Convert.ToInt32(ddlStartDate.SelectedItem.Text)), new DateTime(Convert.ToInt32(ddlEndYear.SelectedItem.Text), Convert.ToInt32(ddlEndMonth.SelectedItem.Text), Convert.ToInt32(ddlEndDate.SelectedItem.Text)), selectedIdea.Interval, myProgres));
                        allTask.Add(loadmydata);
                        loadmydata.ContinueWith((t0) =>
                        {
                            SetText("Applying stochastic indicators to small timeframe");
                            Task<Dictionary<string, List<Model.Candle>>> withIndicators = Task.Run<Dictionary<string, List<Model.Candle>>>(() => TechnicalIndicators.AddIndicators(t0.Result, selectedIdea.TI, new DateTime(Convert.ToInt32(ddlStartYear.SelectedItem.Text), Convert.ToInt32(ddlStartMonth.SelectedItem.Text), Convert.ToInt32(ddlStartDate.SelectedItem.Text)), new DateTime(Convert.ToInt32(ddlEndYear.SelectedItem.Text), Convert.ToInt32(ddlEndMonth.SelectedItem.Text), Convert.ToInt32(ddlEndDate.SelectedItem.Text))));
                            withIndicators.ContinueWith((t1) =>
                            {
                                Task<Dictionary<string, List<Model.Candle>>> loadmydata2 = Task.Run<Dictionary<string, List<Model.Candle>>>(() => stockOHLC.GetOHLC(new DateTime(Convert.ToInt32(ddlStartYear.SelectedItem.Text), Convert.ToInt32(ddlStartMonth.SelectedItem.Text), Convert.ToInt32(ddlStartDate.SelectedItem.Text)), new DateTime(Convert.ToInt32(ddlEndYear.SelectedItem.Text), Convert.ToInt32(ddlEndMonth.SelectedItem.Text), Convert.ToInt32(ddlEndDate.SelectedItem.Text)), selectedIdea.Interval2, myProgres));
                                allTask.Add(loadmydata2);
                                loadmydata2.ContinueWith((t2) =>
                                {
                                    SetText("Applying stochastic indicators to large time frame");
                                    Task<Dictionary<string, List<Model.Candle>>> withIndicators2 = Task.Run<Dictionary<string, List<Model.Candle>>>(() => TechnicalIndicators.AddIndicators(t2.Result, selectedIdea.TI, new DateTime(Convert.ToInt32(ddlStartYear.SelectedItem.Text), Convert.ToInt32(ddlStartMonth.SelectedItem.Text), Convert.ToInt32(ddlStartDate.SelectedItem.Text)), new DateTime(Convert.ToInt32(ddlEndYear.SelectedItem.Text), Convert.ToInt32(ddlEndMonth.SelectedItem.Text), Convert.ToInt32(ddlEndDate.SelectedItem.Text))));
                                    allTask.Add(withIndicators2);

                                    withIndicators2.ContinueWith((t3) =>
                                    {

                                        Task<Dictionary<Guid, Model.StrategyModel>> getTradedStocks = Task.Run<Dictionary<Guid, Model.StrategyModel>>(() => stockOHLC.ApplyDualMomentumStrategyModel(t1.Result, t3.Result, selectedIdea, myProgres));
                                        allTask.Add(getTradedStocks);
                                        Task tradeMyStocks = getTradedStocks.ContinueWith((t4) =>
                                        {

                                            var stocksList = t4.Result;
                                            List<PNL> p = new List<PNL>();
                                            foreach (var s in stocksList)
                                            {

                                                p.Add(new PNL { Stock = s.Value.Stock, Date = s.Value.Date, Direction = Enum.GetName(typeof(Model.Trade), s.Value.Trade) });
                                            }
                                            l.Add(new CustomizedPNL { order = selectedIdea.runOrder, selectedIdea = selectedIdea, Strategyoutput = p });


                                        });




                                    });

                                });
                            });

                        });
                    }
                    else
                    {
                        StockOHLC stockOHLC = new StockOHLC();

                        //Load Data
                        Task<Dictionary<string, List<Model.Candle>>> loadmydata = Task.Run<Dictionary<string, List<Model.Candle>>>(() => stockOHLC.GetOHLC(new DateTime(Convert.ToInt32(ddlStartYear.SelectedItem.Text), Convert.ToInt32(ddlStartMonth.SelectedItem.Text), Convert.ToInt32(ddlStartDate.SelectedItem.Text)), new DateTime(Convert.ToInt32(ddlEndYear.SelectedItem.Text), Convert.ToInt32(ddlEndMonth.SelectedItem.Text), Convert.ToInt32(ddlEndDate.SelectedItem.Text)), selectedIdea.Interval, myProgres));
                        allTask.Add(loadmydata);
                        //loadmydata.Wait();
                        //Apply indicators
                        loadmydata.ContinueWith((t0) =>
                        {
                            SetText("Applying indicators");
                            Task<Dictionary<string, List<Model.Candle>>> withIndicators = Task.Run<Dictionary<string, List<Model.Candle>>>(() => TechnicalIndicators.AddIndicators(t0.Result, selectedIdea.TI, new DateTime(Convert.ToInt32(ddlStartYear.SelectedItem.Text), Convert.ToInt32(ddlStartMonth.SelectedItem.Text), Convert.ToInt32(ddlStartDate.SelectedItem.Text)), new DateTime(Convert.ToInt32(ddlEndYear.SelectedItem.Text), Convert.ToInt32(ddlEndMonth.SelectedItem.Text), Convert.ToInt32(ddlEndDate.SelectedItem.Text))));
                            allTask.Add(withIndicators);
                            Task getTradingStocks = withIndicators.ContinueWith((t1) =>
                            {
                                Task<Dictionary<Guid, Model.StrategyModel>> getTradedStocks = Task.Run<Dictionary<Guid, Model.StrategyModel>>(() => stockOHLC.GetTopMostSolidGapOpenerDayWise(t1.Result, selectedIdea, myProgres));
                                allTask.Add(getTradedStocks);
                                Task tradeMyStocks = getTradedStocks.ContinueWith((t2) =>
                                {
                                    Task<List<Model.PNL>> calculation = Task<List<Model.PNL>>.Run(() => stockOHLC.TradeStocks(t2.Result, t1.Result, selectedIdea, myProgres));
                                    allTask.Add(getTradedStocks);
                                    calculation.ContinueWith((t3) =>
                                    {
                                        l.Add(new CustomizedPNL { order = selectedIdea.runOrder, selectedIdea = selectedIdea, Strategyoutput = t3.Result });

                                    });
                                //calculation.Wait();
                            });
                            });
                        });


                        //loadmydata.Wait();
                        //Task.WaitAll();
                    }
                    Task.Factory.ContinueWhenAll(allTask.ToArray(), FinalWork);
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


        public void FinalWork(System.Threading.Tasks.Task[] allTask)
        {
            rgvStocks.Columns.Clear();
            rgvStocks.AutoGenerateColumns = true;
            while (allTask.Count() != l.Count())
            {
                Thread.Sleep(1000);
            }
            List<PNL> xtraLarge = new List<PNL>();
            if (allTask.All(t => t.Status == TaskStatus.RanToCompletion))
            {

                //foreach (var c in l.OrderBy(a => a.order))
                //{
                //    xtraLarge.AddRange(c.Strategyoutput);

                //}
                //var maxOccurrence = xtraLarge.GroupBy(a => a.Stock).Select(a => new { zz = a.Key, zzz = a.Count() }).OrderByDescending(a => a.zzz).First();
                ////var maxCashFlow = xtraLarge.GroupBy(a => a.Stock).
                //var allWithmaxOccurrence = xtraLarge.GroupBy(a => a.Stock).Where(a => a.Count() == maxOccurrence.zzz).Select(b => b.Key).ToList();
                //var xyz = xtraLarge.Where(a => allWithmaxOccurrence.Contains(a.Stock)).ToList();
                //xyz = xyz.Where(a => a.ChartData.Count() == xyz.Min(b => b.ChartData.Count())) .ToList();
                SetDataSource(l.First().Strategyoutput, l.First().selectedIdea);
                SetText("Idea ran successfully");
                l.Clear();

                // do "some work"
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
            foreach (var p in pnlObj.GroupBy(a => new { a.Date.Date }))
            {
                sb.AppendLine(p.Key.Date + " : " + p.Sum(b => b.Amount));
            }
            sb.AppendLine("Summary");

            sb.AppendLine("Total Trades : " + pnlObj.Count());
            sb.AppendLine("Profitable Trades : " + pnlObj.Count(b => b.Amount > 0));
            sb.AppendLine("Negative Trades : " + pnlObj.Count(b => b.Amount < 0));
            sb.AppendLine("Total Profit : " + pnlObj.Sum(b => b.Amount));
            sb.AppendLine("Total No of Days : " + pnlObj.GroupBy(b => b.Date.Date).Count());
            sb.AppendLine("Max Profit : " + pnlObj.GroupBy(b => b.Date.Date).Max(a => a.Sum(c => c.Amount)));
            sb.AppendLine("Max Loss : " + pnlObj.GroupBy(b => b.Date.Date).Min(a => a.Sum(c => c.Amount)));
            sb.AppendLine("Avg Profit : " + pnlObj.Sum(b => b.Amount) / pnlObj.GroupBy(a => a.Date.Date).Count());
            sb.AppendLine("Max Trade a Single Day : " + pnlObj.GroupBy(b => b.Date.Date).Max(a => a.Count()));
            sb.AppendLine("Max Turnover a Single Trade : " + pnlObj.Max(a => a.Quantity * a.Stoploss));
            sb.AppendLine("Total Losing Day : " + pnlObj.GroupBy(b => b.Date.Date).Count(a => a.Sum(c => c.Amount) <= 0));
            sb.AppendLine("Total Winning Day : " + pnlObj.GroupBy(b => b.Date.Date).Count(a => a.Sum(c => c.Amount) > 0));

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
                        Task<Dictionary<string, List<Model.Candle>>> withIndicators = Task.Run<Dictionary<string, List<Model.Candle>>>(() => TechnicalIndicators.AddIndicators(t0.Result, selectedIdea.TI, new DateTime(year, Convert.ToInt32(ddlStartMonth.SelectedItem.Text), Convert.ToInt32(ddlStartDate.SelectedItem.Text)), new DateTime(Convert.ToInt32(ddlEndYear.SelectedItem.Text), Convert.ToInt32(ddlEndMonth.SelectedItem.Text), Convert.ToInt32(ddlEndDate.SelectedItem.Text))));
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

                    if (c.AllIndicators != null)

                        if (c.AllIndicators != null && c.AllIndicators.SuperTrend != null)
                            cdTransformation.Append(c.AllIndicators.SuperTrend.SuperTrendValue);
                        else
                            cdTransformation.Append(low);

                    cdTransformation.Append(",");
                    if (c.AllIndicators != null && c.AllIndicators.SMA20 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA20);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators != null && c.AllIndicators.SMA50 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA50);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators != null && c.AllIndicators.SMA200 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA200);
                    else
                        cdTransformation.Append(low);

                    cdTransformation.Append(",");

                    if (c.AllIndicators != null && c.AllIndicators.BollingerBand.Upper > 0)
                        cdTransformation.Append(c.AllIndicators.BollingerBand.Upper);
                    else
                        cdTransformation.Append(c.Low);

                    cdTransformation.Append(",");

                    if (c.AllIndicators != null && c.AllIndicators.BollingerBand.Lower > 0)
                        cdTransformation.Append(c.AllIndicators.BollingerBand.Lower);
                    else
                        cdTransformation.Append(c.Low);

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
                    if (c.AllIndicators != null && c.AllIndicators.SuperTrend != null)
                        cdTransformation.Append(c.AllIndicators.SuperTrend.SuperTrendValue);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators != null && c.AllIndicators.SMA20 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA20);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators != null && c.AllIndicators.SMA50 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA50);
                    else
                        cdTransformation.Append(low);
                    cdTransformation.Append(",");
                    if (c.AllIndicators != null && c.AllIndicators.SMA200 > 0)
                        cdTransformation.Append(c.AllIndicators.SMA200);
                    else
                        cdTransformation.Append(low);

                    cdTransformation.Append(",");

                    if (c.AllIndicators != null && c.AllIndicators.BollingerBand.Upper > 0)
                        cdTransformation.Append(c.AllIndicators.BollingerBand.Upper);
                    else
                        cdTransformation.Append(c.Low);

                    cdTransformation.Append(",");

                    if (c.AllIndicators != null && c.AllIndicators.BollingerBand.Lower > 0)
                        cdTransformation.Append(c.AllIndicators.BollingerBand.Lower);
                    else
                        cdTransformation.Append(c.Low);

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
