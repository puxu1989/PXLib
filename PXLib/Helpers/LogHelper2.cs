using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PXLib.Helpers
{
    /// <summary>
    /// 写入一个文件，当文件长度超过设定的最大长度 则创建新的文件
    /// </summary>
    public class LogHelper2 : IDisposable
    {
        private StreamWriter writer;

        private string iniPath;

        private int maxLength = int.MaxValue;

        private bool enabled = true;

        /// <summary>
        /// 当日志文件增加到一定的大小时，将创建一个新的文件记录日志。
        /// </summary>
        public int MaxLength4ChangeFile
        {
            get
            {
                return this.maxLength;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("MaxLength4ChangeFile must greater than 0.");
                }
                this.maxLength = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }
        public LogHelper2(string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = AppDomain.CurrentDomain.BaseDirectory + "log.log";
            }
            if (!File.Exists(filePath))
            {
                FileStream fileStream = File.Create(filePath);
                fileStream.Close();
            }
            this.iniPath = filePath;
            this.writer = new StreamWriter(File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read));
        }

        ~LogHelper2()
        {
            this.Close();
        }

        public void WriteLog(object msg)
        {
            if (this.enabled)
            {
                lock (this.writer)
                {
                    string msg2 = string.Format("====================={0}=======================", DateTime.Now.ToString());
                    msg2 += "\r\n日志信息：" + msg.ToString();
                    this.writer.WriteLine(msg2 + "\r\n");
                    this.writer.Flush();
                    this.CheckAndChangeNewFile();
                }
            }
        }
        private void CheckAndChangeNewFile()
        {
            if (this.writer.BaseStream.Length >= this.maxLength)
            {
                this.writer.Close();
                this.writer = null;
                string fileNameNoPath = Path.GetFileName(iniPath);
                string fileDirectory = Path.GetDirectoryName(iniPath);
                int num = fileNameNoPath.LastIndexOf('.');
                string text = null;
                string str = fileNameNoPath;
                if (num >= 0)
                {
                    text = fileNameNoPath.Substring(num + 1);
                    str = fileNameNoPath.Substring(0, num);
                }
                string path = null;
                for (int i = 1; i < 1000; i++)
                {
                    string text2 = str + "_" + i.ToString("000");
                    if (text != null)
                    {
                        text2 = text2 + "." + text;
                    }
                    path = fileDirectory + "\\" + text2;
                    if (!File.Exists(path))
                    {
                        break;
                    }
                }
                this.writer = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));
            }
            this.Close(); //WEB使用先关闭一次
        }
        private void Close()
        {
            if (this.writer != null)
            {
                try
                {
                    this.writer.Close();
                    this.writer = null;
                }
                catch
                {
                }
            }
        }

        public void Dispose()
        {
            this.Close();
            GC.SuppressFinalize(this);
        }
    }
}
