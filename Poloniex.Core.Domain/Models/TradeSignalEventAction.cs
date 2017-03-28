using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poloniex.Core.Domain.Models
{
    public class TradeSignalEventAction
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TradeSignalEventActionId { get; set; }

        [Required, MaxLength(32)]
        public string TradeSignalEventActionType { get; set; }

        // Foriegn Keys
        [Key, ForeignKey("EventAction")]
        public Guid EventActionId { get; set; }

        [ForeignKey("SignalMovingAverage")]
        public Guid? SignalMovingAverageId { get; set; }

        [ForeignKey("BaseMovingAverage")]
        public Guid? BaseMovingAverageId { get; set; }

        // Navigation Properties
        public virtual EventAction EventAction { get; set; }

        public virtual MovingAverageEventAction SignalMovingAverage { get; set; }

        public virtual MovingAverageEventAction BaseMovingAverage { get; set; }
    }
}
