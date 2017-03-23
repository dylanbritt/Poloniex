using System.Web.SessionState;

namespace Poloniex.Presentation.Framework.Session
{
    public static class SessionManager
    {
        private static HttpSessionState _session => System.Web.HttpContext.Current.Session;

        public static bool IsAuthenticated
        {
            get
            {
                if (_session[nameof(IsAuthenticated)] == null)
                {
                    return false;
                }
                return bool.Parse(_session[nameof(IsAuthenticated)].ToString());
            }
            set
            {
                _session[nameof(IsAuthenticated)] = value;
            }
        }
    }
}