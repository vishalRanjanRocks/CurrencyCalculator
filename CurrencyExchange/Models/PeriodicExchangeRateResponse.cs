using System.Text.Json.Serialization;

namespace CurrencyExchange.Models
{
    public class PeriodicExchangeRateResponse
    {

        [JsonPropertyName("base")]
        public string Base { get; set; } = string.Empty;

        [JsonPropertyName("start_date")]
        public string StartDate { get; set; } = string.Empty;
        [JsonPropertyName("End_date")]
        public string EndDate { get; set; } = string.Empty;

        [JsonPropertyName("rates")]
        public Dictionary<string, Dictionary<string, double>> Rates { get; set; } = new();
    }
}
