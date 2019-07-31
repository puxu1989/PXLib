using PXCache.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PXCache
{
    /// <summary>
    /// 描 述：缓存工厂类 调用入口
    /// </summary>
    public class CacheFactory
    {
        /// <summary>
        /// 程序缓存或Web缓存
        /// </summary>
        /// <returns></returns>
        public static ICache Cache()
        {
            if (HttpContext.Current != null)
            {
                return new WebCache();
            }
            else
            {
                return new MemoryCaches();
            }

        }
        /// <summary>
        /// 使用Redis服务缓存
        /// </summary>
        /// <returns></returns>
        public static ICache RedisCache()
        {
            return new PXCache.Redis.RedisCache();
        }
    }
}
