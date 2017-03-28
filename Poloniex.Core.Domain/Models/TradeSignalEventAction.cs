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

        [Required, MaxLength(16)]
        public string CurrencyPair { get; set; }

        [Required]
        public int SignalMovingAverageInterval { get; set; }

        [Required]
        public int BaseMovingAverageInterval { get; set; }

        // Foriegn Keys
        [Key, ForeignKey("EventAction")]
        public Guid EventActionId { get; set; }

        // Navigation Properties
        public virtual EventAction EventAction { get; set; }
    }
}
