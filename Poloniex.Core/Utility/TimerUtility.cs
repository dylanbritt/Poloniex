using System;

namespace Poloniex.Core.Utility
{
    public static class TimerUtility
    {
        public static int GetAdjustedInterval(int interval)
        {
            DateTime now = DateTime.UtcNow;
            DateTime next = now.AddSeconds(interval);
            next = next.AddMilliseconds(-next.Millisecond);
            if (interval % 5 == 0)
            {
                if (next.Second == 9 || next.Second == 4)
                {
                    next = next.AddSeconds(1);
                }
                if (next.Second == 1 || next.Second == 6)
                {
                    next = next.AddSeconds(-1);
                }
            }
            next = next.AddMilliseconds(5);
            return (int)(next - now).TotalMilliseconds;
        }
    }
}
