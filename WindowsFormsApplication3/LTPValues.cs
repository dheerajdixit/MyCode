using System;

namespace NSA
{
    public class LTPValues
    {
        public double trailingStopLoss
        {
            get;
            set;
        }

        public double LtpClose
        {
            get;
            set;
        }
        public DateTime TimeStamp
        { get; set; }

        public double LtpHigh
        {
            get;
            set;
        }

        public double LtpLow
        {
            get;
            set;
        }

        public double LtpOpen
        {
            get;
            set;
        }

        public bool IsExit
        {
            get;
            set;
        }

        public double PNL
        {
            get;
            set;
        }

        public double HighestPNL
        {
            get;
            set;
        }

        public int ExitCandle
        {
            get;
            set;
        }

        public string ExitLevels
        {
            get;
            set;
        }
    }
}
