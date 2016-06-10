using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Abc.CacheManager.Providers;
using NSubstitute;

namespace Abc.CacheManager.Test
{
    [TestClass]
    public class CacheManagerLoggerTest
    {
        ICacheManager GetCacheManager()
        {
            ICacheManager cm = null;
            var cp = new InMemoryCacheProvider();
            cm = new CacheManager(cp);
            return cm;
        }

        [TestMethod]
        public void WhenUseLoggerLogsShouldProduced()
        {
            ICacheManager cm = GetCacheManager();

            var logger = Substitute.For<ILogger>();

            cm.Logger(logger);

            cm.FlushAll();  //FlushAll is creating  Warn log

            logger.Received().Warn(Arg.Any<string>());
        }
    }
}
