using Microsoft.Extensions.Caching.Memory;
using MiniShop.Resilience;
using Polly;

namespace MiniShop.Cache
{
    public class HybridCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IRedisCacheService _redisCache;
        private readonly Polly.Retry.AsyncRetryPolicy _retryPolicy;

        public HybridCacheService(IMemoryCache memoryCache, IRedisCacheService redisCache)
        {
            _memoryCache = memoryCache;
            _redisCache = redisCache;
            _retryPolicy = ResiliencePolicies.GetRetryPolicy();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            // 1º - Verifica cache in-memory (Polly)
            if (_memoryCache.TryGetValue(key, out T? cached))
            {
                Console.WriteLine($"[Cache] HIT in-memory: {key}");
                return cached;
            }

            // 2º - Verifica Redis
            var redisValue = await _retryPolicy.ExecuteAsync(() => _redisCache.GetAsync<T>(key));
            if (redisValue != null)
            {
                Console.WriteLine($"[Cache] HIT Redis: {key}");
                _memoryCache.Set(key, redisValue, TimeSpan.FromMinutes(1));
                return redisValue;
            }

            Console.WriteLine($"[Cache] MISS: {key}");
            return default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            // Guarda em ambos
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(1));
            await _retryPolicy.ExecuteAsync(() => _redisCache.SetAsync(key, value, expiry));
        }

        public async Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            await _retryPolicy.ExecuteAsync(() => _redisCache.RemoveAsync(key));
        }
    }
}