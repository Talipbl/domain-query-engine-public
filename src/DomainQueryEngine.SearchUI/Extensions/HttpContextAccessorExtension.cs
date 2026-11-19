using DomainQueryEngine.SearchUI.Helpers;
using DomainQueryEngine.SearchUI.Models;

namespace DomainQueryEngine.SearchUI.Extensions
{
    public static class HttpContextAccessorExtension
    {
        public static string GetOrSetCookie(this IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor.HttpContext.Request.Cookies["Engine.Access.Token"] != null)
                return httpContextAccessor.HttpContext.Request.Cookies["Engine.Access.Token"];
            else
            {
                var cookie = CookieHelper.CreateCookie();
                httpContextAccessor.HttpContext.Response.Cookies.Append("Engine.Access.Token", cookie.Token, cookie.Options);
                return cookie.Token;
            }
        }
    }
}
