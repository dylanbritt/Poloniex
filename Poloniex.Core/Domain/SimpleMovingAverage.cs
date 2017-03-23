using System;
using System.ComponentModel.DataAnnotations;

namespace Poloniex.Core.Domain
{
    public class SimpleMovingAverage
    {
        public Guid SimpleMovingAverageId { get; set; }

        [Required, MaxLength(16)]
        public string Currency { get; set; }

        [Required]
        public int Interval { get; set; }

        [Required]
        public DateTime ClosingDateTime { get; set; }

        [Required]
        public decimal ClosingValue { get; set; }
    }
}
