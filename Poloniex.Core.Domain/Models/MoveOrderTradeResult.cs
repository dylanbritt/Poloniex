using System.Collections.Generic;

namespace Poloniex.Core.Domain.Models
{
    public class MoveOrderTradeResult
    {
        public bool success { get; set; }

        public long orderNumber { get; set; }

        public Dictionary<string, List<BuySellTradeResult>> resultingTrades { get; set; }
    }
}
