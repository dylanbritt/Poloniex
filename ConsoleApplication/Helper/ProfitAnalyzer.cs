using Poloniex.Core.Implementation;
using System;
using System.Collections.Generic;

namespace ConsoleApplication.Helper
{
    public class ProfitAnalyzer
    {
        /* Config */
        public static decimal StopLossPercentage = 0.05M;
        public static decimal DecayPercentage = 0.5M;
        public static int DecayingCount = 1;
        public static bool IsDecayingClimb = false;

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


        public static void ProcessMacdMovingAverageSignals(
            decimal shorterMovingAverage,
            decimal longerMovingAverage,
            decimal lastClosingValue,
            DateTime closingDateTime)
        {
            decimal curMacd = shorterMovingAverage - longerMovingAverage;
            decimal macdEma = MovingAverageCalculations.CalculateEma(curMacd, MacdEma, MacdSignalInterval);
            _isBullish = curMacd - macdEma >= 0;

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

                        high = _buyValue * (1M + StopLossPercentage);
                        low = _buyValue * (1M - StopLossPercentage);

                        if (lastClosingValue >= high)
                        {
                            if (IsDecayingClimb)
                            {
                                _buyValue = high * (1M + StopLossPercentage * (decimal)Math.Pow((double)DecayPercentage, DecayingCount));
                                DecayingCount++;
                            }
                            else
                            {
                                _buyValue = high * (1M + StopLossPercentage);
                            }
                        }

                        if (lastClosingValue <= low)
                        {
                            _shouldSell = true;
                            DecayingCount = 1;
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


        public static void Process(decimal shorterMovingAverage, decimal longerMovingAverage, decimal lastClosingValue, DateTime closingDateTime)
        {
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

                        high = _buyValue * (1M + StopLossPercentage);
                        low = _buyValue * (1M - StopLossPercentage);

                        if (lastClosingValue >= high)
                        {
                            if (IsDecayingClimb)
                            {
                                _buyValue = high * (1M + StopLossPercentage * (decimal)Math.Pow((double)DecayPercentage, DecayingCount));
                                DecayingCount++;
                            }
                            else
                            {
                                _buyValue = high * (1M + StopLossPercentage);
                            }
                        }

                        if (lastClosingValue <= low)
                        {
                            _shouldSell = true;
                            DecayingCount = 1;
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