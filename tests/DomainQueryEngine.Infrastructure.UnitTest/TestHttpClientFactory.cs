using Polly;
using HttpClientFactory = DomainQueryEngine.Http.HttpClientFactory;
using IHttpClientFactory = DomainQueryEngine.Http.IHttpClientFactory;

namespace DomainQueryEngine.Infrastructure.Test
{
    /// <summary>
    /// Builds the project's <see cref="IHttpClientFactory"/> for tests, without retries.
    /// </summary>
    public static class TestHttpClientFactory
    {
        public static IHttpClientFactory Create()
            => new HttpClientFactory(Policy.NoOpAsync<HttpResponseMessage>());
    }
}
