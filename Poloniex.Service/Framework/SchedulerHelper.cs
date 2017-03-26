using Poloniex.Core.Implementation;
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
                        _timer.Interval = ConfigurationHelper.TimerTickInterval * 1000;
                        _timer.Elapsed += new ElapsedEventHandler(TimerTick);
                    }

                    return _timer;
                }
            }
        }

        public static int SchedulerId = -1;

        public static void TimerTick(object sender, ElapsedEventArgs e)
        {
            Logger.Write("Entering TimerTick.");

            try
            {
                //TimerHelper.Timer.Stop();
                SchedulerId = (SchedulerId + 1) % 4;
                Logger.Write($"SchedulerId: {SchedulerId}");

                var tls = new TaskLoopScheduler();
                switch (SchedulerId)
                {
                    case 0:
                        tls.PollForTasksToStart();
                        tls.StartTasks();
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        tls.PollForTasksToStop();
                        tls.StopTasks();
                        break;
                }

                Logger.Write($"{new GlobalStateManager().GetCount()} TaskLoops running: {new GlobalStateManager().ToString()}");
            }
            catch (Exception exception)
            {
                Logger.WriteException(exception);
            }
            finally
            {
                //TimerHelper.Timer.Start();
            }

            Logger.Write("Exiting TimerTick.");
        }

        public static void Start()
        {
            Logger.Write("Service started.");
            Logger.Write("Syncing TimerTick.");
            while (DateTime.UtcNow.Second != (60 - ConfigurationHelper.TimerTickInterval) % 60) ;
            Logger.Write("TimerTick synced.");
            TimerHelper.Timer.Start();
        }

        public static void Stop()
        {
            new GlobalStateManager().ClearTaskLoops();
            TaskLoopScheduler.Terminate();

            TimerHelper.Timer.Stop();
            Logger.Write("Service stopped.");
        }
    }
}
