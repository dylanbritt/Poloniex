namespace Poloniex.Core.Utility
{
    public class CurrencyUtility
    {
        public static string GetCurrencyFromUsdtCurrencyPair(string currencyPair)
        {
            string result = null;

            result = currencyPair.Substring(5);

            return result;
        }
    }
}
