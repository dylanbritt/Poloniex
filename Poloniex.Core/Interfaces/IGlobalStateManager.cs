using Poloniex.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Poloniex.Core.Interfaces
{
    public interface IGlobalStateManager
    {
        void AddTaskLoop(TaskLoop taskLoop, Timer timer, List<EventAction> eventActions);

        Tuple<TaskLoop, Timer, List<EventAction>> GetTaskLoop(Guid taskId);

        Tuple<TaskLoop, Timer, List<EventAction>> RemoveTaskLoop(Guid taskId);
    }
}
