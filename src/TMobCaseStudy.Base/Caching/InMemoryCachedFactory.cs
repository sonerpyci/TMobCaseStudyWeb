using TMobCaseStudy.Base.PubSub;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace TMobCaseStudy.Base.Caching
{
    public class InMemoryCachedFactory
    {
        public static readonly InMemoryCachedFactory Instance = new InMemoryCachedFactory();

        private readonly CachedFactory _cachedFactory = new CachedFactory();
        private readonly IDictionary<string, ICache> _caches = new Dictionary<string, ICache>();

        private InMemoryCachedFactory() { }

        public Task<ICached<T>> GetOrCreateAsync<T>(Func<Task<T>> valueFactory, 
            string cacheName, string keyName, TimeSpan expirationDuration,
            ISubscriber subscriber = null) where T : class
        {
            var cache = GetOrCreateCache(cacheName);
            return _cachedFactory.GetOrCreateAsync(valueFactory,
                keyName, expirationDuration, cache, subscriber);
        }

        public ICached<T> GetOrCreate<T>(Func<Task<T>> valueFactory,
            string cacheName, string keyName, TimeSpan expirationDuration,
            ISubscriber subscriber = null) where T : class
        {
            return GetOrCreateAsync(valueFactory, cacheName,
                keyName, expirationDuration, subscriber).Result;
        }

        public ICached<T> GetCached<T>(string cacheName, string keyName) where T : class
        {
            return _cachedFactory.GetCached<T>(cacheName, keyName);
        }

        public Task<T> GetValueAsync<T>(string cacheName, string keyName) where T : class
        {
            return _cachedFactory.GetValueAsync<T>(cacheName, keyName);
        }
        
        public IImmutableDictionary<string, Cached<object>> GetObjectsOfCache(string cacheName)
        {
            return _cachedFactory.GetObjectsOfCache(cacheName);
        }

        public IEnumerable<string> GetKeys(string cacheName)
        {
            return _cachedFactory.GetKeys(cacheName);
        }

        public IImmutableDictionary<string, IImmutableList<string>> GetCacheAndKeyNames()
        {
            return _cachedFactory.GetCacheAndKeyNames();
        }

        public IImmutableList<string> GetCacheNames()
        {
            return _cachedFactory.GetCacheNames();
        }

        public bool RefreshCache(string cacheName)
        {
            return _cachedFactory.RefreshCache(cacheName);
        }

        public Task<bool> RefreshCacheAsync(string cacheName)
        {
            return _cachedFactory.RefreshCacheAsync(cacheName);
        }

        public bool RefreshKey(string cacheName, string keyName)
        {
            return _cachedFactory.RefreshKey(cacheName, keyName);
        }

        public Task<bool> RefreshKeyAsync(string cacheName, string keyName)
        {
            return _cachedFactory.RefreshKeyAsync(cacheName, keyName);
        }

        public bool ClearCache(string cacheName)
        {
            return _cachedFactory.ClearCache(cacheName);
        }

        public bool ClearKey(string cacheName, string keyName)
        {
            return _cachedFactory.ClearKey(cacheName, keyName);
        }

        public void WipeoutEverything()
        {
            _cachedFactory.WipeoutEverything();
        }

        private ICache GetOrCreateCache(string cacheName)
        {
            ICache cache;

            lock (_caches)
            {
                if (!_caches.TryGetValue(cacheName, out cache))
                {
                    cache = new InMemoryCache(cacheName);
                    _caches[cacheName] = cache;
                }
            }

            return cache;
        }
    }
}
