using System.Collections.Generic;

namespace Poloniex.Core.Domain.Models
{
    public class TradeResult
    {
        public long orderNumber { get; set; }

        public List<ResultingTrade> resultingTrades { get; set; }
    }
}
