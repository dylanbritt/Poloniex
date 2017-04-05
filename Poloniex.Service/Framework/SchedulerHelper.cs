using Poloniex.Core.Implementation;
using Poloniex.Core.Utility;
using Poloniex.Log;
using System;
using System.Timers;

namespace Poloniex.Service.Framework
{
    public static class SchedulerHelper
    {


        private static class TimerHelper
        {
            private static int? _timerTickInterval { get; set; }
            public static int TimerTickInterval
            {
                get
                {
                    if (_timerTickInterval == null)
                    {
                        _timerTickInterval = ConfigurationHelper.TimerTickInterval;
                    }
                    return (int)_timerTickInterval;
                }
            }

            private static Timer _timer = null;

            public static Timer Timer
            {
                get
                {
                    if (_timer == null)
                    {
                        _timer = new Timer();
                        _timer.Interval = TimerTickInterval * 1000;
                        _timer.AutoReset = false;
                        _timer.Elapsed += new ElapsedEventHandler(TimerTick);
                    }

                    return _timer;
                }
            }
        }

        public static int SchedulerId = -1;

        public static void TimerTick(object sender, ElapsedEventArgs e)
        {
            Logger.Write("Entering TimerTick.", Logger.LogType.ServiceLog);

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    //TimerHelper.Timer.Stop();
                    SchedulerId = (SchedulerId + 1) % 4;
                    Logger.Write($"SchedulerId: {SchedulerId}", Logger.LogType.ServiceLog);

                    var tls = new TaskLoopScheduler();
                    var eas = new EventActionScheduler();
                    switch (SchedulerId)
                    {
                        case 0:
                            tls.PollForTasksToStart();
                            tls.StartTasks();
                            break;
                        case 1:
                            eas.PollForEventActionsToStart();
                            eas.StartEventActions();
                            break;
                        case 2:
                            tls.PollForTasksToStop();
                            tls.StopTasks();
                            break;
                        case 3:
                            eas.PollForEventActionsToStop();
                            eas.StopEventActions();
                            break;
                    }

                    Logger.Write($"{new GlobalStateManager().GetCount()} TaskLoop(s) running: {new GlobalStateManager().ToString()}", Logger.LogType.ServiceLog);
                }
                catch (Exception exception)
                {
                    Logger.WriteException(exception);
                }
                finally
                {
                    //TimerHelper.Timer.Start();
                }
            });

            Logger.Write("Exiting TimerTick.", Logger.LogType.ServiceLog);

            TimerHelper.Timer.Interval = TimerUtility.GetAdjustedInterval(TimerHelper.TimerTickInterval);
            TimerHelper.Timer.Start();
        }

        public static void Start()
        {
            Logger.Write("Service started.", Logger.LogType.ServiceLog);
            Logger.Write("Syncing TimerTick.", Logger.LogType.ServiceLog);
            while (DateTime.UtcNow.Second != (60 - ConfigurationHelper.TimerTickInterval) % 60) ;
            Logger.Write("TimerTick synced.", Logger.LogType.ServiceLog);
            TimerHelper.Timer.Start();
        }

        public static void Stop()
        {
            new GlobalStateManager().ClearTaskLoops();
            TaskLoopScheduler.Terminate();

            TimerHelper.Timer.Stop();
            Logger.Write("Service stopped.", Logger.LogType.ServiceLog);
        }
    }
}
