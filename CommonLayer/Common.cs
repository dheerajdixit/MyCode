using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CommonFeatures
{
    
    public class Common
    {
        public static string stockFileName = @"C:\Users\dheeraj.dixit\Downloads\NSA-master\NSA-master\WindowsFormsApplication3\StaticData\Stocks.json";
        public static string eqFileName = @"C:\Users\dheeraj.dixit\Downloads\NSA-master\NSA-master\WindowsFormsApplication3\StaticData\AllEQ.json";
        public static string settingFileName = @"C:\Users\dheeraj.dixit\Downloads\NSA-master\NSA-master\WindowsFormsApplication3\StaticData\Settings.json";
        public static string strategyFileName = @"C:\Users\dheeraj.dixit\Downloads\NSA-master\NSA-master\WindowsFormsApplication3\StaticData\Strategy.json";

        public static List<StockInventory> GetEQStocks()
        {
            using (StreamReader reader = new StreamReader(eqFileName))
            {
                return JsonConvert.DeserializeObject<List<StockInventory>>(reader.ReadToEnd());
            }
        }

        public static List<Idea> GetIdeas()
        {
            using (StreamReader reader = new StreamReader(strategyFileName))
            {
                return JsonConvert.DeserializeObject<List<Idea>>(reader.ReadToEnd());
            }
        }

        public static Settings GetSettings()
        {
            using (StreamReader reader = new StreamReader(settingFileName))
            {
                return JsonConvert.DeserializeObject<Settings>(reader.ReadToEnd());
            }
        }

        public static List<StockInventory> GetStocks()
        {
            using (StreamReader reader = new StreamReader(stockFileName))
            {
                return JsonConvert.DeserializeObject<List<StockInventory>>(reader.ReadToEnd());
            }
        }
    }
}

