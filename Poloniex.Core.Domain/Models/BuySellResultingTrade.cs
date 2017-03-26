using System;

namespace Poloniex.Core.Domain.Models
{
    public class BuySellResultingTrade
    {
        decimal amount { get; set; }

        DateTime date { get; set; }

        decimal rate { get; set; }

        decimal total { get; set; }

        int tradeID { get; set; }

        string type { get; set; }
    }
}
