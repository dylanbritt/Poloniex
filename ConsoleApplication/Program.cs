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
            //var dt = DateTime.Parse("2017-03-28 00:15:00.000")
            var dt = DateTime.UtcNow;

            var quarterDaysToGoBack = 4 * 1;

            var signalInterval = 65;
            var baseInterval = 25;

            var currencyPair = CurrencyPairConstants.USDT_BTC;

            // ################################################################

            var secondsBack = quarterDaysToGoBack * 21600;
            quarterDaysToGoBack++;

            // ################################################################

            DateTime startDateTime = dt.AddSeconds(-secondsBack).AddSeconds(1);
            DateTime endDateTime = dt.AddSeconds(1);

            dt = dt.AddSeconds(-dt.Second);
            dt = dt.AddMilliseconds(-dt.Millisecond);

            //GatherTaskManager.BackFillGatherTaskData(1, currencyPair, dt);

            //GatherTaskManager.BackFillGatherTaskData(quarterDaysToGoBack, currencyPair, dt);
            //MovingAverageManager.BackFillEma(currencyPair, signalInterval, dt, dt.AddSeconds(-secondsBack));
            //MovingAverageManager.BackFillEma(currencyPair, baseInterval, dt, dt.AddSeconds(-secondsBack));

            // ################################################################

            endDateTime = endDateTime.AddSeconds(1);
            startDateTime = startDateTime.AddSeconds(1);

            List<MovingAverage> signalMovingAverages;
            List<MovingAverage> baseMovingAverages;
            using (var db = new PoloniexContext())
            {
                signalMovingAverages = db.MovingAverages
                    .Where(x =>
                        x.Interval == signalInterval &&
                        x.CurrencyPair == currencyPair &&
                        x.ClosingDateTime >= startDateTime &&
                        x.ClosingDateTime <= endDateTime)
                    .OrderBy(x => x.ClosingDateTime)
                .ToList();

                baseMovingAverages = db.MovingAverages
                    .Where(x =>
                        x.Interval == baseInterval &&
                        x.CurrencyPair == currencyPair &&
                        x.ClosingDateTime >= startDateTime &&
                        x.ClosingDateTime <= endDateTime)
                    .OrderBy(x => x.ClosingDateTime)
                .ToList();
            }

            for (int i = 0; i < signalMovingAverages.Count; i++)
            {
                ProfitAnalyzer.Process(signalMovingAverages[i].MovingAverageClosingValue, baseMovingAverages[i].MovingAverageClosingValue, signalMovingAverages[i].LastClosingValue, signalMovingAverages[i].ClosingDateTime);
            }

            ProfitAnalyzer.CalculateProfit(75);

            Console.ReadLine();
        }
    }
}
