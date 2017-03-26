using Poloniex.Api.Implementation;
using Poloniex.Core.Domain.Models;
using Poloniex.Data.Contexts;
using Poloniex.Log;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Timers;

namespace Poloniex.Core.Implementation
{
    public static class GatherTaskManager
    {
        public static void BackFillGatherTaskDataForOneMonth(string currencyPair, DateTime? inputDateTime = null)
        {
            var curDateTime = inputDateTime ?? DateTime.UtcNow;

            var tmpDelDateTime = curDateTime.AddSeconds(-2678400);
            using (var db = new PoloniexContext())
            {
                var del =
                    db.CryptoCurrencyDataPoints
                        .Where(x => x.ClosingDateTime <= curDateTime && x.ClosingDateTime >= tmpDelDateTime && x.Currency == currencyPair).ToList();
                db.CryptoCurrencyDataPoints.RemoveRange(del);
                db.SaveChanges();
            }

            // 2678400 seconds = 31 days
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
                        .Where(x => x.Currency == currencyPair)
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

        public static Timer GetGatherTaskTimer(Guid taskId)
        {
            GatherTask gatherTask;
            using (var db = new PoloniexContext())
            {
                gatherTask = db.GatherTasks.Include(x => x.Task.TaskLoop).Single(x => x.TaskId == taskId);
            }

            return GetGatherTaskTimer(gatherTask.CurrencyPair, gatherTask.Task.TaskLoop.Interval, true);
        }

        public static Timer GetGatherTaskTimer(string currencyPair, int inverval, bool startTimer = false)
        {
            var timer = new Timer();

            timer.Interval = inverval * 1000;
            timer.AutoReset = false;
            timer.Elapsed += (sender, elapsedEventArgs) => GatherTaskElapsed(sender, currencyPair, inverval, timer);

            if (startTimer)
            {
                timer.Start();
            }

            return timer;
        }

        public static void GatherTaskElapsed(object sender, string currencyPair, int interval, Timer t)
        {
            t.Interval = GetInterval(interval);
            t.Start();

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    DateTime dateTimeNow = DateTime.UtcNow;
                    DateTime dateTimePast = dateTimeNow.AddSeconds(-(60 * 4));

                    var result = PoloniexExchangeService.Instance.ReturnTradeHistory(currencyPair, dateTimePast, dateTimeNow);
                    result = result.OrderBy(x => x.date).ToList();

                    var dataPoint = new CryptoCurrencyDataPoint
                    {
                        Currency = currencyPair,
                        ClosingDateTime = dateTimeNow,
                    };

                    if (result.Any())
                    {
                        var curRate = result.Last().rate;
                        dataPoint.ClosingValue = curRate;
                    }
                    else
                    {
                        using (var db = new PoloniexContext())
                        {
                            dataPoint.ClosingValue = db.CryptoCurrencyDataPoints
                                .Where(x => x.Currency == dataPoint.Currency)
                                .OrderByDescending(x => x.ClosingDateTime)
                                .Take(1).Single().ClosingValue;
                        }
                    }

                    using (var db = new PoloniexContext())
                    {
                        dataPoint.CreatedDateTime = DateTime.UtcNow;
                        db.CryptoCurrencyDataPoints.Add(dataPoint);
                        db.SaveChanges();
                    }
                }
                catch (Exception exception)
                {
                    Logger.WriteException(exception);
                }
            });
        }

        private static int GetInterval(int interval)
        {
            DateTime now = DateTime.UtcNow;
            DateTime next = now.AddSeconds(interval);
            next = next.AddMilliseconds(-next.Millisecond);
            return (int)(next - now).TotalMilliseconds;
        }
    }
}
