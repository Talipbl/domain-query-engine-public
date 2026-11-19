using System.Net;
using System.Net.Sockets;
using Polly;
using HttpClientFactory = DomainQueryEngine.Http.HttpClientFactory;

namespace DomainQueryEngine.Infrastructure.Test.Http
{
    public class HttpClientFactoryTests
    {
        [Fact]
        public async Task Retries_Send_A_Fresh_Copy_Of_The_Request()
        {
            var port = GetFreePort();
            using var listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();

            var received = new List<(string Body, string? UserAgent)>();
            var server = Task.Run(async () =>
            {
                for (var attempt = 0; attempt < 3; attempt++)
                {
                    var context = await listener.GetContextAsync();
                    using var reader = new StreamReader(context.Request.InputStream);
                    received.Add((await reader.ReadToEndAsync(), context.Request.UserAgent));

                    // Fail the first two attempts so the policy retries.
                    context.Response.StatusCode = attempt < 2 ? 500 : 200;
                    context.Response.Close();
                }
            });

            var policy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => (int)r.StatusCode >= 500)
                .RetryAsync(2);
            var factory = new HttpClientFactory(policy);

            var request = factory.CreateHttpRequestMessage(HttpMethod.Post, new Uri($"http://localhost:{port}/"));
            request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("domain", "ornek.com") });

            var response = await factory.SendAsync(request);
            await server.WaitAsync(TimeSpan.FromSeconds(10));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, received.Count);
            Assert.All(received, r =>
            {
                Assert.Equal("domain=ornek.com", r.Body);
                Assert.Equal(DomainQueryEngine.Http.HttpClientConstants.UserAgent, r.UserAgent);
            });
        }

        private static int GetFreePort()
        {
            using var socket = new TcpListener(IPAddress.Loopback, 0);
            socket.Start();
            return ((IPEndPoint)socket.LocalEndpoint).Port;
        }
    }
}
