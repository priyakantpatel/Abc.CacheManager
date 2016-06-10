using System;
using System.Threading.Tasks;

namespace Abc.CacheManager
{
    public interface ICacheManager
    {
        /// <summary>
        /// Create or Update cache for the key synchronously
        /// </summary>
        /// <typeparam name="T">Type object to cache</typeparam>
        /// <param name="nameSpace">Namespace</param>
        /// <param name="key">Key</param>
        /// <param name="value">Cache value</param>
        /// <param name="expiresMs">Expires in milliseconds</param>
        void Upsert<T>(string nameSpace, string key, T value, TimeSpan? expiry = default(TimeSpan?));

        /// <summary>
        /// Create or Update cache for the key asynchronously
        /// </summary>
        /// <typeparam name="T">Type object to cache</typeparam>
        /// <param name="nameSpace">Namespace</param>
        /// <param name="key">Key</param>
        /// <param name="value">Cache value</param>
        /// <param name="expiresMs">Expires in milliseconds</param>
        Task UpsertAsync<T>(string nameSpace, string key, T value, TimeSpan? expiry = default(TimeSpan?));

        /// <summary>
        /// Delete cached value and entry for the key
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="key"></param>
        void Delete(string nameSpace, string key);
        Task DeleteAsync(string nameSpace, string key);

        /// <summary>
        /// Delete all cache
        /// </summary>
        void FlushAll();

        /// <summary>
        /// Get value from the cache. If value is missing will call IMissingCacheProvider if registered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameSpace">Namespace</param>
        /// <param name="key">Key</param>
        /// <returns>Returns value or default value of the requested Type. Makesure to handle for a null values.</returns>
        T Get<T>(string nameSpace, string key) where T : class;

        /// <summary>
        /// SimplyGet get value from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameSpace">Namespace</param>
        /// <param name="key">Key</param>
        /// <returns></returns>
        T SimplyGet<T>(string nameSpace, string key) where T : class;

        /// <summary>
        /// IMissingCacheProvider resolver
        /// </summary>
        /// <param name="resolver">IMissingCacheProvider resolver</param>
        /// <returns>ICacheManager</returns>
        ICacheManager Use(Func<Type, string, IMissingCacheProvider> resolver);

        /// <summary>
        /// Cash expires timeout
        /// </summary>
        /// <param name="expires"></param>
        /// <returns>ICacheManager</returns>
        ICacheManager Expires(TimeSpan? expires = default(TimeSpan?));

        /// <summary>
        /// Set logger
        /// </summary>
        /// <param name="logger">logger</param>
        /// <returns>ICacheManager</returns>
        ICacheManager Logger(Providers.ILogger logger);
    }
}
