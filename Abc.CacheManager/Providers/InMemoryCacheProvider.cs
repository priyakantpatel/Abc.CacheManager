using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Abc.CacheManager.Providers
{
    public class InMemoryCacheProvider : ICacheProvider
    {
        private class CacheValue
        {
            public string Value { get; set; }
            public long expires { get; set; }
        }

        ReaderWriterLock _lock = new ReaderWriterLock();
        Dictionary<string, CacheValue> _cache = new Dictionary<string, CacheValue>();

        public InMemoryCacheProvider(int celanupResolutionMs = 0)
        {
            if (celanupResolutionMs < 100 || celanupResolutionMs > 5 * 60 * 1000)
            {
                celanupResolutionMs = 1000;
            }

            //Console.WriteLine("[InMemoryCacheProvider] celanupResolutionMs [{0}]", celanupResolutionMs);

            Timer timer = new Timer((e) =>
            {
                try
                {
                    _lock.AcquireWriterLock(Timeout.Infinite);
                    _cache.Where(x => x.Value.expires <= DateTime.Now.Ticks)
                    .Select(x => x.Key)
                    .ToList()
                    .ForEach(x =>
                    {
                        //Console.WriteLine("[InMemoryCacheProvider] ~~~~~ Removeing Expired key [{0}]", x);
                        _cache.Remove(x);
                    });
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("[InMemoryCacheProvider] Exception in expiring cache function");
                    //Console.WriteLine(ex.ToString());
                }
                finally
                {
                    _lock.ReleaseWriterLock();
                }
            },null, celanupResolutionMs, celanupResolutionMs);

        }

        private string getKey(string nameSpace, string key)
        {
            return nameSpace + "|" + key;
        }

        public string Get(string nameSpace, string key)
        {
            try
            {
                string cacheKey = getKey(nameSpace, key);
                _lock.AcquireReaderLock(Timeout.Infinite);
                if (_cache.ContainsKey(cacheKey))
                {
                    var cv = _cache[cacheKey];
                    string strVal = cv.expires > DateTime.Now.Ticks ? _cache[cacheKey].Value : default(string);
                    //string strVal = _cache[cacheKey].Value;
                    return strVal;
                }
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }

            return default(string);
        }

        public void Upsert(string nameSpace, string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            try
            {
                string cacheKey = getKey(nameSpace, key);
                _lock.AcquireWriterLock(Timeout.Infinite);
                _cache[cacheKey] = new CacheValue
                {
                    Value = value,
                    expires = DateTime.Now.AddMilliseconds(expiry.HasValue ? expiry.Value.TotalMilliseconds : 0).Ticks
                };
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void Delete(string nameSpace, string key)
        {
            try
            {
                string cacheKey = getKey(nameSpace, key);

                _lock.AcquireWriterLock(Timeout.Infinite);
                if (_cache.ContainsKey(cacheKey))
                {
                    _cache.Remove(cacheKey);
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void FlushAll()
        {
            try
            {
                _lock.AcquireWriterLock(Timeout.Infinite);
                _cache = new Dictionary<string, CacheValue>();
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }
    }
}
