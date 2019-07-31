using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace PXLib.Helpers
{
    /// <summary>
    /// Web缓存帮助类
    /// </summary>
   public class CacheHelper
    {
       /// <summary>
       /// 获取一个对象
       /// </summary>
       public static T GetCache<T>(string key)
        {
            object cache = GetCache(key);
            return ((cache == null) ? default(T) : ((T)cache));
        }
       /// <summary>
       /// 获取缓存 object
       /// </summary>
       /// <param name="key"></param>
       /// <returns></returns>
        public static object GetCache(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            return HttpContext.Current.Cache.Get(key);
        }
        /// <summary>
        /// 缓存多少小时
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="hours"></param>
        public static void Insert(string key, object obj, int hours)
        {
            if (obj != null)
            {
                HttpContext.Current.Cache.Insert(key, obj, null, Cache.NoAbsoluteExpiration, new TimeSpan(hours, 0, 0));
                //HttpContext.Current.Cache.Insert(key, obj, null,DateTime.Now.AddHours(hours), TimeSpan.Zero); //二选一
            }
        }
        /// <summary>
        /// 插入缓存，自定义时间 最后一次访问后过多久到期 new TimeSpan(int h,int, m int s)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void Insert(string key, object obj, TimeSpan slidingExpiration)
        {
            if (obj != null)
            {
                HttpContext.Current.Cache.Insert(key, obj, null, Cache.NoAbsoluteExpiration, slidingExpiration);
            }
        }
        /// <summary>
        /// 重现在开始到多久时间过期 DateTime.Now.AddDay(1);
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void Insert(string key, object obj, DateTime datetime)
        {
            if (obj != null)
            {
                HttpContext.Current.Cache.Insert(key, obj, null,datetime, TimeSpan.Zero); //
            }
        }
       /// <summary>
       /// 文件缓存 ??
       /// </summary>
        public static void InsertFile(string key, object obj, string filePathName)
        {
            CacheDependency dependencies = new CacheDependency(filePathName);
            HttpContext.Current.Cache.Insert(key, obj, dependencies);//文件依赖项？
        }
       /// <summary>
       /// 缓存是否存在
       /// </summary>
        public static bool IsExist(string strKey)
        {
            return (HttpContext.Current.Cache[strKey] != null);
        }
       /// <summary>
       /// 移除所有缓存
       /// </summary>
        public static void RemoveAllCache()
        {
            Cache cache = HttpRuntime.Cache;
            IDictionaryEnumerator enumerator = cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                cache.Remove(enumerator.Key.ToString());
            }
        }
        /// <summary>
        /// 移除指定缓存
        /// </summary>
        /// <param name="CacheKey"></param>
        public static void RemoveCache(string CacheKey)
        {
            HttpRuntime.Cache.Remove(CacheKey);
        }
    }
}
