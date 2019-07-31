using Newtonsoft.Json.Linq;
using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Api.SMS
{
    /// <summary>
    /// 创瑞短信平台API 创建于2019-1-2
    /// </summary>
    public class SMS_ChuangRui_V2
    {
       
        private string accessKey;
        private string secret;
        private string sign;
        /// <summary>
        /// 创瑞短信平台API
        /// </summary>
        /// <param name="accessKey">accesskey</param>
        /// <param name="secret">secret</param>
        /// <param name="sign">sign</param>
        public SMS_ChuangRui_V2(string accessKey,string secret,string sign)
        {
            ValidationHelper.ArgumentNotNull(accessKey, "创瑞accessKey");
            ValidationHelper.ArgumentNotNull(secret, "创瑞secret");
            ValidationHelper.ArgumentNotNull(sign, "创瑞sign");
            this.accessKey = accessKey;
            this.secret = secret;
            this.sign = sign;
        }
        /// <summary>
        /// 短信群发 
        /// </summary>
        /// <param name="mobile">多个用，分割</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public  string SendSMSToAll(string mobile,string content)
        {
            NameValueCollection data = new NameValueCollection();
            data.Add("accesskey", this.accessKey);
            data.Add("secret", this.secret);
            data.Add("sign", this.sign);
            data.Add("mobile", mobile);
            data.Add("content", content);
            return PushToWeb("http://api.1cloudsp.com/api/v2/send", data);     
        }
        /// <summary>
        /// 短信余额查询
        /// </summary>
        /// <returns></returns>
        public  string QueryAccount()
        {
            NameValueCollection data = new NameValueCollection();
            data.Add("accesskey", this.accessKey);
            data.Add("secret", this.secret);
            return PushToWeb("http://api.1cloudsp.com/query/account", data);
        }
        private  string PushToWeb(string url,NameValueCollection data)
        {
            using (WebClient httpClient = new WebClient())
            {
                httpClient.Encoding = Encoding.UTF8;
                return Encoding.UTF8.GetString(httpClient.UploadValues(url, "POST", data));
            }        
        }
    }
}
