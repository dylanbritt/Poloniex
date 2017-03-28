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
    public class EventActionScheduler
    {

        private readonly IGlobalStateManager _globalStateManager;

        public List<EventAction> _EventActionsToStart { get; set; }
        public List<EventAction> _EventActionsToStop { get; set; }

        public EventActionScheduler()
        {
            _globalStateManager = new GlobalStateManager();
        }

        public EventActionScheduler(IGlobalStateManager globalStateManager)
        {
            _globalStateManager = globalStateManager;
        }

        public void PollForEventActionsToStart()
        {
            using (var db = new PoloniexContext())
            {
                var tmpDateTime = DateTime.UtcNow.AddMinutes(-3);
                _EventActionsToStart = db.EventActions.Where(x => x.EventActionStatus == EventActionStatus.RequestToStart && x.Task.TaskLoop.LoopStartedDateTime < tmpDateTime).ToList();
            }
        }

        public void PollForEventActionsToStop()
        {
            using (var db = new PoloniexContext())
            {
                _EventActionsToStop = db.EventActions.Where(x => x.EventActionStatus == EventActionStatus.RequestToStop).ToList();
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
                    case EventActionType.TradeSignal:
                        ea.Action = TradeSignalManager.ProcessTradeSignalEventAction;
                        break;
                }
                var globalStateEvent = _globalStateManager.GetTaskLoop(ea.TaskId);
                var eventActions = globalStateEvent.Item3;
                ea.EventActionStatus = EventActionStatus.Started;
                using (var db = new PoloniexContext())
                {
                    db.Entry(ea).State = EntityState.Modified;
                    db.SaveChanges();
                }
                eventActions.Add(ea);
                Logger.Write($"Started {ea.EventActionType} with eventActionId: {ea.EventActionId}", Logger.LogType.ServiceLog);
            }
        }

        public void StopEventActions()
        {
            foreach (var ea in _EventActionsToStop)
            {
                var globalStateEvent = _globalStateManager.GetTaskLoop(ea.TaskId);
                var eventActions = globalStateEvent.Item3;
                for (int i = 0; i < eventActions.Count; i++)
                {
                    if (eventActions[i].EventActionId == ea.EventActionId)
                    {
                        eventActions.RemoveAt(i);
                        break;
                    }
                }
                ea.EventActionStatus = EventActionStatus.Stopped;
                using (var db = new PoloniexContext())
                {
                    db.Entry(ea).State = EntityState.Modified;
                    db.SaveChanges();
                }
                Logger.Write($"Stopped {ea.EventActionType} with eventActionId: {ea.EventActionId}", Logger.LogType.ServiceLog);
            }
        }
    }
}
