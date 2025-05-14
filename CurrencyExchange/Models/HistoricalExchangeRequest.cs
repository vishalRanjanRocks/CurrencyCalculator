namespace CurrencyExchange.Models
{
    public class HistoricalExchangeRequest
    {
        public string BaseCurrency { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public int PageNumber { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }
}
