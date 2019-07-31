using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PXLib.Helpers
{
    /// <summary>
    /// 日志帮助类 别人都用Log4NET  2016-9-22简化一个
    /// </summary>
    public class LogHelper : IDisposable
    {
        #region 私有字段
        //是否启用写入日志和日志写入目录
        private bool IsWriteLog = ConfigHelper.GetConfigBool("IsWriteLog");//是否启用日志;
        //AppDomain.CurrentDomain.SetupInformation.ApplicationBase=AppDomain.CurrentDomain.BaseDirectory
        private static string logRootFilePath = string.Format(AppDomain.CurrentDomain.BaseDirectory+ "\\{0}_Log", ConfigHelper.GetConfigAppSettingsValue("SoftName"));//日志输出跟目录路径 如文件夹D:ZFKJ_E3D_Log
        private static string logFilePath;
        private string logFileName;
        //日志文件写入流对象
        private  StreamWriter sw;
        #endregion
        #region 构造方法
        //创建日志 文件名默认以创建时间命名
        public LogHelper()
        {
            CreateLoggerFile(null);
        }
        public LogHelper(string logFileName)
        {
            CreateLoggerFile(logFileName);
        }
        #endregion

        private void CreateLoggerFile(string logFileName)
        {
            if (!this.IsWriteLog)
                return;
            string today = DateTime.Now.ToString("yyyy-MM-dd");//当前日期字符
            this.logFileName = (string.IsNullOrEmpty(logFileName) ? today : logFileName) + ".log";//默认以当前日期命名日志文件
            string rootPath = logRootFilePath;
            if (!string.IsNullOrEmpty(rootPath))//这里会出现根目录为null的情况
            {
                logFilePath = rootPath + "\\" + today;
                if (!Directory.Exists(logFilePath))//如果不存在
                {
                    Directory.CreateDirectory(logFilePath);
                }
            }

        }
        /// <summary>
        /// 写入日志内容
        /// </summary>
        /// <param name="msg">日志消息</param>
        public void WriteLog(string msg,string otherMsg="")
        {
            if (this.IsWriteLog)
            {
                OpenStreamWrite();
                sw.WriteLine("**********************************" + DateTime.Now.ToString("HH:mm:ss") + "**********************************");
                if (!string.IsNullOrEmpty(otherMsg))
                {
                    sw.WriteLine("OtherMsg："+otherMsg);
                }
                if (string.IsNullOrEmpty(msg))
                    sw.WriteLine("WriteLog：msg=null");
                sw.WriteLine(msg);
                sw.Write("\r\n");//空一行出来
                sw.Flush();
                sw.Close();
            }
        }
        /// <summary>
        /// 写入日志内容
        /// </summary>
        /// <param name="ex">各种异常信息</param>
        public void WriteLog(object ex,string otherMsg="")
        {       
            WriteLog("异常信息：\r\n" + ex.ToString(),otherMsg);
        }
        //打开文件准备写入
        private void OpenStreamWrite()
        {
            logFilePath = logRootFilePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd");
            if (!Directory.Exists(logFilePath))//如果不存在
            {
                Directory.CreateDirectory(logFilePath);
            }
            sw = new StreamWriter(logFilePath + "\\" + logFileName, true);
            // 如果文件存在就追加，如果文件不存在就新建一个
        }
        private void Close()
        {
            if (this.sw != null)
            {
                try
                {
                    this.sw.Close();
                    this.sw = null;
                }
                catch
                {
                }
            }
        }
        public void Dispose()//实现IDispose,不实现要报错
        {
            this.Close();
            GC.SuppressFinalize(this);
        }
    }
}
