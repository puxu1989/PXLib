using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PXLib.Helpers
{
   public class BoolHelper
    {
        #region 验证输入字符串是否与模式字符串匹配（正则表达式）
        /// <summary>
        /// 验证输入字符串是否与模式字符串匹配（忽略大小写），匹配返回true
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="pattern">模式字符串</param>        
        public static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }
        #endregion
        #region 是否是电话号码 不限长度 可以是固定号码/手机有效性/
        /// <summary>
       /// 是否是电话号码 不限长度 可以是固定号码
       /// </summary>
       /// <param name="phoneNumber"></param>
       /// <returns></returns>
        public static bool IsPhone(string phoneNumber)
        {
            Match m = new Regex("^[0-9]+[-]?[0-9]+[-]?[0-9]$").Match(phoneNumber);
            return m.Success;
        }
        /// <summary>
        /// 手机有效性
        /// </summary>
        /// <param name="strMobile"></param>
        /// <returns></returns>
        public static bool IsValidMobile(string mobile)
        {
            Regex rx = new Regex(@"^(13|15|17|18|19)\d{9}$", RegexOptions.None);
            Match m = rx.Match(mobile);
            return m.Success;
        }
       #endregion
        #region 检查用户名和密码是否有效
        /// <summary>
        /// 检测用户名格式是否有效 长度在4=20之间且只能是汉字，字母，下划线，数字
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsValidUserName(string userName)
        {
            int userNameLength = Encoding.Default.GetBytes(userName).Length;
            if (userNameLength >= 4 && userNameLength <= 20 && Regex.IsMatch(userName, @"^([\u4e00-\u9fa5A-Za-z_0-9]{0,})$"))
            {   // 判断用户名的长度（4-20个字符）及内容（只能是汉字、字母、下划线、数字）是否合法
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 密码有效性 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^[A-Za-z_0-9]{6,18}$");
        }
        #endregion
        #region 是否是身份证号
        /// <summary>
        /// 验证身份证是否合法  15 和  18位两种
        /// </summary>
        public static bool IsIdCard(string idCard)
        {
            if (string.IsNullOrEmpty(idCard))
            {
                return false;
            }

            if (idCard.Length == 15)
            {
                return Regex.IsMatch(idCard, @"^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$");
            }
            else if (idCard.Length == 18)
            {
                return Regex.IsMatch(idCard, @"^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[A-Z])$", RegexOptions.IgnoreCase);
            }
            else
            {
                return false;
            }
        }

        #endregion
        #region 是否是数字/带符号的数字
        /// <summary>
        /// 是否是数字字符串
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsNumber(string inputData)
        {
            Match m = new Regex("^[0-9]+$").Match(inputData);
            return m.Success;
        }
        /// <summary>
        /// 是否数字字符串 可带正负号
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsNumberSign(string inputData)
        {
            Match m = new Regex("^[+-]?[0-9]+$").Match(inputData);
            return m.Success;
        }
        #endregion
        #region 是否是浮点数/带符号的浮点数
        /// <summary>
        /// 是否是浮点数
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsDecimal(string inputData)
        {
            Match m =  new Regex("^[0-9]+[.]?[0-9]+$").Match(inputData);
            return m.Success;
        }
        /// <summary>
        /// 是否是浮点数 可带正负号
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsDecimalSign(string inputData)
        {
            Match m = new Regex("^[+-]?[0-9]+[.]?[0-9]+$").Match(inputData);
            return m.Success;
        }

        #endregion
        #region 是否含有中文 
        /// <summary>
        /// 检测是否有中文字符
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static bool IsHasCHZN(string inputData)
        {
            Match m = new Regex("[\u4e00-\u9fa5]").Match(inputData);
            return m.Success;
        }

        #endregion
        #region 是否是邮箱地址
        public static bool IsEmail(string inputData) 
        {
            //另外表达式：@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
        Regex RegEmail = new Regex("^[\\w-]+@[\\w-]+\\.(com|net|org|edu|mil|tv|biz|info)$");//w 英文字母或数字的字符串，和 [a-zA-Z0-9] 语法一样 
        return RegEmail.Match(inputData).Success;
        }
        #endregion
        #region 是否是邮编格式
        /// <summary>
        /// 邮编有效性
        /// </summary>
        public static bool IsValidZip(string zip)
        {
            Regex rx = new Regex(@"^\d{6}$", RegexOptions.None);
            Match m = rx.Match(zip);
            return m.Success;
        }
        #endregion
        #region 是否是日期格式
        /// <summary>
        /// 日期格式字符串判断
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDateTime(string str)
        {
            try
            {
                if (!string.IsNullOrEmpty(str))
                {
                    DateTime.Parse(str);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #region 是否为json格式
        /// <summary>
        /// 判断一个字符串是否为json格式  
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsJson(string input)
        {
            input = input.Trim();
            return input.StartsWith("{") && input.EndsWith("}")
                   || input.StartsWith("[") && input.EndsWith("]");
        } 
        #endregion
        #region 是否是IP地址
        public static bool IsIPAddress(string inputData)
        {
            Match m = new Regex(@"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))").Match(inputData);
            return m.Success;
        }
        #endregion
        #region 是否是合法的Url
        /// <summary>
        /// Url有效性
        /// </summary>
        static public bool IsValidURL(string url)
        {
            return Regex.IsMatch(url, @"^(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&%\$#\=~])*[^\.\,\)\(\s]$");
        }

        #endregion
        #region 判断数值是否在范围之内
        /// <summary>
        /// 判断数值是否在范围之内
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool RangeInDefined(int values, int min, int max)
        {
            return Math.Max(min, values) == Math.Min(values, max);
        }

        /// <summary>
        /// 判断数值是否在范围之内
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool RangeInDefined(long values, int min, int max)
        {
            return Math.Max(min, values) == Math.Min(values, max);
        }
        #endregion
        #region 概率是否命中    
       /// <summary>
        ///概率是否命中   percentValue 在0-100之间
       /// </summary>
        public static bool Try(int percentValue=50)
        {
            Random random = new Random();
            return percentValue > random.Next(0, 100);
        }
        #endregion
        #region 判断是否是英文和数字组合
        public static bool IsEnglishAndNum(string inString) 
        {
            string pattern = @"^[a-zA-Z0-9]+$";//正则式子  
            Match m = Regex.Match(inString, pattern);
            return m.Success;
        }
        #endregion
        #region 靓号匹配过滤
       public static bool IsGoodNumber(string inNumber)
       {
           //匹配4-9位连续的数字
           Match m1 = new Regex(@"(?:(?:0(?=1)|1(?=2)|2(?=3)|3(?=4)|4(?=5)|5(?=6)|6(?=7)|7(?=8)|8(?=9)){3,}|(?:9(?=8)|8(?=7)|7(?=6)|6(?=5)|5(?=4)|4(?=3)|3(?=2)|2(?=1)|1(?=0)){3,})\d", RegexOptions.None).Match(inNumber);
           if (m1.Success)
               return true;
           //匹配6位顺增或顺降
           Match m2 = new Regex(@"(?:(?:0(?=1)|1(?=2)|2(?=3)|3(?=4)|4(?=5)|5(?=6)|6(?=7)|7(?=8)|8(?=9)){5}|(?:9(?=8)|8(?=7)|7(?=6)|6(?=5)|5(?=4)|4(?=3)|3(?=2)|2(?=1)|1(?=0)){5})\d", RegexOptions.None).Match(inNumber);
           if (m2.Success)
               return true;  
           ////匹配3位以上的重复数字
           Match m3 = new Regex(@"([\d])\1{2,}", RegexOptions.None).Match(inNumber);
           if (m3.Success)
               return true;
           return false;
       }
        #endregion
    }
}
