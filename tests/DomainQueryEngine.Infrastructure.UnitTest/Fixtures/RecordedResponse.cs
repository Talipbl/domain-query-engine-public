namespace DomainQueryEngine.Infrastructure.Test.Fixtures
{
    /// <summary>
    /// Reads a provider response captured from the live site, stored under Fixtures/Responses.
    /// </summary>
    public static class RecordedResponse
    {
        public static string Read(string fileName)
            => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Responses", fileName));
    }
}
