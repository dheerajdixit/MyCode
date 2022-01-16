using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CommonFeatures
{
    
    public class Common
    {
        public static string stockFileName = @"StaticData\Stocks.json";
        public static string eqFileName = @"StaticData\AllEQ.json";
        public static string settingFileName = @"StaticData\Settings.json";
        public static string strategyFileName = @"StaticData\Strategy.json";

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

