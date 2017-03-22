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
            ServiceLog.Write("Entering TimerTick.");

            try
            {
                TimerHelper.Timer.Stop();

                // TODO:
                ServiceLog.Write("Do work.");

            }
            catch (Exception exception)
            {
                ServiceLog.WriteException(exception);
            }
            finally
            {
                TimerHelper.Timer.Start();
            }

            ServiceLog.Write("Exiting TimerTick.");
        }

        public static void Start()
        {
            ServiceLog.Write("Service started.");
            TimerHelper.Timer.Start();
        }

        public static void Stop()
        {
            TimerHelper.Timer.Stop();
            ServiceLog.Write("Service stopped.");
        }
    }
}
