using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poloniex.Core.Domain.Models
{
    public class TradeSignalConfiguration
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TradeSignalConfigurationId { get; set; }

        [Required]
        public decimal StopLossPercentageUpper { get; set; }

        [Required]
        public decimal StopLossPercentageLower { get; set; }

        [Required]
        public bool IsStopLossTailing { get; set; }

        // Foriegn Keys
        [Key, ForeignKey("TradeSignalEventAction")]
        public Guid EventActionId { get; set; }

        // Navigation Properties
        public virtual TradeSignalEventAction TradeSignalEventAction { get; set; }
    }
}
