using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CurrencyExchange.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;
using CurrencyExchange;
using System.Text;

namespace CurrencyExchange.Tests.IntegrationTests
{

    public class ExchangeRatesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ExchangeRatesControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetLatest_ShouldReturnOk_WithDefaultBaseCurrency()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/ExchangeRates/latest");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Convert_ShouldReturnOk_WithValidRequest()
        {
            var request = new CurrencyConvertRequest
            {
                From = "USD",
                To = "INR",
                Amount = 100
            };

            var response = await _client.PostAsJsonAsync("/api/v1/ExchangeRates/convert", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, decimal>>();
            result.Should().ContainKey("convertedAmount");
            result["convertedAmount"].Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Convert_ShouldReturnBadRequest_WithInvalidAmount()
        {
            var request = new CurrencyConvertRequest
            {
                From = "USD",
                To = "INR",
                Amount = 0
            };

            var response = await _client.PostAsJsonAsync("/api/v1/ExchangeRates/convert", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
