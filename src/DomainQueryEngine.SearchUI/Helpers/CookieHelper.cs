using DomainQueryEngine.SearchUI.Models;
using System.Security.Cryptography;

namespace DomainQueryEngine.SearchUI.Helpers
{
    public class CookieHelper
    {
        public static CookieDto CreateCookie()
        {
            CookieDto cookie = new CookieDto();
            cookie.Options = new CookieOptions();

            cookie.Options.Expires = DateTime.Now.AddMinutes(20);
            cookie.Options.HttpOnly = false;

            var hmac = new HMACSHA256();
            cookie.Token = Convert.ToBase64String(hmac.Key);

            return cookie;
        }
    }
}
