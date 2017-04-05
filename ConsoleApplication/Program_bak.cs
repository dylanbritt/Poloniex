using ConsoleApplication.Helper;
using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Domain.Models;
using Poloniex.Core.Implementation;
using Poloniex.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication
{
    class Program_bak
    {
        static void Main_bak(string[] args)
        {
            /* inclusive latest date */

            Console.WriteLine($"Started: {DateTime.Now}");
            var dt = DateTime.UtcNow;
            // trim to start of month (begin)
            dt = dt.AddMilliseconds(-dt.Millisecond);
            dt = dt.AddSeconds(-dt.Second);
            dt = dt.AddMinutes(-dt.Minute);
            dt = dt.AddHours(-dt.Hour);
            dt = dt.AddDays(-dt.Day + 1);
            // trim to start of month (end)

            var quarterDaysToGoBack = 4 * 30;

            var shorterInterval = 12;
            var longerInterval = 26;

            var minutesPerInterval = 60;

            // reverseSignal
            //var reverseTmp = shorterInterval;
            //shorterInterval = longerInterval;
            //longerInterval = reverseTmp;

            var currencyPair = CurrencyPairConstants.USDT_BTC;

            // ################################################################

            var secondsBack = quarterDaysToGoBack * 21600;
            quarterDaysToGoBack++;

            // ################################################################

            var window = 30d;
            var shifter = 365d / (30 / 2d);
            var numberOfTimesToShift = 365d / shifter;

            DateTime startDateTime = dt.AddSeconds(1);
            DateTime endDateTime = dt.AddSeconds(-secondsBack).AddSeconds(1);

            bool backFill = false;

            //backFill = true;

            if (backFill)
            {
                //GatherTaskManager.BackFillGatherTaskData(quarterDaysToGoBack, currencyPair, dt, DateTime.Parse("1970-01-01 00:00:00.000"));

                MovingAverageManager.BackFillEma(currencyPair, shorterInterval, minutesPerInterval, dt, dt.AddSeconds(-secondsBack), null);
                MovingAverageManager.BackFillEma(currencyPair, longerInterval, minutesPerInterval, dt, dt.AddSeconds(-secondsBack), null);
            }

            // ################################################################

            /* time adjustment for bounds 
             upper bound is inclusive,
             lower bound is exclusive
             */
            endDateTime = endDateTime.AddSeconds(1);
            startDateTime = startDateTime.AddSeconds(-1);

            List<MovingAverage> shorterMovingAverages;
            List<MovingAverage> longerMovingAverages;
            using (var db = new PoloniexContext())
            {
                shorterMovingAverages = db.MovingAverages
                    .Where(x =>
                        x.Interval == shorterInterval &&
                        x.CurrencyPair == currencyPair &&
                        x.ClosingDateTime >= endDateTime &&
                        x.ClosingDateTime <= startDateTime)
                    .OrderBy(x => x.ClosingDateTime)
                .ToList();

                longerMovingAverages = db.MovingAverages
                    .Where(x =>
                        x.Interval == longerInterval &&
                        x.CurrencyPair == currencyPair &&
                        x.ClosingDateTime >= endDateTime &&
                        x.ClosingDateTime <= startDateTime)
                    .OrderBy(x => x.ClosingDateTime)
                .ToList();
            }

            //ProfitAnalyzer.MacdEma = shorterMovingAverages[0].MovingAverageValue - longerMovingAverages[0].MovingAverageValue;
            for (int i = 0; i < shorterMovingAverages.Count; i++)
            {
                //ProfitAnalyzer.ProcessMacdMovingAverageSignals(shorterMovingAverages[i].MovingAverageValue, longerMovingAverages[i].MovingAverageValue, shorterMovingAverages[i].LastClosingValue, shorterMovingAverages[i].ClosingDateTime);
                ProfitAnalyzer.Process(shorterMovingAverages[i].MovingAverageValue, longerMovingAverages[i].MovingAverageValue, shorterMovingAverages[i].LastClosingValue, shorterMovingAverages[i].ClosingDateTime);
            }

            ProfitAnalyzer.CalculateProfit(500);
            Console.WriteLine($"Data-Min: {ProfitAnalyzer.Data_GetMin()}");
            Console.WriteLine($"Data-Max: {ProfitAnalyzer.Data_GetMax()}");
            Console.WriteLine($"Data-Mean: {ProfitAnalyzer.Data_GetMean()}");
            Console.WriteLine($"Data-Median: {ProfitAnalyzer.Data_GetMedian()}");
            Console.WriteLine($"Data-Variance: {ProfitAnalyzer.Data_GetVariance()}");
            Console.WriteLine($"Data-Std: {Math.Sqrt((double)ProfitAnalyzer.Data_GetVariance())}");
            Console.WriteLine($"Profit-Min: {ProfitAnalyzer.Profit_GetMin()}");
            Console.WriteLine($"Profit-Max: {ProfitAnalyzer.Profit_GetMax()}");
            Console.WriteLine($"Profit-Mean: {ProfitAnalyzer.Profit_GetMean()}");
            Console.WriteLine($"Profit-Median: {ProfitAnalyzer.Profit_GetMedian()}");
            Console.WriteLine($"Profit-Variance: {ProfitAnalyzer.Profit_GetVariance()}");
            Console.WriteLine($"Profit-Std: {Math.Sqrt((double)ProfitAnalyzer.Profit_GetVariance())}");

            Console.WriteLine($"Complete: {DateTime.Now}");
            Console.ReadLine();
        }
    }
}
