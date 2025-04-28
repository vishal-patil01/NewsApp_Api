using Microsoft.Extensions.Caching.Memory;
using NewsApp.Services.Interface;

namespace NewsApp.Services.Implementation
{
    /// <summary>
    /// Provides a wrapper around the built-in memory cache functionality.
    /// Implements the <see cref="IMemoryCacheWrapper"/> interface to facilitate caching operations.
    /// </summary>
    public class MemoryCacheWrapper : IMemoryCacheWrapper
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheWrapper(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }


        /// <summary>
        /// Tries to retrieve a value associated with the specified key from the memory cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The cache key identifying the value.</param>
        /// <param name="value">
        /// When this method returns, contains the object from the cache if the key was found; otherwise, the default value for the type.
        /// </param>
        /// <returns>
        /// true if the key was found in the cache; otherwise, false.
        /// </returns>
        public bool TryGetValue<T>(object key, out T value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        /// <summary>
        /// Sets a value in the memory cache with the specified expiration time.
        /// </summary>
        /// <typeparam name="T">The type of the value to be cached.</typeparam>
        /// <param name="key">The cache key identifying the value.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="expiration">The duration after which the cache entry should expire.</param>
        public void Set<T>(object key, T value, TimeSpan expiration)
        {
            // Create cache entry options with absolute expiration
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            // Add or update the cache with the specified key, value, and expiration settings.
            _memoryCache.Set(key, value, options);
        }
    }
}
