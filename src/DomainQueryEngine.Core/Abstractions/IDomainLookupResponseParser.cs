using DomainQueryEngine.Core;

namespace DomainQueryEngine.Core
{
    public interface IDomainLookupResponseParser
    {
        List<DomainResult> ParseDomainResult(string rawResponse);
        string GetCsrfTokenFromHtml(string htmlResponse);
    }
}