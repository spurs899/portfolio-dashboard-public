using Microsoft.Extensions.Caching.Memory;

namespace PortfolioManager.Core.Services
{
    public interface IMemoryCacheWrapper
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);
        void Remove(string key);
    }

    public class MemoryCacheWrapper : IMemoryCacheWrapper
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheWrapper(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T? Get<T>(string key)
        {
            return _cache.TryGetValue(key, out T value) ? value : default;
        }

        public void Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (absoluteExpirationRelativeToNow.HasValue)
                options.SetAbsoluteExpiration(absoluteExpirationRelativeToNow.Value);
            _cache.Set(key, value, options);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
