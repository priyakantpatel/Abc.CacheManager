using System;
using System.Threading.Tasks;
using Abc.CacheManager.Providers;

namespace Abc.CacheManager
{
    public class CacheManager : ICacheManager
    {
        readonly ICacheProvider _cacheProvider = null;
        private TimeSpan? _defaultExpires = null;
        private Func<Type, string, IMissingCacheProvider> _use = null;
        private ILogger _logger = null;

        public CacheManager(ICacheProvider cp)
        {
            _cacheProvider = cp;
            if (_logger != null)
            {
                _logger.Trace("[CacheManager] created");
            }
        }

        private string SerializeObject<T>(T value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }

        private T DeserializeObject<T>(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? default(T) : Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
        }

        public void Upsert<T>(string nameSpace, string key, T value, TimeSpan? expiry = default(TimeSpan?))
        {
            try
            {
                _cacheProvider.Upsert(nameSpace, key, SerializeObject(value), expiry ?? _defaultExpires);
            }
            catch (Exception ex)
            {
                _logger.Error("[CacheManager] Exception");
                _logger.Error(ex.ToString());
            }
        }

        public Task UpsertAsync<T>(string nameSpace, string key, T value, TimeSpan? expiry = default(TimeSpan?))
        {
            //Update cache ASYNC
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    _cacheProvider.Upsert(nameSpace, key, SerializeObject(value), expiry ?? _defaultExpires);
                }
                catch (Exception ex)
                {
                    _logger.Error("[CacheManager] Exception");
                    _logger.Error(ex.ToString());
                }
            });
        }

        public void Delete(string nameSpace, string key)
        {
            try
            {
                _cacheProvider.Delete(nameSpace, key);
            }
            catch (Exception ex)
            {
                _logger.Error("[CacheManager] Exception");
                _logger.Error(ex.ToString());
            }
        }

        public Task DeleteAsync(string nameSpace, string key)
        {
            return Task.Factory.StartNew(() => {
                try
                {
                    _cacheProvider.Delete(nameSpace, key);
                }
                catch (Exception ex)
                {
                    _logger.Error("[CacheManager] Exception");
                    _logger.Error(ex.ToString());
                }
            });
        }

        public void FlushAll()
        {
            try
            {
                if (_logger != null)
                {
                    _logger.Warn("[CacheManager] FlushAll");
                }

                _cacheProvider.FlushAll();
            }
            catch (Exception ex)
            {
                _logger.Error("[CacheManager] Exception");
                _logger.Error(ex.ToString());
            }
        }

        public T SimplyGet<T>(string nameSpace, string key) where T : class
        {
            try
            {
                string strValue = _cacheProvider.Get(nameSpace, key);
                var value = DeserializeObject<T>(strValue);
                return value;
            }
            catch (Exception ex)
            {
                _logger.Error("[CacheManager] Exception");
                _logger.Error(ex.ToString());
            }

            return default(T);
        }

        public T Get<T>(string nameSpace, string key) where T : class
        {
            try
            {
                string strValue = _cacheProvider.Get(nameSpace, key);
                var value = DeserializeObject<T>(strValue);

                if (value == null && _use != null)
                {
                    var mvg = (IMissingCacheProvider<T>)_use(typeof(IMissingCacheProvider<T>), nameSpace);
                    if (null != mvg)
                    {
                        if (_logger != null)
                        {
                            _logger.Trace("[CacheManager] Get value from provider. nameSpace [{0}], key [{1}]", nameSpace, key);
                        }

                        value = mvg.GetValue(this, nameSpace, key);
                    }
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.Error("[CacheManager] Exception");
                _logger.Error(ex.ToString());
            }

            return default(T);
        }

        public ICacheManager Use(Func<Type, string, IMissingCacheProvider> resolver)
        {
            _use = resolver;
            return this;
        }

        public ICacheManager Expires(TimeSpan? expires = default(TimeSpan?))
        {
            _defaultExpires = expires;
            return this;
        }

        public ICacheManager Logger(ILogger logger)
        {
            _logger = logger;
            return this;
        }
    }
}
