using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Model
{
  
    public class Indicator
    {
        public string IndicatorName { get; set; }

        public List<double> numericParametersCollection { get; set; }

        public List<string> textParametersCollection { get; set; }
    }
}

