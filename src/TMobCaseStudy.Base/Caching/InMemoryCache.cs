using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TMobCaseStudy.Base.Caching
{
    /// <summary>
    /// Memory Cache
    /// </summary>
    public class InMemoryCache : ICache
    {
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// Maps key name to object in parallel to memoryCache. This is used to return all keys
        /// registered since MemoryCache does not have an interface for that.
        /// </summary>
        private readonly ConcurrentDictionary<string, object> _values =
            new ConcurrentDictionary<string, object>();
        
        /// <summary>
        /// The name of the cache.
        /// </summary>
        public string Name { get; private set; }

        internal InMemoryCache(string name)
        {
            Name = name;
            _memoryCache = new MemoryCache(new MemoryCacheOptions()
            {
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(500)
            });
        }

        /// <summary>
        /// Retrieves the object associated with the given key.
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <returns></returns>
        public Task<T> GetAsync<T>(string key) where T : class
        {
            return Task.FromResult(_memoryCache.Get(key) as T);
        }

        /// <summary>
        /// Puts the given object/value in memory cache.
        /// </summary>
        /// <param name="key">The key to put in cache</param>
        /// <param name="value">The value to put in cache</param>
        /// <param name="expirationSeconds">Object removal duration in seconds</param>
        public Task PutAsync<T>(string key, T value, int expirationSeconds) 
            where T : class
        {
            return PutAsync(key, value, TimeSpan.FromSeconds(expirationSeconds));
        }
        
        /// <summary>
        /// Puts the given object/value in memory cache.
        /// </summary>
        /// <param name="key">The key to put in cache</param>
        /// <param name="value">The value to put in cache</param>
        /// <param name="expirationDuration">Object removal duration</param>
        public Task PutAsync<T>(string key, T value, TimeSpan expirationDuration) 
            where T : class
        {
            var memoryCacheEntryOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expirationDuration
            };
            memoryCacheEntryOptions.RegisterPostEvictionCallback(PostEvictionCallback);

            _memoryCache.Set(key, value, memoryCacheEntryOptions);
            _values.TryAdd(key, value);

            return Task.CompletedTask;
        }

        private void PostEvictionCallback(object key, object value, 
            EvictionReason reason, object state)
        {
            _values.TryRemove((string)key, out _);
        }

        /// <summary>
        /// Removes the given key from memory cache.
        /// </summary>
        /// <param name="key">The key to remove from cache</param>
        public Task<bool> RemoveAsync(string key)
        {
            var value = _memoryCache.Get(key);
            if (value == null) return Task.FromResult(false);

            _memoryCache.Remove(key);
            // Note that the key is removed from the {values} dictionary in {PostEvictionCallback}
            // defined above.
            return Task.FromResult(true);
        }

        /// <summary>
        /// Get Key Value Pairs in Cache
        /// </summary>
        public List<KeyValuePair<string, object>> GetKeyValuePairs()
        {
            return _values.ToList();
        }

        /// <summary>
        /// Get All Keys in Memory Cache
        /// </summary>
        public List<string> GetKeys()
        {
            return GetKeyValuePairs().Select(x => x.Key).ToList();
        }
    }
}
