using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poloniex.Core.Domain.Models
{
    public class MovingAverageEventAction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MovingAverageEventActionId { get; set; }

        [Required, MaxLength(32)]
        public string MovingAverageType { get; set; }

        [Required, MaxLength(16)]
        public string CurrencyPair { get; set; }

        [Required]
        public int Interval { get; set; }

        [Required]
        public int MinutesPerInterval { get; set; }

        // Foreign Keys
        [Key, ForeignKey("EventAction")]
        public Guid EventActionId { get; set; }

        // Navigation properites
        public virtual EventAction EventAction { get; set; }
    }
}
