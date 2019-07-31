using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PXLib.Helpers
{
    public class XmlHelper
    {
        //XML解析流程
        //1.创建XmlDocument xmlDoc对象 XDocument.Parse 或者xmlDoc.LoadXml(xml)等
        //2.取节点XmlNode xNode = xdoc.SelectSingleNode("speed/is_success");//取is_success节点的值
        //  string is_success = xNode.InnerText;
        public static string CreateTextMsg(XmlDocument xmlDoc, string content)
        {
            string strTpl = string.Format(@"<xml>
                <ToUserName><![CDATA[{0}]]></ToUserName>
                <FromUserName><![CDATA[{1}]]></FromUserName>
                <CreateTime>{2}</CreateTime>
                <MsgType><![CDATA[text]]></MsgType>
                <Content><![CDATA[{3}]]></Content>
                </xml>", GetFromXML(xmlDoc, "FromUserName"), GetFromXML(xmlDoc, "ToUserName"),
                       GetTimeStamp(), content);

            return strTpl;
        }


        private static int GetTimeStamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(DateTime.Now - startTime).TotalSeconds;
        }

        public static string GetFromXML(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode("xml/" + name);
            if (node != null && node.ChildNodes.Count > 0)
            {
                return node.ChildNodes[0].Value;
            }
            return "";
        }
        /// <summary>
        /// 反序列化XML为类实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlObj"></param>
        /// <returns></returns>
        public static T DeserializeXML<T>(string xmlObj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xmlObj))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// 序列化类实例为XML
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeXML<T>(T obj)
        {
            using (StringWriter writer = new StringWriter())
            {
                new XmlSerializer(obj.GetType()).Serialize((TextWriter)writer, obj);
                return writer.ToString();
            }
        }
    }
}