using Microsoft.Extensions.Primitives;
using System.Net;

namespace DomainQueryEngine.SearchUI.Helpers
{
    public class NetworkHelper
    {
        public static string GetRequestIp(IHttpContextAccessor _httpContextAccessor)
        {
            string ip = String.Empty;
            if (_httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress != null)
                ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrEmpty(ip))
                ip = GetHeaderValueAs<string>(_httpContextAccessor, "REMOTE_ADDR");

            if (string.IsNullOrEmpty(ip))
                ip = GetHeaderValueAs<string>(_httpContextAccessor, "HTTP_X_FORWARDED_FOR");

            if (string.IsNullOrEmpty(ip))
                ip = GetHeaderValueAs<string>(_httpContextAccessor, "CF-Connecting-IP");

            return ip;
        }

        protected static T GetHeaderValueAs<T>(IHttpContextAccessor _httpContextAccessor, string headerName)
        {
            StringValues values;
            string raw = string.Empty;

            if (_httpContextAccessor.HttpContext?.Request?.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                raw = values.ToString();
                if (!string.IsNullOrEmpty(raw))
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }

        public static string GetHostAddress(string Host)
        {
            try
            {
                var _getHost = Dns.GetHostAddresses(Host).FirstOrDefault().ToString();
                return _getHost;
            }
            catch (Exception Ex)
            {
                return string.Empty;
            }
        }
    }
}
