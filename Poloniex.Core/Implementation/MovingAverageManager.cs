using Poloniex.Core.Domain.Constants;
using Poloniex.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Poloniex.Core.Implementation
{
    public static class MovingAverageManager
    {
        public static void InitEma(Guid eventActionId)
        {
            using (var db = new PoloniexContext())
            {
                var eventAction = db.EventActions.Include(x => x.Task).Single(x => x.EventActionId == eventActionId);

                string currencyPair = string.Empty;
                switch (eventAction.Task.TaskType)
                {
                    case TaskType.GatherTask:
                        currencyPair = eventAction.Task.GatherTask.CurrencyPair;
                        break;
                }

                // db.CryptoCurrencyDataPoints.Where(x => x.CurrencyPair == currencyPair).OrderByDescending(x => x.ClosingDateTime).Take(eventAction.)
            }
        }

        public static void UpdateEma(Guid taskId)
        {

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
