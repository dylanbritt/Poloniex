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
        public string TradeSignalType { get; set; }

        [Required, MaxLength(16)]
        public string CurrencyPair { get; set; }

        [Required]
        public int ShorterMovingAverageInterval { get; set; }

        [Required]
        public int LongerMovingAverageInterval { get; set; }

        [Required]
        public int MinutesPerInterval { get; set; }

        // Foriegn Keys
        [Key, ForeignKey("EventAction")]
        public Guid EventActionId { get; set; }

        // Navigation Properties
        public virtual EventAction EventAction { get; set; }

        public virtual TradeSignalConfiguration TradeSignalConfiguration { get; set; }
    }
}
