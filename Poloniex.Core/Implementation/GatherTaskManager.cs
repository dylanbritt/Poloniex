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

            if (result.Count >= 49000)
            {
                for (int i = numberOfIntervals; i > 0; i--)
                {
                    var intervalBeginningDateTime = dateTimeNow.AddSeconds(-((interval * (i - 1)) + 3600));
                    var intervalEndDateTime = dateTimeNow.AddSeconds(-(interval * (i - 1)));

                    var poloniexData = PoloniexExchangeService.Instance.ReturnTradeHistory(currencyPair, intervalBeginningDateTime, intervalEndDateTime);

                    var dataPoint = new CryptoCurrencyDataPoint
                    {
                        Currency = currencyPair,
                        Interval = interval,
                        ClosingDateTime = intervalEndDateTime,
                    };

                    if (poloniexData.Any())
                    {
                        curRate = poloniexData.OrderBy(x => x.date).Last().rate;
                    }

                    dataPoint.ClosingValue = curRate;

                    cryptoCurrencyDataPoints.Add(dataPoint);

                }

                using (var db = new PoloniexContext())
                {
                    db.CryptoCurrencyDataPoints.AddRange(cryptoCurrencyDataPoints);
                    db.SaveChanges();
                }

                return;
            }

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
