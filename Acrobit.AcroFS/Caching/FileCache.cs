using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Acrobit.AcroFS.Caching
{
    public class FileCache //: IMemoryCache
    {
        private readonly ISystemClock _systemClock;
        private readonly IMemoryCache _memCache;
        private readonly FileStore _fileStore;
        private readonly string cacheCluster = "__cache__";

        public FileCache(ISystemClock systemClock, IMemoryCache memCache, string repositoryRoot = null)
        {
            _fileStore = FileStore.CreateStore(repositoryRoot);
            _systemClock = systemClock;
            _memCache = memCache;
        }

        public ICacheEntry CreateEntry(object key)
        {
            return _memCache.CreateEntry(key);
        }

        public void Dispose()
        {
            _memCache.Dispose();
        }

        public void Remove(object key)
        {
            _memCache.Remove(key);

            _fileStore.Remove(key, cacheCluster);
            _fileStore.RemoveAttachment(key, "FsCacheEntryOptions", cacheCluster);

        }

        //public bool TryGetValue(object key, out object value)
        //{
        //    return _cache.TryGetValue(key, out value);
        //}

        public void Persist<T>(ICacheEntry entry)
        {
            if (entry.HasExpiration())
            {
                var options = new FileCacheEntryOptions
                {
                    AbsoluteExpiration = entry.AbsoluteExpiration,
                    SlidingExpiration = entry.SlidingExpiration,
                    AbsoluteExpirationRelativeToNow = entry.AbsoluteExpirationRelativeToNow,
                    Size = entry.Size,
                    Priority = entry.Priority
                };

                _fileStore.Attach(entry.Key, "FsCacheEntryOptions", options, cacheCluster);
            }

            _fileStore.StoreByKey(entry.Key, (T)entry.Value, cacheCluster);
        }

        public async Task PersistAsync<T>(ICacheEntry entry)
        {
            if (entry.HasExpiration())
            {
                var options = new FileCacheEntryOptions
                {
                    AbsoluteExpiration = entry.AbsoluteExpiration,
                    SlidingExpiration = entry.SlidingExpiration,
                    AbsoluteExpirationRelativeToNow = entry.AbsoluteExpirationRelativeToNow,
                    Size = entry.Size,
                    Priority = entry.Priority
                };

                await _fileStore.AttachAsync(entry.Key, "FsCacheEntryOptions", options, cacheCluster);
            }

            await _fileStore.StoreByKeyAsync(entry.Key, (T)entry.Value, cacheCluster);
        }

        public bool TryGetValue<T>(object key, [NotNullWhen(true)] out T value)
        {
            if (!_memCache.TryGetValue(key, out value)) // check inside memory
            {
                if (_fileStore.Exists(key, cacheCluster)) // check inside disk
                {
                    // check expiration
                    var cacheEntryOptions = _fileStore.LoadAttachment<FileCacheEntryOptions>(key, "FsCacheEntryOptions", cacheCluster);
                    var expired = false;
                    if (cacheEntryOptions != null)
                        expired = cacheEntryOptions.CheckExpired(_systemClock.UtcNow);

                    if (!expired)
                    {
                        value = _fileStore.Load<T>(key, cacheCluster); // load from disk

                        // create cache in-memory
                        var entry = CreateEntry(key);
                        if (cacheEntryOptions != null)
                            entry.SetOptions(cacheEntryOptions.ToMemoryOptions());
                        entry.SetValue(value);

                        // need to manually call dispose instead of having a using
                        // in case the factory passed in throws, in which case we
                        // do not want to add the entry to the cache
                        entry.Dispose();
                    }

                    return !expired;
                }
            }
            else
                return true;

            return false;
        }

        public async Task<(bool, T?)> TryGetValueAsync<T>(object key) where T : class
        {
            if (!_memCache.TryGetValue(key, out T value)) // check inside memory
            {
                if (_fileStore.Exists(key, cacheCluster)) // check inside disk
                {
                    // check expiration
                    var cacheEntryOptions = _fileStore.LoadAttachment<FileCacheEntryOptions>(key, "FsCacheEntryOptions", cacheCluster);
                    var expired = false;
                    if (cacheEntryOptions != null)
                        expired = cacheEntryOptions.CheckExpired(_systemClock.UtcNow);

                    if (!expired)
                    {
                        value = await _fileStore.LoadAsync<T>(key, cacheCluster); // load from disk

                        // create cache in-memory
                        var entry = CreateEntry(key);
                        if (cacheEntryOptions != null)
                        {
                            entry.SetOptions(cacheEntryOptions.ToMemoryOptions());
                        }

                        entry.SetValue(value);

                        // need to manually call dispose instead of having a using
                        // in case the factory passed in throws, in which case we
                        // do not want to add the entry to the cache
                        entry.Dispose();
                    }

                    return (!expired, value);
                }
            }
            else
            {
                return (true, value);
            }

            return (false, default(T));
        }
    }
}
