using System;
using System.Collections.Generic;

namespace Poloniex.Core.Domain
{
    public static class TradeOrderType
    {
        public const string buy = "buy";
        public const string sell = "sell";
    }

    public class ReturnTradeHistoryResult
    {
        public List<TradeOrder> TradeOrders { get; set; }

        public class TradeOrder
        {
            public DateTime date { get; set; }
            public string type { get; set; }
            public decimal rate { get; set; }
        }
    }
}
