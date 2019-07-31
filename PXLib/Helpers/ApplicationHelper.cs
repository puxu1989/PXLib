using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace PXLib.Helpers
{
    public class ApplicationHelper
    {
        /// <summary>
        /// StartApplication 启动一个应用程序/进程
        /// </summary>       
        public static void StartApplication(string appFilePath)
        {
            Process process = new Process();
            process.StartInfo.FileName = appFilePath;
            process.Start();
        }
        /// <summary>
        /// OpenUrl 在浏览器中打开wsUrl链接
        /// </summary>        
        public static void OpenUrl(string url)
        {
            Process.Start(url);
        }

        public static bool IsApplicationStarted(string processesName)
        {
            bool isStart = false;
            Process[] vProcesses = Process.GetProcesses();
            foreach (Process vProcess in vProcesses)
            {
                if (vProcess.ProcessName.Equals(processesName, StringComparison.OrdinalIgnoreCase))
                {
                    isStart = true;
                    break;
                }
            } 
            return isStart;
        }
        /// <summary>
        /// 此函数用于判断某一外部进程是否打开
        /// </summary>
        /// <param name="processName">参数为进程名</param>
        /// <returns>如果打开了，就返回true，没打开，就返回false</returns>
        public static bool IsProcessStarted(string processName)
        {
            Process[] temp = Process.GetProcessesByName(processName);
            if (temp.Length > 0) 
                return true;
            else
                return false;
        }

        #region IsAppInstanceExist
        /// <summary>
        /// IsAppInstanceExist 目标应用程序是否已经启动。通常用于判断单实例应用。将占用锁。
        /// </summary>       
        public static bool IsAppInstanceExist(string instanceName)
        {

            bool createdNew = false;
            ApplicationHelper.MutexForSingletonExe = new Mutex(false, instanceName, out createdNew);

            return (!createdNew);
        }

        private static Mutex MutexForSingletonExe = null;

        /// <summary>
        /// 释放由IsAppInstanceExist占用的锁。
        /// </summary>        
        public static void ReleaseAppInstance(string instanceName)
        {
            if (ApplicationHelper.MutexForSingletonExe != null)
            {
                ApplicationHelper.MutexForSingletonExe.Close();
                ApplicationHelper.MutexForSingletonExe = null;
            }
        }
        #endregion
    }
}
