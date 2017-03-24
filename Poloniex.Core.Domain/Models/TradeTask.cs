using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poloniex.Core.Domain.Models
{
    public class TradeTask
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TradeTaskId { get; set; }

        [Required, MaxLength(32)]
        public string CurrencyPair { get; set; }

        [Required]
        public int Interval { get; set; }

        // Foreign key
        [Key, ForeignKey("Task")]
        public Guid TaskId { get; set; }

        // Navigation properties
        public Task Task { get; set; }
    }
}
