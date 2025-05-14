using CurrencyExchange.Interfaces;
using CurrencyExchange.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CurrencyExchange.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public ExchangeRatesController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [EnableRateLimiting("fixed")]
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest([FromQuery] string baseCurrency = "EUR")
        {
            try
            {
                var result = await _currencyService.GetLatestRatesAsync(baseCurrency);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [EnableRateLimiting("fixed")]
        // [Authorize(Roles = "User,Admin")]
        [HttpPost("convert")]
        public async Task<IActionResult> Convert([FromBody] CurrencyConvertRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request.");
            }

            if (request.Amount <= 0)
            {
                return BadRequest("Invalid amount.");
            }

            if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.From))
            {
                return BadRequest("Invalid request.");
            }

            try
            {
                var result = await _currencyService.ConvertAsync(request.From, request.To, request.Amount);
                return Ok(new { convertedAmount = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [ApiVersion("1.0")]
        [EnableRateLimiting("fixed")]
        //  [Authorize(Roles = "User,Admin")]
        [HttpPost("periodic_exchange_rate")]
        public async Task<IActionResult> GetHistoricalExchangeRates([FromBody] HistoricalExchangeRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request.");
            }
            try
            {

                var result = await _currencyService.GetHistoricalRatesAsync(request.BaseCurrency, request.Start_Date, request.End_Date, request.PageNumber=1, request.pageSize=10);
                return result != null ? Ok(result) : NotFound("No data found for the given date range.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

}
