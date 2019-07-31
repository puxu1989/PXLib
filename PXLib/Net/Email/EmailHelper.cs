using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using PXLib;

namespace PXLib.Net.Email
{
    /// <summary>
    /// 邮件帮助类  最简单的解释 SMTP是发送邮件的协议  POP3是用来接收邮件的协议
    /// </summary>
    public class EmailHelper
    {
        #region 使用配置文件账户信息发送电子邮件
        
        /// <summary>
        /// 邮件服务器地址
        /// </summary>
        private static string MailServer =ConfigHelper.GetCacheConfigString("MailHost");
        /// <summary>
        /// 用户名
        /// </summary>
        private static string MailUserName = ConfigHelper.GetCacheConfigString("MailUserName");
        /// <summary>
        /// 密码
        /// </summary>
        private static string MailPassword = ConfigHelper.GetCacheConfigString("MailPassword");
        /// <summary>
        /// 名称
        /// </summary>
        private static string MailName = ConfigHelper.GetCacheConfigString("MailName");
        /// <summary>
        /// 同步发送邮件
        /// </summary>
        /// <param name="to">收件人邮箱地址</param>
        /// <param name="subject">主题</param>
        /// <param name="body">内容</param>
        /// <param name="encoding">编码</param>
        /// <param name="isBodyHtml">是否Html</param>
        /// <param name="enableSsl">是否SSL加密连接</param>
        /// <returns>是否成功</returns>
        public static bool Send(string to, string subject, string body, string encoding = "UTF-8", bool isBodyHtml = true, bool enableSsl = false)
        {
            try
            {
                MailMessage message = new MailMessage();
                // 接收人邮箱地址
                message.To.Add(new MailAddress(to));
                message.From = new MailAddress(MailUserName, MailName);
                message.BodyEncoding = Encoding.GetEncoding(encoding);
                message.Body = body;
                //GB2312
                message.SubjectEncoding = Encoding.GetEncoding(encoding);
                message.Subject = subject;
                message.IsBodyHtml = isBodyHtml;

                SmtpClient smtpclient = new SmtpClient(MailServer, 25);
                smtpclient.Credentials = new System.Net.NetworkCredential(MailUserName, MailPassword);
                //SSL连接
                smtpclient.EnableSsl = enableSsl;
                smtpclient.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 异步发送邮件 独立线程
        /// </summary>
        /// <param name="to">邮件接收人</param>
        /// <param name="title">邮件标题</param>
        /// <param name="body">邮件内容</param>
        /// <param name="port">端口号</param>
        /// <returns></returns>
        public static void SendByThread(string to, string title, string body, int port = 25)
        {
            new Thread(new ThreadStart(delegate()
            {
                try
                {
                    SmtpClient smtp = new SmtpClient();
                    //邮箱的smtp地址
                    smtp.Host = MailServer;
                    //端口号
                    smtp.Port = port;
                    //构建发件人的身份凭据类
                    smtp.Credentials = new NetworkCredential(MailUserName, MailPassword);
                    //构建消息类
                    MailMessage objMailMessage = new MailMessage();
                    //设置优先级
                    objMailMessage.Priority = MailPriority.High;
                    //消息发送人
                    objMailMessage.From = new MailAddress(MailUserName, "提醒", System.Text.Encoding.UTF8);
                    //收件人
                    objMailMessage.To.Add(to);
                    //标题
                    objMailMessage.Subject = title.Trim();
                    //标题字符编码
                    objMailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                    //正文
                    objMailMessage.Body = body.Trim();
                    objMailMessage.IsBodyHtml = true;
                    //内容字符编码
                    objMailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                    //发送
                    smtp.Send(objMailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            })).Start();
        }
        #endregion

        /// <summary>
        ///  群发电子邮件
        /// </summary>
        /// <param name="senderUserMailAddress">发件人</param>
        /// <param name="mailAddressList">群发邮件的列表</param>
        /// <param name="mailSubject">邮件主题或者标题</param>
        /// <param name="mailBodyOrHtml">内容 可以是HTML格式</param>
        /// <param name="password">如qq邮箱请填写申请的16位授权码 我的是yscyfztezbxbbehh 更改QQ密码以及独立密码会触发授权码过期，需要重新获取新的授权码登录</param>
        /// <param name="smtpserver">SMTP服务器地址{ "smtp.163.com", "smtp.sohu.com", "smtp.sina.com.cn", "smtp.mail.yahoo.com", "smtp.qq.com", "smtp.126.com" };</param>
        /// <returns></returns>
        public static bool SendEmail(string senderUserMailAddress, List<string> mailAddressList, string mailSubject, string mailBodyOrHtml, string password, string smtpserver, string senderNickName, bool isBodyHtml = true, bool enableSsl = false)
        {
            try
            {
                Regex macth = new Regex("^[\\w-]+@[\\w-]+\\.(com|net|org|edu|mil|tv|biz|info)$");
                MailMessage msg = new MailMessage();//邮件消息配置
                msg.From = new MailAddress(senderUserMailAddress.Trim(), senderNickName);
                msg.Subject = mailSubject;
                msg.IsBodyHtml = isBodyHtml;//邮件内容是否为html格式
                msg.Body = mailBodyOrHtml;
                //邮件的优先级,有三个值:高(在邮件主题前有一个红色感叹号,表示紧急),低(在邮件主题前有一个蓝色向下箭头,表示缓慢),正常(无显示).
                msg.Priority = MailPriority.High;
                foreach (string recvMailAddress in mailAddressList)
                {
                    if (macth.Match(recvMailAddress.Trim()).Success)
                        msg.To.Add(recvMailAddress.Trim());
                }
                SmtpClient client = new SmtpClient(smtpserver, 25);//创建客服端发送邮件 //邮件服务器的主机地址 //默认是25端口号
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(senderUserMailAddress, password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(msg);
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 获取邮件列表 开启pop服务的qq邮箱可设置接收多少天前的邮件  邮件很多若一次性接收速度非常慢
        /// </summary>
        /// <param name="hostname">pop邮件服务器地址</param>
        /// <param name="port"> 默认端口号是110 若使用ssl 端口号是995</param>
        /// <param name="useSsl">是否使用</param>
        /// <param name="username">用户名或者用户邮箱地址</param>
        /// <param name="password">密码/授权码</param>
        /// <returns></returns>
        public static List<PopMessage> GetAllMessages(string hostname, int port, bool useSsl, string username, string password)
        {
            using (EmailClient client = new EmailClient())
            {
                client.Connect(hostname, port, useSsl);
                client.Authenticate(username, password);
                return client.GetAllMessages();
            }
        }
        public static List<PopMessage> GetAllMessages(string userName, string password) 
        {
            string[] popServerArray = { "pop.163.com", "pop.sohu.com", "pop.sina.com.cn", "pop.mail.yahoo.com", "pop.qq.com", "pop.126.com" };
            string popserver=string.Empty;
            foreach (string pop in popServerArray)
            {
                if(pop.IndexOf(userName.Split('@')[1], StringComparison.InvariantCultureIgnoreCase)>0)
                {
                    popserver = pop;
                }
            }
            return GetAllMessages(popserver, 995, true, userName, password);
        }
    }
}
