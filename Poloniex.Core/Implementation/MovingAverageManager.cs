using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Domain.Models;
using Poloniex.Core.Utility;
using Poloniex.Data.Contexts;
using Poloniex.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace Poloniex.Core.Implementation
{
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

    public static class MovingAverageManager
    {
        public static void InitEmaBySma(Guid eventActionId)
        {
            using (var db = new PoloniexContext())
            {
                var eventAction = db.EventActions.Include(x => x.MovingAverageEventAction).Single(x => x.EventActionId == eventActionId);

                try
                {
                    var beginDateTime = DateTime.UtcNow.AddMilliseconds(333);
                    var endDateTime = beginDateTime.AddHours(-12);

                    var currencyPair = eventAction.MovingAverageEventAction.CurrencyPair;
                    var interval = eventAction.MovingAverageEventAction.Interval;
                    var minutesPerInterval = eventAction.MovingAverageEventAction.MinutesPerInterval;

                    decimal? prevEma = db.MovingAverages
                        .Where(x => x.CurrencyPair == currencyPair && x.Interval == interval)
                        .OrderByDescending(x => x.ClosingDateTime)
                        .FirstOrDefault()?.MovingAverageValue;

                    if (prevEma == null)
                    {
                        var smaInputClosingValues = db.CurrencyDataPoints
                            .Where(x => x.CurrencyPair == eventAction.MovingAverageEventAction.CurrencyPair)
                            .OrderByDescending(x => x.ClosingDateTime)
                            .Take(eventAction.MovingAverageEventAction.Interval)
                            .Select(x => x.ClosingValue)
                            .ToList();

                        prevEma = MovingAverageCalculations.CalculateSma(smaInputClosingValues);
                    }

                    BackFillEma(currencyPair, interval, minutesPerInterval, beginDateTime, endDateTime, prevEma);
                }
                catch (Exception exception)
                {
                    Logger.WriteException(exception);
                }
            }
        }

        public static void UpdateEma(Guid eventActionId)
        {
            using (var db = new PoloniexContext())
            {
                var eventAction = db.EventActions.Include(x => x.MovingAverageEventAction).Single(x => x.EventActionId == eventActionId);

                var closingValue = db.CurrencyDataPoints
                    .Where(x => x.CurrencyPair == eventAction.MovingAverageEventAction.CurrencyPair)
                    .OrderByDescending(x => x.ClosingDateTime)
                    .First();

                var prevEma = db.MovingAverages
                    .Where(x =>
                        x.MovingAverageType == eventAction.MovingAverageEventAction.MovingAverageType &&
                        x.CurrencyPair == eventAction.MovingAverageEventAction.CurrencyPair &&
                        x.Interval == eventAction.MovingAverageEventAction.Interval &&
                        x.MinutesPerInterval == eventAction.MovingAverageEventAction.MinutesPerInterval)
                    .OrderByDescending(x => x.ClosingDateTime)
                    .First();

                // -15 seconds to account for timer skew
                if (closingValue.ClosingDateTime >= prevEma.ClosingDateTime.AddSeconds(-15).AddMinutes(prevEma.Interval))
                {
                    var curEma = new MovingAverage()
                    {
                        MovingAverageType = MovingAverageType.ExponentialMovingAverage,
                        CurrencyPair = eventAction.MovingAverageEventAction.CurrencyPair,
                        Interval = eventAction.MovingAverageEventAction.Interval,
                        MinutesPerInterval = eventAction.MovingAverageEventAction.MinutesPerInterval,
                        ClosingDateTime = DateTime.UtcNow,
                        MovingAverageValue = MovingAverageCalculations.CalculateEma(closingValue.ClosingValue, prevEma.MovingAverageValue, eventAction.MovingAverageEventAction.Interval),
                        LastClosingValue = closingValue.ClosingValue
                    };

                    db.MovingAverages.Add(curEma);
                    db.SaveChanges();
                }
            }
        }

        /* additional helpers */

        public static void BackFillEma(string currencyPair, int interval, int minutesPerInterval, DateTime beginDateTime, DateTime endDateTime, decimal? prevEmaSeed)
        {
            // add time buffer to guarantee beginDate inclusive / endDate exclusive
            endDateTime = endDateTime.AddSeconds(1);
            beginDateTime = beginDateTime.AddSeconds(1);

            List<CurrencyDataPoint> dataPoints;
            List<decimal> smaInput;
            decimal prevEma;
            using (var db = new PoloniexContext())
            {
                var delMovingAverages = db.MovingAverages
                    .Where(x =>
                        x.CurrencyPair == currencyPair &&
                        x.Interval == interval &&
                        x.MinutesPerInterval == minutesPerInterval &&
                        x.ClosingDateTime <= beginDateTime &&
                        x.ClosingDateTime >= endDateTime);
                db.MovingAverages.RemoveRange(delMovingAverages);
                db.SaveChanges();

                dataPoints = db.CurrencyDataPoints
                    .Where(x =>
                        x.CurrencyPair == currencyPair &&
                        x.ClosingDateTime <= beginDateTime &&
                        x.ClosingDateTime >= endDateTime)
                    .ToList();

                if (prevEmaSeed == null)
                {
                    smaInput = db.CurrencyDataPoints
                        .Where(x =>
                            x.CurrencyPair == currencyPair &&
                            x.ClosingDateTime < endDateTime)
                        .OrderBy(x => x.ClosingDateTime)
                        .Select(x => x.ClosingValue)
                        .Take(interval)
                        .ToList();

                    prevEma = MovingAverageCalculations.CalculateSma(smaInput);
                }
                else
                {
                    prevEma = prevEmaSeed.Value;
                }

            }

            // Begin calculating
            List<MovingAverage> movingAveragesData = new List<MovingAverage>();
            for (int i = 0; i < dataPoints.Count; i++)
            {
                if (i % minutesPerInterval == 0)
                {
                    var newMovingAverage = new MovingAverage()
                    {
                        MovingAverageType = MovingAverageType.ExponentialMovingAverage,
                        CurrencyPair = currencyPair,
                        Interval = interval,
                        MinutesPerInterval = minutesPerInterval,
                        ClosingDateTime = dataPoints[i].ClosingDateTime,
                        MovingAverageValue = MovingAverageCalculations.CalculateEma(dataPoints[i].ClosingValue, prevEma, interval),
                        LastClosingValue = dataPoints[i].ClosingValue
                    };
                    movingAveragesData.Add(newMovingAverage);
                    prevEma = newMovingAverage.MovingAverageValue;
                }
            }
            BulkInsertMovingAverages(movingAveragesData);
        }

        private static void BulkInsertMovingAverages(List<MovingAverage> movingAverages)
        {
            var dataTable = movingAverages.ToDataTable<MovingAverage>();

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["PoloniexContext"].ToString()))
            {
                SqlTransaction transaction = null;
                connection.Open();
                try
                {
                    transaction = connection.BeginTransaction();
                    using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction))
                    {
                        sqlBulkCopy.DestinationTableName = "MovingAverages";
                        //sqlBulkCopy.ColumnMappings.Add("MovingAverageId", "MovingAverageId");
                        sqlBulkCopy.ColumnMappings.Add("MovingAverageType", "MovingAverageType");
                        sqlBulkCopy.ColumnMappings.Add("CurrencyPair", "CurrencyPair");
                        sqlBulkCopy.ColumnMappings.Add("Interval", "Interval");
                        sqlBulkCopy.ColumnMappings.Add("MinutesPerInterval", "MinutesPerInterval");
                        sqlBulkCopy.ColumnMappings.Add("ClosingDateTime", "ClosingDateTime");
                        sqlBulkCopy.ColumnMappings.Add("MovingAverageValue", "MovingAverageValue");
                        sqlBulkCopy.ColumnMappings.Add("LastClosingValue", "LastClosingValue");

                        sqlBulkCopy.WriteToServer(dataTable);
                    }
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    Logger.WriteException(exception);
                    transaction.Rollback();
                }

            }
        }
    }
}
