using System.Collections.Immutable;
using TMobCaseStudy.Base.PubSub;

namespace TMobCaseStudy.Base.Caching
{
    public class CachedFactory
    {
        // Keeps track of all CachedObjects, group first by cache name, then by key name.
        private readonly IDictionary<string, IDictionary<string, CachedInternal>> _cachedObjects =
            new Dictionary<string, IDictionary<string, CachedInternal>>();

        private SemaphoreSlim _cachedObjectsLock = new SemaphoreSlim(1, 1);

        public async Task<ICached<T>> GetOrCreateAsync<T>(Func<Task<T>> valueFactory,
            string keyName, TimeSpan expirationDuration,
            ICache cache, ISubscriber subscriber) where T : class
        {
            await _cachedObjectsLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var cachedInternal = CreateAndAddToDictionary(
                    valueFactory, keyName, expirationDuration, cache);
                return new Cached<T>(cachedInternal);
            }
            finally
            {
                _cachedObjectsLock.Release();
            }
        }

        public ICached<T> GetOrCreate<T>(Func<Task<T>> valueFactory,
            string keyName, TimeSpan expirationDuration, ICache cache) where T : class
        {
            _cachedObjectsLock.Wait();
            try
            {
                var cachedInternal = CreateAndAddToDictionary(
                    valueFactory, keyName, expirationDuration, cache);
                return new Cached<T>(cachedInternal);
            }
            finally
            {
                _cachedObjectsLock.Release();
            }
        }

        private CachedInternal CreateAndAddToDictionary<T>(Func<Task<T>> valueFactory,
            string keyName, TimeSpan expirationDuration, ICache cache)
        {
            var dict = GetOrCreateCacheDictionary(cache.Name);
            if (!dict.TryGetValue(keyName, out var cachedInternal))
            {
                cachedInternal = CachedInternal.Create(
                    async () => await valueFactory().ConfigureAwait(false),
                    keyName, expirationDuration, cache);
                dict[keyName] = cachedInternal;
            }
            return cachedInternal;
        }

        private IDictionary<string, CachedInternal> GetOrCreateCacheDictionary(string cacheName)
        {
            if (!_cachedObjects.TryGetValue(cacheName, out var dict))
            {
                dict = new Dictionary<string, CachedInternal>();
                _cachedObjects[cacheName] = dict;
            }
            return dict;
        }

        public Cached<T> GetCached<T>(string cacheName, string keyName) where T : class
        {
            var cachedInternal = GetCachedInternal(cacheName, keyName);
            return cachedInternal == null ? null : new Cached<T>(cachedInternal);
        }

        public Task<T> GetValueAsync<T>(string cacheName, string keyName) where T : class
        {
            var cachedInternal = GetCachedInternal(cacheName, keyName);
            return cachedInternal == null ? null : cachedInternal.GetValueAsync<T>();
        }
        
        public IImmutableDictionary<string, Cached<object>> GetObjectsOfCache(string cacheName)
        {
            IDictionary<string, CachedInternal> dict = null;

            lock (_cachedObjects)
            {
                if (!_cachedObjects.TryGetValue(cacheName, out dict)) return null;

                return dict.ToDictionary(x => x.Key, x => new Cached<object>(x.Value)).
                    ToImmutableDictionary();
            }
        }
        
        public IEnumerable<string> GetKeys(string cacheName)
        {
            var cacheDict = GetCacheDictionary(cacheName);
            return cacheDict.Select(x => x.Value.KeyName).ToList();
        }

        public IImmutableDictionary<string, IImmutableList<string>> GetCacheAndKeyNames()
        {
            lock (_cachedObjects)
            {
                return _cachedObjects.ToDictionary(
                    x => x.Key, 
                    y => (IImmutableList<string>)y.Value.Keys.ToImmutableList()).
                    ToImmutableDictionary();
            }
        }

        public IImmutableList<string> GetCacheNames()
        {
            lock (_cachedObjects)
            {
                return _cachedObjects.Keys.ToImmutableList();
            }
        }

        public bool ClearCache(string cacheName)
        {
            IDictionary<string, CachedInternal> dict;

            lock (_cachedObjects)
            {
                if (!_cachedObjects.TryGetValue(cacheName, out dict)) return false;
                
                foreach (var item in dict)
                {
                    item.Value.ClearAsync().Wait();
                }

                _cachedObjects.Remove(cacheName);
            }

            return true;
        }

        public bool ClearKey(string cacheName, string keyName)
        {
            lock (_cachedObjects)
            {
                IDictionary<string, CachedInternal> dict;
                CachedInternal cachedInternal = null;

                if (!_cachedObjects.TryGetValue(cacheName, out dict) ||
                    !dict.TryGetValue(keyName, out cachedInternal))
                {
                    return false;
                }
                
                return dict.Remove(keyName) && cachedInternal.ClearAsync().Result;
            }
        }
        
        public bool RefreshCache(string cacheName)
        {
            var cacheDict = GetCacheDictionary(cacheName);

            foreach (var item in cacheDict)
            {
                item.Value.RefreshAsync().Wait();
            }

            return true;
        }

        public async Task<bool> RefreshCacheAsync(string cacheName)
        {
            var cacheDict = GetCacheDictionary(cacheName);

            foreach (var item in cacheDict)
            {
                await item.Value.RefreshAsync();
            }

            return true;
        }

        public bool RefreshKey(string cacheName, string keyName)
        {
            var cachedInternal = GetCachedInternal(cacheName, keyName);
            if (cachedInternal == null) return false;

            cachedInternal.RefreshAsync().Wait();
            return true;
        }

        public async Task<bool> RefreshKeyAsync(string cacheName, string keyName)
        {
            var cachedInternal = GetCachedInternal(cacheName, keyName);
            if (cachedInternal == null) return false;

            await cachedInternal.RefreshAsync();
            return true;
        }

        private IImmutableDictionary<string, CachedInternal> GetCacheDictionary(string cacheName)
        {
            IDictionary<string, CachedInternal> dict;
            lock (_cachedObjects)
            {
                if (!_cachedObjects.TryGetValue(cacheName, out dict)) return null;

                return new Dictionary<string, CachedInternal>(dict).ToImmutableDictionary();
            }
        }

        private CachedInternal GetCachedInternal(string cacheName, string keyName)
        {
            lock (_cachedObjects)
            {
                IDictionary<string, CachedInternal> dict = null;
                CachedInternal cachedInternal = null;

                if (_cachedObjects.TryGetValue(cacheName, out dict) &&
                    dict.TryGetValue(keyName, out cachedInternal))
                {
                    return cachedInternal;
                }
            }
            return null;
        }

        public void WipeoutEverything()
        {
            lock (_cachedObjects)
            {
                foreach (var cachedObjectDict in _cachedObjects.Values)
                {
                    foreach (var cache in cachedObjectDict.Values)
                    {
                        cache.ClearAsync().Wait();
                    }
                }
                _cachedObjects.Clear();
            }
        }
    }
}
