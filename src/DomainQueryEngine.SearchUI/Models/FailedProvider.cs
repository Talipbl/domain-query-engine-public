namespace DomainQueryEngine.SearchUI.Models
{
    /// <param name="ReasonKey">Localization key of the reason, e.g. "Error.NotFound".</param>
    public sealed record FailedProvider(string Name, string ReasonKey);
}
