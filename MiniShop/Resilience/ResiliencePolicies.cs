using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace MiniShop.Resilience
{
    public static class ResiliencePolicies
    {
        public static AsyncRetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} após {timeSpan.TotalSeconds}s devido a: {exception.Message}");
                    });
        }

        public static AsyncCircuitBreakerPolicy GetCircuitBreakerPolicy()
        {
            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (exception, timeSpan) =>
                    {
                        Console.WriteLine($"Circuit aberto por {timeSpan.TotalSeconds}s devido a: {exception.Message}");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("Circuit fechado — serviço recuperado.");
                    });
        }
    }
}