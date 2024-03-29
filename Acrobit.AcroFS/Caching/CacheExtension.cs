﻿using Acrobit.AcroFS.Caching;

using Microsoft.Extensions.Internal;

using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Memory
{
    public static class FileCacheExtensions
    {
        public static object Get(this FileCache cache, object key)
        {
            cache.TryGetValue(key, out object value);
            return value;
        }

        public static TItem? Get<TItem>(this FileCache cache, object key)
        {
            return (TItem?)(cache.Get(key) ?? default(TItem));
        }

        public static TItem Set<TItem>(this FileCache cache, object key, TItem value)
        {
            var entry = cache.CreateEntry(key);
            entry.Value = value;
            entry.Dispose();

            if (cache.Get(key) != null)
            {
                cache.Persist<TItem>(entry);
            }

            return value;
        }

        public static async Task<TItem> SetAsync<TItem>(this FileCache cache, object key, TItem value)
        {
            var entry = cache.CreateEntry(key);
            entry.Value = value;
            entry.Dispose();

            if (cache.Get(key) != null)
            {
                await cache.PersistAsync<TItem>(entry);
            }

            return value;
        }

        public static TItem Set<TItem>(this FileCache cache, object key, TItem value, DateTimeOffset absoluteExpiration)
        {
            var entry = cache.CreateEntry(key);
            entry.AbsoluteExpiration = absoluteExpiration;
            entry.Value = value;

            entry.Dispose();

            if (cache.Get(key) != null)
            {
                cache.Persist<TItem>(entry);
            }

            return value;
        }

        public static async Task<TItem> SetAsync<TItem>(this FileCache cache, object key, TItem value, DateTimeOffset absoluteExpiration)
        {
            var entry = cache.CreateEntry(key);
            entry.AbsoluteExpiration = absoluteExpiration;
            entry.Value = value;

            entry.Dispose();

            if (cache.Get(key) != null)
            {
                await cache.PersistAsync<TItem>(entry);
            }

            return value;
        }

        public static TItem Set<TItem>(this FileCache cache, object key, TItem value, TimeSpan expirationInterval, bool isSlidingExpiration = false)
        {
            var entry = cache.CreateEntry(key);
            if (isSlidingExpiration)
                entry.SlidingExpiration = expirationInterval;
            else entry.AbsoluteExpirationRelativeToNow = expirationInterval;
            entry.Value = value;

            entry.Dispose();

            if (cache.Get(key) != null)
            {
                cache.Persist<TItem>(entry);
            }

            return value;
        }

        public static async Task<TItem> SetAsync<TItem>(this FileCache cache, object key, TItem value, TimeSpan expirationInterval, bool isSlidingExpiration = false)
        {
            var entry = cache.CreateEntry(key);
            if (isSlidingExpiration)
                entry.SlidingExpiration = expirationInterval;
            else entry.AbsoluteExpirationRelativeToNow = expirationInterval;
            entry.Value = value;

            entry.Dispose();

            if (cache.Get(key) != null)
            {
                await cache.PersistAsync<TItem>(entry);
            }

            return value;
        }

        public static TItem Set<TItem>(this FileCache cache, object key, TItem value, MemoryCacheEntryOptions options)
        {
            using (var entry = cache.CreateEntry(key))
            {
                if (options != null)
                {
                    entry.SetOptions(options);
                }

                entry.Value = value;

                if (cache.Get(key) != null)
                {
                    cache.Persist<TItem>(entry);
                }
            }

            return value;
        }

        public static async Task<TItem> SetAsync<TItem>(this FileCache cache, object key, TItem value, MemoryCacheEntryOptions options)
        {
            using (var entry = cache.CreateEntry(key))
            {
                if (options != null)
                {
                    entry.SetOptions(options);
                }

                entry.Value = value;

                if (cache.Get(key) != null)
                {
                    await cache.PersistAsync<TItem>(entry);
                }
            }

            return value;
        }

        public static TItem? GetOrCreate<TItem>(this FileCache cache, object key, Func<ICacheEntry, TItem> factory)
        {
            if (!cache.TryGetValue(key, out object? result))
            {
                var entry = cache.CreateEntry(key);
                result = factory(entry);
                entry.SetValue(result);

                // need to manually call dispose instead of having a using
                // in case the factory passed in throws, in which case we
                // do not want to add the entry to the cache
                entry.Dispose();

                if (cache.Get(key) != null)
                {
                    cache.Persist<TItem>(entry);
                }
            }

            return (TItem?)result;
        }

        public static async Task<TItem> GetOrCreateAsync<TItem>(this FileCache cache, object key, Func<ICacheEntry, Task<TItem>> factory)
        {
            if (!cache.TryGetValue(key, out TItem result))
            {
                var entry = cache.CreateEntry(key);
                result = await factory(entry);
                entry.SetValue(result);

                // need to manually call dispose instead of having a using
                // in case the factory passed in throws, in which case we
                // do not want to add the entry to the cache
                entry.Dispose();

                if (cache.Get(key) != null)
                {
                    await cache.PersistAsync<TItem>(entry);
                }
            }

            return (TItem)result;
        }

        internal static MemoryCacheEntryOptions ToMemoryOptions(this FileCacheEntryOptions options)
        {
            return new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                Priority = options.Priority,
                Size = options.Size,
                SlidingExpiration = options.SlidingExpiration,
            };
        }

        internal static bool HasExpiration(this ICacheEntry entry)
        {
            return entry.AbsoluteExpiration != null || entry.SlidingExpiration != null || entry.AbsoluteExpirationRelativeToNow != null;

        }

        public static FileCache Persistent(this IMemoryCache cache, string? repositoryRoot = null)
        {
            return new FileCache(new SystemClock(), cache, repositoryRoot);
        }

        public static FileCache Persistent(this IMemoryCache cache, ISystemClock clock, string? repositoryRoot = null)
        {
            return new FileCache(clock, cache, repositoryRoot);
        }
    }
}
