using CurrencyExchange.Interfaces;
using CurrencyExchange.Models;

namespace CurrencyExchange.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyProvider _provider;
        private readonly string[] _excludedCurrencies = { "TRY", "PLN", "THB", "MXN" };

        public CurrencyService(ICurrencyProvider provider)
        {
            _provider = provider;
        }

        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency)
        {
            var data = await _provider.GetLatestRatesAsync(baseCurrency);
            return data;
        }

        public async Task<decimal> ConvertAsync(string from, string to, decimal amount)
        {
            if (_excludedCurrencies.Contains(from) || _excludedCurrencies.Contains(to))
                throw new ArgumentException("Unsupported currency.");

            // Assuming _provider.ConvertAsync returns a decimal value
            var result = await _provider.ConvertAsync(from, to);
            var convertedValue = result.Rates[to.ToUpper()] * amount;
            return Math.Round(convertedValue, 2);
        }

        public async Task<PeriodicExchangeRateResponse> GetHistoricalRatesAsync(string baseCurrency, DateTime from, DateTime to, int page, int pageSize)
        {
           
            // Pass the DateTime arguments directly instead of converting them to strings
            var data = await _provider.GetHistoricalRatesAsync(baseCurrency, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"));
            if (data == null)
                throw new Exception("No data found for the given date range.");
            else
            {
                var filteredData = data.Rates
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Convert the filtered list back to a dictionary

                if (filteredData.Count == 0)
                    throw new Exception("No data found for the given date range.");
                data.Rates = filteredData;
            }
            return data;
        }
    }
}
