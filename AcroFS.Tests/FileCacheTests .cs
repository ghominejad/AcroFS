using System.IO;
using Moq;
using Xunit;
using Acrobit.AcroFS.Tests.Helpers;
using AcroFS.Tests;
using Acrobit.AcroFS.Caching;
using Microsoft.Extensions.Internal;
using System;
using Microsoft.Extensions.Caching.Memory;

namespace Acrobit.AcroFS.Tests
{   
    public class FileCacheTests
    {
        public FileCacheTests()
        {
            StoragePaths.CleanRoots();
        }

        private IMemoryCache CreateCache(ISystemClock clock)
        {
            return new MemoryCache(new MemoryCacheOptions { Clock = clock });
        }


        [Fact]
        public void FileCacheReloadsCacheDataAfterRestart()
        {
            var clock = new TestClock();
            var cache = CreateCache(clock)
                .Persistant(clock);

            var key = "myKey";
            var value = "myValue";

            var result = cache.Set(key, value, clock.UtcNow + TimeSpan.FromMinutes(1));
            Assert.Same(value, result);

            var found = cache.TryGetValue(key, out result);
            Assert.True(found);
            Assert.Equal(value, result);

            // restart by new memory cache instantiation
            var memCache = CreateCache(clock);

            found = memCache.TryGetValue(key, out result);
            Assert.False(found);
            Assert.Null(result);

            // try with persistant cache
            cache = CreateCache(clock)
                .Persistant(clock);

            found = cache.TryGetValue(key, out result);
            Assert.True(found);
            Assert.Equal(value, result);

        }


        [Fact]
        public void ExpirationAfterRestartDoesntAddEntry()
        {
            var clock = new TestClock();
            var cache = CreateCache(clock)
                .Persistant(clock);

            var key = "myKey";
            var value = "myValue";

            var result = cache.Set(key, value, clock.UtcNow + TimeSpan.FromMinutes(1));
            Assert.Same(value, result);

            var found = cache.TryGetValue(key, out result);
            Assert.True(found);
            Assert.Equal(value, result);

            // Expire manually
            clock.Add(TimeSpan.FromMinutes(2));

            // restart by new memory cache instantiation
            var memCache = CreateCache(clock);

            found = memCache.TryGetValue(key, out result);
            Assert.False(found);
            Assert.Null(result);

            // try with persistant cache
            cache = CreateCache(clock)
                .Persistant(clock);

            found = cache.TryGetValue(key, out result);
            Assert.False(found);
            Assert.NotEqual(value, result);



        }

       



    }
}
