using System;
using System.ComponentModel.DataAnnotations;

namespace Poloniex.Core.Domain
{
    public class ExponentialMovingAverage
    {
        public Guid ExponentialMovingAverageId { get; set; }

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
