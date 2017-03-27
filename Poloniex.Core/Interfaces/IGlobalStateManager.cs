using Poloniex.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Poloniex.Core.Interfaces
{
    public interface IGlobalStateManager
    {
        void AddTaskLoop(TaskLoop taskLoop, Timer timer, List<GatherTaskEventAction> eventActions);

        Tuple<TaskLoop, Timer, List<GatherTaskEventAction>> GetTaskLoop(Guid taskId);

        Tuple<TaskLoop, Timer, List<GatherTaskEventAction>> RemoveTaskLoop(Guid taskId);
    }
}
