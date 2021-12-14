using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using TMobCaseStudy.Base.Caching;
using TMobCaseStudy.Base.Serialization;
using TMobCaseStudy.Base.Tests.TestData;
using TMobCaseStudy.Data.Entities;
using TMobCaseStudy.Data.Entities.SpaceCraft;
using Xunit;

namespace TMobCaseStudy.Base.Tests.Caching
{
    public class LRUCacheTest
    {
        private readonly IImmutableList<ISpaceCraftBase> _spaceCrafts;
        private readonly Planet _planet;

        public LRUCacheTest()
        {
            _spaceCrafts = SpaceCraftTestData.SpaceCrafts.Value;
            _planet = PlanetTestData.Planet.Value;
        }

        [Fact]
        public void AddOrReplace()
        {
            // Arrange.
            var maxCacheSizeBytes = Megabytes(10);
            var cache = new LRUCache(maxCacheSizeBytes);
            var spaceCraft = _spaceCrafts.First();
            var expectedData = Serialize(spaceCraft);

            // Act.
            cache.AddOrReplace(spaceCraft.ToString(), expectedData, /*expiration:*/null);
            var actualData = cache.Get(spaceCraft.ToString());

            // Assert.
            Assert.NotNull(actualData);
            Assert.Equal(expectedData, actualData);
        }

        [Fact]
        public void Indexer()
        {
            // Arrange.
            var maxCacheSizeBytes = Megabytes(10);
            var cache = new LRUCache(maxCacheSizeBytes);
            var spaceCraft = _spaceCrafts.First();
            var expectedData = Serialize(spaceCraft);

            // Act.
            cache[spaceCraft.ToString()] = expectedData;
            var actualData = cache[spaceCraft.ToString()];

            // Assert.
            Assert.NotNull(actualData);
            Assert.Equal(expectedData, actualData);
        }

        [Fact]
        public void Remove()
        {
            // Arrange.
            var maxCacheSizeBytes = 10000;
            var cache = new LRUCache(maxCacheSizeBytes);
            foreach (var spaceCraft in _spaceCrafts.Take(4))
            {
                cache[spaceCraft.ToString()] = Serialize(spaceCraft);
            }

            // Act & Assert.
            var sizeBeforeRemove = cache.SizeInBytes;
            Assert.True(cache.Remove(_spaceCrafts[0].ToString()));
            Assert.True(cache.Remove(_spaceCrafts[1].ToString()));
            Assert.False(cache.Remove("not-exists"));
            var sizeAfterRemove = cache.SizeInBytes;

            Assert.True(sizeAfterRemove < sizeBeforeRemove,
                "Cache size must decrease after an item is removed.");
        }

        [Fact]
        public void Clear()
        {
            // Arrange.
            var maxCacheSizeBytes = Megabytes(50);
            var cache = new LRUCache(maxCacheSizeBytes);
            foreach (var spaceCraft in _spaceCrafts)
            {
                cache[spaceCraft.ToString()] = Serialize(spaceCraft);
            }

            // Act.
            cache.Clear();

            // Assert.
            Assert.Equal(0L, cache.SizeInBytes);
        }

        [Fact]
        public void Count()
        {
            // Arrange.
            var maxCacheSizeBytes = Megabytes(100);
            var cache = new LRUCache(maxCacheSizeBytes);
            for (int i = 0; i < 5; ++i)
            {
                var spaceCraft = _spaceCrafts[i];
                cache[spaceCraft.ToString()] = Serialize(spaceCraft);
            }
            
            // Act & Assert.
            Assert.Equal(5, cache.Count);
        }
        
        [Fact]
        public void GetKeys()
        {
            // Arrange.
            var maxCacheSizeBytes = Megabytes(100);
            var cache = new LRUCache(maxCacheSizeBytes);
            var spaceCrafts = _spaceCrafts.Take(5);
            foreach (var spaceCraft in spaceCrafts)
            {
                cache[spaceCraft.ToString()] = Serialize(spaceCraft);
            }

            // Act.
            var keys = cache.GetKeys();

            // Assert.
            Assert.Equal(5, keys.Count);
            foreach (var spaceCraft in spaceCrafts)
            {
                Assert.Contains(spaceCraft.ToString(), keys);
            }
        }

        [Fact]
        public void RecentlyUsedNotEvicted()
        {
            // Arrange.
            var maxCacheSizeBytes = 5000;
            var cache = new LRUCache(maxCacheSizeBytes);
            var spaceCraftCode = _spaceCrafts.First().ToString();

            // Act & Assert.
            foreach (var spaceCraft in _spaceCrafts)
            {
                cache[spaceCraft.ToString()] = Serialize(spaceCraft);
                Assert.True(cache.SizeInBytes <= maxCacheSizeBytes,
                    "Cache size must be smaller than max size.");
                // Read the first item to make sure it never gets evicted.
                Assert.NotNull(cache[spaceCraftCode]);
                Thread.Sleep(1);
            }
        }

        private static long Megabytes(int megabytes)
        {
            return megabytes * 1024 * 1024;
        }

        private byte[] Serialize<T>(T obj)
        {
            return SerializerFactory.Get(SerializationType.Json).SerializeToBytes(obj);
        }
    }
}
