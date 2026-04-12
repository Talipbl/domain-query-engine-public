using DomainQueryEngine.Core;

namespace DomainQueryEngine.Core
{
    public interface IStrategyParser
    {
        List<DomainResult> ParseDomainResult(string rawResponse);
        string GetCsrfTokenFromHtml(string htmlResponse);
    }
}