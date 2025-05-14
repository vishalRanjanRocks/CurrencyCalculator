using CurrencyExchange.Interfaces;
using CurrencyExchange.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace CurrencyExchange.Services
{
    public class FrankfurterProvider : ICurrencyProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FrankfurterProvider> _logger;
        private readonly IMemoryCache _cache;

        public FrankfurterProvider(HttpClient httpClient, ILogger<FrankfurterProvider> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ExchangeRateResponse> ConvertAsync(string fromCurrency, string toCurrency)
        {
            var cacheKey = $"convert_{fromCurrency}_{toCurrency}";

            if (_cache.TryGetValue(cacheKey, out ExchangeRateResponse? cachedResponse))
            {
                return cachedResponse!;
            }

            var response = await _httpClient.GetAsync($"https://api.frankfurter.dev/v1/latest?base={fromCurrency}&symbols={toCurrency}");
            response.EnsureSuccessStatusCode();

            if (response.Content == null)
            {
                _logger.LogError("Response content is null for request: {Url}", response.RequestMessage?.RequestUri);
                throw new NullReferenceException("Response content is null.");
            }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogError("Response content is empty or whitespace for request: {Url}", response.RequestMessage?.RequestUri);
                throw new JsonException("Response content is empty or invalid JSON.");
            }

            var result = JsonSerializer.Deserialize<ExchangeRateResponse>(json);
            if (result == null)
            {
                _logger.LogError("Failed to deserialize response content for request: {Url}", response.RequestMessage?.RequestUri);
                throw new JsonException("Failed to deserialize response content.");
            }

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }


        public async Task<PeriodicExchangeRateResponse> GetHistoricalRatesAsync(string baseCurrency, string from, string to)
        {
            var cacheKey = $"historical_{baseCurrency}_{from}_{to}";

            if (_cache.TryGetValue(cacheKey, out PeriodicExchangeRateResponse? cachedData)) // Use nullable type for cachedData
            {
                return cachedData!;
            }
            var response = await _httpClient.GetAsync($"https://api.frankfurter.dev/v1/{from}..{to}?base={baseCurrency}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<PeriodicExchangeRateResponse>(json);
            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(10));
            return data!;
        }

        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency)
        {
            var cacheKey = $"latest_{baseCurrency.ToUpper()}";
            if (_cache.TryGetValue(cacheKey, out ExchangeRateResponse? cachedResponse))
            {
                _logger.LogInformation("✅ Cache hit for {BaseCurrency}", baseCurrency);
                return cachedResponse!;
            }
            _logger.LogInformation("⏳ Cache miss for {BaseCurrency}. Calling Frankfurter API.", baseCurrency);
            var response = await _httpClient.GetAsync($"https://api.frankfurter.dev/v1/latest?from={baseCurrency}");
            response.EnsureSuccessStatusCode();
           
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<ExchangeRateResponse>(json);
            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(10));
            return data!;
        }
    }

}
