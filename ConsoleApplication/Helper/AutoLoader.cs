using Poloniex.Core.Implementation;
using System;
using System.Configuration;
using System.Linq;

namespace ConsoleApplication.Helper
{
    public static class AutoLoader
    {
        public static void GetData()
        {
            /* inclusive latest date */
            var dt = DateTime.UtcNow;
            // trim to start of day (begin)
            dt = dt.AddMilliseconds(-dt.Millisecond);
            dt = dt.AddSeconds(-dt.Second);
            dt = dt.AddMinutes(-dt.Minute);
            dt = dt.AddHours(-dt.Hour);
            // trim to start of day (end)

            var numberOfDays = int.Parse(ConfigurationManager.AppSettings["numberOfDaysToBackfill"]);
            var currencyPairsStr = ConfigurationManager.AppSettings["currencyPair"];

            var quarterDaysToGoBack = 4 * numberOfDays;

            // ################################################################

            var secondsBack = quarterDaysToGoBack * 21600;
            quarterDaysToGoBack++;

            // ################################################################

            var currencyPairs = currencyPairsStr.Split(',').ToList();

            currencyPairs.ForEach(currencyPair =>
            {
                GatherTaskManager.BackFillGatherTaskData(quarterDaysToGoBack, currencyPair, dt, DateTime.Parse("1970-01-01 00:00:00.000"));
            });


            // ################################################################
        }
    }
}
