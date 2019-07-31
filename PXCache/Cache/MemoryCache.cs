using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace PXCache.Cache
{
    /// <summary>
    /// 内存缓存
    /// </summary>
    public class MemoryCaches : ICache
    {
        private static readonly Object _locker = new object();
        public T GetCache<T>(string cacheKey) where T : class
        {
            if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentException("不合法的key!");
            if (!MemoryCache.Default.Contains(cacheKey))
                throw new ArgumentException("获取失败,不存在该key!");
            if (!(MemoryCache.Default[cacheKey] is T))
                throw new ArgumentException("未找到所需类型数据!");
            return (T)MemoryCache.Default[cacheKey];
        }
        /// <summary>
        /// 写入缓存 默认10分钟
        /// </summary>
        public void WriteCache<T>(T value, string cacheKey) where T : class
        {
            var item = new CacheItem(cacheKey, value);
            var policy = CreatePolicy(null, DateTime.Now.AddMinutes(10));
            lock (_locker)
                MemoryCache.Default.Add(item, policy);
        }
        /// <summary>
        /// 写入缓存 从插入起多久过期
        /// </summary>
        /// <param name="value">对象数据</param>
        /// <param name="cacheKey">键</param>
        /// <param name="expireTime">到期时间</param>

        public void WriteCache<T>(T value, string cacheKey, DateTime expireTime) where T : class
        {
            var item = new CacheItem(cacheKey, value);
            var policy = CreatePolicy(null, expireTime);
            lock (_locker)
                MemoryCache.Default.Add(item, policy);
        }
        /// <summary>
        /// 插入缓存 到最后一次访问多久过期
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="cacheKey"></param>
        /// <param name="expireTime"></param>
        public void WriteCache<T>(T value, string cacheKey, TimeSpan expireTime) where T : class
        {
            var item = new CacheItem(cacheKey, value);
            var policy = CreatePolicy(expireTime, null);
            lock (_locker)
                MemoryCache.Default.Add(item, policy);
        }

        public void RemoveCache(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentException("不合法的key!");
            if (!MemoryCache.Default.Contains(cacheKey))
                throw new ArgumentException("获取失败,不存在该key!");
            MemoryCache.Default.Remove(cacheKey);
        }

        public void RemoveAllCache()
        {
            while (MemoryCache.Default.GetCount() > 0)
            {
                MemoryCache.Default.Remove(MemoryCache.Default.ElementAt(0).Key);
            }
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
