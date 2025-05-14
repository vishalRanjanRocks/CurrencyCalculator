using CurrencyExchange.Interfaces;
using CurrencyExchange.Models;
using CurrencyExchange.Services;
using FluentAssertions;
using Moq;

namespace CurrencyExchange.Tests.Services
{
    public class CurrencyServiceTests
    {
        private readonly Mock<ICurrencyProvider> _providerMock;
        private readonly CurrencyService _service;

        public CurrencyServiceTests()
        {
            _providerMock = new Mock<ICurrencyProvider>();
            _service = new CurrencyService(_providerMock.Object);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldReturnRates()
        {
            var expected = new ExchangeRateResponse { Base = "EUR" };
            _providerMock.Setup(p => p.GetLatestRatesAsync("EUR")).ReturnsAsync(expected);

            var result = await _service.GetLatestRatesAsync("EUR");

            result.Should().Be(expected);
            _providerMock.Verify(p => p.GetLatestRatesAsync("EUR"), Times.Once);
        }

        [Fact]
        public async Task ConvertAsync_ShouldReturnConvertedValue()
        {
            var rateResponse = new ExchangeRateResponse
            {
                Base = "USD",
                Rates = new Dictionary<string, decimal> { { "EUR", 0.92m } }
            };

            _providerMock.Setup(p => p.ConvertAsync("USD", "EUR")).ReturnsAsync(rateResponse);

            var result = await _service.ConvertAsync("USD", "EUR", 100);

            result.Should().Be(92);
        }

        [Theory]
        [InlineData("TRY", "USD")]
        [InlineData("USD", "PLN")]
        public async Task ConvertAsync_WithExcludedCurrency_ShouldThrow(string from, string to)
        {
            Func<Task> act = async () => await _service.ConvertAsync(from, to, 100);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Unsupported currency.");
        }

        [Fact]
        public async Task GetHistoricalRatesAsync_ShouldReturnPaginatedResult()
        {
            var rates = new Dictionary<string, Dictionary<string, double>>
        {
            { "2024-01-01", new Dictionary<string, double> { { "USD", 1.1 } } },
            { "2024-01-02", new Dictionary<string, double> { { "USD", 1.2 } } },
            { "2024-01-03", new Dictionary<string, double> { { "USD", 1.3 } } },
            { "2024-01-04", new Dictionary<string, double> { { "USD", 1.4 } } }
        };

            var response = new PeriodicExchangeRateResponse { Base = "EUR", Rates = rates };

            _providerMock.Setup(p => p.GetHistoricalRatesAsync("EUR", "2024-01-01", "2024-01-10"))
                         .ReturnsAsync(response);

            var result = await _service.GetHistoricalRatesAsync("EUR", new DateTime(2024, 1, 1), new DateTime(2024, 1, 10), page: 1, pageSize: 2);

            result.Rates.Should().HaveCount(2);
            result.Base.Should().Be("EUR");
        }

        [Fact]
        public async Task GetHistoricalRatesAsync_ReturnsEmptyPage_ShouldThrow()
        {
            var rates = new Dictionary<string, Dictionary<string, double>>
        {
            { "2024-01-01", new Dictionary<string, double> { { "USD", 1.1 } } }
        };

            var response = new PeriodicExchangeRateResponse { Base = "EUR", Rates = rates };

            _providerMock.Setup(p => p.GetHistoricalRatesAsync("EUR", "2024-01-01", "2024-01-02"))
                         .ReturnsAsync(response);

            // Page size of 2 but only 1 record => page 2 = empty
            Func<Task> act = async () => await _service.GetHistoricalRatesAsync("EUR", new DateTime(2024, 1, 1), new DateTime(2024, 1, 2), page: 2, pageSize: 2);

            await act.Should().ThrowAsync<Exception>().WithMessage("No data found for the given date range.");
        }

        [Fact]
        public async Task GetHistoricalRatesAsync_WithNullResult_ShouldThrow()
        {
            _providerMock.Setup(p => p.GetHistoricalRatesAsync("EUR", "2024-01-01", "2024-01-02"))
                         .ReturnsAsync((PeriodicExchangeRateResponse?)null);

            Func<Task> act = async () => await _service.GetHistoricalRatesAsync("EUR", new DateTime(2024, 1, 1), new DateTime(2024, 1, 2), 1, 10);

            await act.Should().ThrowAsync<Exception>().WithMessage("No data found for the given date range.");
        }
    }
}
