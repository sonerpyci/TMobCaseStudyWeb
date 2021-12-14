using System;
using System.Threading.Tasks;

namespace TMobCaseStudy.Base.Caching
{
    internal class CachedInternal
    {
        private readonly Func<Task<object>> _valueFactory;
        private readonly string _keyName;
        private readonly TimeSpan _expirationDuration;
        private readonly ICache _cache;

        private CachedInternal(Func<Task<object>> valueFactory, string keyName,
            TimeSpan expirationDuration, ICache cache)
        {
            _valueFactory = valueFactory;
            _keyName = keyName;
            _expirationDuration = expirationDuration;
            _cache = cache;
        }

        internal static CachedInternal Create(Func<Task<object>> valueFactory,
            string keyName, TimeSpan expirationDuration, ICache cache)
        {
            var instance = new CachedInternal(valueFactory,
                keyName, expirationDuration, cache);
            return instance;
        }

        internal string CacheName
        {
            get { return _cache.Name; }
        }

        internal string KeyName
        {
            get { return _keyName; }
        }
        
        internal async virtual Task<T> GetValueAsync<T>() where T : class
        {
            var value = await _cache.GetAsync<T>(_keyName).ConfigureAwait(false);
            if (value == null)
            {
                value = await GetFromValueFactoryAndPutToCacheAsync().ConfigureAwait(false) as T;
            }
            return value;
        }

        internal async virtual Task RefreshAsync()
        {
            await _cache.RemoveAsync(_keyName).ConfigureAwait(false);
            await GetFromValueFactoryAndPutToCacheAsync().ConfigureAwait(false);
        }

        internal virtual Task<bool> ClearAsync()
        {
            return _cache.RemoveAsync(_keyName);
        }

        private async Task<object> GetFromValueFactoryAndPutToCacheAsync()
        {
            var value = await _valueFactory().ConfigureAwait(false);
            if (value == null) return null;

            await _cache.PutAsync(_keyName, value, _expirationDuration).ConfigureAwait(false);
            return value;
        }
    }
}
