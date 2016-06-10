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

        private const string CahcheKeyFormate = "{0}#{1}";
        private string getKey(string nameSpace, string key)
        {
            if(string.IsNullOrWhiteSpace(nameSpace) ||
                string.IsNullOrWhiteSpace(key))
            {
                throw new Exception("Invalid namespace or key");
            }
            return string.Format(CahcheKeyFormate, nameSpace, key);
        }

        public void Delete(string nameSpace, string key)
        {
            IDatabase db = redis.GetDatabase();
            string cacheKey = getKey(nameSpace, key);
            db.KeyDeleteAsync(cacheKey);
        }

        //Priyakant: need to test this
        public void DeleteKeys(string nameSpacePattern, string keyPattern = null)
        {
            var server = redis.GetServer(redis.GetEndPoints().First());
            IDatabase db = redis.GetDatabase();

            if (string.IsNullOrWhiteSpace(nameSpacePattern))
            {
                throw new Exception("Invalid namespace");
            }

            string scanPattern = string.Format(CahcheKeyFormate
                , nameSpacePattern
                , string.IsNullOrWhiteSpace(keyPattern) ? "" : keyPattern);

            foreach (var key in server.Keys(pattern: scanPattern))
            {
                Console.WriteLine("Deleting key [{0}]", key);
                db.KeyDeleteAsync(key);
            }
        }

        public void FlushAll()
        {
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
