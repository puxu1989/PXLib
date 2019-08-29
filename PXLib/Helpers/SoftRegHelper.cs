using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace PXLib.Helpers
{
    public class SoftRegHelper
    {
        public static string machineCode = SecurityHelper.MD5String16("**********");
        public SoftRegHelper(string softWareRegCode)
        {
            this.Registration(softWareRegCode);
        }
        private  void  Registration(string softWareRegCode)
        {
            try
            {
                RegistryHelper registry = new RegistryHelper(CommonHelper.SoftwareName);
                bool IsAllowTryUse = CommonHelper.IsAllowTryUse;//软件是否试用
                if (IsAllowTryUse)
                {
                    DateTime SetupTime = new DateTime();
                    if (!registry.IsRegeditExit("SetupTime"))
                    {
                        registry.WTRegedit("SetupTime", SecurityHelper.DESEncrypt(DateTime.Now.AddDays(+30).Date.ToString()));
                    }
                    SetupTime = SecurityHelper.DESEncrypt(registry.GetRegistData("SetupTime").ToString()).ToDate(); ;
                    if (DateTime.Now > SetupTime)
                    {
                        throw new Exception("您的试用期已过，请购买正版！");
                    }
                }
                else
                {
                    string code = SecurityHelper.DESEncrypt(softWareRegCode);
                    if (!registry.IsRegeditExit("IsAuthorization"))
                    {
                        registry.WTRegedit("IsAuthorization", SecurityHelper.DESEncrypt("TRUE"));
                    }
                    else if (code != SoftRegHelper.machineCode)
                    {
                        throw new Exception("授权码错误，请购买正版获取授权码！");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static void Registration(string SN, string licenseCode)
        {
            try
            {
                string s1 = SecurityHelper.DESDecrypt(licenseCode);
                SecurityHelper se = new SecurityHelper();
                string s2 = se.Decrypt3DESString(SN, licenseCode.Substring(3, 32));
                if (s2 != SecurityHelper.MD5String16(SoftRegHelper.machineCode) || s1 != SoftRegHelper.machineCode)
                {
                    throw new Exception("请购买正版获取序列号和授权码！");
                }
                RegistryHelper registry = new RegistryHelper(CommonHelper.SoftwareName);
                if (!registry.IsRegeditExit("IsAuthorization"))
                {
                    registry.WTRegedit("IsAuthorization", SecurityHelper.DESEncrypt("TRUE"));
                }

            }
            catch (Exception ex)
            {
                throw new Exception("软件注册失败："+ex.Message);
            }
        }

    }
    /// <summary>
    /// 注册表写入、读取
    /// </summary>
    public class RegistryHelper
    {
        public string softwareName
        {
            get;
            set;
        }
        public RegistryHelper(string softwareName) 
        {
            this.softwareName = softwareName;        
        }

        /// <summary>
        /// 读取指定名称的注册表的值 
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public object GetRegistData(string name)
        {
            string registData;
            //RegistryKey hkml = Registry.LocalMachine;//要以管理员省份运行
            RegistryKey hkml = Registry.CurrentUser;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            RegistryKey aimdir = software.OpenSubKey(softwareName, true);
            registData = aimdir.GetValue(name).ToString();
            return registData;
        }
        /// <summary>
        /// 向注册表中写数据 
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="tovalue">值</param>
        public void WTRegedit(string name, object tovalue)
        {
 
            //RegistryKey hkml = Registry.LocalMachine;//要以管理员省份运行
            RegistryKey hkml = Registry.CurrentUser;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            RegistryKey aimdir = software.CreateSubKey(softwareName);
            aimdir.SetValue(name, tovalue);
        }
        /// <summary>
        /// 判断指定注册表项是否存在 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsRegeditExit(string name)
        {
            try
            {
                this.GetRegistData(name);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
