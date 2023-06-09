using AcroFS.Tests;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;

using System;
using System.IO;
using System.Threading.Tasks;

using Xunit;

namespace Acrobit.AcroFS.Tests
{
    public class FileCacheTests
    {
        [Fact]
        public void FileCacheReloadsCacheDataAfterRestart()
        {
            var root = Path.GetRandomFileName();
            var clock = new TestClock();
            var cache = CreateCache(clock)
                .Persistent(clock, root);

            var key = "myKey";
            var value = "myValue";

            var result = cache.Set(key, value, clock.UtcNow + TimeSpan.FromMinutes(1));
            Assert.Same(value, result);

            var found = cache.TryGetValue(key, out result);
            Assert.True(found);
            Assert.Equal(value, result);

            // Restart by new memory cache instantiation
            var memCache = CreateCache(clock);

            found = memCache.TryGetValue(key, out result);
            Assert.False(found);
            Assert.Null(result);

            // Try with persistant cache
            cache = CreateCache(clock)
                .Persistent(clock, root);

            found = cache.TryGetValue(key, out result);
            Assert.True(found);
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task FileCacheReloadsCacheDataAfterRestartAsync()
        {
            var root = Path.GetRandomFileName();
            var clock = new TestClock();
            var cache = CreateCache(clock)
                .Persistent(clock, root);

            var key = "myKey";
            var value = "myValue";

            var result = await cache.SetAsync(key, value, clock.UtcNow + TimeSpan.FromMinutes(1));
            Assert.Same(value, result);

            var (found1, result1) = await cache.TryGetValueAsync<string>(key);
            Assert.True(found1);
            Assert.Equal(value, result1);

            // Restart by new memory cache instantiation
            var memCache = CreateCache(clock);

            var found2 = memCache.TryGetValue(key, out var result2);
            Assert.False(found2);
            Assert.Null(result2);

            // Try with persistant cache
            cache = CreateCache(clock)
                .Persistent(clock, root);

            var (found3, result3) = await cache.TryGetValueAsync<string>(key);
            Assert.True(found3);
            Assert.Equal(value, result3);
        }

        [Fact]
        public void ExpirationAfterRestartDoesntAddEntry()
        {
            var root = Path.GetRandomFileName();
            var clock = new TestClock();
            var cache = CreateCache(clock)
                .Persistent(clock, root);

            var key = "myKey";
            var value = "myValue";

            var result = cache.Set(key, value, clock.UtcNow + TimeSpan.FromMinutes(1));
            Assert.Same(value, result);

            var found = cache.TryGetValue(key, out result);
            Assert.True(found);
            Assert.Equal(value, result);

            // Expire manually
            clock.Add(TimeSpan.FromMinutes(2));

            // Restart by new memory cache instantiation
            var memCache = CreateCache(clock);

            found = memCache.TryGetValue(key, out result);
            Assert.False(found);
            Assert.Null(result);

            // Try with persistant cache
            cache = CreateCache(clock)
                .Persistent(clock, root);

            found = cache.TryGetValue(key, out result);
            Assert.False(found);
            Assert.NotEqual(value, result);
        }

        [Fact]
        public async Task ExpirationAfterRestartDoesntAddEntryAsync()
        {
            var root = Path.GetRandomFileName();
            var clock = new TestClock();
            var cache = CreateCache(clock)
                .Persistent(clock, root);

            var key = "myKey";
            var value = "myValue";

            var result = await cache.SetAsync(key, value, clock.UtcNow + TimeSpan.FromMinutes(1));
            Assert.Same(value, result);

            var (found1, result1) = await cache.TryGetValueAsync<string>(key);
            Assert.True(found1);
            Assert.Equal(value, result1);

            // Expire manually
            clock.Add(TimeSpan.FromMinutes(2));

            // Restart by new memory cache instantiation
            var memCache = CreateCache(clock);

            var found2 = memCache.TryGetValue(key, out var result2);
            Assert.False(found2);
            Assert.Null(result2);

            // Try with persistant cache
            cache = CreateCache(clock)
                .Persistent(clock, root);

            var (found3, result3) = await cache.TryGetValueAsync<string>(key);
            Assert.False(found3);
            Assert.NotEqual(value, result3);
        }

        private static IMemoryCache CreateCache(ISystemClock clock)
        {
            return new MemoryCache(new MemoryCacheOptions { Clock = clock });
        }
    }
}
