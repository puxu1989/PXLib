using PXLib.Caches;
using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Api.SMS
{
    public class SMSCommons
    {
        public static int VerificationCodeTimeOutMinutes = 10;//验证有效过期时间
        /// <summary>
        /// 判断在多久时间内可以发送短信
        /// </summary>
        /// <param name="phoneNum"></param>
        /// <param name="sendOneTime"></param>
        /// <returns></returns>
        public static bool IsEnabledSend(string phoneNum, int sendOneTime) //多久时间 分钟内是可以发短信
        {
            string lastSendTimeKey = phoneNum + "_lastSendTime";
            string lastSendTime = CacheFactory.Cache().GetCache<string>(lastSendTimeKey);
            if (lastSendTime.IsNullEmpty())
            {
                CacheFactory.Cache().WriteCache<string>(lastSendTimeKey, TimeHelper.GetTimeStamp().ToString(), DateTime.Now.AddMinutes(sendOneTime));//用来检验1分钟之内只能发一条
                return true;
            }
            long delter = TimeHelper.GetTimeStamp() - Convert.ToInt64(lastSendTime);
            if (delter > 60 * sendOneTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static string CreateVerificationCode(int length)
        {
            int num;
            char code;
            string checkcode = String.Empty;
            Random random = new Random();
            //用i设置验证码的字数
            for (int i = 0; i < length; i++)
            {
                num = random.Next();
                code = (char)('0' + (char)(num % 10)); //只返回数值型校验码
                //num=偶数时，用数字表示；num=奇数时，用字母表示
                //if (num % 2 == 0) { code = (char)('0' + (char)(num % 10)); }  //num%10，是为了让得的数字在0~9
                //else { code = (char)('A' + (char)(num % 26)); }  //num%26，是为了让得到的数字在0~25，满足字母A到Z的要示
                checkcode += code.ToString();
            }
            return checkcode;
        }
        /// <summary>
        /// 获取生成好的验证码并加入缓存 默认10分钟
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static string GetStoreVerifiyCode(string mobile)
        {
            string mobileKey = "VerifiyCode_KEY_" + mobile;
            string code = CacheFactory.Cache().GetCache<string>(mobileKey);
            if (!code.IsNullEmpty())
            {
                CacheFactory.Cache().RemoveCache(mobileKey);//每次生成新的
            }
            code = CreateVerificationCode(6);
            CacheFactory.Cache().WriteCache(mobileKey, code, DateTime.Now.AddMinutes(SMSCommons.VerificationCodeTimeOutMinutes));
            return code;
        }
        public static bool CheckVerifiyCode(string mobile, string verifyCode)
        {
            string mobileKey = "VerifiyCode_KEY_" + mobile;
            try
            {
                string code = CacheFactory.Cache().GetCache<string>(mobileKey);
                return code == verifyCode ? true : false;
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 提交到服务器
        /// </summary>
        /// <param name="weburl"></param>
        /// <param name="data"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string PushToWeb(string weburl, string postData, Encoding encode)
        {
            byte[] byteArray = encode.GetBytes(postData == null ? "" : postData);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(weburl));
            webRequest.Method = "POST";
            webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;
            webRequest.Timeout = 60 * 1000;
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
    }
}
