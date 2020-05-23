using System;
using System.Runtime.CompilerServices;

namespace Model
{
   
    public class StockInventory
    {
        public string StockName { get; set; }

        public int IsSelected
        {
            get
            {
                if (this.Use)
                    return 1;
                else
                    return 0;
            }
            
           
        }

        public bool Use { get; set; }
    }
}

