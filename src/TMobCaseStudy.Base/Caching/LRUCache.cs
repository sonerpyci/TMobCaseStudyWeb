using System;
using System.Collections.Generic;
using System.Linq;
using TMobCaseStudy.Base.Collections;


namespace TMobCaseStudy.Base.Caching
{
    /// <summary>
    /// A simple LRU cache implementation with an additional time based expiration. When the
    /// capacity is full, first expired items, if any, are evicted; then least recently used ones
    /// until there is enough space for the new item.
    /// </summary>
    public class LRUCache
    {
        private class Node
        {
            internal byte[] Data { get; private set; }

            internal DateTime Added { get; private set; }

            internal TimeSpan? Expiration { get; private set; }

            internal DateTime LastUsed { get; set; }

            internal Node(byte[] data, TimeSpan? expiration)
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                var now = DateTime.Now;
                Added = now;
                Expiration = expiration;
                LastUsed = now;
            }

            internal bool HasExpired(DateTime now)
            {
                return Expiration != null && Added + Expiration.Value < now;
            }

            internal int SizeInBytes
            {
                get
                {
                    // DateTime and TimeSpan don't have fixed sizes but they most probably won't be
                    // larger than 8 bytes.
                    const int sizeOfDateTime = 8;
                    const int sizeOfTimeSpan = 8;
                    return Data.Length +     // sizeof(Data)
                           IntPtr.Size +     // sizeof(byte[]*)
                           sizeOfDateTime +  // sizeof(LastUsed)
                           sizeOfDateTime +  // sizeof(Added)
                           sizeOfTimeSpan +  // sizeof(Expiration)
                           IntPtr.Size;      // sizeof(Nullable<TimeSpan>*) [for Expiration]
                }
            }

            public override string ToString()
            {
                return string.Format(
                    "Data=[{0} bytes] Added=[{1}] Expiration=[{2}] LastUsed=[{3}]",
                    Data.Length, Added, Expiration, LastUsed);
            }
        }

        public class KeyStatistics
        {
            public int Hits { get; private set; }

            public int Misses { get; private set; }

            internal KeyStatistics() { }

            internal void Hit() { Hits++; }

            internal void Miss() { Misses++; }

            static internal int SizeInBytes =>
                sizeof(int) + // sizeof(HitCount)
                sizeof(int);  // sizeof(MissCount)

            public override string ToString()
            {
                return string.Format("Hits={0} Misses={1}", Hits, Misses);
            }
        }

        private readonly long _maxCacheSizeInBytes;
        private readonly object _cacheLock = new object();
        private Dictionary<string, Node> _cache;
        private Dictionary<string, KeyStatistics> _statisticsDict;

        /// <summary>
        /// Initialize the cache.
        /// </summary>
        /// <param name="maxCacheSizeInBytes">Max size of the cache in bytes.</param>
        public LRUCache(long maxCacheSizeInBytes)
        {
            if (maxCacheSizeInBytes < 1024)
            {
                throw new ArgumentException(
                    "Cannot be smaller than 1MB.", nameof(maxCacheSizeInBytes));
            }
            _maxCacheSizeInBytes = maxCacheSizeInBytes;
            _cache = new Dictionary<string, Node>();
            _statisticsDict = new Dictionary<string, KeyStatistics>();
            TotalHits = 0;
            TotalMisses = 0;
            SizeInBytes = 0;
            TotalEvicts = 0;
        }

        /// <summary>
        /// Current size of the cache in megabytes.
        /// </summary>
        public long SizeInBytes { get; private set; }

        /// <summary>
        /// Total number of hits.
        /// </summary>
        public int TotalHits { get; private set; }

        /// <summary>
        /// Total number of misses.
        /// </summary>
        public int TotalMisses { get; private set; }

        /// <summary>
        /// Ratio of <see cref="TotalHits"/> to <see cref="TotalHits"/> + <see cref="TotalMisses"/>.
        /// </summary>
        public decimal? HitRatio
        {
            get
            {
                var totalGetRequests = TotalHits + TotalMisses;
                if (totalGetRequests == 0) return null;
                return (decimal)TotalHits / totalGetRequests;
            }
        }
        
        /// <summary>
        /// Total number of evicted items.
        /// </summary>
        public int TotalEvicts { get; private set; }

        /// <summary>
        /// Number of entries in the cache.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_cacheLock)
                {
                    return _cache.Count;
                }
            }
        }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            lock (_cacheLock)
            {
                SizeInBytes = 0;
                _cache = new Dictionary<string, Node>();
                _statisticsDict = new Dictionary<string, KeyStatistics>();
                TotalHits = 0;
                TotalMisses = 0;
                TotalEvicts = 0;
            }
        }

        /// <summary>
        /// Retrieve all keys in the cache.
        /// </summary>
        /// <returns>List of keys.</returns>
        public List<string> GetKeys()
        {
            lock (_cacheLock)
            {
                return new List<string>(_cache.Keys);
            }
        }

        /// <summary>
        /// Statistics per key.
        /// </summary>
        public IDictionary<string, KeyStatistics> Statistics
        {
            get
            {
                lock (_cacheLock)
                {
                    return new Dictionary<string, KeyStatistics>(_statisticsDict);
                }
            }
        }

        /// <summary>
        /// Indexer. Corresponds to <see cref="Get(string)"/> and
        /// <see cref="AddOrReplace(string, byte[], TimeSpan?)"/> methods.
        /// </summary>
        public byte[] this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                AddOrReplace(key, value, /*expiration:*/null);
            }
        }

        /// <summary>
        /// Retrieve a key's value from the cache.
        /// </summary>
        /// <param name="key">The key associated with the data you wish to retrieve.</param>
        /// <returns>The object data associated with the key.</returns>
        public byte[] Get(string key)
        {
            if (TryGet(key, out var value))
            {
                return value;
            }
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Retrieve a key's value from the cache.
        /// </summary>
        /// <param name="key">The key associated with the data you wish to retrieve.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <returns>True if key is found.</returns>
        public bool TryGet(string key, out byte[] value)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var now = DateTime.Now;
            value = null;
            lock (_cacheLock)
            {
                var statistics = GetOrCreateStatisticsForKey(key);
                if (!_cache.TryGetValue(key, out var node))
                {
                    statistics.Miss();
                    ++TotalMisses;
                    return false;
                }
                if (node.HasExpired(now))
                {
                    RemoveExisting(key, node);
                    return false;
                }
                node.LastUsed = DateTime.Now;
                value = node.Data;
                statistics.Hit();
                ++TotalHits;
                return true;
            }
        }

        private KeyStatistics GetOrCreateStatisticsForKey(string key)
        {
            if (!_statisticsDict.TryGetValue(key, out var statistics))
            {
                statistics = new KeyStatistics();
                _statisticsDict[key] = statistics;
            }
            return statistics;
        }

        /// <summary>
        /// Add or replace a key's value in the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="expiration">If set, the key is evicted automatically after this.</param>
        public void AddOrReplace(string key, byte[] value, TimeSpan? expiration)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            lock (_cacheLock)
            {
                EvictExpired();
                _cache.TryGetValue(key, out var existingNode);
                var newNode = new Node(value, expiration);

                SizeInBytes += ComputeIncreaseInSize(key, existingNode?.Data, newNode);
                if (SizeInBytes >= _maxCacheSizeInBytes)
                {
                    EvictLeastRecentlyUsed();
                }
                _cache[key] = newNode;
            }
        }

        /// <summary>
        /// Removes the entry identified by <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to be removed.</param>
        /// <returns>Whether the key existed, thus removed, or not.</returns>
        public bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(key, out var node))
                {
                    return false;
                }
                RemoveExisting(key, node);
                return true;
            }
        }

        private static int ComputeIncreaseInSize(
            string key, byte[] existingData, Node newNode)
        {
            if (existingData != null)
            {
                // Key already exists so we are going to replace the value. The cache may not
                // may not increase in size depending on the size of {val} and {value.Data}.
                return newNode.Data.Length - existingData.Length;
            }
            else
            {
                return GetKeySize(key) + GetNodeSize(newNode);
            }
        }

        private void EvictExpired()
        {
            var now = DateTime.Now;
            var expiredEntries = _cache.Where(x => x.Value.HasExpired(now)).ToList();
            foreach (var expiredEntry in expiredEntries)
            {
                RemoveExisting(expiredEntry.Key, expiredEntry.Value);
            }
        }

        private void EvictLeastRecentlyUsed()
        {
            do
            {
                var lruEntry = _cache.MinItem(x => x.Value.LastUsed);
                RemoveExisting(lruEntry.Key, lruEntry.Value);
                ++TotalEvicts;
            } while (SizeInBytes > _maxCacheSizeInBytes && _cache.Count > 0);
        }

        private void RemoveExisting(string key, Node node)
        {
            var entrySizeInBytes = GetKeySize(key) + GetNodeSize(node);
            SizeInBytes -= entrySizeInBytes;
            _cache.Remove(key);
        }

        private static int GetKeySize(string key)
        {
            // Strings are unicode so each character is 2 bytes.. We also add the size of the
            // reference/pointer that points to the key in Dictionary's internal structure.
            // Since we have two dictionaries (_cache and _statisticsDict), we multiply the key
            // size by two.
            return (key.Length * 2 + IntPtr.Size) * 2;
        }
        
        private static int GetNodeSize(Node node)
        {
            var cacheValueSize =
                node.SizeInBytes + // sizeof(Node)
                IntPtr.Size;       // sizeof(Node*) [used by Dictionary]
            var statisticsValueSize =
                KeyStatistics.SizeInBytes + // sizeof(KeyStatistics)
                IntPtr.Size;                // sizeof(KeyStatistics*) [used by Dictionary]
            return cacheValueSize + statisticsValueSize;
        }
    }
}
