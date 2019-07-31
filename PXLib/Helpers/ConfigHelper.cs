using PXLib.Caches;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace PXLib.Helpers
{
    public class ConfigHelper
    {
        /// <summary>
        /// 获取配置文件里的AppSettings的值
        /// </summary>
        /// <param name="key"></param>
        public static string GetConfigAppSettingsValue(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key].Trim();//没有配置会报错
            }
            catch
            {
                return string.Empty;
            }

        }
        /// <summary>
        /// 得到AppSettings中的配置字符串信息  加缓存 Web使用
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetCacheConfigString(string key)
        {
            string cacheKey = "AppSettings_" + key;
            string objValue = CacheFactory.Cache().GetCache<string>(cacheKey);
            if (objValue == null) 
            {
                objValue = GetConfigAppSettingsValue(key);
                CacheFactory.Cache().WriteCache(cacheKey, objValue, DateTime.Now.AddDays(2));
            }
            return objValue;
        }

        /// <summary>
        /// 得到AppSettings中的配置Bool信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetConfigBool(string key)
        {
            return GetConfigAppSettingsValue(key).ToBool();
        }
    }
}
