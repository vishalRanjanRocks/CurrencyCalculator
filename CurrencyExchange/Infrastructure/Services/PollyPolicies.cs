using Polly.Extensions.Http;
using Polly;
using System.Net;

namespace CurrencyExchange.Infrastructure.Services
{
    public static class PollyPolicies
    {
       public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"⚠️ Retry {retryAttempt} after {timespan.TotalSeconds}s due to {outcome.Result?.StatusCode}");
                    });
        }

       public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, breakDelay) =>
                    {
                        Console.WriteLine($"🚨 Circuit broken! Pausing for {breakDelay.TotalSeconds}s.");
                    },
                    onReset: () => Console.WriteLine("✅ Circuit reset. Requests will flow again."),
                    onHalfOpen: () => Console.WriteLine("🔁 Testing connection..."));
        }
    }
}
