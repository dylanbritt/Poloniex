using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poloniex.Core.Domain.Models
{
    public class TradeSignalOrder
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TradeSignalOrderId { get; set; }

        [Required, MaxLength(16)]
        public string CurrencyPair { get; set; }

        [Required, MaxLength(32)]
        public string TradeOrderType { get; set; }

        [Required]
        public decimal LastValueAtRequest { get; set; }

        public decimal? LastValueAtProcessing { get; set; }

        public decimal? PlaceValueTradedAt { get; set; }

        public decimal? MoveValueTradedAt { get; set; }

        [Required]
        public bool IsProcessed { get; set; }

        [Required]
        public bool InProgress { get; set; }

        [Required]
        public DateTime OrderRequestedDateTime { get; set; }

        public DateTime? OrderCompletedDateTime { get; set; }

        [Required]
        public Guid CreatedByEventActionId { get; set; }

        public Guid? ProcessedByEventActionId { get; set; }
    }
}
