using Poloniex.Core.Domain.Models;
using Poloniex.Core.Interfaces;
using Poloniex.Data.Contexts;
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

        public void PollForTasksToStartOrStop()
        {
            using (var db = new PoloniexContext())
            {
                _tasksToStart = db.TaskLoops.Where(x => x.LoopStatus == "RequestToStart").Include(x => x.Task.GatherTask).Include(x=> x.Task.TradeTask).ToList();
                _tasksToStop = db.TaskLoops.Where(x => x.LoopStatus == "RequestToStop").Include(x => x.Task.GatherTask).Include(x => x.Task.TradeTask).ToList();
            }
        }

        public void StartTasks()
        {
            foreach(var taskLoop in _tasksToStart)
            {
                switch(taskLoop.Task.TaskType)
                {
                    case "":
                        break;
                }
            }
        }

        public void StopTasks()
        {

        }
    }
}
