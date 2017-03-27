using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Domain.Models;
using Poloniex.Core.Interfaces;
using Poloniex.Data.Contexts;
using Poloniex.Log;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Poloniex.Core.Implementation
{
    public class TaskLoopScheduler
    {
        private readonly IGlobalStateManager _globalStateManager;

        private List<TaskLoop> _tasksToStart { get; set; }
        private List<TaskLoop> _tasksToStop { get; set; }

        public TaskLoopScheduler()
        {
            _globalStateManager = new GlobalStateManager();
        }

        public TaskLoopScheduler(IGlobalStateManager globalStateManager)
        {
            _globalStateManager = globalStateManager;
        }

        public void PollForTasksToStart()
        {
            using (var db = new PoloniexContext())
            {
                _tasksToStart = db.TaskLoops.Where(x => x.LoopStatus == LoopStatus.RequestToStart).Include(x => x.Task.GatherTask).Include(x => x.Task.TradeTask).ToList();
            }
        }

        public void PollForTasksToStop()
        {
            using (var db = new PoloniexContext())
            {
                _tasksToStop = db.TaskLoops.Where(x => x.LoopStatus == LoopStatus.RequestToStop).Include(x => x.Task.GatherTask).Include(x => x.Task.TradeTask).ToList();
            }
        }

        public void StartTasks()
        {
            foreach (var taskLoop in _tasksToStart)
            {
                switch (taskLoop.Task.TaskType)
                {
                    case "GatherTask":
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            try
                            {
                                GatherTaskManager.BackFillGatherTaskDataForOneMonth(taskLoop.Task.GatherTask.CurrencyPair);
                            }
                            catch (Exception exception)
                            {
                                Logger.WriteException(exception);
                            }
                        });
                        var eventActions = new List<EventAction>();
                        _globalStateManager.AddTaskLoop(taskLoop, GatherTaskManager.GetGatherTaskTimer(taskLoop.TaskId, eventActions), eventActions);
                        using (var db = new PoloniexContext())
                        {
                            taskLoop.LoopStatus = LoopStatus.Started;
                            taskLoop.LoopStartedDateTime = DateTime.UtcNow;
                            db.Entry(taskLoop).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        Logger.Write($"Started {taskLoop.Task.TaskType} with taskId: {taskLoop.TaskId}", Logger.LogType.ServiceLog);
                        break;
                }
            }
        }

        public void StopTasks()
        {
            foreach (var taskLoop in _tasksToStop)
            {
                switch (taskLoop.Task.TaskType)
                {
                    case "GatherTask":
                        var tuple = _globalStateManager.RemoveTaskLoop(taskLoop.TaskId);
                        tuple.Item2.Stop();
                        using (var db = new PoloniexContext())
                        {
                            taskLoop.LoopStatus = LoopStatus.Stopped;
                            db.Entry(taskLoop).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        var eventActions = tuple.Item3;
                        foreach(var ea in eventActions)
                        {
                            ea.EventActionStatus = EventActionStatus.Stopped;
                            using (var db = new PoloniexContext())
                            {
                                db.Entry(ea).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            Logger.Write($"Stopped {ea.EventActionType} with eventActionId: {ea.EventActionId}", Logger.LogType.ServiceLog);
                        }
                        Logger.Write($"Stopped {taskLoop.Task.TaskType} with taskId: {taskLoop.TaskId}", Logger.LogType.ServiceLog);
                        break;
                }
            }
        }

        public static void Terminate()
        {
            using (var db = new PoloniexContext())
            {
                var taskLoops = db.TaskLoops.Where(x => x.LoopStatus == LoopStatus.Started || x.LoopStatus == LoopStatus.RequestToStop).ToList();
                taskLoops.ForEach(x =>
                {
                    x.LoopStatus = LoopStatus.Stopped;
                });
                db.SaveChanges();
            }
        }
    }
}
