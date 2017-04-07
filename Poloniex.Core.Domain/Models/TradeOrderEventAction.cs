using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poloniex.Core.Domain.Models
{
    public class TradeOrderEventAction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TradeOrderEventActionId { get; set; }

        [Required, MaxLength(16)]
        public string CurrencyPair { get; set; }

        // Foriegn Keys
        [Key, ForeignKey("EventAction")]
        public Guid EventActionId { get; set; }

        // Navigation Properties
        public virtual EventAction EventAction { get; set; }
    }
}
