using Newtonsoft.Json.Linq;
using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using PXLib;
using PXLib.Caches;

namespace PXLib.Api.Push
{
    /// <summary>
    ///  GeTuiApi 需要依赖于Newtonsoft.Json.Linq;
    ///  private static string appId = ConfigHelper.GetConfigAppSettingsValue("GeTuiAppId");
    ///  private static string appKey = ConfigHelper.GetConfigAppSettingsValue("GeTuiAppKey");
    ///  private static string masterSecret = ConfigHelper.GetConfigAppSettingsValue("GeTuiMasterSecret");
    /// </summary>
    /// <summary>
    /// GeTuiApi V1版 需要依赖于Newtonsoft.Json.Linq;文档地址http://docs.getui.com/server/rest/push/
    /// </summary>
    public class GeTuiRestApi
    {

        private string appId;
        public string AppId { get { return appId; } set { appId = value; } }
        private string appKey;
        private string masterSecret;
        private int cacheTokenTimeHour;
        public GeTuiRestApi()
        {

        }
        public GeTuiRestApi(string appId, string appKey, string masterSecret, int cacheTokenTimeHour)
        {
            this.appId = appId;
            this.appKey = appKey;
            this.masterSecret = masterSecret;
            this.cacheTokenTimeHour = cacheTokenTimeHour;
        }
        private static GeTuiRestApi Singleton;
        /// <summary>
        /// 单例模式  cacheTokenTimeHourToken为过期时间最大24小时
        /// </summary>
        public static GeTuiRestApi GetInstance(string appId, string appKey, string masterSecret, int cacheTokenTimeHour)
        {
            if (Singleton == null)
            {
                Singleton = new GeTuiRestApi(appId, appKey, masterSecret, cacheTokenTimeHour);
            }
            return Singleton;
        }
        /// <summary>
        /// 获取授权 带缓存 文档是1天 这里设置23小时
        /// </summary>
        /// <returns></returns>
        public string GetAuthToken()
        {
            string cacheKey = "GetTuiAuthToken";
            string authToken = CacheFactory.Cache().GetCache<string>(cacheKey);
            if (!authToken.IsNullEmpty())
            {
                return authToken;
            }
            string timestamp = TimeHelper.GetTimeStamp(false).ToString();
            JObject job = new JObject();
            job.Add("appkey", this.appKey);
            job.Add("timestamp", timestamp);
            job.Add("sign", SecurityHelper.SHA256String(this.appKey + timestamp + this.masterSecret));//(appkey+timestamp+mastersecret)
            string url = string.Format("https://restapi.getui.com/v1/{0}/auth_sign", this.appId);
            string res = PostWebRequest(url, job.ToJson());
            if (BoolHelper.IsJson(res))
            {
                job = JObject.Parse(res);/*Jobject里的每一个字段都可以用JToken来获取*/
                if (job["result"].ToString() == "ok")
                {
                    res = job["auth_token"].ToString();                   
                    CacheFactory.Cache().WriteCache(cacheKey, res, DateTime.Now.AddHours(this.cacheTokenTimeHour));//缓存23小时         
                }
            }
            return res;
        }
        /// <summary>
        /// 关闭授权
        /// </summary>
        /// <returns></returns>
        public string AuthClose()
        {
            string url = string.Format("https://restapi.getui.com/v1/{0}/auth_close", this.appId);
            return RequestHelper.GetWebRequest(url);
        }
        /// <summary>
        /// 单个人推送
        /// </summary>
        public  string PushToSingle(string cid, string title, string content, string device)
        {
            string url = string.Format("https://restapi.getui.com/v1/{0}/push_single", this.appId);
            JObject jObject;
            if (device != null && device.ToLower() == "ios")
            {
                jObject = this.SetPostTransmissionTemp(title, content);
            }
            else
            {
                jObject = this.SetPostNotificationTemp(title, content);
            }
            jObject.Add("cid", cid);
            jObject.Add("requestid", this.CreateRequestId());

            string res = PostWebRequest(url, jObject.ToJson(), GetAuthToken());
            return res;
        }
        private JObject SetPostNotificationTemp(string title, string content)
        {
            /*JToken 不能示例化，若要生成新的Json，使用Jobject*/
            JObject postJson = new JObject();
            JObject message = new JObject();
            message.Add("appkey", this.appKey);
            message.Add("is_offline", true);
            message.Add("offline_expire_time", 10000000);
            message.Add("msgtype", "notification");
            JObject style = new JObject(new JProperty("type", 0), new JProperty("text", content), new JProperty("title", title));
            JObject notification = new JObject();
            notification.Add("style", style);
            notification.Add("transmission_type", false);
            notification.Add("transmission_content", content);
            postJson.Add("message", message);
            postJson.Add("notification", notification);
            return postJson;
        }
        /// <summary>
        /// 透传消息模板
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private JObject SetPostTransmissionTemp(string title, string content)
        {
            //{
            //        "message":{
            //            "appkey":"pMEgGQ9bgz5LVAPX8q8WH4",
            //            "is_offline":false,
            //            "msgtype":"transmission"
            //        },
            //        "transmission":{
            //            "transmission_type":false,
            //            "transmission_content":"this is the transmission_content",
            //            "duration_begin":"2017-03-22 11:40:00",
            //            "duration_end":"2017-03-29 11:40:00"
            //        },
            //        "push_info": {
            //                "aps": {
            //                    "alert": {
            //                        "title": "xxxx",
            //                        "body": "xxxxx"
            //                    },
            //                    "autoBadge": "+1",
            //                    "content-available": 1
            //                }

            //            },
            //        "cid":"1fa0795a57c863ecc9a9ea6437b8924f",
            //        "requestid":"123456789"
            //     }'
            /*JToken 不能示例化，若要生成新的Json，使用Jobject*/
            JObject postJson = new JObject();
            JObject message = new JObject();
            message.Add("appkey", this.appKey);
            message.Add("is_offline", true);
            message.Add("msgtype", "transmission");
            JObject transmission = new JObject();
            transmission.Add("transmission_type", true);
            
            //transmission.Add("transmission_content", content);
            //安卓的Hbuder要写下面的格式
            JObject todo = new JObject(new JProperty("todo", "abc"));
            JObject transmission_content = new JObject();
            transmission_content.Add("title", title);
            transmission_content.Add("content", content);
            transmission_content.Add("payload",todo.ToString());
            transmission.Add("transmission_content",transmission_content.ToString());
            
            transmission.Add("duration_begin", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            transmission.Add("duration_end", DateTime.Now.AddDays(3).ToString("yyyy-MM-dd HH:mm:ss"));
            JObject alert = new JObject(new JProperty("title", title), new JProperty("body", content));
            JObject aps = new JObject();
            aps.Add("alert", alert);
            aps.Add("autoBadge", "+1");
            aps.Add("content-available", 1);
            aps.Add("sound", "default");
            postJson.Add("message", message);
            postJson.Add("transmission", transmission);
            postJson.Add("push_info", new JObject(new JProperty("aps", aps)));
            return postJson;

        }
        /// <summary>
        /// 需首先执行save_list_body接口，将推送消息保存在服务器上，后面可以重复调用tolist接口将保存的消息发送给不同的目标用户。
        /// </summary>
        public bool SavaListBody(string title, string content, ref string taskId)
        {
            string url = string.Format("https://restapi.getui.com/v1/{0}/save_list_body", this.appId);
            //JObject postJson = this.SetPostNotificationTemp(title, content);
            JObject postJson = this.SetPostTransmissionTemp(title, content);
           
       string res = PostWebRequest(url, postJson.ToJson(),GetAuthToken());
            if (BoolHelper.IsJson(res) && (JObject.Parse(res)["result"].ToString() == "ok"))
            {
                taskId = JObject.Parse(res)["taskid"].ToString();
                return true;
            }
            else
                return false;

        }
        /// <summary>
        /// 组推
        /// </summary>
        /// <returns></returns>
        public string PushToList(string title,string content,string[] cids)
        {
            string taskId=string.Empty;
            this.SavaListBody(title, content, ref taskId);
            string url = string.Format("https://restapi.getui.com/v1/{0}/push_list", this.appId);
            JObject postJson = new JObject();
            JArray aad = new JArray(cids);
            postJson.Add("cid", aad);
            postJson.Add("taskid", taskId);
            postJson.Add("need_detail", true);
            string res = PostWebRequest(url, postJson.ToJson(), GetAuthToken());
            return res;
        }
        /// <summary>
        /// 推送给所有用户 可以加推送条件 这里未处理 成功返回{"result":"ok", "taskid":"RASA_0111_4fbcdb8155eb8e9002bec597f78d9999"}
        /// </summary>
        public string PustToApp(string title, string content)
        {

            /*  '{
                 "message": {
                    "appkey": "GVvUv4M8FZAF7u5a9H79m6",
                    "is_offline": false,
                    "msgtype": "notification"
                 },
                 "notification": {
                     "style": {
                         "type": 0,
                         "text": "text",
                         "title": "tttt"
                     },
                     "transmission_type": true,
                     "transmission_content": "透传内容"
                 },
               "condition":[{"key":"phonetype", "values":["ANDROID"], "opt_type":0},
                                          {"key":"region", "values":["11000000", "12000000"], "opt_type":0},
                                          {"key":"tag", "values":["usertag"], "opt_type":0}],
                 "requestid":"12341111111115678978"
                 }'
              */
           string url = string.Format("https://restapi.getui.com/v1/{0}/push_app", this.appId);
           JObject postJson = this.SetPostTransmissionTemp(title, content);
            postJson.Add("requestid", CreateRequestId());//必须要10-20长度
            //string res = PostWebRequest(url, JsonHelper.SerializeObject<JObject>(postJson), this.GetAuthToken());
            string res = PostWebRequest(url, postJson.ToJson(),GetAuthToken());
            return res;
        }
        public string CreateRequestId()
        {
            Random random = new Random();
            string strRandom = random.Next(1000, 10000).ToString(); //生成编号 
            string code = DateTime.Now.ToString("yyyyMMddHHmmss") + strRandom;//形如
            return code;
        }
        private string PostWebRequest(string url, string postData, string headAuthToken = "")
        {
            byte[] bytes = Encoding.UTF8.GetBytes((postData == null) ? "" : postData);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = (long)bytes.Length;
            if (!string.IsNullOrEmpty(headAuthToken))
            {
                httpWebRequest.Headers.Add("authtoken", headAuthToken);
            }
            string result;
            try
            {
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                    result = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

    }
}
