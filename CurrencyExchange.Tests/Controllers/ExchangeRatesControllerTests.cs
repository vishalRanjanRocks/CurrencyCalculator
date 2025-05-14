using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using CurrencyExchange.Controllers;
using CurrencyExchange.Interfaces;
using CurrencyExchange.Models;
using System.Threading.Tasks;
using System;

namespace CurrencyExchange.Tests.Controllers
{
    public class ExchangeRatesControllerTests
    {
        private readonly Mock<ICurrencyService> _mockCurrencyService;
        private readonly ExchangeRatesController _controller;

        public ExchangeRatesControllerTests()
        {
            _mockCurrencyService = new Mock<ICurrencyService>();
            _controller = new ExchangeRatesController(_mockCurrencyService.Object);
        }

        [Fact]
        public async Task GetLatest_ReturnsOk_WithRates()
        {
            // Arrange
            var baseCurrency = "EUR";
            var expected = new ExchangeRateResponse { Base = baseCurrency };
            _mockCurrencyService.Setup(s => s.GetLatestRatesAsync(baseCurrency))
                                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetLatest(baseCurrency);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(200);
            okResult?.Value.Should().Be(expected);
        }

        [Fact]
        public async Task GetLatest_ReturnsBadRequest_OnException()
        {
            _mockCurrencyService.Setup(s => s.GetLatestRatesAsync(It.IsAny<string>()))
                                .ThrowsAsync(new Exception("API error"));

            var result = await _controller.GetLatest("EUR");

            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("API error");
        }

        [Fact]
        public async Task Convert_ReturnsOk_WithConvertedAmount()
        {
            var request = new CurrencyConvertRequest { From = "EUR", To = "USD", Amount = 100 };
            _mockCurrencyService.Setup(s => s.ConvertAsync(request.From, request.To, request.Amount))
                                .ReturnsAsync(110);

            var result = await _controller.Convert(request);

            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult?.Value.Should().BeEquivalentTo(new { convertedAmount = 110 });
        }

        [Fact]
        public async Task Convert_ReturnsBadRequest_OnException()
        {
            var request = new CurrencyConvertRequest { From = "EUR", To = "TRY", Amount = 100 };
            _mockCurrencyService.Setup(s => s.ConvertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
                                .ThrowsAsync(new Exception("Invalid currency"));

            var result = await _controller.Convert(request);

            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("Invalid currency");
        }

        [Fact]
        public async Task GetHistoricalExchangeRates_ReturnsOk_WithData()
        {
            var request = new HistoricalExchangeRequest
            {
                BaseCurrency = "EUR",
                Start_Date = DateTime.Parse("2024-01-01"),
                End_Date = DateTime.Parse("2024-01-05"),
                PageNumber = 1,
                pageSize = 10
            };

            var expected = new PeriodicExchangeRateResponse { Base = "EUR" };

            _mockCurrencyService.Setup(s => s.GetHistoricalRatesAsync(request.BaseCurrency, request.Start_Date, request.End_Date, request.PageNumber, request.pageSize))
                                .ReturnsAsync(expected);

            var result = await _controller.GetHistoricalExchangeRates(request);

            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult?.Value.Should().Be(expected);
        }

        [Fact]
        public async Task GetHistoricalExchangeRates_ReturnsNotFound_WhenDataNull()
        {
            var request = new HistoricalExchangeRequest
            {
                BaseCurrency = "EUR",
                Start_Date = DateTime.Parse("2024-01-01"),
                End_Date = DateTime.Parse("2024-01-05"),
                PageNumber = 1,
                pageSize = 10
            };

            _mockCurrencyService.Setup(s => s.GetHistoricalRatesAsync(request.BaseCurrency, request.Start_Date, request.End_Date, request.PageNumber, request.pageSize))
                                .ReturnsAsync((PeriodicExchangeRateResponse?)null);

            var result = await _controller.GetHistoricalExchangeRates(request);

            var notFound = result as NotFoundObjectResult;
            notFound.Should().NotBeNull();
            notFound?.Value.Should().Be("No data found for the given date range.");
        }

        [Fact]
        public async Task GetHistoricalExchangeRates_ReturnsBadRequest_OnException()
        {
            var request = new HistoricalExchangeRequest
            {
                BaseCurrency = "EUR",
                Start_Date = DateTime.Parse("2024-01-01"),
                End_Date = DateTime.Parse("2024-01-05"),
                PageNumber = 1,
                pageSize = 10
            };

            _mockCurrencyService.Setup(s => s.GetHistoricalRatesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                                .ThrowsAsync(new Exception("API failure"));

            var result = await _controller.GetHistoricalExchangeRates(request);

            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("API failure");
        }

        [Fact]
        public async Task Convert_ReturnsBadRequest_WhenRequestIsNull()
        {
            var result = await _controller.Convert(null);

            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("Invalid request.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task Convert_ReturnsBadRequest_WhenAmountInvalid(decimal amount)
        {
            var request = new CurrencyConvertRequest { From = "USD", To = "EUR", Amount = amount };

            var result = await _controller.Convert(request);

            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("Invalid amount.");
        }

        [Fact]
        public async Task GetHistorical_ReturnsBadRequest_WhenDatesMissing()
        {
            var request = new HistoricalExchangeRequest
            {
                BaseCurrency = "EUR",
                Start_Date = default(DateTime), // Use default value for DateTime
                End_Date = default(DateTime),  // Use default value for DateTime
                PageNumber = 1,
                pageSize = 10
            };

            var result = await _controller.GetHistoricalExchangeRates(request);

            var badRequest = result as NotFoundObjectResult;
            badRequest.Should().NotBeNull();
        }

        [Fact]
        public async Task GetLatest_CallsCurrencyServiceExactlyOnce()
        {
            var baseCurrency = "EUR";
            _mockCurrencyService.Setup(s => s.GetLatestRatesAsync(baseCurrency))
                                .ReturnsAsync(new ExchangeRateResponse());

            await _controller.GetLatest(baseCurrency);

            _mockCurrencyService.Verify(s => s.GetLatestRatesAsync(baseCurrency), Times.Once);
        }

        [Fact]
        public async Task Convert_ReturnsBadRequest_WhenToCurrencyIsBlocked()
        {
            var request = new CurrencyConvertRequest { From = "USD", To = "TRY", Amount = 50 };

            _mockCurrencyService.Setup(s => s.ConvertAsync(request.From, request.To, request.Amount))
                                .ThrowsAsync(new ArgumentException("TRY is not allowed"));

            var result = await _controller.Convert(request);

            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("TRY is not allowed");
        }
    }
}
