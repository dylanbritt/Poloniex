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

        private static bool EnableLog
        {
            get
            {
                var str = ConfigurationManager.AppSettings[nameof(EnableLog)];
                if (string.IsNullOrWhiteSpace(str))
                {
                    return false;
                }
                return bool.Parse(str);
            }
        }

        public static void Write(string message)
        {
            lock (_syncRoot)
            {
                if (EnableLog)
                {
                    using (var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + $"\\{LOG_NAME}", true))
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
