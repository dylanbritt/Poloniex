using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication.Helper
{
    public static class AutoLoader
    {
        public static void GetData()
        {
            /* inclusive latest date */
            var dt = DateTime.UtcNow;
            // trim to start of month (begin)
            dt = dt.AddMilliseconds(-dt.Millisecond);
            dt = dt.AddSeconds(-dt.Second);
            dt = dt.AddMinutes(-dt.Minute);
            dt = dt.AddHours(-dt.Hour);
            dt = dt.AddDays(-dt.Day + 1);
            // trim to start of month (end)

            var numberOfDays = 365;

            var quarterDaysToGoBack = 4 * numberOfDays;

            var shorterInterval = 12;
            var longerInterval = 26;

            // ################################################################

            var secondsBack = quarterDaysToGoBack * 21600;
            quarterDaysToGoBack++;

            // ################################################################

            GatherTaskManager.BackFillGatherTaskData(quarterDaysToGoBack, CurrencyPairConstants.USDT_BTC, dt, DateTime.Parse("1970-01-01 00:00:00.000"));

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, shorterInterval, 15, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, longerInterval, 15, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, shorterInterval, 30, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, longerInterval, 30, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, shorterInterval, 45, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, longerInterval, 45, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, shorterInterval, 60, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, longerInterval, 60, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, shorterInterval, 90, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, longerInterval, 90, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, shorterInterval, 120, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, longerInterval, 120, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, shorterInterval, 240, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_BTC, longerInterval, 240, dt, dt.AddSeconds(-secondsBack), null);

            // ################################################################

            GatherTaskManager.BackFillGatherTaskData(quarterDaysToGoBack, CurrencyPairConstants.USDT_ETH, dt, DateTime.Parse("1970-01-01 00:00:00.000"));

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, shorterInterval, 15, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, longerInterval, 15, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, shorterInterval, 30, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, longerInterval, 30, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, shorterInterval, 45, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, longerInterval, 45, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, shorterInterval, 60, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, longerInterval, 60, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, shorterInterval, 90, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, longerInterval, 90, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, shorterInterval, 120, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, longerInterval, 120, dt, dt.AddSeconds(-secondsBack), null);

            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, shorterInterval, 240, dt, dt.AddSeconds(-secondsBack), null);
            MovingAverageManager.BackFillEma(CurrencyPairConstants.USDT_ETH, longerInterval, 240, dt, dt.AddSeconds(-secondsBack), null);

            // ################################################################
        }
    }
}
