using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poloniex.Core.Domain.Models
{
    public class CryptoCurrencyDataPoint
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid CryptoCurrencyDataPointId { get; set; }

        [Required, MaxLength(16)]
        public string Currency { get; set; }

        [Required]
        public int Interval { get; set; }

        [Required]
        public DateTime ClosingDateTime { get; set; }

        [Required]
        public decimal ClosingValue { get; set; }

        [Required]
        public DateTime CreatedDateTime { get; set; }
    }
}
