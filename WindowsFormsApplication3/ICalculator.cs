using System;
using System.Data;

namespace NSA
{
    public interface ICalculator
    {
        bool CalculateEntryIndicators(string scrip, string direction, out int entryCandle, out double entryPrice, ref bool isBreakOut, out double breakOutHigh, out double breakOutLow, int testAtMinute, int backTestDay, int goTimeCount);

        bool CalculateExitIndicators(string scrip, string direction, int startCandle, out int exitCandle, out double entryPrice, out double exitPrice, int lotSize, double entry, double bHigh, double bLow, int testAtMinute, int backTestDay, bool backTestStatus, int goTimeCount);

        DataTable ScanStocks(int testAtMinute, int backTestDay, int goTimeCount);

        DataTable ScanStocksSuperTrend(int testAtMinute, int backTestDay, int goTimeCount);
    }
}
