using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zerodha
{
    public class ZerodhaOperations
    {
        public static Dictionary<string, string> LoadInstruments()
        {
            DataSet instrToken = new DataSet();
            Dictionary<string, string> instrumentNamebyKey = new Dictionary<string, string>();
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

                var appliedStocks = CommonFeatures.Common.GetStocks();
                foreach (DataRow dr in instrToken.Tables[0].Rows)
                {

                    if (dr["exchange"].ToString() == "NSE" && dr["instrument_type"].ToString() == "EQ" && appliedStocks.Where(a => a.StockName == Convert.ToString(dr["tradingsymbol"]) && a.Use).Count() > 0)
                        instrumentNamebyKey.Add(dr["instrument_token"].ToString(), Convert.ToString(dr["tradingsymbol"]));

                }

            }

            return instrumentNamebyKey;

        }
    }
}
