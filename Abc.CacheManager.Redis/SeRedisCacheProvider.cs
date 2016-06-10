using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abc.CacheManager.Redis
{
    public class SeRedisCacheProvider : ICacheProvider
    {
        ConnectionMultiplexer redis;
        public SeRedisCacheProvider(string configuration)
        {
            redis = ConnectionMultiplexer.Connect(configuration);
        }

        //public SeRedisCacheProvider(Func<ConfigurationOptions> config)
        //{
        //    redis = ConnectionMultiplexer.Connect(config());
        //}

        private string getKey(string nameSpace, string key)
        {
            return nameSpace + "|" + key;
        }

        public void Delete(string nameSpace, string key)
        {
            IDatabase db = redis.GetDatabase();
            string cacheKey = getKey(nameSpace, key);
            db.KeyDeleteAsync(cacheKey);
        }

        public void FlushAll()
        {
            IDatabase db = redis.GetDatabase();
            redis.GetServer(redis.GetEndPoints().First()).FlushDatabase();
        }

        public string Get(string nameSpace, string key)
        {
            IDatabase db = redis.GetDatabase();
            string cacheKey = getKey(nameSpace, key);
            var strVal = db.StringGet(cacheKey);
            return strVal;
        }

        public void Upsert(string nameSpace, string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            IDatabase db = redis.GetDatabase();
            string cacheKey = getKey(nameSpace, key);
            db.StringSet(cacheKey, value, expiry: expiry);
        }
    }
}
