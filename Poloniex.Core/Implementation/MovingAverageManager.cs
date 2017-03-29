using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Domain.Models;
using Poloniex.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Poloniex.Core.Implementation
{
    public static class MovingAverageManager
    {
        public static void InitEmaBySma(Guid eventActionId)
        {
            using (var db = new PoloniexContext())
            {
                var eventAction = db.EventActions.Include(x => x.MovingAverageEventAction).Single(x => x.EventActionId == eventActionId);

                var smaInputClosingValues = db.CryptoCurrencyDataPoints
                    .Where(x => x.CurrencyPair == eventAction.MovingAverageEventAction.CurrencyPair)
                    .OrderByDescending(x => x.ClosingDateTime)
                    .Take(eventAction.MovingAverageEventAction.Interval)
                    .Select(x => x.ClosingValue)
                    .ToList();

                var curEma = new MovingAverage()
                {
                    MovingAverageType = MovingAverageType.ExponentialMovingAverage,
                    CurrencyPair = eventAction.MovingAverageEventAction.CurrencyPair,
                    Interval = eventAction.MovingAverageEventAction.Interval,
                    ClosingDateTime = DateTime.UtcNow,
                    MovingAverageClosingValue = MovingAverageCalculations.CalculateSma(smaInputClosingValues),
                    LastClosingValue = smaInputClosingValues.First()
                };

                db.MovingAverages.Add(curEma);
                db.SaveChanges();
            }
        }

        public static void UpdateEma(Guid eventActionId)
        {
            using (var db = new PoloniexContext())
            {
                var eventAction = db.EventActions.Include(x => x.MovingAverageEventAction).Single(x => x.EventActionId == eventActionId);

                var closingValue = db.CryptoCurrencyDataPoints
                    .Where(x => x.CurrencyPair == eventAction.MovingAverageEventAction.CurrencyPair)
                    .OrderByDescending(x => x.ClosingDateTime)
                    .First();

                var prevEma = db.MovingAverages
                    .Where(x =>
                        x.MovingAverageType == eventAction.MovingAverageEventAction.MovingAverageType &&
                        x.CurrencyPair == eventAction.MovingAverageEventAction.CurrencyPair &&
                        x.Interval == eventAction.MovingAverageEventAction.Interval)
                    .OrderByDescending(x => x.ClosingDateTime)
                    .First();

                var curEma = new MovingAverage()
                {
                    MovingAverageType = MovingAverageType.ExponentialMovingAverage,
                    CurrencyPair = eventAction.MovingAverageEventAction.CurrencyPair,
                    Interval = eventAction.MovingAverageEventAction.Interval,
                    ClosingDateTime = DateTime.UtcNow,
                    MovingAverageClosingValue = MovingAverageCalculations.CalculateEma(closingValue.ClosingValue, prevEma.MovingAverageClosingValue, eventAction.MovingAverageEventAction.Interval),
                    LastClosingValue = closingValue.ClosingValue
                };

                db.MovingAverages.Add(curEma);
                db.SaveChanges();
            }
        }

        public static void BackFillEma(string currencyPair, int interval, DateTime beginDateTime, DateTime endDateTime)
        {
            // add time buffer to guarantee beginDate inclusive / endDate exclusive
            endDateTime = endDateTime.AddSeconds(1);
            beginDateTime = beginDateTime.AddSeconds(1);

            List<CryptoCurrencyDataPoint> dataPoints;
            List<decimal> smaInput;
            decimal prevEma;
            using (var db = new PoloniexContext())
            {
                var delMovingAverages = db.MovingAverages
                    .Where(x =>
                        x.CurrencyPair == currencyPair &&
                        x.Interval == interval &&
                        x.ClosingDateTime <= beginDateTime &&
                        x.ClosingDateTime >= endDateTime);
                db.MovingAverages.RemoveRange(delMovingAverages);
                db.SaveChanges();

                dataPoints = db.CryptoCurrencyDataPoints
                    .Where(x =>
                        x.CurrencyPair == currencyPair &&
                        x.ClosingDateTime <= beginDateTime &&
                        x.ClosingDateTime >= endDateTime)
                    .ToList();

                smaInput = db.CryptoCurrencyDataPoints
                    .Where(x =>
                        x.CurrencyPair == currencyPair &&
                        x.ClosingDateTime < endDateTime)
                    .OrderBy(x => x.ClosingDateTime)
                    .Select(x => x.ClosingValue)
                    .Take(interval)
                    .ToList();

                prevEma = MovingAverageCalculations.CalculateSma(smaInput);

            }
            
            var ctx = new PoloniexContext();
            ctx.Configuration.AutoDetectChangesEnabled = false;

            // Being calculating
            for (int i = 0; i < dataPoints.Count; i++)
            {
                if (i % 100 == 0)
                {
                    ctx.SaveChanges();
                    ctx = new PoloniexContext();
                    ctx.Configuration.AutoDetectChangesEnabled = false;
                }
                var newMovingAverage = new MovingAverage()
                {
                    MovingAverageType = MovingAverageType.ExponentialMovingAverage,
                    CurrencyPair = currencyPair,
                    Interval = interval,
                    ClosingDateTime = dataPoints[i].ClosingDateTime,
                    MovingAverageClosingValue = MovingAverageCalculations.CalculateEma(dataPoints[i].ClosingValue, prevEma, interval),
                    LastClosingValue = dataPoints[i].ClosingValue
                };
                ctx.MovingAverages.Add(newMovingAverage);
                prevEma = newMovingAverage.MovingAverageClosingValue;
            }
            ctx.SaveChanges();
            ctx.Dispose();
        }
    }

    public static class MovingAverageCalculations
    {
        public static decimal CalculateSma(List<decimal> closingValues)
        {
            return closingValues.Sum() / closingValues.Count;
        }

        public static decimal CalculateEma(decimal closingValue, decimal previousEma, int intervalCount)
        {
            var multipler = 2M / ((decimal)intervalCount + 1M);

            return ((closingValue - previousEma) * multipler) + previousEma;
        }
    }
}
