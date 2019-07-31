using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace PXLib.Helpers
{
    //4.0版本以上
    public class MemoryCacheHelper//.net自带的缓存
    {
        private static readonly Object _locker = new object();

        public static T GetCacheItem<T>(String key, Func<T> cachePopulate, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentException("Invalid cache key");
            if (cachePopulate == null) throw new ArgumentNullException("cachePopulate");
            if (slidingExpiration == null && absoluteExpiration == null) throw new ArgumentException("Either a sliding expiration or absolute must be provided");

            if (MemoryCache.Default[key] == null)
            {
                lock (_locker)
                {
                    if (MemoryCache.Default[key] == null)
                    {
                        var item = new CacheItem(key, cachePopulate());
                        var policy = CreatePolicy(slidingExpiration, absoluteExpiration);

                        MemoryCache.Default.Add(item, policy);
                    }
                }
            }
            return (T)MemoryCache.Default[key];
        }

        //是否存在
        public static bool IsExist(string key)
        {
            return MemoryCache.Default.Contains(key);
        }

        /// <summary>
        /// 获取Catch元素
        /// </summary>
        /// <typeparam name="T">所获取的元素的类型</typeparam>
        /// <param name="key">元素的键</param>
        /// <returns>特定的元素值</returns>
        public static T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("不合法的key!");
            if (!MemoryCache.Default.Contains(key))
                throw new ArgumentException("获取失败,不存在该key!");
            if (!(MemoryCache.Default[key] is T))
                throw new ArgumentException("未找到所需类型数据!");
            return (T)MemoryCache.Default[key];
        }

        /// <summary>
        /// 添加Catch元素 如果时间为null 则是整个软件运行期间
        /// </summary>
        /// <param name="key">元素的键</param>
        /// <param name="value">元素的值</param>
        /// <param name="slidingExpiration">元素未被访问的过期时间(时间间隔) new TimeSpan(0, 15, 0);</param>
        /// <param name="absoluteExpiration">元素过期时间(绝对时间)DateTime.Now.AddMinutes(15)</param>
        /// <returns></returns>
        public static bool Add(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            var item = new CacheItem(key, value);
            var policy = CreatePolicy(slidingExpiration, absoluteExpiration);
            lock (_locker)
                return MemoryCache.Default.Add(item, policy);
        }

        /// <summary>
        /// 移出Cache元素
        /// </summary>
        /// <typeparam name="T">待移出元素的类型</typeparam>
        /// <param name="key">待移除元素的键</param>
        /// <returns>已经移出的元素</returns>
        public static T Remove<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("不合法的key!");
            if (!MemoryCache.Default.Contains(key))
                throw new ArgumentException("获取失败,不存在该key!");
            var value = MemoryCache.Default.Get(key);
            if (!(value is T))
                throw new ArgumentException("未找到所需类型数据!");
            return (T)MemoryCache.Default.Remove(key);
        }

        /// <summary>
        /// 移出多条缓存数据,默认为所有缓存
        /// </summary>
        /// <typeparam name="T">待移出的缓存类型</typeparam>
        /// <param name="keyList"></param>
        /// <returns></returns>
        public static List<T> RemoveAll<T>(IEnumerable<string> keyList = null)
        {
            if (keyList != null)
                return (from key in keyList
                        where MemoryCache.Default.Contains(key)
                        where MemoryCache.Default.Get(key) is T
                        select (T)MemoryCache.Default.Remove(key)).ToList();
            while (MemoryCache.Default.GetCount() > 0)
                MemoryCache.Default.Remove(MemoryCache.Default.ElementAt(0).Key);
            return new List<T>();
        }

        /// <summary>
        /// 设置过期信息
        /// </summary>
        /// <param name="slidingExpiration"></param>
        /// <param name="absoluteExpiration"></param>
        /// <returns></returns>
        private static CacheItemPolicy CreatePolicy(TimeSpan? slidingExpiration, DateTime? absoluteExpiration)
        {
            var policy = new CacheItemPolicy();
            if (slidingExpiration.HasValue)//指定时间内未被访问后移除          
                policy.SlidingExpiration = slidingExpiration.Value;
            else if (absoluteExpiration.HasValue)//指定的持续时间之后移除缓存            
                policy.AbsoluteExpiration = absoluteExpiration.Value;
            policy.Priority = CacheItemPriority.Default;
            return policy;
        }
    }
}
