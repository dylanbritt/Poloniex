using Poloniex.Api.Implementation;
using Poloniex.Core.Domain.Models;
using Poloniex.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Poloniex.Core.Implementation
{
    public static class GatherTaskManager
    {
        public static void BackFillGatherTaskDataForOneMonthAtMinuteIntervals(string currencyPair, DateTime? inputDateTime = null)
        {
            var curDateTime = inputDateTime ?? DateTime.UtcNow;

            // 2678400 seconds = 1 month
            // 21600 seconds = 6 hours
            for (int i = 2678400; i > 0; i = i - 21600)
            {
                var intervalBeginningDateTime = curDateTime.AddSeconds(-i);
                var intervalEndDateTime = curDateTime.AddSeconds(-(i - 21600));

                var poloniexData = PoloniexExchangeService.Instance.ReturnTradeHistory(currencyPair, intervalBeginningDateTime, intervalEndDateTime);

                poloniexData = poloniexData.OrderBy(x => x.date).ToList();

                var dataPoints = new List<CryptoCurrencyDataPoint>();
                decimal rate = poloniexData.First().rate;

                // how many minute intervals in 21600 seconds ... 360
                for (int j = 0; j < 360; j++)
                {
                    int pos = 0;

                    var dataPoint = new CryptoCurrencyDataPoint
                    {
                        Currency = currencyPair,
                        Interval = 60,
                        ClosingDateTime = intervalBeginningDateTime.AddSeconds((j + 1) * 60),
                        CreatedDateTime = DateTime.UtcNow
                    };

                    bool isAnyData = false;
                    while (pos < poloniexData.Count && poloniexData[pos].date < dataPoint.ClosingDateTime)
                    {
                        isAnyData = true;
                        pos++;
                    }
                    if (pos == poloniexData.Count)
                    {
                        pos--;
                    }

                    if (isAnyData)
                    {
                        rate = poloniexData[pos].rate;
                    }

                    dataPoint.ClosingValue = rate;

                    dataPoints.Add(dataPoint);
                }

                using (var db = new PoloniexContext())
                {
                    db.CryptoCurrencyDataPoints.AddRange(dataPoints);
                    db.SaveChanges();
                }
            }

            return;
        }

        public static void BackFillGatherTaskData(string currencyPair, int interval, int numberOfIntervals, DateTime? dateTimeNowOptional = null)
        {
            using (var db = new PoloniexContext())
            {
                var del =
                    db.CryptoCurrencyDataPoints
                        .Where(x => x.Interval == interval && x.Currency == currencyPair)
                        .OrderByDescending(x => x.ClosingDateTime)
                        .Take(numberOfIntervals);
                db.CryptoCurrencyDataPoints.RemoveRange(del);
                db.SaveChanges();
            }

            DateTime dateTimeNow = dateTimeNowOptional ?? DateTime.UtcNow;
            DateTime dateTimePast = dateTimeNow.AddSeconds(-(interval * numberOfIntervals));

            var result = PoloniexExchangeService.Instance.ReturnTradeHistory(currencyPair, dateTimePast, dateTimeNow);
            result = result.OrderBy(x => x.date).ToList();

            List<CryptoCurrencyDataPoint> cryptoCurrencyDataPoints = new List<CryptoCurrencyDataPoint>();
            var curRate = result.First().rate;

            int curPos = 0;

            for (int i = numberOfIntervals; i > 0; i--)
            {
                var intervalBeginningDateTime = dateTimeNow.AddSeconds(-(interval * i));
                var intervalEndDateTime = dateTimeNow.AddSeconds(-(interval * (i - 1)));

                var dataPoint = new CryptoCurrencyDataPoint
                {
                    Currency = currencyPair,
                    Interval = interval,
                    ClosingDateTime = intervalEndDateTime,
                };

                bool isAnyData = false;
                while (curPos < result.Count && result[curPos].date < intervalEndDateTime)
                {
                    isAnyData = true;
                    curPos++;
                }
                if (curPos == result.Count)
                {
                    curPos--;
                }

                if (isAnyData)
                {
                    curRate = result[curPos].rate;
                }

                dataPoint.ClosingValue = curRate;

                cryptoCurrencyDataPoints.Add(dataPoint);
            }

            using (var db = new PoloniexContext())
            {
                db.CryptoCurrencyDataPoints.AddRange(cryptoCurrencyDataPoints);
                db.SaveChanges();
            }
        }

        public static Timer GetGatherTaskTimer(string currencyPair, int inverval, bool startTimer = false)
        {
            var timer = new Timer();

            timer.Interval = inverval * 1000;
            timer.Elapsed += (sender, elapsedEventArgs) => GatherTaskElapsed(sender, currencyPair, inverval);

            if (startTimer)
            {
                timer.Start();
            }

            return timer;
        }

        private static void GatherTaskElapsed(object sender, string currencyPair, int interval)
        {
            DateTime dateTimeNow = DateTime.UtcNow;
            DateTime dateTimePast = dateTimeNow.AddSeconds(-(interval * 4));

            var result = PoloniexExchangeService.Instance.ReturnTradeHistory(currencyPair, dateTimePast, dateTimeNow);
            result = result.OrderBy(x => x.date).ToList();

            if (result.Any())
            {
                var curRate = result.Last().rate;
                var dataPoint = new CryptoCurrencyDataPoint
                {
                    Currency = currencyPair,
                    Interval = interval,
                    ClosingDateTime = dateTimeNow,
                };

                dataPoint.ClosingValue = curRate;

                using (var db = new PoloniexContext())
                {
                    dataPoint.CreatedDateTime = DateTime.UtcNow;
                    db.CryptoCurrencyDataPoints.Add(dataPoint);
                    db.SaveChanges();
                }
            }
        }
    }
}
