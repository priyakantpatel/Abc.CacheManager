using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abc.CacheManager
{
    public interface IMissingCacheProvider
    {
    }

    public interface IMissingCacheProvider<TVal>
        : IMissingCacheProvider where TVal : class
    {
        TVal GetValue(ICacheManager cm, string ns, string k);
    }
}
