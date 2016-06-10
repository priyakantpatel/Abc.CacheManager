using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Abc.CacheManager.Test.Models;
using Abc.CacheManager.Providers;
using StructureMap;

namespace Abc.CacheManager.Test
{
    [TestClass]
    public class CacheManagerTestIoc
    {
        [TestMethod]
        public void WhenUseIocGetSetShouldWork()
        {
            var container = new Container(_ =>
            {
                bool runTestForRedisProvider = false;

                if (runTestForRedisProvider)
                {
                    //Make sure you have redis installed locally and have default port 6379. Or update following redisConfig value
                    string redisConfig = "localhost:6379,defaultDatabase=1";
                    _.For<ICacheProvider>().Use(() => new Redis.SeRedisCacheProvider(redisConfig));
                }
                else
                {
                    //Register Cache Provider
                    _.For<ICacheProvider>().Use(() => new InMemoryCacheProvider(1000));
                }

                //Register CacheManager
                _.For<ICacheManager>()
                .Use<CacheManager>()
                .Singleton();

                _.For<ILogger>().Use<ConsoleLogger>();

                _.For<IMissingCacheProvider<Car>>().Use<CarValueProvider>();

                _.For<ITestIocClass>().Use<TestIocClass >();
                _.For<IRandomCarGenerator>().Use<RandomCarGenerator>();
            });

            container
                .GetInstance<ICacheManager>()
                .Use((t, nameSpace) => (IMissingCacheProvider)container.GetInstance(t))
                .Expires(TimeSpan.FromSeconds(4))
                .Logger(container.GetInstance<ILogger>());

            //Call Test
            container
                .GetInstance<ITestIocClass>()
                .Test();
        }
    }

    public interface ITestIocClass
    {
        void Test();
    }
    public class TestIocClass : ITestIocClass
    {
        public readonly ICacheManager _cm = null;
        public TestIocClass (ICacheManager cm)
        {
            _cm = cm;
        }

        public void Test()
        {
            string ns = "TestNs";
            string key = "TestKey";

            var retValue = _cm.Get<Car>(ns, key);

            Assert.IsNotNull(retValue.Id);
            Assert.IsNotNull(retValue.Kind);
        }
    }

    public interface IRandomCarGenerator
    {
        Car GetCar();
    }
    public class RandomCarGenerator : IRandomCarGenerator
    {
        public Car GetCar()
        {
            return new Car
            {
                Id = 51,
                Kind = "Honda"
            };
        }
    }

    public class CarValueProvider : IMissingCacheProvider<Car>
    {
        readonly IRandomCarGenerator _rcg = null;
        public CarValueProvider(IRandomCarGenerator rcg)
        {
            _rcg = rcg;
        }
        public Car GetValue(ICacheManager cm, string nameSpace, string key)
        {
            var rv = _rcg.GetCar();

            cm.Upsert(nameSpace, key, rv, TimeSpan.FromMilliseconds(1000));
            return rv;
        }
    }
}
