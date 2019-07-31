using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace PXLib.Api.SMS
{
    public class SMS_ChuangRui
    {
        //这里配置参数
        private readonly static string SMSServiceUrl = ConfigHelper.GetConfigAppSettingsValue("ChuangRuiSMSPostUrl");//"http://web.cr6868.com/asmx/smsservice.aspx"; //短信提交地址
        private readonly static string Account = ConfigHelper.GetConfigAppSettingsValue("ChuangRuiSMSAccount");//商户账号
        private readonly static string PWD = ConfigHelper.GetConfigAppSettingsValue("ChuangRuiSMSPwd");//登陆平台，管理中心--基本资料--接口密码（28位密文）；复制使用即可。
        private readonly static string Sign = ConfigHelper.GetConfigAppSettingsValue("ChuangRuiSMSSign");//商户的短信签名


        #region 短信发送接口
        /// <summary>
        /// 发送短信接口  
        /// </summary>
        /// <param name="mobile">手机号码 可以群发 用,分割</param>
        /// <param name="content">内容</param>
        /// <returns></returns>
        public static string SendSMS(string mobile, string content)
        {
            return SendSMS(mobile, content, 0);
        }

        /// <summary>
        /// 发送短信接口
        /// </summary>
        /// <param name="mobile">手机号码 可以群发 用,分割</param>
        /// <param name="content">内容</param>
        /// <param name="sendOneTime">一分钟后可以发送</param>
        /// <param name="smsSign">s短信签名</param>
        /// <returns></returns>

        public static string SendSMS(string mobile, string content, int sendOneTime, string smsSign = "")
        {

            if (string.IsNullOrEmpty(mobile))
            {
                return "手机号不能为空";
            }
            if (sendOneTime > 0 && !SMSCommons.IsEnabledSend(mobile, sendOneTime))
            {
                return "操作频繁，" + sendOneTime + "分钟内只可以发一次";
            }
            if (string.IsNullOrEmpty(content))
            {
                return "短信内容不能为空";
            }
            StringBuilder sms = new StringBuilder();
            sms.AppendFormat("name={0}", Account);//账号
            sms.AppendFormat("&pwd={0}", PWD);//密码
            sms.AppendFormat("&content={0}", content);
            sms.AppendFormat("&mobile={0}", mobile);
            sms.AppendFormat("&sign={0}", smsSign.IsNullEmpty() ? Sign : smsSign);// 公司的简称或产品的简称都可以
            sms.Append("&type=pt");
            try
            {
                string resp = SMSCommons.PushToWeb(SMSServiceUrl, sms.ToString(), Encoding.UTF8);
                string[] msg = resp.Split(',');
                if (msg[0] == "0")
                {
                    //errMsg = "提交成功：SendID=" + msg[1];
                    return "OK";
                }
                else
                {
                    return "发送失败：错误信息=" + msg[1];
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion

    }
}
