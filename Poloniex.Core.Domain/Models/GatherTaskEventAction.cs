using System;

namespace Poloniex.Core.Domain.Models
{
    public class GatherTaskEventAction
    {
        public Guid TaskId { get; set; }

        public Guid EventActionId { get; set; }

        public string EventType { get; set; }

        public Action Action { get; set; }
    }
}
