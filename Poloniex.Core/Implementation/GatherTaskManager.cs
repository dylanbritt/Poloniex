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
        public static void BackFillGatherTaskData(int numberOfQuarterDays, string currencyPair, DateTime? inputDateTime = null, DateTime? markerDate = null)
        {
            markerDate = markerDate ?? DateTime.Parse("01/01/1970");

            var totalTimeToGoBack = numberOfQuarterDays * 21600;

            var curDateTime = inputDateTime ?? DateTime.UtcNow;

            //var tmpDelDateTime = curDateTime.AddSeconds(-2678400).AddMilliseconds(500);
            var tmpDelDateTime = curDateTime.AddSeconds(-totalTimeToGoBack).AddMilliseconds(500);
            using (var db = new PoloniexContext())
            {
                var del =
                    db.CryptoCurrencyDataPoints
                        .Where(x => x.ClosingDateTime <= curDateTime && x.ClosingDateTime >= tmpDelDateTime && x.CurrencyPair == currencyPair).ToList();
                db.CryptoCurrencyDataPoints.RemoveRange(del);
                db.SaveChanges();
            }

            // 2678400 seconds = 31 days
            // 21600 seconds = 6 hours
            //for (int i = 2678400; i > 0; i = i - 21600)
            for (int i = totalTimeToGoBack; i > 0; i = i - 21600)
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
                        CurrencyPair = currencyPair,
                        ClosingDateTime = intervalBeginningDateTime.AddSeconds((j + 1) * 60),
                        CreatedDateTime = markerDate.Value // notify was populated by backfill
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

        public static Timer GetGatherTaskTimer(Guid taskId, List<EventAction> eventActions)
        {
            GatherTask gatherTask;
            using (var db = new PoloniexContext())
            {
                gatherTask = db.GatherTasks.Include(x => x.Task.TaskLoop).Single(x => x.TaskId == taskId);
            }

            return GetGatherTaskTimer(gatherTask.CurrencyPair, gatherTask.Task.TaskLoop.Interval, eventActions, true);
        }

        public static Timer GetGatherTaskTimer(string currencyPair, int inverval, List<EventAction> eventActions, bool startTimer)
        {
            var timer = new Timer();

            timer.Interval = inverval * 1000;
            timer.AutoReset = false;
            timer.Elapsed += (sender, elapsedEventArgs) => GatherTaskElapsed(sender, currencyPair, inverval, timer, eventActions);

            if (startTimer)
            {
                timer.Start();
            }

            return timer;
        }

        public static void GatherTaskElapsed(object sender, string currencyPair, int interval, Timer t, List<EventAction> eventActions)
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
                        CurrencyPair = currencyPair,
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
                                .Where(x => x.CurrencyPair == dataPoint.CurrencyPair)
                                .OrderByDescending(x => x.ClosingDateTime)
                                .First().ClosingValue;
                        }
                    }

                    using (var db = new PoloniexContext())
                    {
                        dataPoint.CreatedDateTime = DateTime.UtcNow;
                        db.CryptoCurrencyDataPoints.Add(dataPoint);
                        db.SaveChanges();
                    }

                    if (eventActions != null)
                    {
                        var sortedActions = eventActions.OrderBy(x => x.Priority).ToList();
                        for (int i = 0; i < sortedActions.Count(); i++)
                        {
                            Logger.Write($"Executing action {i + 1} of {sortedActions.Count()} - {sortedActions[i].EventActionType}", Logger.LogType.ServiceLog);
                            var threadEventAction = sortedActions[i];
                            threadEventAction.Action(threadEventAction.EventActionId);
                        }
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
            if (interval % 5 == 0)
            {
                if (next.Second == 9 || next.Second == 4)
                {
                    next = next.AddSeconds(1);
                }
                if (next.Second == 1 || next.Second == 6)
                {
                    next = next.AddSeconds(-1);
                }
            }
            next = next.AddMilliseconds(5);
            return (int)(next - now).TotalMilliseconds;
        }
    }
}
