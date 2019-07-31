using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using PXLib;
using PXLib.Attributes;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PXLib.Helpers
{
    /// <summary>
    /// 网络请求类  .net下尽量使用HttpWebRequest core下使用HttpClient
    /// </summary>
    public class RequestHelper
    {
        /// <summary>  
        /// 发起异步GET请求   使用HttpClient 
        /// </summary>  
        /// <param name="url"></param>  
        /// <returns></returns>  
        public static string GetAsyncResponse(string url)
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = httpClient.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                httpClient.Dispose();
                return result;
            }
            return null;
        }
        public static string GetWebRequest(string url)
        {
            return GetWebRequest(url, null, Encoding.UTF8);
        }
        #region 发情Get/Post请求
        /// <summary>
        /// 发起Get请求 例子string str=GetWebRequest("http://www.wedn.net",new{p1="hello",p2="world"});
        /// </summary>
        /// <returns></returns>
        public static string GetWebRequest(string url, object queryData, Encoding encodeing)
        {

            string res = string.Empty;
            try
            {
                string queryString = string.Empty;
                if (queryData != null)
                {
                    queryString = string.Join("&", from p in queryData.GetType().GetProperties() select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(queryData, null).ToString()));
                    string s = url.Contains("?") ? "&" : "?";
                    url = string.Format("{0}{1}{2}", url,s,queryString);
                }                  
                var request = WebRequest.Create(url) as HttpWebRequest;
                if (request == null)
                    return string.Empty;
                request.Timeout = 19600;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.94 Safari/537.36";
                var response = (HttpWebResponse)request.GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        var reader = new StreamReader(stream, encodeing);
                        var sb = new StringBuilder();
                        while (-1 != reader.Peek())
                        {
                            sb.Append(reader.ReadLine() + "\r\n");
                        }
                        res = sb.ToString();
                    }
                }
                response.Close();
            }
            catch (Exception ee)
            {
                res = ee.Message;
            }
            return res;
        }
        /// <summary>
        /// 发起Web请求 默认POST  conentType默认application/x-www-form-urlencoded
        /// </summary>
        public static string PostWebRequest(string url, string postData, Encoding encode, ContentType type = ContentType.urlencoded, WebHeaderCollection header = null)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(postData == null ? "" : postData);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
            webRequest.Method = "POST";
            webRequest.Accept = "*/*";
            webRequest.Timeout = 20000;
            webRequest.AllowAutoRedirect = false;
            webRequest.ContentLength = byteArray.Length;
            webRequest.ContentType = type.GetEnumText();//可选application/json
            if (header != null)
            {
                webRequest.Headers = header;
            }
            try
            {
                using (Stream newStream = webRequest.GetRequestStream())
                {
                    newStream.Write(byteArray, 0, byteArray.Length);
                    newStream.Close();
                }
                //接收返回信息：               
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), encode);
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion
        #region 使用WebClient
        public static Task<string> PostStringTaskAsync(string url, string postData)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers.Add("Content-Type", ContentType.json.GetEnumText());
                return webClient.UploadStringTaskAsync(url, postData);
            }
        }
        #endregion
    }
    public enum ContentType
    {
        [EnumText("application/x-www-form-urlencoded")]
        urlencoded,
        [EnumText("application/json")]
        json,
    }
}
