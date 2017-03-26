using System.Collections.Generic;

namespace Poloniex.Core.Domain.Models
{
    public class BuySellTradeResult
    {
        public long orderNumber { get; set; }

        public List<BuySellResultingTrade> resultingTrades { get; set; }
    }
}
