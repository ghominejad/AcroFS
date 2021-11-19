using System;
using System.Collections.Generic;
using System.Text;

using System;
using System.Threading.Tasks;
using global::Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Acrobit.AcroFS.Caching;

namespace Microsoft.Extensions.Caching.Memory
{
    public static class FileCacheExtensions
    {
        public static object Get(this FileCache cache, object key)
        {
            cache.TryGetValue(key, out object value);
            return value;
        }

        public static TItem Get<TItem>(this FileCache cache, object key)
        {
            return (TItem)(cache.Get(key) ?? default(TItem));
        }

        //public static bool TryGetValue<TItem>(this FsCache cache, object key, out TItem value)
        //{
        //    if (cache.TryGetValue(key, out object result))
        //    {
        //        if (result is TItem item)
        //        {
        //            value = item;
        //            return true;
        //        }
        //    }

        //    value = default;
        //    return false;
        //}

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

        public static TItem Set<TItem>(this FileCache cache, object key, TItem value, TimeSpan expirationInterval, bool isSlidingExpiration = false)
        {
            var entry = cache.CreateEntry(key);
            if(isSlidingExpiration)
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

        public static TItem GetOrCreate<TItem>(this FileCache cache, object key, Func<ICacheEntry, TItem> factory)
        {
            if (!cache.TryGetValue(key, out object result))
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

            return (TItem)result;
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
                    cache.Persist<TItem>(entry);
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

        public static FileCache Persistent(this IMemoryCache cache)
        {
            return new FileCache(new SystemClock(), cache);
        }

        public static FileCache Persistent(this IMemoryCache cache, ISystemClock clock)
        {
            return new FileCache(clock, cache);
        }





    }
}
