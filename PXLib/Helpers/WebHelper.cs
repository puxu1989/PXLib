using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PXLib.Helpers
{
    public class WebHelper
    {
        /// <summary>
        /// 判断是否是Web程序
        /// </summary>
        /// <returns></returns>
        public static bool IsWebApp()
        {
            return HttpContext.Current != null ? true : false;
        }
        #region 获取访问的页面 /参数个数
        /// <summary>
        /// 获取页面的请求参数 
        /// </summary>
        /// <returns></returns>
        public static string GetQueryString()//query=?id=5&name=kelli
        {
            string query = HttpContext.Current.Request.Url.Query;
            return query;
        }
        /// <summary>
        /// 判断当前页面是否接收到了Post请求
        /// </summary>
        /// <returns>是否接收到了Post请求</returns>
        public static bool IsPost()
        {
            return HttpContext.Current.Request.HttpMethod.Equals("POST");
        }
        /// <summary>
        /// 判断当前页面是否接收到了Get请求
        /// </summary>
        /// <returns>是否接收到了Get请求</returns>
        public static bool IsGet()
        {
            return HttpContext.Current.Request.HttpMethod.Equals("GET");
        }

        /// <summary>
        /// 返回指定的服务器变量信息
        /// </summary>
        /// <param name="strName">服务器变量名</param>
        /// <returns>服务器变量信息</returns>
        public static string GetServerString(string strName)
        {
            //
            if (HttpContext.Current.Request.ServerVariables[strName] == null)
            {
                return "";
            }
            return HttpContext.Current.Request.ServerVariables[strName].ToString();
        }

        /// <summary>
        /// 返回上一个页面的地址
        /// </summary>
        /// <returns>上一个页面的地址</returns>
        public static string GetUrlReferrer()
        {
            string retVal = null;

            try
            {
                retVal = HttpContext.Current.Request.UrlReferrer.ToString();
            }
            catch { }

            if (retVal == null)
                return "";

            return retVal;

        }
        /// <summary>
        /// 获取当前访问页面地址
        /// </summary>
        public static string GetScriptName
        {
            get
            {
                return HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"].ToString();
            }
        }
        /// <summary>
        /// 获得当前页面的名称
        /// </summary>
        /// <returns>当前页面的名称</returns>
        public static string GetPageName()
        {
            string[] urlArr = HttpContext.Current.Request.Url.AbsolutePath.Split('/');
            return urlArr[urlArr.Length - 1].ToLower();
        }
        /// <summary>
        /// 返回表单或Url参数的总个数 
        /// </summary>
        /// <returns></returns>
        public static int GetParamCount()
        {
            return HttpContext.Current.Request.Form.Count + HttpContext.Current.Request.QueryString.Count;
        }
        /// <summary>
        /// 获得当前完整Url地址
        /// </summary>
        /// <returns>当前完整Url地址</returns>
        public static string GetUrl()
        {
            return HttpContext.Current.Request.Url.ToString();
        }
        /// <summary>
        /// 获得指定Url参数的值
        /// </summary>
        /// <param name="strName">Url参数</param>
        /// <returns>Url参数的值</returns>
        public static string GetQueryStringValue(string strName)
        {
            if (HttpContext.Current.Request.QueryString[strName] == null)
            {
                return "";
            }
            return HttpContext.Current.Request.QueryString[strName];
        }


        #endregion
        #region 获取站点绝对目录
        /// <summary>
        /// 站点绝对目录 例子D:\SVN\GYBOFWEB\
        /// </summary>
        public static string SiteRootPath
        {
            get
            {            
                return HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"];//=  HttpContext.Current.Request.PhysicalApplicationPath;//=AppDomain.CurrentDomain.BaseDirectory
            }
        }
        #endregion
        #region 获取客户端的IP 端口 主机名 主机域名
        /// <summary>    
        /// 获取客户端的IP    
        /// </summary>    
        public static string GetClientIPAddress
        {
            get
            {
                var result = string.Empty;
                if (HttpContext.Current != null)//获取web远程IP
                {
                    var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    foreach (var hostAddress in Dns.GetHostAddresses(ip))
                    {
                        if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
                            result = hostAddress.ToString();
                    }
                }
                if (result.IsNullEmpty()) //获取局域网IP
                {
                    foreach (var hostAddress in Dns.GetHostAddresses(Dns.GetHostName()))
                    {
                        if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
                            result = hostAddress.ToString();
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 得到当前服务器完整协议+主机头+:端口 
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentServerHostOrIPPort
        {
            get
            {
                HttpRequest request = System.Web.HttpContext.Current.Request;
                // 是否为SSL认证站点
                string secure = request.ServerVariables["HTTPS"];
                string httpProtocol = (secure == "on" ? "https" : request.Url.Scheme);//或者request.Url.Scheme但不带s
                if (!request.Url.IsDefaultPort)
                    return string.Format("{0}://{1}:{2}", httpProtocol, request.Url.Host, request.Url.Port.ToString());
                return string.Format("{0}://{1}", httpProtocol, request.Url.Host);
            }
        }
        /// <summary>
        /// 获取本地（非远程）主机名 例PX-PC
        /// </summary>
        public static string GetClientHostPCName
        {
            get
            {
                if (!HttpContext.Current.Request.IsLocal)
                    return string.Empty;
                var ip = GetClientIPAddress;
                var result = Dns.GetHostEntry(IPAddress.Parse(ip)).HostName;
                if (result == "localhost.localdomain")//C:\Windows\System32\drivers\etc\hosts文件
                    result = Dns.GetHostName();
                return result;
            }
        }
        /// <summary> 
        /// 获取本地主机名 
        /// </summary> 
        /// <returns></returns> 
        public static string HostName
        {
            get
            {
                string hostname = Dns.GetHostName();
                return hostname;
            }
        }
        /// <summary>
        /// 获取主机名,即域名，
        /// 范例：用户输入网址http://www.a.com/b.htm?a=1&amp;b=2，
        /// 返回值为: www.a.com
        /// </summary>
        public static string HostUrl
        {
            get
            {
                return HttpContext.Current.Request.Url.Host;
            }
        }
        #endregion

        #region 根据URL获取网站域名
        /// <summary>
        /// 根据URL获取网站域名
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetWebDomainName(string url)
        {
            Uri u = new Uri(url);
            return u.DnsSafeHost;
            //string p = @"(?<=(http|ftp|https)://)[\w\.]+[^/]";
            //Regex reg = new Regex(p, RegexOptions.IgnoreCase);
            //Match m = reg.Match(url);
            //return m.Groups[0].Value;
        }
        #endregion
        #region 根据URL获取文件名称 只是文件名称
        /// <summary>
        /// 根据网站rul获取文件名称 只是文件名称
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetFileNameByUrl(string url)
        {
            //抓取网址字符串中的文件名称
            int at = 0;
            int start = 0;
            int notei = 0;
            int endi = 0;
            int[] myIntArray = new int[10];
            string fileName = "";
            while ((start < url.Length) && (at > -1))
            {
                at = url.IndexOf('/', start);
                if (at == -1) break;
                myIntArray[notei] = at;
                start = at + 1;
                notei = notei + 1;
                endi = at;
            }

            for (int i = 0; i < notei; i++)
            {
                if (myIntArray[i] > 0)
                {
                    if (myIntArray[i + 1] == 0)
                    {
                        fileName = url.Substring(myIntArray[i] + 1, url.Length - myIntArray[i] - 1);
                    }
                }
            }
            return fileName;
        }
        #endregion
        #region 根据URL获取文件相对路径
        /// <summary>
        /// 根据URL获取文件相对路径
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetRelativeFilePathByUrl(string url)
        {
            Uri u = new Uri(url);
            return u.LocalPath;
        }
        #endregion

        #region 浏览器信息
        /// <summary>
        /// 判断当前访问是否来自浏览器软件
        /// </summary>
        /// <returns>当前访问是否来自浏览器软件</returns>
        public static bool IsBrowserGet()
        {
            string[] BrowserName = { "explorer", "opera", "netscape", "mozilla", "konqueror", "firefox", "chrome" };
            string curBrowser = HttpContext.Current.Request.Browser.Type.ToLower();
            for (int i = 0; i < BrowserName.Length; i++)
            {
                if (curBrowser.IndexOf(BrowserName[i]) >= 0)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 获取浏览器信息
        /// </summary>
        public static string GetBrowserInfo
        {
            get
            {
                if (HttpContext.Current == null)
                    return string.Empty;
                var browser = HttpContext.Current.Request.Browser;
                return string.Format("{0} {1}", browser.Browser, browser.Version);
            }
        }
        #endregion
        #region GetFileControls(获取客户端文件控件集合) 

        /// <summary>
        /// 获取有效客户端文件控件集合,文件控件必须上传了内容，为空将被忽略,
        /// 注意:Form标记必须加入属性 enctype="multipart/form-data",服务器端才能获取客户端file控件. list[0]是第一个有效的
        /// </summary>
        public static List<HttpPostedFile> GetFileControls()
        {
            var result = new List<HttpPostedFile>();
            var files = HttpContext.Current.Request.Files;
            if (files.Count == 0)
                return result;
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file.ContentLength == 0)
                    continue;
                result.Add(files[i]);
            }
            return result;
        }
        #region 保存上传文件
        /// <summary>
        /// 保存用户上传的文件
        /// </summary>
        /// <param name="path">保存路径</param>
        public static void SaveRequestFile(string path)
        {
            if (HttpContext.Current.Request.Files.Count > 0)
            {
                HttpContext.Current.Request.Files[0].SaveAs(path);
            }
        }
        #endregion
        #endregion
        #region 设取Token
        /// <summary>
        /// 获得当前Session里保存的标志 HttpContext.Current.Request.Form.Get("txt_hiddenToken").Equals(GetToken())
        /// </summary>
        /// <returns></returns>
        public static string GetToken()
        {
            HttpContext rq = HttpContext.Current;
            if (null != rq.Session["Token"])
            {
                return rq.Session["Token"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 生成标志，并保存到Session
        /// </summary>
        public static void SetToken()
        {
            HttpContext rq = HttpContext.Current;
            rq.Session.Add("Token", SecurityHelper.MD5String(rq.Session.SessionID + DateTime.Now.Ticks.ToString()));
        }
        public static string GetHeadValue(string key)
        {
            HttpContext rq = HttpContext.Current;
            string headValue = rq.Request.Headers.Get(key);
           return headValue;
        }
        #endregion
        #region Cookie操作

        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        public static void WriteCookie(string strName, string strValue)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName];
            if (cookie == null)
            {
                cookie = new HttpCookie(strName);
            }
            cookie.Value = strValue;
            HttpContext.Current.Response.AppendCookie(cookie);
        }
        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        /// <param name="strValue">过期时间(分钟)</param>
        public static void WriteCookie(string strName, string strValue, int expires)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName];
            if (cookie == null)
            {
                cookie = new HttpCookie(strName);
            }
            cookie.Value = strValue;
            cookie.Expires = DateTime.Now.AddMinutes(expires);
            HttpContext.Current.Response.AppendCookie(cookie);
        }
        /// <summary>
        /// 读cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <returns>cookie值</returns>
        public static string GetCookie(string strName)
        {
            if (HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies[strName] != null)
            {
                return HttpContext.Current.Request.Cookies[strName].Value.ToString();
            }
            return "";
        }
        /// <summary>
        /// 删除Cookie对象
        /// </summary>
        /// <param name="CookiesName">Cookie对象名称</param>
        public static void RemoveCookie(string CookiesName)
        {
            HttpCookie objCookie = new HttpCookie(CookiesName.Trim());
            objCookie.Expires = DateTime.Now.AddYears(-5);
            HttpContext.Current.Response.Cookies.Add(objCookie);
        }
        #endregion
        #region Session操作
        /// <summary>
        /// 写Session
        /// </summary>
        /// <typeparam name="T">Session键值的类型</typeparam>
        /// <param name="key">Session的键名</param>
        /// <param name="value">Session的键值</param>
        public static void WriteSession<T>(string key, T value)
        {
            if (key.IsNullEmpty())
                return;
            HttpContext.Current.Session[key] = value;
        }

        /// <summary>
        /// 写Session
        /// </summary>
        /// <param name="key">Session的键名</param>
        /// <param name="value">Session的键值</param>
        public static void WriteSession(string key, string value)
        {
            WriteSession<string>(key, value);
        }

        /// <summary>
        /// 读取Session的值
        /// </summary>
        /// <param name="key">Session的键名</param>        
        public static string GetSession(string key)
        {
            if (key.IsNullEmpty())
                return string.Empty;
            return HttpContext.Current.Session[key] as string;
        }
        /// <summary>
        /// 删除指定Session
        /// </summary>
        /// <param name="key">Session的键名</param>
        public static void RemoveSession(string key)
        {
            if (key.IsNullEmpty())
                return;
            HttpContext.Current.Session.Contents.Remove(key);
        }

        #endregion
    }
}
