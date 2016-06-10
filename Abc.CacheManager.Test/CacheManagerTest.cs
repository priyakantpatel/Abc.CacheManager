using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Abc.CacheManager.Test.Models;
using Abc.CacheManager.Providers;
using System.Threading;

namespace Abc.CacheManager.Test
{
    [TestClass]
    public class CacheManagerTest
    {
        ICacheManager GetCacheManager()
        {
            bool runTestForRedisProvider = true;

            ICacheManager cm = null;

            if (runTestForRedisProvider)
            {
                //Make sure you have redis installed locally and have default port 6379. Or update following redisConfig value
                string redisConfig = "localhost:6379,defaultDatabase=1,allowAdmin=true";
                var redisCp = new Redis.SeRedisCacheProvider(redisConfig);
                cm = new CacheManager(redisCp);
            }
            else
            {
                var cp = new InMemoryCacheProvider();
                cm = new CacheManager(cp);
            }

            return cm;
        }

        [TestMethod]
        public void WhenSetValueSyncShouldGetSameValue()
        {
            ICacheManager cm = GetCacheManager();

            string ns = "TestNs";
            string key = "TestKey";
            string value = "TestValue";
            cm.Upsert(ns, key, value, TimeSpan.FromSeconds(4));

            string retValue = cm.Get<string>(ns, key);

            Assert.AreEqual(value, retValue);           
        }

        [TestMethod]
        public void WhenSetValueAsyncShouldGetSameValue()
        {
            ICacheManager cm = GetCacheManager();

            string ns = "TestNs";
            string key = "TestKey";
            string value = "TestValue";
            var task = cm.UpsertAsync(ns, key, value, TimeSpan.FromSeconds(4));
            if(!task.Wait(TimeSpan.FromSeconds(4)))
            {
                throw new Exception("Timeout expired");
            }

            string retValue = cm.Get<string>(ns, key);

            Assert.AreEqual(value, retValue);
        }

        [TestMethod]
        public void WhenDeleteKeySyncShouldGetNullValue()
        {
            ICacheManager cm = GetCacheManager();

            string ns = "TestNs";
            string key = "TestKey";
            string value = "TestValue";
            string retValue = default(string);


            cm.Upsert(ns, key, value, TimeSpan.FromSeconds(4));

            retValue = cm.Get<string>(ns, key);
            Assert.AreEqual(value, retValue);

            cm.Delete(ns, key);

            retValue = cm.Get<string>(ns, key);
            Assert.IsNull(retValue);
        }

        [TestMethod]
        public void WhenDeleteAsyncKeySyncShouldGetNullValue()
        {
            ICacheManager cm = GetCacheManager();

            string ns = "TestNs";
            string key = "TestKey";
            string value = "TestValue";
            string retValue = default(string);

            cm.Upsert(ns, key, value, TimeSpan.FromSeconds(4));

            retValue = cm.Get<string>(ns, key);
            Assert.AreEqual(value, retValue);

            var task = cm.DeleteAsync(ns, key);

            if (!task.Wait(TimeSpan.FromSeconds(4)))
            {
                throw new Exception("Timeout expire");
            }

            retValue = cm.Get<string>(ns, key);
            Assert.IsNull(retValue);
        }

        [TestMethod]
        public void WhenSetValueExpiredShouldGetNullValue()
        {
            ICacheManager cm = GetCacheManager();

            string ns = "TestNs";
            string key = "TestKey";
            string value = "TestValue";
            cm.Upsert(ns, key, value, TimeSpan.FromMilliseconds(10));

            Thread.Sleep(20);  //Should expired by now

            string retValue = cm.Get<string>(ns, key);

            Assert.IsNull(retValue);
        }

        [TestMethod]
        public void WhenSetComplexValueShouldGetSameComplexValue()
        {
            ICacheManager cm = GetCacheManager();

            string ns = "TestNs";
            string key = "TestKey";
            var value = new Car { Id = 1, Kind = "Ford" };

            cm.Upsert(ns, key, value, TimeSpan.FromSeconds(4));

            var retValue = cm.Get<Car>(ns, key);

            Assert.AreEqual(value.Id, retValue.Id);
            Assert.AreEqual(value.Kind, retValue.Kind);
        }

        [TestMethod]
        public void WhenFlushAllItShouldRemoveAllValuesValue()
        {
            ICacheManager cm = GetCacheManager();

            string ns = "TestNs";
            string key = "TestKey";
            var value = new Car { Id = 1, Kind = "Ford" };

            cm.Upsert(ns, key, value, TimeSpan.FromSeconds(4));

            string ns2 = "TestNs2";
            string key2 = "TestKey2";
            var value2 = "TestValue2";
            cm.Upsert(ns2, key2, value2, TimeSpan.FromSeconds(4));

            cm.FlushAll();

            Thread.Sleep(5000); //Wait some time. redis could take few seconds

            var retValue = cm.Get<Car>(ns, key);
            Assert.AreEqual(null, retValue);

            var retValue2 = cm.Get<string>(ns2, key2);
            Assert.AreEqual(null, retValue2);
        }

        [TestMethod]
        public void WhenSetValueNullShouldNotThrowError()
        {
            ICacheManager cm = GetCacheManager();

            string ns = "TestNs";
            string key = "TestKey";
            Car value = null;

            cm.Upsert(ns, key, value, TimeSpan.FromSeconds(4));

            var retValue = cm.Get<Car>(ns, key);

            Assert.IsNull(retValue);
        }
    }
}
