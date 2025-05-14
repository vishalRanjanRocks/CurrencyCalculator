namespace CurrencyExchange.Models
{
    public class CurrencyConvertRequest
    {
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
    }
}
