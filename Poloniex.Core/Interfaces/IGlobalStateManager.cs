using Poloniex.Core.Domain.Models;
using System;
using System.Timers;

namespace Poloniex.Core.Interfaces
{
    public interface IGlobalStateManager
    {
        void AddTaskLoop(TaskLoop taskLoop, Timer timer);

        Tuple<TaskLoop, Timer> RemoveTaskLoop(Guid taskId);
    }
}
