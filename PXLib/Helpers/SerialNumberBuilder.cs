using PXLib.Threading.Application;
using PXLib.Threading.Locker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PXLib.Helpers
{
    /// <summary>
    /// 流水号生成器
    /// </summary>
    public class SerialNumberBuilder
    {
        /// <summary>
        /// 当前的生成序列号
        /// </summary>
        private long CurrentIndex = 0;
        /// <summary>
        /// 流水号的文本头
        /// </summary>
        private string TextHead = string.Empty;
        /// <summary>
        /// 时间格式默认年月日
        /// </summary>
        private string TimeFormate = "yyyyMMdd";
        /// <summary>
        /// 流水号数字应该显示的长度
        /// </summary>
        private int NumberLength = 5;
        /// <summary>
        ///文件保存路径
        /// </summary>
        private string FileSavePath;
        /// <summary>
        /// 简单同步锁对象
        /// </summary>
        private SimpleHybirdLock HybirdLock = new SimpleHybirdLock();
        /// <summary>
        /// 高性能存储块
        /// </summary>
        private AsyncCoordinator _AsyncCoordinator { get; set; }
        #region 初始化处理 构造
        public SerialNumberBuilder()
        {
            this.FileSavePath = AppDomain.CurrentDomain.BaseDirectory + @"\\ZSerialNumberLog.txt";
            string directPath = Path.GetDirectoryName(FileSavePath);
            if (!Directory.Exists(directPath))
                Directory.CreateDirectory(directPath);
            LoadByFileAndInitAsyncCoordinator();
        }
        /// <summary>
        /// 实例化一个流水号生成的对象
        /// </summary>
        /// <param name="textHead">流水号的头文本</param>
        /// <param name="timeFormate">流水号带的时间信息</param>
        /// <param name="numberLength">流水号数字的标准长度，不够补0</param>
        /// <param name="fileSavePath">流水号存储的文本位置，不能放在bin目录导致session过期</param>
        public SerialNumberBuilder(string textHead, string timeFormate, int numberLength, string fileSavePath = "")
        {

            TextHead = textHead;
            TimeFormate = timeFormate;
            NumberLength = numberLength;
            if (fileSavePath.IsNullEmpty())
            {
                this.FileSavePath = AppDomain.CurrentDomain.BaseDirectory + @"\\ZSerialNumberLog.txt";
                string directPath = Path.GetDirectoryName(FileSavePath);
                if (!Directory.Exists(directPath))
                    Directory.CreateDirectory(directPath);
            }
            LoadByFileAndInitAsyncCoordinator();
        }
        private void LoadByFileAndInitAsyncCoordinator()
        {
            LoadByFile(m => m);
            _AsyncCoordinator = new AsyncCoordinator(() =>
            {
                if (!string.IsNullOrEmpty(FileSavePath))
                {
                    using (StreamWriter sw = new StreamWriter(FileSavePath, false, Encoding.UTF8))
                    {
                        sw.Write(CurrentIndex);
                    }
                }
            });
        }
        /// <summary>
        /// 使用用户自定义的解密方法从文件读取数据
        /// </summary>
        /// <param name="decrypt">用户自定义的解密方法</param>
        /// Converter<string,string>代理表示字符串转字符串 比如有这个方法：
        //public string ConvertIntToString(int n)
        //{
        //   return n.ToString();
        //}
        //这个方法是把整数转成字符串，所以可以这样写：Converter<int, string> convert = ConvertIntToString;
        //然后就可以这样调用：
        //string str = convert(100);
        public void LoadByFile(Converter<string, string> converter)
        {
            if (!FileSavePath.IsNullEmpty() && File.Exists(FileSavePath))
            {

                HybirdLock.Enter();
                try
                {
                    using (StreamReader sr = new StreamReader(FileSavePath, Encoding.UTF8))
                    {
                        LoadByString(converter(sr.ReadToEnd()));
                    }
                }
                catch (Exception ex)
                {
                    new LogHelper("SerialNumberBuilder类").WriteLog(ex);
                }
                finally
                {
                    HybirdLock.Leave();
                }
            }

        }
        /// <summary>
        /// 加载流水号
        /// </summary>
        /// <param name="content"></param>
        private void LoadByString(string content)
        {
            CurrentIndex = Convert.ToInt64(content);
        }
        #endregion
        #region Public Method 调用
        /// <summary>
        /// 清除流水号计数，进行重新计数
        /// </summary>
        public void ClearSerialNumber()
        {
            Interlocked.Exchange(ref CurrentIndex, 0);
            _AsyncCoordinator.StartHandlerOperater();
        }

        /// <summary>
        /// 获取流水号数据
        /// </summary>
        /// <returns></returns>
        public string GetSerialNumber()
        {
            long number = Interlocked.Increment(ref CurrentIndex);
            _AsyncCoordinator.StartHandlerOperater();
            if (string.IsNullOrEmpty(TimeFormate))
            {
                return TextHead + number.ToString().PadLeft(NumberLength, '0');
            }
            else
            {
                return TextHead + DateTime.Now.ToString(TimeFormate) + number.ToString().PadLeft(NumberLength, '0');
            }
        }

        /// <summary>
        /// 获取流水号数据
        /// </summary>
        /// <param name="textHead">指定一个新的文本头</param>
        /// <returns></returns>
        public string GetSerialNumber(string textHead)
        {
            long number = Interlocked.Increment(ref CurrentIndex);
            _AsyncCoordinator.StartHandlerOperater();
            if (string.IsNullOrEmpty(TimeFormate))
            {
                return textHead + number.ToString().PadLeft(NumberLength, '0');
            }
            else
            {
                return textHead + DateTime.Now.ToString(TimeFormate) + number.ToString().PadLeft(NumberLength, '0');
            }
        }

        /// <summary>
        /// 单纯的获取数字形式的流水号
        /// </summary>
        /// <returns></returns>
        public long GetLongSerialNumber()
        {
            long number = Interlocked.Increment(ref CurrentIndex);
            _AsyncCoordinator.StartHandlerOperater();
            return number;
        }
        #endregion
    }
}
