using System;

namespace Poloniex.Core.Domain.Models
{
    public class TradeSignalEventOptions
    {
        public Guid TradeSignalEventOptionsId { get; set; }
        public int MacdSignalInterval { get; set; }
        public int ShorterInterval { get; set; }
        public int LongerInterval { get; set; }
        public decimal StopLossPercentage { get; set; }
        public bool EnableStopLoss { get; set; }
        public string StopLossType { get; set; }
        public decimal? DecayPercentage { get; set; }
    }
}
