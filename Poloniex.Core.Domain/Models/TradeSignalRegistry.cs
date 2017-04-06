namespace Poloniex.Core.Domain.Models
{
    public class TradeSignalRegistry
    {
        public TradeSignalRegistry()
        {
            Init = true;
            WasBullish = IsBullish = HasHoldings = ShouldBuy = ShouldSell = false;
        }

        public bool Init { get; set; }
        public bool WasBullish { get; set; }
        public bool IsBullish { get; set; }
        public bool HasHoldings { get; set; }
        public bool ShouldBuy { get; set; }
        public bool ShouldSell { get; set; }

        public decimal BuyValue { get; set; }
    }
}
