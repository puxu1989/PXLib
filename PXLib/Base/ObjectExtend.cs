using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PXLib.Attributes;
using PXLib.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace PXLib
{
    /// <summary>
    /// 扩展方法 必须是静态类 vs2013以下版本需要手动添加using 命名空间
    /// </summary>
    public static class ObjectExtension
    {

        #region 枚举描述/int转枚举扩展
        private static readonly Dictionary<string, Dictionary<string, string>> enumCache = new Dictionary<string, Dictionary<string, string>>();
        //调用此方法 请手动引用命名空间 PXLib 编辑器识别不了 
        /// <summary>
        /// 获取[EnumText("描述名称")]枚举名称 使用了缓存
        /// </summary>
        /// <param name="inenum"></param>
        /// <returns></returns>
        public static string GetEnumText(this Enum inenum)
        {
            if (null == inenum) return string.Empty;
            Type type = inenum.GetType();
            if (!enumCache.ContainsKey(type.FullName))
            {
                var fields = type.GetFields();
                Dictionary<string, string> temp = new Dictionary<string, string>();
                foreach (var item in fields)
                {
                    var attrs = item.GetCustomAttributes(typeof(EnumText), false);
                    if (attrs.Length == 1)
                    {
                        string v = ((EnumText)attrs[0]).Description;
                        temp.Add(item.Name, v);
                    }
                }
                enumCache.Add(type.FullName, temp);
            }
            if (enumCache[type.FullName].ContainsKey(inenum.ToString()))
            {
                return enumCache[type.FullName][inenum.ToString()];
            }
            return inenum.ToString().Trim();
        }
        public static string GetEnumTextNoCache(this Enum enumType)
        {
            Type t = enumType.GetType();
            MemberInfo[] m = t.GetMember(enumType.ToString());
            if (m != null && m.Length > 0)
            {
                object[] agrs = m[0].GetCustomAttributes(typeof(EnumText), false);
                if (agrs != null && agrs.Length > 0)
                {
                    return ((EnumText)agrs[0]).Description;
                }
            }
            return enumType.ToString();//返回枚举键名称

        }
        /// <summary>
        /// Objcet转枚举  传入的值必须是枚举的枚举基或基础类型，如 Int32 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToEnmu<T>(this object value)
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            value = Convert.ToInt32(value);
            return (T)Enum.ToObject(enumType, value);
        }
        #endregion
        #region 各进制数间转换
        /// <summary>
        /// 实现各进制数间的转换。ConvertBase("15",10,16)表示将十进制数15转换为16进制的数。
        /// </summary>
        /// <param name="value">要转换的值,即原值</param>
        /// <param name="from">原值的进制,只能是2,8,10,16四个值。</param>
        /// <param name="to">要转换到的目标进制，只能是2,8,10,16四个值。</param>
        public static string ConvertBase(this string value, int from, int to)
        {
            int intValue = Convert.ToInt32(value, from);  //先转成10进制
            string result = Convert.ToString(intValue, to);  //再转成目标进制
            if (to == 2)
            {
                int resultLength = result.Length;  //获取二进制的长度
                switch (resultLength)
                {
                    case 7:
                        result = "0" + result;
                        break;
                    case 6:
                        result = "00" + result;
                        break;
                    case 5:
                        result = "000" + result;
                        break;
                    case 4:
                        result = "0000" + result;
                        break;
                    case 3:
                        result = "00000" + result;
                        break;
                }
            }
            return result;
        }
        #endregion
        #region 字符串扩展
        /// <summary>
        /// 将Byte数组输出成string 
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static string ToBytesString(this byte[] buf)
        {
            string s = "";
            if (buf == null)
            {
                return "";
            }
            int x = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                x += 1;
                if (x >= 200)
                {
                    s = "";
                    x = 0;
                }
                s += buf[i];
                s += " ";
            }
            return s;
        }
        /// <summary>
        /// 同string.IsNullOrEmpty
        /// </summary>
        public static bool IsNullEmpty(this string value)
        {
            if (value == null)
                return true;
            return string.IsNullOrWhiteSpace(value);
        }
        /// <summary>
        /// 对象是否为空
        /// </summary>
        /// <param name="value">值</param>
        public static bool IsNullEmpty(this object value)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        //public static bool SameAS(this string str1, string str2)
        //{
        //    return str1.ToLower().Equals(str2.ToLower());
        //}
        /// <summary>
        /// 返回移除字符串末尾指定长度的字符串剩余的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public static string RemoveLastLength(this string value, int len = 1)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            value = value.Substring(0, value.Length - len);//
            return value;
        }
        /// <summary>
        /// 字符串分隔扩展 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="splitString"></param>
        /// <returns></returns>
        public static string[] Split(this string value, string splitString)
        {
            if (value.IsNullEmpty())
                return new string[0];
            return value.Split(new string[] { splitString }, StringSplitOptions.RemoveEmptyEntries);
        }
        /// <summary>
        ///  省略字符串 保留length个长度，中文算两个 后的用...代替  
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <param name="dotstr"></param>
        /// <returns></returns>
        public static string GetOmitString(this string str, int length, string dotstr="")
        {
            string temp = str;
            int j = 0;
            int k = 0;
            for (int i = 0; i < temp.Length; i++)
            {
                if (Regex.IsMatch(temp.Substring(i, 1), @"[\u4e00-\u9fa5]+"))
                {
                    j += 2;
                }
                else
                {
                    j += 1;
                }
                if (j <= length)
                {
                    k += 1;
                }
                if (j >= length)
                {
                    return temp.Substring(0, k) + dotstr;
                }
            }
            return temp;
        }
        /// <summary>
        /// 保留 len长度的字符串 （中文一个字算一个长度）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <param name="dotstr"></param>
        /// <returns></returns>
        public static string CutString(this string str, int len, string dotstr = "")
        {

            if (str.Length <= len)
            {
                return str;
            }
            else
            {
                return str.Substring(0, len) + dotstr;
            }
        }
        #endregion
        #region Json扩展
        public static object ToJson(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject(Json);
        }
        public static string ToJson(this object obj)
        {
            Newtonsoft.Json.JsonSerializerSettings setting = new Newtonsoft.Json.JsonSerializerSettings();
            JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
            {
                //日期类型默认格式化处理
                setting.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                setting.DateFormatString = "yyyy-MM-dd HH:mm:ss";

                //空值处理
                setting.NullValueHandling = NullValueHandling.Ignore;
                //setting.FloatParseHandling = FloatParseHandling.Decimal;
                return setting;
            });
            return JsonConvert.SerializeObject(obj, Formatting.None, setting);//Formatting.Indented缩进排版
        }
        public static string ToJson(this object obj, string datetimeformats)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = datetimeformats };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }
        /// <summary>
        /// json反序列化成对象
        /// </summary>
        public static T ToObject<T>(this string Json)
        {
            return Json == null ? default(T) : JsonConvert.DeserializeObject<T>(Json);
        }
        public static List<T> ToList<T>(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject<List<T>>(Json);
        }
        public static DataTable ToTable(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject<DataTable>(Json);
        }
        public static JObject ToJObject(this string Json)
        {
            return Json == null ? JObject.Parse("{}") : JObject.Parse(Json.Replace("&nbsp;", ""));
        }
        public static string EnumToJson(this Type type)
        {

            //var results = Enum.GetValues(type).Cast<object>() .ToDictionary(enumValue => enumValue.ToString(), enumValue => (int)enumValue);
            var dic = DataConvertHelper.EnumListToDic(type);
            return dic.ToJson();
        }
        #endregion
        #region 日期转换
        /// <summary>
        /// 转换为日期 如果为NULL或者转换失败 这转成DateTime.Now
        /// </summary>
        /// <param name="data">数据</param>
        public static DateTime ToDate(this object data)
        {
            if (data == null)
                return DateTime.Now;
            DateTime result;
            return DateTime.TryParse(data.ToString(), out result) ? result : DateTime.Now;
        }

        /// <summary>
        /// 转换为可空日期
        /// </summary>
        /// <param name="data">数据</param>
        public static DateTime? ToDateOrNull(this object data)
        {
            if (data == null)
                return null;
            DateTime result;
            bool isValid = DateTime.TryParse(data.ToString(), out result);
            if (isValid)
                return result;
            return null;
        }

        #endregion
        #region 数值转换
        /// <summary>
        /// 转换为整型 null为0
        /// </summary>
        /// <param name="data">数据</param>
        public static int ToInt(this object data)
        {
            if (data == null)
                return 0;
            Type type = data.GetType(); ;
            if (type.IsEnum)
            {
                return (int)data;
            }
            int result;
            var success = int.TryParse(data.ToString(), out result);
            if (success)
                return result;
            try
            {
                return Convert.ToInt32(ToDouble(data, 0));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 转换为双精度浮点数
        /// </summary>
        /// <param name="data">数据</param>
        public static double ToDouble(this object data)
        {
            if (data == null)
                return 0;
            double result;
            return double.TryParse(data.ToString(), out result) ? result : 0;
        }

        /// <summary>
        /// 转换为双精度浮点数,并按指定的小数位4舍5入
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="digits">小数位数</param>
        public static double ToDouble(this object data, int digits)
        {
            return Math.Round(ToDouble(data), digits);
        }

        /// <summary>
        /// 转换为可空双精度浮点数
        /// </summary>
        /// <param name="data">数据</param>
        public static double? ToDoubleOrNull(this object data)
        {
            if (data == null)
                return null;
            double result;
            bool isValid = double.TryParse(data.ToString(), out result);
            if (isValid)
                return result;
            return null;
        }
        /// <summary>
        /// 转换到Int64
        /// </summary>
        /// <param name="data">数据</param>
        public static long? ToLongOrNull(this object data)
        {
            if (data == null)
                return null;
            long result;
            bool isValid = long.TryParse(data.ToString(), out result);
            if (isValid)
                return result;
            return null;
        }
        /// <summary>
        /// 转换为高精度浮点数
        /// </summary>
        /// <param name="data">数据</param>
        public static decimal ToDecimal(this object data)
        {
            if (data.IsNullEmpty())
                return 0;
            decimal result;
            return decimal.TryParse(data.ToString(), out result) ? result : 0;
        }

        /// <summary>
        /// 转换为高精度浮点数,并按指定的小数位4舍5入
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="digits">小数位数</param>
        public static decimal ToDecimal(this  object data, int digits)
        {
            return Math.Round(ToDecimal(data), digits);
        }

        /// <summary>
        /// 转换为可空高精度浮点数
        /// </summary>
        /// <param name="data">数据</param>
        public static decimal? ToDecimalOrNull(this  object data)
        {
            if (data == null)
                return null;
            decimal result;
            bool isValid = decimal.TryParse(data.ToString(), out result);
            if (isValid)
                return result;
            return null;
        }

        /// <summary>
        /// 转换为可空高精度浮点数,并按指定的小数位4舍5入
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="digits">小数位数</param>
        public static decimal? ToDecimalOrNull(this object data, int digits)
        {
            var result = ToDecimalOrNull(data);
            if (result == null)
                return null;
            return Math.Round(result.Value, digits);
        }

        #endregion
        #region 布尔转换
        /// <summary>
        /// 转换为布尔值
        /// </summary>
        /// <param name="data">数据</param>
        public static bool ToBool(this object data)
        {
            if (data == null)
                return false;
            bool? value = GetBool(data);
            if (value != null)
                return value.Value;
            bool result;
            return bool.TryParse(data.ToString(), out result) && result;
        }

        /// <summary>
        /// 获取布尔值
        /// </summary>
        private static bool? GetBool(this object data)
        {
            switch (data.ToString().Trim().ToLower())
            {
                case "false":
                    return false;
                case "true":
                    return true;
                case "0":
                    return false;
                case "1":
                    return true;
                case "是":
                    return true;
                case "否":
                    return false;
                case "yes":
                    return true;
                case "no":
                    return false;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 转换为可空布尔值
        /// </summary>
        /// <param name="data">数据</param>
        public static bool? ToBoolOrNull(this object data)
        {
            if (data == null)
                return null;
            bool? value = GetBool(data);
            if (value != null)
                return value.Value;
            bool result;
            bool isValid = bool.TryParse(data.ToString(), out result);
            if (isValid)
                return result;
            return null;
        }

        #endregion
        #region DataTable转换成ArrayList
        /// <summary>
        /// DataTable转换成ArrayList 解决JsonConvert.SerializeObject序列化时间出错问题
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static object ToArrayList(this DataTable dataTable)
        {
            if (dataTable == null||dataTable.Rows.Count<=0)
                return null;
            ArrayList arrayList = new ArrayList();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    if (!dataRow[dataColumn.ColumnName].IsNullEmpty() && dataRow[dataColumn.ColumnName] != DBNull.Value)
                        dictionary.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToString());
                    else
                        dictionary.Add(dataColumn.ColumnName, "");
                }
                arrayList.Add(dictionary); //ArrayList集合中添加键值
            }
            return arrayList;
        }
        #endregion
        #region StingFormat
        public static string FormatWith(this string format, object arg0)
        {
            return format.FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				arg0
			});
        }

        public static string FormatWith(this string format,  object arg0, object arg1)
        {
            return format.FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				arg0,
				arg1
			});
        }

        public static string FormatWith(this string format, object arg0, object arg1, object arg2)
        {
            return format.FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				arg0,
				arg1,
				arg2
			});
        }

        public static string FormatWith(this string format, object arg0, object arg1, object arg2, object arg3)
        {

            return format.FormatWith(CultureInfo.InvariantCulture, new object[]
			{
				arg0,
				arg1,
				arg2,
				arg3
			});
        }

        private static string FormatWith(this string format, IFormatProvider provider, params object[] args)
        {
            ValidationHelper.ArgumentNotNull(format, "format");
            return string.Format(provider, format, args);
        }
        #endregion
    }

}
