using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace PXLib.Helpers
{
    public class ComputerHelper
    {
         private static PerformanceCounter CpuPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");//静态统计
        ///<summary>
        /// 获取硬盘卷标号 8位长度
        ///</summary>
        ///<returns></returns>
        private static string GetDiskVolumeSerialNumber()
        {
            ManagementClass mc = new ManagementClass("win32_NetworkAdapterConfiguration");
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        }

        ///<summary>
        /// 获取CPU序列号  16位长度
        ///</summary>
        ///<returns></returns>
        private static string GetCpuSeriesNumber()
        {
            string strCpu = null;
            ManagementClass myCpu = new ManagementClass("win32_Processor");
            ManagementObjectCollection myCpuCollection = myCpu.GetInstances();
            foreach (ManagementObject myObject in myCpuCollection)
            {
                strCpu = myObject.Properties["Processorid"].Value.ToString();
            }
            return strCpu;
            
        }
        ///<summary>
        /// 软件特征码
        ///</summary>
        ///<returns>软件特征码是获取正式版软件</returns>
        public static string GetMNum()
        {
            string strNum = GetCpuSeriesNumber() + GetDiskVolumeSerialNumber();
            //strNum = strNum.Substring(0, 24);    //截取前24位作为机器码
            return strNum;
        }
        /// <summary>
        /// GetMacAddress 获取网卡mac地址
        /// </summary>        
        public static IList<string> GetMacAddress()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            IList<string> strArr = new List<string>();

            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"])
                {
                    strArr.Add(mo["MacAddress"].ToString().Replace(":", "-"));
                }
                mo.Dispose();
            }

            return strArr;
        }
        /// <summary>
        /// 获取可用总物理内存大小 单位kb
        /// </summary>
        /// <returns>可用物理内存  ref 剩余未使用的内存</returns>
        public static ulong GetPhysicalMemorySize(ref ulong FreePhysicalMemory)
        {
            ulong PhysicalMemorySize = 0;
            ManagementClass osClass = new ManagementClass("Win32_OperatingSystem");
            foreach (ManagementObject obj in osClass.GetInstances())
            {
                if (obj["TotalVisibleMemorySize"] != null)
                    PhysicalMemorySize = (ulong)obj["TotalVisibleMemorySize"];
                if (obj["FreePhysicalMemory"] != null)
                    FreePhysicalMemory = (ulong)obj["FreePhysicalMemory"];
                break;
            }
            osClass.Dispose();
            return PhysicalMemorySize;
        }
         /// <summary>
         /// 获取Cpu信息
         /// </summary>
         /// <returns></returns>
        public static List<CpuInfo> GetCpuInfo()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_Processor");
            ManagementObjectCollection list = searcher.Get();
            uint count = 0;
            foreach (ManagementObject obj2 in list)
            {
                ++count;
            }
            List<CpuInfo> cpuList = new List<CpuInfo>();
            foreach (ManagementObject obj2 in list)
            {
                cpuList.Add(new CpuInfo(obj2.GetPropertyValue("Name").ToString(), (uint)obj2.GetPropertyValue("CurrentClockSpeed"), (uint)(Environment.ProcessorCount / count)));
            }
            return cpuList;
        }
        /// <summary>
        ///  获取Cpu性能参数  返回CPU利用率  out物理内存利用率
        /// </summary>
        /// <param name="memoryUsage">物理内存利用率</param>
        public static float GetPerformanceUsage(out float memoryUsage)
        {           
            float cpuUsage = CpuPerformanceCounter.NextValue();//可能还有最好的方法？
            ulong FreePhysicalMemory=0;
            ulong PhysicalMemorySize = GetPhysicalMemorySize(ref FreePhysicalMemory);
            if (PhysicalMemorySize == 0)
                memoryUsage = 0;
            else
                memoryUsage = (PhysicalMemorySize - FreePhysicalMemory) * 100 / PhysicalMemorySize;
            return cpuUsage;
        }
        [DllImport("kernel32.dll")]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out UInt64 lpFreeBytesAvailable, out UInt64 lpTotalNumberOfBytes, out UInt64 lpTotalNumberOfFreeBytes);
        /// <summary>
        /// 获取磁盘的可用空间大小 单位 GB
        /// </summary>
        /// <param name="diskName">磁盘的名称。如"C:\"</param>
        /// <returns>磁盘的剩余控件</returns>      
        public static ulong GetDiskFreeSpace(string diskName)
        {
            ulong freeBytesAvailable = 0;
            ulong totalNumberOfBytes = 0;
            ulong totalNumberOfFreeBytes = 0;

            GetDiskFreeSpaceEx(diskName, out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes);
            return freeBytesAvailable/(1024*1024*1024);
        }

        #region 检测本机是否联网（互联网）

        [DllImport("sensapi.dll")]
        private extern static bool IsNetworkAlive(out int connectionDescription);     
        /// <summary>
        /// 可以及时反应网络连通情况，但是需要服务System Event Notification支持（系统默认自动启动该服务）当计算机有多个网卡如虚拟机的只要有一个启用就代表联网的
        /// </summary>       
        public static bool IsConnectedInternet()
        {
            int flags;//上网方式 
            bool online = IsNetworkAlive(out flags);
            return online;
        }
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        /// <summary>
        /// 对网络状况不能及时反应（推荐使用）当主网卡断开后代表不联网状态
        /// </summary>        
        public static bool IsConnectedInternet2()
        {
            int flags;//上网方式 
            bool online = InternetGetConnectedState(out flags, 0);
            #region Details
            //int INTERNET_CONNECTION_MODEM = 1;
            //int INTERNET_CONNECTION_LAN = 2;
            //int INTERNET_CONNECTION_PROXY = 4;
            //int INTERNET_CONNECTION_MODEM_BUSY = 8;
           //string outPutDesc = "";
           // if (online)//在线   
           // {
           //     if ((flags & INTERNET_CONNECTION_MODEM) == INTERNET_CONNECTION_MODEM)
           //     {
           //         outPutDesc = "在线：拨号上网\n";
           //     }
           //     if ((flags & INTERNET_CONNECTION_LAN) == INTERNET_CONNECTION_LAN)
           //     {
           //         outPutDesc = "在线：通过局域网\n";
           //     }
           //     if ((flags & INTERNET_CONNECTION_PROXY) == INTERNET_CONNECTION_PROXY)
           //     {
           //         outPutDesc = "在线：代理\n";
           //     }
           //     if ((flags & INTERNET_CONNECTION_MODEM_BUSY) == INTERNET_CONNECTION_MODEM_BUSY)
           //     {
           //         outPutDesc = "MODEM被其他非INTERNET连接占用\n";
           //     }
           // }
           // else
           // {
           //     outPutDesc = "不在线\n";
           // } 
            #endregion
            return online;   
        }
        #endregion
        //获取系统启动到当前时间的毫秒数
        [DllImport("kernel32")]
        public static extern uint GetTickCount();

    }
     public struct CpuInfo
     {
         public CpuInfo(string cpuname, uint speed, uint coreCount)
         {
             this.CpuName = cpuname;
             this.ClockSpeed = speed;
             this.CoreCount = coreCount;
         }

         /// <summary>
         /// CPU名称。
         /// </summary>
         public string CpuName;
         /// <summary>
         /// 主频。
         /// </summary>
         public uint ClockSpeed;
         /// <summary>
         /// 核心数目
         /// </summary>
         public uint CoreCount;
     } 
}
