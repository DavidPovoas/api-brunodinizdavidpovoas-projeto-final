using Polly;
using StackExchange.Redis;
using System.Text.Json;
using MiniShop.Resilience;

namespace MiniShop.Cache
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
    }

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _db;
        private readonly Polly.Retry.AsyncRetryPolicy _retryPolicy;
        private readonly Polly.CircuitBreaker.AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
            _retryPolicy = ResiliencePolicies.GetRetryPolicy();
            _circuitBreakerPolicy = ResiliencePolicies.GetCircuitBreakerPolicy();
        }

        public virtual async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                return await _retryPolicy.WrapAsync(_circuitBreakerPolicy).ExecuteAsync(async () =>
                {
                    var value = await _db.StringGetAsync(key);
                    if (value.IsNullOrEmpty) return default;
                    return JsonSerializer.Deserialize<T>(value!);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cache GET falhou: {ex.Message}");
                return default;
            }
        }

        public virtual async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                await _retryPolicy.WrapAsync(_circuitBreakerPolicy).ExecuteAsync(async () =>
                {
                    var json = JsonSerializer.Serialize(value);
                    await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(5));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cache SET falhou: {ex.Message}");
            }
        }

        public virtual async Task RemoveAsync(string key)
        {
            try
            {
                await _retryPolicy.WrapAsync(_circuitBreakerPolicy).ExecuteAsync(async () =>
                {
                    await _db.KeyDeleteAsync(key);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cache REMOVE falhou: {ex.Message}");
            }
        }
    }
}