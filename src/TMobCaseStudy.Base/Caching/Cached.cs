using System.Threading.Tasks;

namespace TMobCaseStudy.Base.Caching
{
    public class Cached<T> : ICached<T> where T : class
    {
        private readonly CachedInternal _cachedInternal;
        
        internal Cached(CachedInternal cachedInternal)
        {
            _cachedInternal = cachedInternal;
        }

        public string CacheName
        {
            get { return _cachedInternal.CacheName; }
        }

        public string KeyName
        {
            get { return _cachedInternal.KeyName; }
        }
        
        public virtual Task<T> GetValueAsync()
        {
            return _cachedInternal.GetValueAsync<T>();
        }

        public Task RefreshAsync()
        {
            return _cachedInternal.RefreshAsync();
        }

        public Task ClearAsync()
        {
            return _cachedInternal.ClearAsync();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            var rhs = obj as Cached<T>;
            return rhs != null && ReferenceEquals(_cachedInternal, rhs._cachedInternal);
        }

        public override int GetHashCode()
        {
            if (_cachedInternal == null) return 0;
            return _cachedInternal.GetHashCode();
        }
    }
}
