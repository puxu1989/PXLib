using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PXLib.DataAccess
{
    public class DbLog
    {
        /// <summary>
        /// 只读对象用于锁
        /// </summary>
        private static readonly object locks = new object();
        private static StreamWriter streamWriter; //写文件的Writer  
        public static void WriteLog(Exception exception)
        {
            WriteLog(exception, "");
        }
        public static void WriteLog(Exception exception, string otherMsg)
        {
            lock (locks)
            {
                try
                {
                    DateTime dt = DateTime.Now;
                    string directPath = AppDomain.CurrentDomain.BaseDirectory + "\\DbLog";
                    //记录错误日志文件的路径
                    if (!Directory.Exists(directPath))                   
                        Directory.CreateDirectory(directPath);                   
                    directPath += string.Format(@"\{0}.log", dt.ToString("yyyy-MM-dd"));
                    if (streamWriter == null)
                        InitLog(directPath);                    
                    streamWriter.WriteLine("********************************" + dt.ToString("HH:mm:ss") + "*************************************");
                    if (!string.IsNullOrEmpty(otherMsg))
                        streamWriter.WriteLine("错误信息：其他错误" + otherMsg);
                    if (exception != null)
                        streamWriter.WriteLine("异常信息：\r\n" + exception.ToString());
                }
                finally
                {
                    if (streamWriter != null)
                    {
                        streamWriter.Flush();
                        streamWriter.Dispose();
                        streamWriter = null;
                    }
                }
            }
        }
        private static void InitLog(string filPath)
        {
            streamWriter = !File.Exists(filPath) ? File.CreateText(filPath) : File.AppendText(filPath);
        }
    }
}
