using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Domain.Models;
using Poloniex.Data.Contexts;
using Poloniex.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
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
                    MinutesPerInterval = eventAction.MovingAverageEventAction.MinutesPerInterval,
                    ClosingDateTime = DateTime.UtcNow,
                    MovingAverageValue = MovingAverageCalculations.CalculateSma(smaInputClosingValues),
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

        public static void BackFillEma(string currencyPair, int interval, int minutesPerInterval, DateTime beginDateTime, DateTime endDateTime, decimal? prevEmaSeed)
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
                        x.MinutesPerInterval == minutesPerInterval &&
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

                if (prevEmaSeed == null)
                {
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
                else
                {
                    prevEma = prevEmaSeed.Value;
                }

            }

            //var ctx = new PoloniexContext();
            //ctx.Configuration.AutoDetectChangesEnabled = false;

            //// Begin calculating
            //for (int i = 0; i < dataPoints.Count; i++)
            //{
            //    if (i % 100 == 0)
            //    {
            //        ctx.SaveChanges();
            //        ctx = new PoloniexContext();
            //        ctx.Configuration.AutoDetectChangesEnabled = false;
            //    }
            //    var newMovingAverage = new MovingAverage()
            //    {
            //        MovingAverageType = MovingAverageType.ExponentialMovingAverage,
            //        CurrencyPair = currencyPair,
            //        Interval = interval,
            //        ClosingDateTime = dataPoints[i].ClosingDateTime,
            //        MovingAverageClosingValue = MovingAverageCalculations.CalculateEma(dataPoints[i].ClosingValue, prevEma, interval),
            //        LastClosingValue = dataPoints[i].ClosingValue
            //    };
            //    ctx.MovingAverages.Add(newMovingAverage);
            //    prevEma = newMovingAverage.MovingAverageClosingValue;
            //}
            //ctx.SaveChanges();
            //ctx.Dispose();

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

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Poloniex.Data.Contexts.PoloniexContext"].ToString()))
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

        private static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
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
