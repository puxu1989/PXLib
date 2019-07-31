using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PXLib.Net.WebSocketHandler
{
    /// <summary>
    /// 用于webscoket握手
    /// </summary>
    public class WebSocketHandshake
    {
        //用于保存请求串的键值对
        private Hashtable KeyValues;  
        public bool IsWebSocket;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="request">请求串</param>
        public WebSocketHandshake(byte[] content)
        {
            string request = Encoding.UTF8.GetString(content);
            //Console.WriteLine(request);
            //初始化哈希表
            KeyValues = new Hashtable();

            //分割字符串，用于分割每一行
            string[] separator1 = { "\r\n" };
            string[] rows = request.Split(separator1, StringSplitOptions.RemoveEmptyEntries);
            foreach (string row in rows)
            {
                //':'在每一行的第一个匹配项索引
                int splitIndex = row.IndexOf(':');
                if (splitIndex > 0)
                {//是键值对，保存到哈希表
                    string key1 = row.Substring(0, splitIndex).Trim(); //键                    
                    if (key1.ToLower().IndexOf("origin") > 0)//因为有时返回的是Sec-WebSocket-Origin，有时返回的是Origin，所以做这样的处理
                        key1 = "Origin";
                    string value1 = row.Substring(splitIndex + 1).Trim();  //值
                    KeyValues.Add(key1, value1);  //保存到哈希表KeyValues
                }
            }
            if (this.GetValue("Upgrade").ToLower() == "websocket") 
            {
                this.IsWebSocket = true;
            }
        }

        /// <summary>
        /// 返回的验证码
        /// </summary>
        private string KeyAccept
        {
            get
            {
                string secWebSocketKey = GetValue("Sec-WebSocket-Key");
                string m_Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                return Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(secWebSocketKey + m_Magic)));
            }
        }

        /// <summary>
        /// 根据键获取对应值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            //在哈希表中查询是否存在对应的键值
            if (KeyValues.ContainsKey(key))
                return KeyValues[key].ToString();
            else
                return string.Empty; //没有匹配的键值
        }

        /// <summary>
        /// 响应串
        /// </summary>
        public string Response
        {
            get
            {
                StringBuilder response = new StringBuilder(); //响应串
                response.Append("HTTP/1.1 101 Web Socket Protocol Handshake\r\n");

                //将请求串的键值转换为对应的响应串的键值并添加到响应串
                response.AppendFormat("Upgrade: {0}\r\n", GetValue("Upgrade"));
                response.AppendFormat("Connection: {0}\r\n", GetValue("Connection"));
                response.AppendFormat("Sec-WebSocket-Accept: {0}\r\n", KeyAccept);
                response.AppendFormat("WebSocket-Origin: {0}\r\n", GetValue("Origin"));
                response.AppendFormat("WebSocket-Location: {0}\r\n", GetValue("Host"));

                response.Append("\r\n");

                return response.ToString();
            }
        }
    }
}
