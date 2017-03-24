using Poloniex.Log;
using System;
using System.Timers;

namespace Poloniex.Service.Framework
{
    public static class SchedulerHelper
    {
        private static class TimerHelper
        {
            private static Timer _timer = null;

            public static Timer Timer
            {
                get
                {
                    if (_timer == null)
                    {
                        _timer = new Timer();
                        _timer.Interval = ConfigurationHelper.TimerTickInterval;
                        _timer.Elapsed += new ElapsedEventHandler(TimerTick);
                    }

                    return _timer;
                }
            }
        }

        public static void TimerTick(object sender, ElapsedEventArgs e)
        {
            Logger.Write("Entering TimerTick.");

            try
            {
                TimerHelper.Timer.Stop();

                // TODO:
                Logger.Write("Do work.");

            }
            catch (Exception exception)
            {
                Logger.WriteException(exception);
            }
            finally
            {
                TimerHelper.Timer.Start();
            }

            Logger.Write("Exiting TimerTick.");
        }

        public static void Start()
        {
            Logger.Write("Service started.");
            Logger.Write("Syncing TimerTick.");
            while (DateTime.UtcNow.Second != 0) ;
            Logger.Write("TimerTick synced.");
            TimerHelper.Timer.Start();
        }

        public static void Stop()
        {
            TimerHelper.Timer.Stop();
            Logger.Write("Service stopped.");
        }
    }
}
