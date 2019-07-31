using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PXLib.Helpers;
using Newtonsoft.Json.Linq;

namespace PXLib.Api.Baidu
{
    public class IPlocation
    {
        /// <summary>
        /// 获取IP地址信息
        /// </summary>
        /// 
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string GetLocation(string ip)
        {
            string res = "";
            ////try
            ////{
            ////    string url = "http://apis.juhe.cn/ip/ip2addr?ip=" + ip + "&dtype=json&key=b39857e36bee7a305d55cdb113a9d725";//聚合数据接口 限定每天500次
            ////    res = RequestHelper.GetWebRequest(url);
            ////    JObject jobj= res.ToJObject();
            ////    JToken jtoken;
            ////    var rcode=jobj.GetValue("resultcode").ToString();
            ////    if (rcode == "200")
            ////    {
            ////        jtoken = jobj.GetValue("result");
            ////        jobj.TryGetValue("result", out jtoken);
            ////        res = jtoken["area"] + " " + jtoken["location"];
            ////    }
            ////    else 
            ////    {
            ////        res = "聚合数据IP"+jobj.GetValue("reason").ToString();
            ////    }
               
            ////}
            ////catch
            ////{
            ////    res = "";
            ////}
            //if (!string.IsNullOrEmpty(res))
            //{
            //    return res;
            //}
            try
            {
                string url = "https://sp0.baidu.com/8aQDcjqpAAV3otqbppnN2DJv/api.php?query=" + ip + "&resource_id=6006&ie=utf8&oe=gbk&format=json";
                res = RequestHelper.GetWebRequest(url, null, Encoding.GetEncoding("GBK"));
                var resjson = res.ToObject<locObj>();
                res = resjson.data[0].location;
            }
            catch
            {
                res = "百度查询IP接口错误";
            }
            return res;
        }
        /// <summary>
        /// 百度接口
        /// </summary>
        public class locObj
        {
            public List<dataone> data { get; set; }
        }
        public class dataone
        {
            public string location { get; set; }
        }
    }
}
