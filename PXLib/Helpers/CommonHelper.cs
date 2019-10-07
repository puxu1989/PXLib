using MySql.Data.MySqlClient;
using PXLib.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Linq;

namespace PXLib.Helpers
{
    public class CommonHelper
    {
        #region 定义公共常量区
        public static string SplitChar = "≌";
        public static string SoftwareName = ConfigHelper.GetConfigAppSettingsValue("SoftName");
        public static string CurAppRootPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;//WebFrom WinFrom


        public const bool IsAllowTryUse = false;//软件是否允许试用
        #endregion

        public static string GetValidString(string sourceStr, ref string returnDest, string[] splitChar)
        {
            char[] Div = new char[splitChar.Length];
            int i;
            for (i = 0; i < splitChar.Length; i++)
            {
                Div[i] = splitChar[i][0];
            }

            string[] Ary = sourceStr.Split(Div, 2, StringSplitOptions.RemoveEmptyEntries);//返回不包含空的值
            if (Ary.Length > 0)
                returnDest = Ary[0];//目标置为第一个
            else
                returnDest = "";
            if (Ary.Length > 1)
                return Ary[1];//返回第二个
            else
                return "";
        }
        //获取格式化好的日期 包括时分秒
        public static string GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //string.Format("{0:F}", DateTime.Now)
        }
        #region 创建ID
        /// <summary>
        /// 获取一个全球唯一码GUID字符串  32位
        /// </summary>
        public static string GetGuid
        {
            get
            {
                return Guid.NewGuid().ToString("N").ToLower();
            }
        }
        public static string IdCode()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 8).ToString().Substring(8, 9);

        }
        /// <summary>
        /// 自动生成编号18位  201510061145409865
        /// </summary>
        /// <returns></returns>
        public static string CreateNo(string prefix = "")
        {
            int seed = GetRandomSeed();//循环调用 提高随机数不重复概率的种子生成方法（推荐）
            Random random = new Random(seed);
            string strRandom = random.Next(1000, 10000).ToString(); //生成编号 
            string code = DateTime.Now.ToString("yyyyMMddHHmmss") + strRandom;//形如
            return prefix + code;
        }
        /// <summary>
        /// 自动生成长编号28位  2019093021001004070217142408 模仿微信支付流水号
        /// </summary>
        /// <returns></returns>
        public static string CreateLongNo()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            string lastRandomCode = BitConverter.ToInt64(buffer, 8).ToString();
            lastRandomCode = lastRandomCode.Substring(lastRandomCode.Length - 11, 11);//截取最后11位
            string code = DateTime.Now.ToString("yyyyMMddHHmmssfff") + lastRandomCode;//17+11位
            return code;
        }
        //获取随机种子 -1672519795 5828452252
        public static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }
        }
        /// <summary>
        /// 根据多少分钟生成期数Id201712040001 默认1分钟一次
        /// </summary>
        /// <returns></returns>
        public static string CreateIdByMinute(int minute = 1, bool isNext = false)
        {
            if (minute <= 0)
                minute = 1;
            DateTime dtNow = DateTime.Now;
            if (isNext)
                dtNow = dtNow.AddMinutes(minute);
            int qishu = (dtNow.Hour * 60 + dtNow.Minute) / minute;
            if (qishu < 1440 / minute)
            {
                qishu++;
            }
            else
            {
                qishu = 1;
            }
            return dtNow.ToString("yyyyMMdd") + qishu.ToString().PadLeft(4, '0');
        }
        public static string CreateIdBySecond(int second = 1, bool isNext = false)
        {
            if (second <= 0)
                second = 1;
            DateTime dtNow = DateTime.Now;
            if (isNext)
                dtNow = dtNow.AddSeconds(second);
            int qishu = (dtNow.Hour * 60 * 60 + dtNow.Minute * 60 + dtNow.Second) / second;
            int totalQishu = 1440 * 60 / second;
            if (qishu < totalQishu)
            {
                qishu++;
            }
            else
            {
                qishu = 1;
            }
            int pad = 0;
            if (totalQishu.ToString().Length <= 4)
            {
                pad = 4;
            }
            else
            {
                pad = totalQishu.ToString().Length;
            }
            return dtNow.ToString("yyyyMMdd") + qishu.ToString().PadLeft(pad, '0');
        }

        #endregion
        #region 邀请码<=>用户Id
        private static readonly string source_string = "2YU9IP6ASDFG8QWERTHJ7KLZX4CV5B3ONM1";//自定义35进制 此算法不能改
        /// <summary>
        /// 创建邀请码 Id必须为Int
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static string CreateInviteCode(int Id)
        {
            string code = "";
            int mod;
            while (Id > 0)
            {
                mod = Id % source_string.Length;
                Id = (Id - mod) / source_string.Length;
                code = source_string.ToCharArray()[mod] + code;

            }
            return code.PadRight(6, '0').ToLower();//不足六位补0
        }
        /// <summary>
        /// 解码邀请码返回用户Id
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static int DecodeInviteCode(string code)
        {
            code = code.ToUpper();
            code = new string((from s in code where s != '0' select s).ToArray());
            int num = 0;
            for (int i = 0; i < code.ToCharArray().Length; i++)
            {
                for (int j = 0; j < source_string.ToCharArray().Length; j++)
                {
                    if (code.ToCharArray()[i] == source_string.ToCharArray()[j])
                    {
                        num += j * Convert.ToInt32(Math.Pow(source_string.Length, code.ToCharArray().Length - i - 1));
                    }
                }
            }
            return num;
        }
        #endregion
        public static string GetMethodInfo()
        {
            string str = "";
            //取得当前方法命名空间
            str += "命名空间名:" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "\n";
            //取得当前方法类全名 包括命名空间
            str += "命名空间+类名:" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName + "\n";
            //获得当前类名
            str += "类名:" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\n";
            //取得当前方法名
            str += "方法名:" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\n";
            str += "\n";
            StackTrace ss = new StackTrace(true);
            MethodBase parent = ss.GetFrame(1).GetMethod();
            //取得父方法命名空间
            str += "父方法命名空间:" + parent.DeclaringType.Namespace + "\n";
            //取得父方法类名
            str += "父方法类名" + parent.DeclaringType.Name + "\n";
            //取得父方法类全名
            str += "父方法类全名:" + parent.DeclaringType.FullName + "\n";
            //取得父方法名
            str += "父方法名:" + parent.Name + "\n";
            return str;
        }
        /// <summary>
        /// 获得当前调用此方法的类名/方法名
        /// </summary>
        /// <returns></returns>
        public static string GetClassMethodName()
        {
            StackTrace ss = new StackTrace(true);
            MethodBase mb = ss.GetFrame(1).GetMethod();
            return mb.DeclaringType.Name + "/" + mb.Name;
        }
    }

}
