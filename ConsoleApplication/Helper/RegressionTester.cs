using Poloniex.Core.Domain.Models;
using Poloniex.Core.Implementation;
using Poloniex.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ConsoleApplication.Helper
{
    public static class RegressionTester
    {
        private static Dictionary<string, RegressionResults> _regressionResults = new Dictionary<string, Helper.RegressionResults>();

        public static void Test()
        {
            var currencyPairsStr = ConfigurationManager.AppSettings["currencyPairs"];
            var currencyPairs = currencyPairsStr.Split(',').ToList();
            for (int counter = 1; counter <= 4; counter++)
            {
                foreach (var currencyPair in currencyPairs)
                {
                    /* inclusive latest date */
                    var dt = DateTime.UtcNow;
                    // trim to start of hour (begin)
                    dt = dt.AddMilliseconds(-dt.Millisecond);
                    dt = dt.AddSeconds(-dt.Second);
                    dt = dt.AddMinutes(-dt.Minute);
                    //dt = dt.AddHours(-dt.Hour);
                    //dt = dt.AddDays(-dt.Day + 1);
                    // trim to start of hour (end)

                    var numberOfDays = 45;

                    int quarterDaysToGoBack = (int)(4 * (double)numberOfDays);

                    var shorterInterval = 12;
                    var longerInterval = 26;

                    var minutesPerInterval = 15 * counter;

                    dt = dt.AddMinutes(-(2 * minutesPerInterval));

                    //var currencyPair = CurrencyPairConstants.USDT_BTC; // 15
                    //var currencyPair = CurrencyPairConstants.USDT_DASH; // 30 // 45
                    //var currencyPair = CurrencyPairConstants.USDT_ETH; // 15
                    //var currencyPair = CurrencyPairConstants.USDT_LTC; // 30 // 45
                    //var currencyPair = CurrencyPairConstants.USDT_ZEC; // 30 // 45

                    // ################################################################

                    var secondsBack = quarterDaysToGoBack * 21600;
                    quarterDaysToGoBack++;

                    // ################################################################

                    var window = numberOfDays;
                    //var window = 30;
                    //var window = 60;
                    //var window = 120;
                    var numberOfTimesToShift = numberOfDays / window;

                    DateTime startDateTime = dt;
                    DateTime endDateTime = dt.AddSeconds(-secondsBack);

                    bool backFillCurrencyDataPoints = false;

                    backFillCurrencyDataPoints = counter == 1;//true;
                    if (backFillCurrencyDataPoints)
                    {
                        //GatherTaskManager.BackFillGatherTaskData(quarterDaysToGoBack, currencyPair, dt, DateTime.Parse("1970-01-01 00:00:00.000"));

                        // only need to backfill 6 quarter days if AutoLoader is used.
                        GatherTaskManager.BackFillGatherTaskData(6, currencyPair, dt, DateTime.Parse("1970-01-01 00:00:00.000"));
                    }

                    bool backFillMovingAverages = false;

                    backFillMovingAverages = true;
                    if (backFillMovingAverages)
                    {
                        MovingAverageManager.BackFillEma(currencyPair, shorterInterval, minutesPerInterval, dt, dt.AddSeconds(-secondsBack), null);
                        MovingAverageManager.BackFillEma(currencyPair, longerInterval, minutesPerInterval, dt, dt.AddSeconds(-secondsBack), null);
                    }

                    // ################################################################

                    // start manipulations
                    startDateTime = endDateTime.AddDays(window);
                    List<RegressionResults> results = new List<RegressionResults>();

                    for (int looper = 0; looper < numberOfTimesToShift; looper++)
                    {
                        ProfitAnalyzer.ResetMemory();
                        ProfitAnalyzer.ResetStats();

                        List<MovingAverage> shorterMovingAverages;
                        List<MovingAverage> longerMovingAverages;
                        using (var db = new PoloniexContext())
                        {
                            shorterMovingAverages = db.MovingAverages
                                .Where(x =>
                                    x.Interval == shorterInterval &&
                                    x.MinutesPerInterval == minutesPerInterval &&
                                    x.CurrencyPair == currencyPair &&
                                    x.ClosingDateTime >= endDateTime &&
                                    x.ClosingDateTime <= startDateTime)
                                .OrderBy(x => x.ClosingDateTime)
                            .ToList();

                            longerMovingAverages = db.MovingAverages
                                .Where(x =>
                                    x.Interval == longerInterval &&
                                    x.MinutesPerInterval == minutesPerInterval &&
                                    x.CurrencyPair == currencyPair &&
                                    x.ClosingDateTime >= endDateTime &&
                                    x.ClosingDateTime <= startDateTime)
                                .OrderBy(x => x.ClosingDateTime)
                            .ToList();
                        }

                        //ProfitAnalyzer.MacdEma = shorterMovingAverages[0].MovingAverageValue - longerMovingAverages[0].MovingAverageValue;
                        for (int i = 0; i < shorterMovingAverages.Count; i++)
                        {
                            //ProfitAnalyzer.ProcessMacdMovingAverageSignals(shorterMovingAverages[i].MovingAverageValue, longerMovingAverages[i].MovingAverageValue, shorterMovingAverages[i].LastClosingValue, shorterMovingAverages[i].ClosingDateTime);
                            ProfitAnalyzer.Process(shorterMovingAverages[i].MovingAverageValue, longerMovingAverages[i].MovingAverageValue, shorterMovingAverages[i].LastClosingValue, shorterMovingAverages[i].ClosingDateTime);
                        }
                        ProfitAnalyzer.CloseOpenPosition();

                        var inputBalance = 500M;

                        ProfitAnalyzer.CalculateProfit(inputBalance);

                        var dataMin = ProfitAnalyzer.Data_GetMin();
                        var dataMax = ProfitAnalyzer.Data_GetMax();
                        var dataMean = ProfitAnalyzer.Data_GetMean();
                        var dataMedian = ProfitAnalyzer.Data_GetMedian();
                        var dataStd = (decimal)Math.Sqrt((double)ProfitAnalyzer.Data_GetVariance());

                        var profitMin = ProfitAnalyzer.Profit_GetMin();
                        var profitMax = ProfitAnalyzer.Profit_GetMax();
                        var profitMean = ProfitAnalyzer.Profit_GetMean();
                        var profitMedian = ProfitAnalyzer.Profit_GetMedian();
                        var profitStd = (decimal)Math.Sqrt((double)ProfitAnalyzer.Profit_GetVariance());
                        var totalProfit = ProfitAnalyzer.GetTotalProfit();

                        //Console.WriteLine($"CurrencyPair: {currencyPair}");
                        //Console.WriteLine($"Data-Min: {dataMin}");
                        //Console.WriteLine($"Data-Max: {dataMax}");
                        //Console.WriteLine($"Data-Mean: {dataMean}");
                        //Console.WriteLine($"Data-Median: {dataMedian}");
                        //Console.WriteLine($"Data-Std: {dataStd}");
                        //Console.WriteLine($"Profit-Min: {profitMin}");
                        //Console.WriteLine($"Profit-Max: {profitMax}");
                        //Console.WriteLine($"Profit-Mean: {profitMean}");
                        //Console.WriteLine($"Profit-Median: {profitMedian}");
                        //Console.WriteLine($"Profit-Std: {profitStd}");
                        //Console.WriteLine($"TotalProfit: {totalProfit}");


                        results.Add(new RegressionResults
                        {
                            CurrencyPair = currencyPair,
                            MinutesPerInterval = minutesPerInterval,
                            StartDateTime = startDateTime,
                            EndDateTime = endDateTime,
                            DataMin = dataMin,
                            DataMax = dataMax,
                            DataMean = dataMean,
                            DataMedian = dataMedian,
                            DataStd = dataStd,
                            ProfitMin = profitMin,
                            ProfitMax = profitMax,
                            ProfitMean = profitMean,
                            ProfitMedian = profitMedian,
                            ProfitStd = profitStd,
                            PercentageChange = (totalProfit) / inputBalance,
                            TotalProfit = totalProfit
                        });

                        startDateTime = startDateTime.AddDays(window);
                        endDateTime = endDateTime.AddDays(window);
                    }

                    foreach (var item in results)
                    {
                        Console.WriteLine(item.ToString());
                        Console.WriteLine("################################################################");
                    }

                    if (!_regressionResults.ContainsKey(currencyPair) || _regressionResults[currencyPair].TotalProfit < results[0].TotalProfit)
                    {
                        if (_regressionResults.ContainsKey(currencyPair))
                        {
                            _regressionResults[currencyPair] = results[0];
                        }
                        else
                        {
                            _regressionResults.Add(currencyPair, results[0]);
                        }
                    }
                }
            }
            foreach (var item in _regressionResults)
            {
                Console.WriteLine("XxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXx");
                Console.WriteLine($"key: {item.Key}\nvalue: {item.Value.ToString()}");
                Console.WriteLine("-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_");
            }
        }
    }
}
