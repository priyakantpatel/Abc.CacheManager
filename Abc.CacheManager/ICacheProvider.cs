using System;

namespace Abc.CacheManager
{
    public interface ICacheProvider
    {
        /// <summary>
        /// Get value from the cache
        /// </summary>
        /// <param name="nameSpace">Namespace</param>
        /// <param name="key">Key</param>
        /// <returns>Expires in milliseconds</returns>
        string Get(string nameSpace, string key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameSpace">Namespace</param>
        /// <param name="key">Key</param>
        /// <param name="value">Cache value</param>
        /// <param name="expiresMs">Expires in milliseconds</param>
        void Upsert(string nameSpace, string key, string value, TimeSpan? expiry = default(TimeSpan?));

        /// <summary>
        /// Delete value from the cache
        /// </summary>
        /// <param name="nameSpace">Namespace</param>
        /// <param name="key">Key</param>
        void Delete(string nameSpace, string key);

        /// <summary>
        /// Delete all cache values
        /// </summary>
        void FlushAll();
    }
}
