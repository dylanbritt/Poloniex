using Poloniex.Api.Implementation;
using Poloniex.Core.Constants;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class Program
    {
        public static string LogName = AppDomain.CurrentDomain.BaseDirectory + $"\\{DateTime.UtcNow.ToString("yyyyMMddhhmmssfff")}-ConsoleApplicationFile.txt";

        public static void WriteLog(string message)
        {
            using (var sw = new StreamWriter(LogName, true))
            {
                sw.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss:fff")}: {message}");
            }
        }

        static void Main(string[] args)
        {
            WriteLog("Console application started");

            //using (var db = new PoloniexContext())
            //{
            //    var cp = new CryptoCurrencyDataPoint
            //    {
            //        CryptoCurrencyDataPointId = Guid.NewGuid(),
            //        Currency = CurrencyConstants.BTC,
            //        ClosingDateTime = DateTime.UtcNow,
            //        ClosingValue = 0.12M,
            //        Interval = 60000
            //    };

            //    db.CryptoCurrencyDataPoints.Add(cp);

            //    db.SaveChanges();
            //}

            //var interval = 20;

            //DateTime dateTimeNow = DateTime.UtcNow;
            //dateTimeNow = dateTimeNow.AddSeconds(-dateTimeNow.Second);
            //DateTime dateTimePast = dateTimeNow.AddMinutes(-60);

            //var result = PoloniexExchangeService.Instance.ReturnTradeHistory(CurrencyPairConstants.BTC_ETH, DateTime.UtcNow.AddMinutes(-60), DateTime.UtcNow);
            //result = result.OrderBy(x => x.date).ToList();

            //List<CryptoCurrencyDataPoint> cryptoCurrencyDataPoints = new List<CryptoCurrencyDataPoint>();

            //int curPos = 0;
            //var curRate = result.First().rate;

            //for (int i = interval; i > 0; i--)
            //{
            //    var intervalBeginningDateTime = dateTimeNow.AddMinutes(-i);
            //    var intervalEndDateTime = dateTimeNow.AddMinutes(-(i + 1));

            //    var dataPoint = new CryptoCurrencyDataPoint
            //    {
            //        CryptoCurrencyDataPointId = Guid.NewGuid(),
            //        Currency = CurrencyPairConstants.BTC_ETH,
            //        Interval = interval,
            //        ClosingDateTime = intervalEndDateTime,
            //    };

            //    bool isAnyData = false;
            //    while (result[curPos].date < intervalEndDateTime && curPos < result.Count)
            //    {
            //        isAnyData = true;
            //        curPos++;
            //    }

            //    if (isAnyData)
            //    {
            //        curRate = result[curPos].rate;
            //    }

            //    dataPoint.ClosingValue = curRate;

            //    cryptoCurrencyDataPoints.Add(dataPoint);
            //}

            //// ################################################################
            //// Calculate simple moving average
            //// ################################################################

            //var simpleMovingAverage = new SimpleMovingAverage
            //{
            //    SimpleMovingAverageId = Guid.NewGuid(),
            //    Currency = CurrencyPairConstants.BTC_ETH,
            //    Interval = interval,
            //    ClosingDateTime = dateTimeNow
            //};
            //simpleMovingAverage.ClosingValue = cryptoCurrencyDataPoints.Sum(x => x.ClosingValue) / interval;

            //// ################################################################
            //// Calculate exponential moving average
            //// ################################################################

            //var timePeriods = interval;
            //var multiplier = 2M / (timePeriods + 1M);
            //decimal ema = 0;

            //// get previous otherwise start at first;
            //var previous = cryptoCurrencyDataPoints[0].ClosingValue;
            //for (int i = 0; i < interval; i++)
            //{
            //    ema = ((cryptoCurrencyDataPoints[0].ClosingValue - previous) * multiplier) + previous;
            //    previous = ema;
            //}

            //var exponentialMovingAverage = new ExponentialMovingAverage
            //{
            //    ExponentialMovingAverageId = Guid.NewGuid(),
            //    Currency = CurrencyPairConstants.BTC_ETH,
            //    Interval = interval,
            //    ClosingDateTime = dateTimeNow,
            //    ClosingValue = ema
            //};

            // ################################################################
            // Testing thread lock
            // ################################################################

            Task.Run(() =>
            {
                var tmp1 = PoloniexExchangeService.Instance.ReturnTradeHistory(CurrencyPairConstants.BTC_ETH, DateTime.UtcNow.AddMinutes(-60), DateTime.UtcNow);
                WriteLog("First thread completed");
            });

            Task.Run(() =>
            {
                var tmp = PoloniexExchangeService.Instance.ReturnTradeHistory(CurrencyPairConstants.BTC_ETH, DateTime.UtcNow.AddMinutes(-60), DateTime.UtcNow);
                WriteLog("Second thread completed");
            });


            int stop = 0;
            Console.ReadLine();
        }
    }
}
