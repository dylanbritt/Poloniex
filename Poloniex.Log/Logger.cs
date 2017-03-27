using System;
using System.Configuration;
using System.IO;

namespace Poloniex.Log
{
    public static class Logger
    {
        private static readonly object _syncRoot = new object();
        private const string DATE_TIME_FORMAT = "MM/dd/yyyy hh:mm:ss:fff";
        private static string LOG_NAME = $"{DateTime.UtcNow.ToString("yyyyMMddhhmmssfff")}-Log.txt";

        public static class LogType
        {
            public const string ObjectLifetimeLog = "ObjectLifetimeLog";
            public const string RestLog = "RestLog";
            public const string ServiceLog = "ServiceLog";
            public const string TransactionLog = "TransactionLog";
        }

        private static bool? _enableLog = null;
        private static bool EnableLog
        {
            get
            {
                if (_enableLog == null)
                {
                    var str = ConfigurationManager.AppSettings[nameof(EnableLog)];
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        _enableLog = false;
                    }
                    else
                    {
                        _enableLog = bool.Parse(str);
                    }
                }
                return (bool)_enableLog;
            }
        }

        private static bool? _enableObjectLifetimeLog = null;
        private static bool EnableObjectLifetimeLog
        {
            get
            {
                if (_enableObjectLifetimeLog == null)
                {
                    var str = ConfigurationManager.AppSettings[nameof(EnableObjectLifetimeLog)];
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        _enableObjectLifetimeLog = false;
                    }
                    else
                    {
                        _enableObjectLifetimeLog = bool.Parse(str);
                    }
                }
                return (bool)_enableObjectLifetimeLog;
            }
        }

        private static bool? _enableRestLog = null;
        private static bool EnableRestLog
        {
            get
            {
                if (_enableRestLog == null)
                {
                    var str = ConfigurationManager.AppSettings[nameof(EnableRestLog)];
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        _enableRestLog = false;
                    }
                    else
                    {
                        _enableRestLog = bool.Parse(str);
                    }
                }
                return (bool)_enableRestLog;
            }
        }

        private static bool? _enableServiceLog = null;
        private static bool EnableServiceLog
        {
            get
            {
                if (_enableServiceLog == null)
                {
                    var str = ConfigurationManager.AppSettings[nameof(EnableServiceLog)];
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        _enableServiceLog = false;
                    }
                    else
                    {
                        _enableServiceLog = bool.Parse(str);
                    }
                }
                return (bool)_enableServiceLog;
            }
        }

        private static bool? _enableTransactionLog = null;
        private static bool EnableTransactionLog
        {
            get
            {
                if (_enableTransactionLog == null)
                {
                    var str = ConfigurationManager.AppSettings[nameof(EnableTransactionLog)];
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        _enableTransactionLog = false;
                    }
                    else
                    {
                        _enableTransactionLog = bool.Parse(str);
                    }
                }
                return (bool)_enableTransactionLog;
            }
        }

        private static bool IsLogTypeEnabled(string logType)
        {
            switch (logType)
            {
                case LogType.RestLog:
                    return EnableRestLog;
                case LogType.ServiceLog:
                    return EnableRestLog;
                case LogType.TransactionLog:
                    return EnableRestLog;
                default:
                    return false;
            }
        }

        public static void Write(string message, string logType = null)
        {
            lock (_syncRoot)
            {
                if (EnableLog)
                {
                    if (logType != null && !IsLogTypeEnabled(logType))
                        return;

                    var logName = AppDomain.CurrentDomain.BaseDirectory + (logType == null ? $"\\{LOG_NAME}" : $"\\{logType}-{LOG_NAME}");
                    using (var sw = new StreamWriter(logName, true))
                    {
                        sw.WriteLine(DateTime.Now.ToString(DATE_TIME_FORMAT) + ": " + message.Trim());
                    }
                }
            }
        }

        public static void WriteException(Exception exception)
        {
            lock (_syncRoot)
            {
                using (var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + $"\\{LOG_NAME}", true))
                {
                    var str = DateTime.Now.ToString(DATE_TIME_FORMAT)
                        + ": " + exception.Source.ToString().Trim()
                        + "; " + exception.Message.ToString().Trim();

                    if (exception.InnerException != null)
                    {
                        str += "; InnerException: " + exception.InnerException.Message.ToString().Trim();
                    }

                    str += "; StackTrace: " + exception.StackTrace.ToString().Trim();

                    str += "; ToString: " + exception.ToString().Trim();

                    sw.WriteLine(str);
                }
            }
        }
    }
}
