using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PXLib.Net
{
    /// <summary>
    /// 此类是处理WebSocket解包和打包的静态方法
    /// </summary>
    public class WebSocketUtils
    {
        private static string WebSocketVersion = "Sec-WebSocket-Version:";
        public static byte[] CheckClientType(byte[] receivedDataBuffer, int offset, int count)
        {
            string rawClientHandshake = Encoding.UTF8.GetString(receivedDataBuffer, offset, count);
            //Console.WriteLine(rawClientHandshake);
            if (rawClientHandshake.IndexOf(WebSocketVersion) != -1)//如果包含有  //现在使用的是比较新的Websocket协议
            {
               
                string[] rawClientHandshakeLines = rawClientHandshake.Split(new string[] { Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
                string acceptKey = "";
                foreach (string Line in rawClientHandshakeLines)
                {
                    if (Line.Contains("Sec-WebSocket-Key:"))
                    {
                        acceptKey = GetSecKeyAccept(Line.Substring(Line.IndexOf(":") + 2));
                    }
                }
                byte[] newHandshakeText = PackHandShakeData(acceptKey);
                rawClientHandshake = null;//用完可以清除
                return newHandshakeText;
            }
            else
                return null;
           
        }
        private static string GetSecKeyAccept(string ClientHandshakeMsg)
        {
            const String MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            string  secWebSocketAccept = String.Empty;    
            string ret = ClientHandshakeMsg + MagicKEY;
            using (SHA1 sha = new SHA1CryptoServiceProvider()) 
            {
                byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));
                secWebSocketAccept = Convert.ToBase64String(sha1Hash);
            }
            return secWebSocketAccept;
        }

        private static byte[] PackHandShakeData(string secKeyAccept)
        {
            StringBuilder responseBuilder = new StringBuilder();
            responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + "\r\n");
            responseBuilder.Append("Upgrade: websocket" + "\r\n");
            responseBuilder.Append("Connection: Upgrade" + "\r\n");
            responseBuilder.Append("Sec-WebSocket-Accept: " + secKeyAccept + "\r\n\r\n");
            return Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }

        /// <summary>
        /// 解析客户端发送来的数据  最大只能解析128k数据
        /// </summary>
        public static byte[] AnalyzeClientData(byte[] recBytes, int length)
        {
            if (length < 2)
            {
                return null;
            }
            bool fin = (recBytes[0] & 0x80) == 0x80; // 1bit，1表示最后一帧  
            if (!fin)
            {
                return null;// 超过一帧暂不处理 
            }
            bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码  
            if (!mask_flag)
            {
                return null;// 不包含掩码的暂不处理
            }

            int payload_len = recBytes[1] & 0x7F; // 数据长度  254&127=126
            byte[] masks = new byte[4];
            byte[] payload_data;

            if (payload_len == 126)
            {
                Array.Copy(recBytes, 4, masks, 0, 4);
                payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 8, payload_data, 0, payload_len);// 通过
            
            }
            else if (payload_len == 127)//最长走这里
            {
                Array.Copy(recBytes, 10, masks, 0, 4);
                byte[] uInt64Bytes = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    uInt64Bytes[i] = recBytes[9 - i];
                }
                UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

                payload_data = new byte[len];
                //for (UInt64 i = 0; i < len; i++)
                //{
                //    payload_data[i] = recBytes[i + 14];
                //}
                Array.Copy(recBytes, 14, payload_data, 0, recBytes.Length - 14);
            }
            else//短数据走这里
            {
                Array.Copy(recBytes, 2, masks, 0, 4);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 6, payload_data, 0, payload_len);//通过

            }

            for (var i = 0; i < payload_data.Length; i++)
            {
                payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
            }
            return payload_data;
        }

        /// <summary>
        /// WebSocket发送的数据
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] PackServerData(string msg)
        {
            byte[] content = null;
            byte[] temp = Encoding.UTF8.GetBytes(msg);

            if (temp.Length < 126)
            {
                content = new byte[temp.Length + 2];
                content[0] = 0x81;
                content[1] = (byte)temp.Length;
                Array.Copy(temp, 0, content, 2, temp.Length);
            }
            else if (temp.Length < 0xFFFF)
            {
                content = new byte[temp.Length + 4];
                content[0] = 0x81;
                content[1] = 126;
                content[2] = (byte)(temp.Length & 0xFF);
                content[3] = (byte)(temp.Length >> 8 & 0xFF);
                Array.Copy(temp, 0, content, 4, temp.Length);
            }
            else
            {
                content = new byte[temp.Length + 10];
                content[0] = 0x81;
                content[1] = 127;
                content[2] = 0;
                content[3] = 0;
                content[4] = 0;
                content[5] = 0;
                content[6] = (byte)(temp.Length >> 24);
                content[7] = (byte)(temp.Length >> 16);
                content[8] = (byte)(temp.Length >> 8);
                content[9] = (byte)(temp.Length & 0xFF);
                Array.Copy(temp, 0, content, 10, temp.Length);
            }
            return content;
        }
        
        /// <summary>
        /// 把发送给客户端消息打包处理
        /// </summary>
        public static byte[] PackServerData(byte[] msg)
        {
            byte[] content = null;
           // byte[] temp = Encoding.Convert(Encoding.UTF8, Encoding.Default, msg);
            byte[] temp = msg;
            if (temp.Length < 126)//小于126的数据
            {
                content = new byte[temp.Length + 2];
                content[0] = 130;
                content[1] = (byte)temp.Length;
                Buffer.BlockCopy(temp, 0, content, 2, temp.Length);//通过
            }
            else if (temp.Length < 0xFFFF)//小于65535的数据
            {
                content = new byte[temp.Length + 4];
                content[0] = 0x80|2;//1.text; 2.binary
                content[1] = 126;
                content[2] = (byte)(temp.Length >> 8);
                content[3] = (byte)(temp.Length & 0xFF);            
                Buffer.BlockCopy(temp, 0, content, 4, temp.Length);//通过
            }
            else
            {
                content = new byte[temp.Length + 10];
                content[0] = 0x80 | 2;
                content[1] = 127;
                content[2] = 0;
                content[3] = 0;
                content[4] = 0;
                content[5] = 0;
                content[6] = (byte)(temp.Length >> 24);
                content[7] = (byte)(temp.Length >> 16);
                content[8] = (byte)(temp.Length >> 8);
                content[9] = (byte)(temp.Length & 0xFF);
                Buffer.BlockCopy(temp, 0, content, 10, temp.Length);

            }
            return content;
        }
    }
}
