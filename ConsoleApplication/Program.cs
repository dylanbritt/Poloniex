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
    class Program
    {
        static void Main(string[] args)
        {
            /* inclusive latest date */

            Console.WriteLine($"Started: {DateTime.Now}");
            var dt = DateTime.UtcNow;
            // trim to start of month (begin)
            //dt = dt.AddDays(-dt.Day).AddHours(dt.Hour);
            // trim to start of month (end)
            dt = dt.AddSeconds(-dt.Second).AddMilliseconds(-dt.Millisecond);

            var quarterDaysToGoBack = 4 * 30;

            var shorterInterval = 12 * 60;
            var longerInterval = 26 * 60;

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

            backFill = true;

            if (backFill)
            {
                GatherTaskManager.BackFillGatherTaskData(quarterDaysToGoBack, currencyPair, dt, DateTime.Parse("1970-01-01 00:00:00.000"));

                MovingAverageManager.BackFillEma(currencyPair, shorterInterval, dt, dt.AddSeconds(-secondsBack), null);
                MovingAverageManager.BackFillEma(currencyPair, longerInterval, dt, dt.AddSeconds(-secondsBack), null);
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

            ProfitAnalyzer.MacdEma = shorterMovingAverages[0].MovingAverageValue - longerMovingAverages[0].MovingAverageValue;
            for (int i = 0; i < shorterMovingAverages.Count; i++)
            {
                ProfitAnalyzer.ProcessMacdMovingAverageSignals(shorterMovingAverages[i].MovingAverageValue, longerMovingAverages[i].MovingAverageValue, shorterMovingAverages[i].LastClosingValue, shorterMovingAverages[i].ClosingDateTime);
            }

            ProfitAnalyzer.CalculateProfit(500);

            Console.WriteLine($"Complete: {DateTime.Now}");
            Console.ReadLine();
        }
    }
}
