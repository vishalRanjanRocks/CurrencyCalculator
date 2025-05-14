using Microsoft.AspNetCore.Mvc.Testing;

namespace CurrencyExchange.Tests.IntegrationTests
{
    public class RateLimitingTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public RateLimitingTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        //[Fact]
        //public async Task Should_Return_429_After_Exceeding_Request_Limit()
        //{
        //    // First 3 requests should succeed
        //    for (int i = 0; i < 3; i++)
        //    {
        //        var response = await _client.GetAsync("/api/v1/ExchangeRates/latest?baseCurrency=USD");
        //        response.StatusCode.Should().Be(HttpStatusCode.OK);
        //    }

        //    // 4th request should be rate-limited
        //    var rateLimitedResponse = await _client.GetAsync("/api/v1/ExchangeRates/latest?baseCurrency=USD");
        //    rateLimitedResponse.StatusCode.Should().Be((HttpStatusCode)429);
        //}
    }

}
