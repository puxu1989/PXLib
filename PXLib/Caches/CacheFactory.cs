using PXLib.Caches.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PXLib.Caches
{
    /// <summary>
    /// 描 述：缓存工厂类 调用入口
    /// </summary>
    public class CacheFactory
    {
        /// <summary> 
        /// 创建ICache缓存对象
        /// </summary>
        /// <returns></returns>
        public static ICache Cache()
        {

            if (RedisConfigInfo.GetConfig() != null)
            {
                return new Redis.RedisCache();
            }
            else if (HttpContext.Current != null)
            {
                return new WebCache();
            }
            else
            {
                return new MemoryCaches();
            }

        }
    }
}
