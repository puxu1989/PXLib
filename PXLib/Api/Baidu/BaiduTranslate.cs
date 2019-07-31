using Newtonsoft.Json.Linq;
using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Baidu.Api
{
    public class BaiduTranslate
    {
        #region 使用百度API翻译字符串
        /// <summary>
        /// 翻译一个字符串
        /// </summary>
        public static string Translate(string query, TranslateTo TranslateTo)
        {
            // 多个query可以用\n连接  如 query='apple\norange\nbanana\npear'
            //http://api.fanyi.baidu.com/api/trans/vip/translate?q=apple&from=en&to=zh&appid=2015063000000001&salt=1435660288&sign=f89f9594663708c1605f3d736d01d2d4
            //md5加密的参数：str为拼接appid=2015063000000001+q=apple+salt=1435660288+密钥=12345678
            const string serverUrl = "http://api.fanyi.baidu.com/api/trans/vip/translate";
            const string appid = "20170619000059276";
            const string appsecret = "gzTeKfsNbwJcimeJWNxp";
            string salt = TimeHelper.GetTimeStamp().ToString();
            string sign = SecurityHelper.MD5String(appid + query + salt + appsecret).ToLower();
            string to = "auto";
            if (TranslateTo == TranslateTo.ToEn)
                to = "en";
            if (TranslateTo == TranslateTo.ToZh)
                to = "zh";
            string json = RequestHelper.GetWebRequest(serverUrl, new { from = "auto", to = to, appid = appid, q = query, salt = salt, sign = sign }, Encoding.UTF8);
            JObject jsonObj = JsonHelper.DeserializeJObject(json);
            if (!jsonObj.HasValues) throw new Exception("Translate Failed");
            JToken temp;
            if (jsonObj.TryGetValue("error_code", out temp))
            {
                //返回错误代码http://api.fanyi.baidu.com/api/trans/product/apidoc
                return jsonObj.GetValue("error_msg").ToString();
            }
            JToken res = jsonObj.GetValue("trans_result");
            var ress = res[0]["dst"].ToString();
            return ress;
        }
        public enum TranslateTo
        {
            ToZh,
            ToEn
        }
        #endregion
    }
}
