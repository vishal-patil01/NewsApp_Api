using Microsoft.Extensions.Caching.Memory;
using NewsApp.Services.Interface;

namespace NewsApp.Services.Implementation
{
    public class MemoryCacheWrapper : IMemoryCacheWrapper
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheWrapper(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool TryGetValue<T>(object key, out T value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        public void Set<T>(object key, T value, TimeSpan expiration)
        {
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            _memoryCache.Set(key, value, options);
        }
    }
}
