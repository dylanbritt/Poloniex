using System.Collections.Generic;

namespace Poloniex.Core.Domain.Models
{
    /* todo: not used */
    public class ReturnOpenOrdersResult
    {
        public List<OpenOrder> OpenOrders { get; set; }

        public class OpenOrder
        {
            public long orderNumber { get; set; }

            public string type { get; set; }

            public decimal rate { get; set; }

            public decimal amount { get; set; }

            public decimal total { get; set; }
        }
    }
}
