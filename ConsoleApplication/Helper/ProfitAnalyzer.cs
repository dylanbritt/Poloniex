using Poloniex.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication.Helper
{
    public class ProfitAnalyzer
    {
        /* Config */
        public static decimal StopLossPercentageUpper = 0.03M;
        public static decimal StopLossPercentageLower = 0.03M;
        public static bool IsClimbing = true;

        /* data stats */
        public static List<decimal> DataTracker = new List<decimal>();
        public static decimal Data_GetMin()
        {
            return DataTracker.Min(x => x);
        }
        public static decimal Data_GetMax()
        {
            return DataTracker.Max(x => x);
        }
        public static decimal Data_GetMean()
        {
            return DataTracker.Sum() / (decimal)DataTracker.Count();
        }
        public static decimal Data_GetMedian()
        {
            DataTracker = DataTracker.OrderBy(x => x).ToList();
            return DataTracker[DataTracker.Count / 2];
        }
        public static decimal Data_GetVariance()
        {
            DataTracker = DataTracker.OrderBy(x => x).ToList();

            decimal sum = 0;
            decimal mean = DataTracker.Sum() / (decimal)DataTracker.Count();

            for (int i = 0; i < DataTracker.Count; i++)
            {
                sum += (decimal)Math.Pow((double)(DataTracker[i] - mean), 2);
            }
            return sum / (decimal)(DataTracker.Count - 1);
        }
        /* profit stats */
        public static List<decimal> ProfitTracker = new List<decimal>();
        public static decimal Profit_GetMin()
        {
            if (!ProfitTracker.Any())
                return -1;
            return ProfitTracker.Min(x => x);
        }
        public static decimal Profit_GetMax()
        {
            if (!ProfitTracker.Any())
                return -1;
            return ProfitTracker.Max(x => x);
        }
        public static decimal Profit_GetMean()
        {
            if (!ProfitTracker.Any())
                return -1;
            return ProfitTracker.Sum() / (decimal)ProfitTracker.Count();
        }
        public static decimal Profit_GetMedian()
        {
            if (!ProfitTracker.Any())
                return -1;
            ProfitTracker = ProfitTracker.OrderBy(x => x).ToList();
            return ProfitTracker[ProfitTracker.Count / 2];
        }
        public static decimal Profit_GetVariance()
        {
            if (!ProfitTracker.Any())
                return 1;
            ProfitTracker = ProfitTracker.OrderBy(x => x).ToList();

            decimal sum = 0;
            decimal mean = ProfitTracker.Sum() / (decimal)ProfitTracker.Count();

            for (int i = 0; i < ProfitTracker.Count; i++)
            {
                sum += (decimal)Math.Pow((double)(ProfitTracker[i] - mean), 2);
            }
            if (ProfitTracker.Count == 1)
                return sum;
            return sum / (decimal)(ProfitTracker.Count - 1);
        }
        public static decimal GetTotalProfit()
        {
            if (!ProfitTracker.Any())
                return -1;
            return ProfitTracker.Sum();
        }
        /* stats methods */
        public static void ResetStats()
        {
            DataTracker = new List<decimal>();
            ProfitTracker = new List<decimal>();
        }

        /* macd signals */
        public static int MacdSignalInterval = 9;
        public static decimal MacdEma = 0;

        /* Internal variables */
        private static bool _init = true;

        private static bool _wasBullish = false;
        private static bool _isBullish = false;

        public static bool _hasHoldings = false;

        private static bool _shouldBuy = false;
        private static bool _shouldSell = false;

        private static decimal _buyValue = 0;

        private static bool _isDelay = false;

        private static string LogName = $"{DateTime.UtcNow.ToString("yyyyMMddhhmmssfff")}-CrossOverLog.txt";

        private class MemObj
        {
            public string Command;
            public decimal LastClosingValue;
            public DateTime ClosingDateTime;
        }
        private static List<MemObj> Objs = new List<MemObj>();
        private static void WriteToMemory(string command, decimal lastClosingValue, DateTime closingDateTime)
        {
            Objs.Add(new MemObj
            {
                Command = command,
                LastClosingValue = lastClosingValue,
                ClosingDateTime = closingDateTime
            });
        }
        public static void ResetMemory()
        {
            Objs = new List<MemObj>();

            _init = true;
            _wasBullish = false;
            _isBullish = false;
            _hasHoldings = false;
            _shouldBuy = false;
            _shouldSell = false;
            _buyValue = 0;
            _isDelay = false;
        }

        public static void ProcessMacdMovingAverageSignals(
            decimal shorterMovingAverage,
            decimal longerMovingAverage,
            decimal lastClosingValue,
            DateTime closingDateTime)
        {
            /* stats (begin) */
            DataTracker.Add(lastClosingValue);
            /* stats (end) */

            decimal curMacd = shorterMovingAverage - longerMovingAverage;
            MacdEma = MovingAverageCalculations.CalculateEma(curMacd, MacdEma, MacdSignalInterval);
            _isBullish = curMacd - MacdEma >= 0;

            if (!_init)
            {

                if (!_isDelay)
                {

                    if (!_wasBullish && _isBullish && !_hasHoldings)
                    {
                        // BUY
                        _shouldBuy = true;
                        _buyValue = lastClosingValue;
                    }

                    if (_hasHoldings)
                    {
                        decimal high;
                        decimal low;

                        high = _buyValue * (1M + StopLossPercentageUpper);
                        low = _buyValue * (1M - StopLossPercentageLower);

                        if (IsClimbing)
                        {
                            if (lastClosingValue >= high)
                            {
                                _buyValue = high * (1M + StopLossPercentageUpper);
                            }

                            if (lastClosingValue <= low)
                            {
                                _shouldSell = true;
                            }
                        }
                        else
                        {
                            if (lastClosingValue >= high || lastClosingValue <= low)
                            {
                                _shouldSell = true;
                            }
                        }
                    }

                    if (_shouldBuy)
                    {
                        // write to log
                        WriteToMemory("buy", lastClosingValue, closingDateTime);
                        _hasHoldings = true;
                    }

                }

                if (_shouldSell)
                {
                    //if(!_isDelay)
                    //{
                    //    _isDelay = true;
                    //    return;
                    //}

                    // write to log
                    WriteToMemory("sell", lastClosingValue, closingDateTime);
                    _hasHoldings = false;

                    //_isDelay = false;
                }

                _shouldBuy = false;
                _shouldSell = false;
            }
            else
            {
                _init = false;
            }

            _wasBullish = _isBullish;
        }
        public static void CloseOpenPosition()
        {
            if (Objs.Count % 2 == 1)
            {
                WriteToMemory("sell", DataTracker.Last(), DateTime.MinValue);
            }
        }


        public static void Process(decimal shorterMovingAverage, decimal longerMovingAverage, decimal lastClosingValue, DateTime closingDateTime)
        {
            /* stats (begin) */
            DataTracker.Add(lastClosingValue);
            /* stats (end) */

            _isBullish = shorterMovingAverage - longerMovingAverage >= 0;

            if (!_init)
            {

                if (!_isDelay)
                {

                    if (!_wasBullish && _isBullish && !_hasHoldings)
                    {
                        // BUY
                        _shouldBuy = true;
                        _buyValue = lastClosingValue;
                    }

                    if (_hasHoldings)
                    {
                        decimal high;
                        decimal low;

                        high = _buyValue * (1M + StopLossPercentageUpper);
                        low = _buyValue * (1M - StopLossPercentageLower);

                        if (IsClimbing)
                        {
                            if (lastClosingValue >= high)
                            {
                                _buyValue = high * (1M + StopLossPercentageUpper);
                            }

                            if (lastClosingValue <= low)
                            {
                                _shouldSell = true;
                            }
                        }
                        else
                        {
                            if (lastClosingValue >= high || lastClosingValue <= low)
                            {
                                _shouldSell = true;
                            }
                        }
                    }

                    if (_shouldBuy)
                    {
                        // write to log
                        WriteToMemory("buy", lastClosingValue, closingDateTime);
                        _hasHoldings = true;
                    }

                }

                if (_shouldSell)
                {
                    //if(!_isDelay)
                    //{
                    //    _isDelay = true;
                    //    return;
                    //}

                    // write to log
                    WriteToMemory("sell", lastClosingValue, closingDateTime);
                    _hasHoldings = false;

                    //_isDelay = false;
                }

                _shouldBuy = false;
                _shouldSell = false;
            }
            else
            {
                _init = false;
            }

            _wasBullish = _isBullish;
        }

        public static void CalculateProfit(decimal startingAmount)
        {
            decimal balance = startingAmount;

            decimal buyAmount = int.MinValue;
            decimal sellAmount = int.MinValue;

            decimal percentChange = int.MinValue;

            int count = 0;
            bool isInit = true;

            foreach (var obj in Objs)
            {
                var isBuy = false;
                if (count % 2 == 0)
                {
                    isBuy = true;
                }
                string orderType = obj.Command;
                decimal orderPrice = obj.LastClosingValue;
                DateTime dt = obj.ClosingDateTime;

                if (isBuy)
                {
                    buyAmount = orderPrice;
                }
                else
                {
                    sellAmount = orderPrice;
                }

                if (!isInit)
                {

                    Console.WriteLine($"{orderType}, {orderPrice}, {dt}");

                    percentChange = (sellAmount - sellAmount * 0.0015M) / (buyAmount + buyAmount * 0.0025M);

                    if (!isBuy)
                    {
                        var profit = (balance * percentChange) - balance;
                        ProfitTracker.Add(profit);
                    }
                    balance = balance * percentChange;

                    Console.WriteLine($"New balance: {balance}");
                }
                else
                {
                    isInit = false;
                    Console.WriteLine($"{orderType}, {orderPrice}, {dt}");

                    Console.WriteLine($"New balance: {balance}");
                }

                count++;
            }
        }
    }
}