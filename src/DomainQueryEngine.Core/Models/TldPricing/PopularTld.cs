namespace DomainQueryEngine.Core.TldPricing
{
    /// <param name="Tld">Without the leading dot: "com.tr".</param>
    public sealed record PopularTld(string Tld, int Count)
    {
        public string Slug => Tld.Replace('.', '-');
    }
}
