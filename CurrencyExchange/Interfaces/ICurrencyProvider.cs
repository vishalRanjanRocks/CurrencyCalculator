using CurrencyExchange.Models;

namespace CurrencyExchange.Interfaces
{
    public interface ICurrencyProvider
    {
        Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency);
        Task<PeriodicExchangeRateResponse> GetHistoricalRatesAsync(string baseCurrency, string from, string to);
        Task<ExchangeRateResponse> ConvertAsync(string fromCurrency, string toCurrency);
    }
}
