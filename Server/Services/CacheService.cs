using System.Collections.Concurrent;

namespace Server.Services
{
    public class CacheService
    {
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30); // 30 minutes, you can adjust as needed

        public async Task<T> GetAsync<T>(string key)
        {
            if (_cache.TryGetValue(key, out var cachedItem))
            {
                var item = (CachedItem<T>)cachedItem;
                if (item.ExpirationTime == null || item.ExpirationTime > DateTimeOffset.Now)
                {
                    return item.Value;
                }
                else
                {
                    _cache.TryRemove(key, out _);
                }
            }

            return default(T);
        }

        public async Task SetAsync<T>(string key, T value)
        {
            var item = new CachedItem<T> { Value = value, ExpirationTime = DateTimeOffset.Now.Add(_cacheDuration) };
            _cache[key] = item;
        }

        public bool ContainsKey(string key)
        {
            return _cache.ContainsKey(key);
        }

        private class CachedItem<T>
        {
            public T Value { get; set; }
            public DateTimeOffset? ExpirationTime { get; set; }
        }
    }
}
