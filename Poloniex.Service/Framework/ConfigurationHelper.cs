using System;
using System.Configuration;

namespace Poloniex.Service.Framework
{
    public static class ConfigurationHelper
    {
        public static bool EnableLog
        {
            get
            {
                var str = ConfigurationManager.AppSettings[nameof(EnableLog)];
                if (string.IsNullOrWhiteSpace(str))
                {
                    throw new InvalidOperationException($"{nameof(EnableLog)} does not have a valid configuration setting");
                }
                return bool.Parse(str);
            }
        }

        public static int TimerTickInterval
        {
            get
            {
                var str = ConfigurationManager.AppSettings[nameof(TimerTickInterval)];
                if (string.IsNullOrWhiteSpace(str))
                {
                    throw new InvalidOperationException($"{nameof(TimerTickInterval)} does not have a valid configuration setting");
                }
                return int.Parse(str);
            }
        }
    }
}
