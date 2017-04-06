using Poloniex.Core.Domain.Constants;

namespace Poloniex.Core.Utility
{
    public class CurrencyUtility
    {
        public static string GetCurrencyFromUsdtCurrencyPair(string currencyPair)
        {
            string result = null;

            switch (currencyPair)
            {
                case CurrencyPairConstants.USDT_BTC:
                    result = CurrencyConstants.BTC;
                    break;
                case CurrencyPairConstants.USDT_ETH:
                    result = CurrencyConstants.ETH;
                    break;
            }

            return result;
        }
    }
}
