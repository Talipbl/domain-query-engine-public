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
        public static UserAgentModel GetUserAgent(this IHttpContextAccessor httpContextAccessor)
        {
            UserAgentModel model = new UserAgentModel()
            {
                Agent = httpContextAccessor.HttpContext.Request.Headers["User-Agent"],
                Host = httpContextAccessor.HttpContext.Request.Headers["Host"],
            };
            return model;
        }
    }
}
