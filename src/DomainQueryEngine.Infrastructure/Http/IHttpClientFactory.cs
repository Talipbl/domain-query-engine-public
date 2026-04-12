namespace DomainQueryEngine.Http
{
    public interface IHttpClientFactory
    {
        HttpClient CreateClient();
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default);
        HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, Uri uri);
    }
}
