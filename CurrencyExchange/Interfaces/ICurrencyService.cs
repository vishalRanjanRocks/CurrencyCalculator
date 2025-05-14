using CurrencyExchange.Models;

namespace CurrencyExchange.Interfaces
{
    public interface ICurrencyService
    {
        Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency);
        Task<PeriodicExchangeRateResponse> GetHistoricalRatesAsync(string baseCurrency, DateTime from, DateTime to, int page, int pageSize);
        Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount);
    }
}
