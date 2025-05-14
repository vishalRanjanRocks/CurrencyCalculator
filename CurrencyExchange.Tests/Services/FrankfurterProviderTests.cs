using System.Net;
using System.Text.Json;
using CurrencyExchange.Models;
using CurrencyExchange.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace CurrencyExchange.Tests.Services
{
    public class FrankfurterProviderTests
    {
        private readonly Mock<HttpMessageHandler> _httpHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly FrankfurterProvider _provider;
        private readonly ILogger<FrankfurterProvider> _logger;

        public FrankfurterProviderTests()
        {
            _httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_httpHandlerMock.Object);
            _memoryCacheMock = new Mock<IMemoryCache>();
            _logger = new LoggerFactory().CreateLogger<FrankfurterProvider>();
            _provider = new FrankfurterProvider(_httpClient, _logger, _memoryCacheMock.Object);
        }

        private static ExchangeRateResponse GetSampleExchangeRate() =>
            new() { Base = "USD", Rates = new Dictionary<string, decimal> { { "EUR", 0.92m } } };

        private static PeriodicExchangeRateResponse GetSamplePeriodicRates() =>
            new() { Base = "USD", Rates = new Dictionary<string, Dictionary<string, double>> { { "2024-01-01", new() { { "EUR", 0.92 } } } } };

        [Fact]
        public async Task ConvertAsync_ShouldReturnFromCache_WhenAvailable()
        {
            var cacheKey = "convert_USD_EUR";
            var expected = GetSampleExchangeRate();
            object outVal = expected;

            _memoryCacheMock.Setup(m => m.TryGetValue(cacheKey, out outVal)).Returns(true);

            //Act
            var result = await _provider.ConvertAsync("USD", "EUR");

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task ConvertAsync_ShouldCallApi_AndCacheResult()
        {
            // Arrange
            var url = "https://api.frankfurter.dev/v1/latest?base=USD&symbols=EUR";
            var expected = GetSampleExchangeRate();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expected))
            };

            _httpHandlerMock.SetupSendAsync(url, httpResponse);

            object dummy;
            _memoryCacheMock.Setup(m => m.TryGetValue("convert_USD_EUR", out dummy)).Returns(false);

            // ✅ Mock the cache entry creation instead of using .Set(...)
            var cacheEntryMock = new Mock<ICacheEntry>();
            cacheEntryMock.SetupAllProperties();
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                            .Returns(cacheEntryMock.Object);

            // Act
            var result = await _provider.ConvertAsync("USD", "EUR");

            // Assert
            result.Should().NotBeNull();
            result.Base.Should().Be("USD");
            result.Rates.Should().ContainKey("EUR");
        }


        [Fact]
        public async Task GetHistoricalRatesAsync_ReturnsCached_WhenPresent()
        {
            var expected = GetSamplePeriodicRates();

            string cacheKey = "historical_USD_2024-01-01_2024-01-05";
            object outVal = expected;

            _memoryCacheMock.Setup(m => m.TryGetValue(cacheKey, out outVal)).Returns(true);

            var result = await _provider.GetHistoricalRatesAsync("USD", "2024-01-01", "2024-01-05");

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetHistoricalRatesAsync_CallsApiAndCaches_WhenCacheMiss()
        {
            // Arrange
            var expected = GetSamplePeriodicRates();
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expected))
            };

            var url = "https://api.frankfurter.dev/v1/2024-01-01..2024-01-05?base=USD";
            _httpHandlerMock.SetupSendAsync(url, mockResponse);

            object dummy;
            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out dummy)).Returns(false);

            // ✅ Correct mocking for caching using CreateEntry
            var cacheEntryMock = new Mock<ICacheEntry>();
            cacheEntryMock.SetupAllProperties();
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                            .Returns(cacheEntryMock.Object);

            // Act
            var result = await _provider.GetHistoricalRatesAsync("USD", "2024-01-01", "2024-01-05");

            // Assert
            result.Should().NotBeNull();
            result.Base.Should().Be("USD");
            result.Rates.Should().ContainKey("2024-01-01");
        }


        [Fact]
        public async Task GetLatestRatesAsync_CallsApiAndCaches()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(GetSampleExchangeRate()))
            };

            var url = "https://api.frankfurter.dev/v1/latest?from=USD";
            _httpHandlerMock.SetupSendAsync(url, mockResponse);

            object dummy;
            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out dummy)).Returns(false);
            // ✅ Correct mocking for caching using CreateEntry
            var cacheEntryMock = new Mock<ICacheEntry>();
            cacheEntryMock.SetupAllProperties();
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                            .Returns(cacheEntryMock.Object);

            var result = await _provider.GetLatestRatesAsync("USD");

            result.Base.Should().Be("USD");
        }

        [Fact]
        public async Task ConvertAsync_ShouldThrow_WhenApiReturnsError()
        {
            var url = "https://api.frankfurter.dev/v1/latest?base=USD&symbols=EUR";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _httpHandlerMock.SetupSendAsync(url, mockResponse);

            object dummy;
            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out dummy)).Returns(false);

            Func<Task> act = async () => await _provider.ConvertAsync("USD", "EUR");

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task ConvertAsync_ShouldThrow_WhenJsonIsInvalid()
        {
            var url = "https://api.frankfurter.dev/v1/latest?base=USD&symbols=EUR";
            var badJson = "{ invalid json }";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(badJson)
            };

            _httpHandlerMock.SetupSendAsync(url, response);

            object dummy;
            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out dummy)).Returns(false);

            Func<Task> act = async () => await _provider.ConvertAsync("USD", "EUR");

            await act.Should().ThrowAsync<JsonException>();
        }
        [Fact]
        public async Task ConvertAsync_ShouldCacheResult_AfterApiCall()
        {
            var cacheKey = "convert_USD_EUR";
            var url = "https://api.frankfurter.dev/v1/latest?base=USD&symbols=EUR";
            var expected = GetSampleExchangeRate();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expected))
            };

            _httpHandlerMock.SetupSendAsync(url, response);

            object dummy;
            _memoryCacheMock.Setup(m => m.TryGetValue(cacheKey, out dummy)).Returns(false);

            // ✅ Proper way to mock caching
            var cacheEntryMock = new Mock<ICacheEntry>();
            cacheEntryMock.SetupAllProperties();
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

            var result = await _provider.ConvertAsync("USD", "EUR");

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task ConvertAsync_ShouldThrow_WhenResponseContentIsNull()
        {
            var url = "https://api.frankfurter.dev/v1/latest?base=USD&symbols=EUR";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = null
            };

            _httpHandlerMock.SetupSendAsync(url, response);

            object dummy;
            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out dummy)).Returns(false);

            Func<Task> act = async () => await _provider.ConvertAsync("USD", "EUR");

            await act.Should().ThrowAsync<JsonException>()
                 .WithMessage("*invalid JSON*"); // Optional message match;
        }
        [Fact]
        public async Task GetHistoricalRatesAsync_ShouldThrow_WhenApiFails()
        {
            var url = "https://api.frankfurter.dev/v1/2024-01-01..2024-01-05?base=USD";
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);

            _httpHandlerMock.SetupSendAsync(url, response);

            object dummy;
            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out dummy)).Returns(false);

            Func<Task> act = async () => await _provider.GetHistoricalRatesAsync("USD", "2024-01-01", "2024-01-05");

            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
