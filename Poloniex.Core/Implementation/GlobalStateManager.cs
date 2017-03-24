using Poloniex.Core.Domain.Models;
using Poloniex.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Poloniex.Core.Implementation
{
    public class GlobalStateManager : IGlobalStateManager
    {
        public void AddTaskLoop(TaskLoop taskLoop, Timer timer)
        {
            GlobalStateManagerHelper.TaskLoops.Add(new Tuple<TaskLoop, Timer>(taskLoop, timer));
        }

        public Tuple<TaskLoop, Timer> RemoveTaskLoop(Guid taskId)
        {
            Tuple<TaskLoop, Timer> result = null;

            for (int i = 0; i < GlobalStateManagerHelper.TaskLoops.Count; i++)
            {
                if (GlobalStateManagerHelper.TaskLoops[i].Item1.TaskId == taskId)
                {
                    var taskLoop = GlobalStateManagerHelper.TaskLoops[i].Item1;
                    var timer = GlobalStateManagerHelper.TaskLoops[i].Item2;
                    GlobalStateManagerHelper.TaskLoops.RemoveAt(i);
                    result = new Tuple<TaskLoop, Timer>(taskLoop, timer);
                    break;
                }
            }

            return result;
        }
    }

    public static class GlobalStateManagerHelper
    {
        public static List<Tuple<TaskLoop, Timer>> TaskLoops = new List<Tuple<TaskLoop, Timer>>();
    }
}
