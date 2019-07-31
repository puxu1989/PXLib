using Newtonsoft.Json.Linq;
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
    /// <summary>
    /// 企业宝短信
    /// </summary>
    public class SMS_QYB
    {
        private static string postUrl = ConfigHelper.GetConfigAppSettingsValue("QYBSMSPostUrl");//提交地址
        private static string username = ConfigHelper.GetConfigAppSettingsValue("QYBSMSAccount");//配置短信帐号
        private static string passwd = ConfigHelper.GetConfigAppSettingsValue("QYBSMSPwd");//配置短信密码
        private static string smssign = "【" + ConfigHelper.GetConfigAppSettingsValue("QYBSMSSign") + "】";//配置签名

        /// <summary>
        /// 发送短消息
        /// </summary>
        /// <param name="phones">群发可以提交不超过50000个手机号码，每个号码用英文逗号间隔。</param>
        /// <param name="content">为短信内容，须加上签名，例如"【微软科技】"，短信内容为utf-8编码，有特殊符号请urlencode内容</param>
        /// <param name="sendOneTime">相同手机多久可以发送一次短信</param>
        /// <param name="needstatus">是否需要推送短信 值为true或false;</param>
        /// <param name="port">又说是端口(又说是扩展码)，默认为空</param>
        /// <param name="sendtime">发送时间（定时）为空则立即发送</param>
        /// <returns></returns>
        public static String SendMsg(string phones, string content, int sendOneTime, String needstatus, String port, String sendtime)
        {
            if (string.IsNullOrEmpty(phones))
            {
                return "手机号不能为空";
            }
            if (!SMSCommons.IsEnabledSend(phones, sendOneTime))
            {
                return "操作频繁，" + sendOneTime + "分钟内只可以发一次";
            }

            if (string.IsNullOrEmpty(content))
            {
                return "短信内容不能为空";
            }
            string postDate = string.Format("username={0}&passwd={1}&phone={2}&msg={3}{4}&needstatus={5}&port={6}&sendtime={7}", username, passwd, phones, smssign, content, needstatus, port, sendtime);
            try
            {
                return SMSCommons.PushToWeb(postUrl, postDate, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 发送短信 直接返回发送描述信息
        /// </summary>
        /// <param name="phone">电话</param>
        /// <param name="msg">内容</param>
        /// <param name="sendOneTime">相同手机多久可以发送一条短信</param>
        /// <returns></returns>
        public static String SendMsg(String phone, String msg, int sendOneTime)
        {
            return SendMsg(phone, msg, sendOneTime, "true", "", "");

        }
        /// <summary>
        /// 解析发送结果
        /// </summary>
        /// <param name="resultJson"></param>
        /// <returns></returns>
        public static string GetSendRespDesc(string resultJson)
        {
            if (!BoolHelper.IsJson(resultJson))
                return resultJson;
            JObject jobj = resultJson.ToJObject();
            return jobj["respdesc"].ToString();
        }
        /// <summary>
        /// 判断是否发送成功
        /// </summary>
        /// <param name="resultJson"></param>
        /// <returns></returns>
        public static bool IsSendSuccess(string resultJson)
        {
            if (!BoolHelper.IsJson(resultJson))
                return false;
            JObject jobj = resultJson.ToJObject();
            return jobj["respcode"].ToString() == "0" ? true : false;
        }
    }
}
