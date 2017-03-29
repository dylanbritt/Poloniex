using System;
using System.IO;

namespace ConsoleApplication.Helper
{
    public class ProfitAnalyzer
    {
        private static bool _init = true;

        private static bool _wasBullish = false;
        private static bool _isBullish = false;

        public static bool _hasHoldings = false;

        private static bool _shouldBuy = false;
        private static bool _shouldSell = false;

        private static string LogName = $"{DateTime.UtcNow.ToString("yyyyMMddhhmmssfff")}-CrossOverLog.txt";

        private static void WriteToLog(string command, decimal lastClosingValue, DateTime closingDateTime)
        {
            using (var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + $"\\{LogName}", true))
            {
                sw.WriteLine($"{command},{lastClosingValue},{closingDateTime}");
            }
        }

        public static void Process(decimal signalMovingAverageClosingValue, decimal baseMovingAverageClosingValue, decimal lastClosingValue, DateTime closingDateTime)
        {
            _isBullish = signalMovingAverageClosingValue > baseMovingAverageClosingValue;

            if (!_init)
            {

                if (!_wasBullish && _isBullish && !_hasHoldings)
                {
                    // BUY
                    _shouldBuy = true;
                }
                if (_wasBullish && !_isBullish && _hasHoldings)
                {
                    // SELL
                    _shouldSell = true;
                }

                if (_shouldBuy)
                {
                    // write to log
                    WriteToLog("buy", lastClosingValue, closingDateTime);
                    _hasHoldings = true;
                }

                if (_shouldSell)
                {
                    // write to log
                    WriteToLog("sell", lastClosingValue, closingDateTime);
                    _hasHoldings = false;
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
            using (var sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + $"\\{LogName}"))
            {
                decimal balance = startingAmount;

                decimal buyAmount = int.MinValue;
                decimal sellAmount = int.MinValue;

                decimal percentChange = int.MinValue;

                int count = 0;
                bool isInit = true;
                while (!sr.EndOfStream)
                {
                    var isBuy = false;
                    if (count % 2 == 0)
                    {
                        isBuy = true;
                    }

                    var str = sr.ReadLine();
                    var strs = str.Split(',');
                    string orderType = strs[0];
                    decimal orderPrice = decimal.Parse(strs[1]);
                    DateTime dt = DateTime.Parse(strs[2]);

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

                        percentChange = sellAmount / buyAmount;

                        balance = balance * percentChange;

                        Console.WriteLine($"New balance: {balance}");
                    }
                    else
                    {
                        isInit = false;
                    }

                    count++;
                }
            }
        }
    }
}