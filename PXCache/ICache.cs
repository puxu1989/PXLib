using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXCache
{
    /// <summary>
    /// 定义缓存接口 WebCache,Redis,MemoryCache可统一继承该接口 用一个工厂模式来决定创建存储模式
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// 读取缓存
        /// </summary>
        /// <param name="cacheKey">键</param>
        /// <returns></returns>
        T GetCache<T>(string cacheKey) where T : class;
        /// <summary> 
        /// 写入缓存  Web和Memory默认10分钟 Redis以连接信息时间为准
        /// </summary>
        /// <param name="value">对象数据</param>
        /// <param name="cacheKey">键</param>
        void WriteCache<T>(T value, string cacheKey) where T : class;
        /// <summary>
        /// 写入缓存 从插入起多久过期  例子：DateTime.Now.AddDay(1);
        /// </summary>
        /// <param name="value">对象数据</param>
        /// <param name="cacheKey">键</param>
        /// <param name="expireTime">到期时间</param>
        void WriteCache<T>(T value, string cacheKey, DateTime absoluteExpiration) where T : class;
        /// <summary>
        /// 写入缓存 最后一次访问后多久过期 相对时间
        /// </summary>
        void WriteCache<T>(T value, string cacheKey, TimeSpan slidingExpiration) where T : class;
        /// <summary>
        /// 移除指定数据缓存
        /// </summary>
        /// <param name="cacheKey">键</param>
        void RemoveCache(string cacheKey);
        /// <summary>
        /// 移除全部缓存
        /// </summary>
        void RemoveAllCache();
    }
}
