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
                    .Where(x => x.CurrencyPair == eventAction.MovingAverageEventAction .CurrencyPair)
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
