using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Domain.Models;
using Poloniex.Core.Implementation;
using Poloniex.Log;
using System;

namespace ConsoleApplication
{
    class Program
    {
        public static string LogName = AppDomain.CurrentDomain.BaseDirectory + $"\\{DateTime.UtcNow.ToString("yyyyMMddhhmmssfff")}-ConsoleApplicationFile.txt";

        static void Main(string[] args)
        {
            //Logger.Write("Console application started");

            //// ################################################################
            //// Testing database
            //// ################################################################

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

            //// ################################################################
            //// Testing Poloniex API
            //// ################################################################

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

            //// ################################################################
            //// Testing thread lock
            //// ################################################################

            //System.Threading.Tasks.Task.Run(() =>
            //{
            //    var tmp1 = PoloniexExchangeService.Instance.ReturnTradeHistory(CurrencyPairConstants.BTC_ETH, DateTime.UtcNow.AddMinutes(-60), DateTime.UtcNow);
            //    Logger.Write("First thread completed");
            //});

            //System.Threading.Tasks.Task.Run(() =>
            //{
            //    var tmp = PoloniexExchangeService.Instance.ReturnTradeHistory(CurrencyPairConstants.BTC_ETH, DateTime.UtcNow.AddMinutes(-60), DateTime.UtcNow);
            //    Logger.Write("Second thread completed");
            //});


            //int stop = 0;
            //Console.ReadLine();

            // ################################################################
            // Testing Task data access
            // ################################################################

            //using (var db = new PoloniexContext())
            //{
            //    db.Tasks.Add(new Task()
            //    {
            //        TaskType = "GatherTask",
            //        TaskLoop = new TaskLoop()
            //        {
            //            LoopStatus = "RequestToStart",
            //            Interval = 3000,
            //        },
            //        GatherTask = new GatherTask()
            //        {
            //            CurrencyPair = CurrencyPairConstants.BTC_ETH,
            //            Interval = 30000
            //        }
            //    });

            //    db.SaveChanges();
            //}

            //TaskLoopScheduler tls = new TaskLoopScheduler();
            //tls.PollForTasksToStartOrStop();

            // ################################################################
            // Testing GatherTaskManager
            // ################################################################

            //GatherTaskManager.GatherTaskElapsed(null, CurrencyPairConstants.BTC_ETH, 60);

            Logger.Write("test started.");
            Logger.Write("syncing test tick.");
            while (DateTime.UtcNow.Second != 0) ;
            Logger.Write("test tick synced.");


            var taskLoop1 = new TaskLoop()
            {
                Interval = 60
            };

            var interval = 60;

            var timer1 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_BTC, interval, true);
            var timer2 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_ETH, interval, true);
            var timer3 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_DASH, interval, true);
            var timer4 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_XRP, interval, true);
            var timer5 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_XMR, interval, true);
            var timer6 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_ETC, interval, true);
            var timer7 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_LTC, interval, true);
            var timer8 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_ZEC, interval, true);
            var timer9 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_REP, interval, true);
            var timer10 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_STR, interval, true);
            var timer11 = GatherTaskManager.GetGatherTaskTimer(CurrencyPairConstants.USDT_NXT, interval, true);

            GlobalStateManager gsm = new GlobalStateManager();
            gsm.AddTaskLoop(taskLoop1, timer1);

            Console.ReadLine();
        }
    }
}
