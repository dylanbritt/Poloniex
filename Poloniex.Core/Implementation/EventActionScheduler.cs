using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Domain.Models;
using Poloniex.Data.Contexts;
using Poloniex.Log;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Poloniex.Core.Implementation
{
    public class EventActionScheduler
    {
        public List<EventAction> _EventActionsToStart { get; set; }

        public void PollForEventActionsToStart()
        {
            using (var db = new PoloniexContext())
            {
                var tmpDateTime = DateTime.UtcNow.AddMinutes(-3);
                _EventActionsToStart = db.EventActions.Where(x => x.EventActionStatus == EventActionStatus.RequestToStart && x.Task.TaskLoop.LoopStartedDateTime < tmpDateTime).ToList();
            }
        }

        public void StartEventActions()
        {
            foreach (var ea in _EventActionsToStart)
            {
                switch (ea.EventActionType)
                {
                    case EventActionType.MovingAverage:
                        MovingAverageManager.InitEmaBySma(ea.EventActionId);
                        ea.Action = MovingAverageManager.UpdateEma;
                        break;
                }
                var globalStateEvent = new GlobalStateManager().GetTaskLoop(ea.TaskId);
                var eventAction = globalStateEvent.Item3;
                ea.EventActionStatus = EventActionStatus.Started;
                using(var db = new PoloniexContext())
                {
                    db.Entry(ea).State = EntityState.Modified;
                    db.SaveChanges();
                }
                eventAction.Add(ea);
                Logger.Write($"Started {ea.EventActionType} with eventActionId: {ea.EventActionId}", Logger.LogType.ServiceLog);
            }
        }
    }
}
