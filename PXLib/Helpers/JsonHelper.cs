using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;

namespace PXLib.Helpers
{
    /// <summary>
    ///  2016-7-27 目前主流的三种主流的序列化和反序列化 1.JavaScriptSerializer(3.5内置) 2.DataContractJsonSerializer(3.5内置) 3.Newtonsoft.Json（第三方）
    ///  1,2,3小数据时性能忽略不计 大数据时2和3最高，稳定性和综合性还是Newtonsoft.Json比较强 （最高的应该是二进制序列化，可以考虑）
    /// </summary>
    public class JsonHelper
    {
        #region 私有 字符转义
        /// <summary>
        /// Json特符字符过滤，参见http://www.json.org/
        /// </summary>
        private static string JsonCharFilter(string sourceStr)
        {
            sourceStr = sourceStr.Replace("\\", "\\\\");
            sourceStr = sourceStr.Replace("\b", "\\b");
            sourceStr = sourceStr.Replace("\t", "\\t");
            sourceStr = sourceStr.Replace("\n", "\\n");
            sourceStr = sourceStr.Replace("\f", "\\f");
            sourceStr = sourceStr.Replace("\r", "\\r");
            return sourceStr.Replace("\"", "\\\"");
        }
        /// <summary>
        /// 格式化字符型、日期型、布尔型
        /// </summary>
        private static string StringFormat(string str, Type type)
        {
            if (type == typeof(string))
            {
                str = String2Json(str);
                str = "\"" + str + "\"";
            }
            else if (type == typeof(DateTime))
            {
                str = "\"" + str + "\"";
            }
            else if (type == typeof(bool))
            {
                str = str.ToLower();
            }
            else if (type != typeof(string) && string.IsNullOrEmpty(str))
            {
                str = "\"" + str + "\"";
            }
            return str;
        }
        /// <summary>
        /// 过滤特殊字符
        /// </summary>
        private static string String2Json(String s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s.ToCharArray()[i];
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\""); break;
                    case '\\':
                        sb.Append("\\\\"); break;
                    case '/':
                        sb.Append("\\/"); break;
                    case '\b':
                        sb.Append("\\b"); break;
                    case '\f':
                        sb.Append("\\f"); break;
                    case '\n':
                        sb.Append("\\n"); break;
                    case '\r':
                        sb.Append("\\r"); break;
                    case '\t':
                        sb.Append("\\t"); break;
                    default:
                        sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
        #endregion
        #region 以下注释部分用到.Net3.5
        #region List-JSON序列化和反序列化
        public static List<T> JsonToList<T>(string JsonStr)
        {
            //JavaScriptSerializer命名空间System.Web.Script.Serialization;
            //System.Web.Extensions（位于 System.Web.Extensions.dll）
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            List<T> objs = Serializer.Deserialize<List<T>>(JsonStr);
            return objs;
        }

        public static string ListToJson<T>(IList list)//IList到json 修改过
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                if (list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        T obj = Activator.CreateInstance<T>();
                        PropertyInfo[] pi = obj.GetType().GetProperties();
                        sb.Append("{");
                        for (int j = 0; j < pi.Length; j++)
                        {
                            sb.Append("\"");
                            sb.Append(pi[j].Name.ToString());
                            sb.Append("\":\"");
                            if (pi[j].GetValue(list[i], null) != null && pi[j].GetValue(list[i], null) != DBNull.Value && pi[j].GetValue(list[i], null).ToString() != "")
                            {
                                //sb.Append(pi[j].GetValue(list[i], null)).Replace("\\", "/");
                                sb.Append(JsonCharFilter(pi[j].GetValue(list[i], null).ToString()));//特殊转移字符过滤
                            }
                            else
                            {
                                sb.Append("");
                            }
                            sb.Append("\",");
                        }
                        sb = sb.Remove(sb.Length - 1, 1);
                        sb.Append("},");
                    }
                    sb = sb.Remove(sb.Length - 1, 1);
                }
                sb.Append("]");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                // Logger.WriteLog(ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// IList转Json
        /// </summary>
        /// <param name="dt">IList</param>
        /// <param name="dtName">json名</param>
        /// <returns></returns>
        public static string ListToJson<T>(IList list, string dtName)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{\"");
                sb.Append(dtName);
                sb.Append("\":[");
                if (list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        T obj = Activator.CreateInstance<T>();
                        PropertyInfo[] pi = obj.GetType().GetProperties();
                        sb.Append("{");
                        for (int j = 0; j < pi.Length; j++)
                        {
                            sb.Append("\"");
                            sb.Append(pi[j].Name.ToString());
                            sb.Append("\":\"");
                            if (pi[j].GetValue(list[i], null) != null && pi[j].GetValue(list[i], null) != DBNull.Value && pi[j].GetValue(list[i], null).ToString() != "")
                            {
                                sb.Append(pi[j].GetValue(list[i], null)).Replace("\\", "/");
                            }
                            else
                            {
                                sb.Append("");
                            }
                            sb.Append("\",");
                        }
                        sb = sb.Remove(sb.Length - 1, 1);
                        sb.Append("},");
                    }
                    sb = sb.Remove(sb.Length - 1, 1);
                }
                sb.Append("]}");
                return JsonCharFilter(sb.ToString());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion
        #region 对象-JSON序列化和反序列化
        public static T JsonToObject<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                //同时 下是System.Runtime.Serialization System.ServceModel.Web
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                return (T)serializer.ReadObject(ms);
            }
        }
        public static string ObjectToJson<T>(T obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
        #region JSON字符串反序列化成字典
        public static Dictionary<string, object> JsonToDic(string json)
        {
            JavaScriptSerializer Jss = new JavaScriptSerializer();
            Dictionary<string, object> dic = (Dictionary<string, object>)Jss.DeserializeObject(json);
            return dic;
        }
        #endregion
        #endregion
        #region DataTable-JSON序列化反序列化
        /// <summary>
        /// 序列化DataTable到json
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string SerializeDataTableToJson(DataTable dt)
        {
            if (dt == null)
                return null;
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
            ArrayList arrayList = new ArrayList();
            foreach (DataRow dataRow in dt.Rows)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    dictionary.Add(dataColumn.ColumnName, ToStr(dataRow[dataColumn.ColumnName]));
                }
                arrayList.Add(dictionary); //ArrayList集合中添加键值
            }

            return javaScriptSerializer.Serialize(arrayList);  //返回一个json字符串
        }
        /// <summary>
        /// json反序列化到DataTable
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static DataTable DeserializeJsonToDataTable(string json)
        {
            DataTable dataTable = new DataTable();  //实例化
            try
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
                ArrayList arrayList = javaScriptSerializer.Deserialize<ArrayList>(json);
                if (arrayList.Count > 0)
                {
                    foreach (Dictionary<string, object> dictionary in arrayList)
                    {
                        if (dictionary.Keys.Count == 0)
                        {
                            return null;
                        }
                        if (dataTable.Columns.Count == 0)
                        {
                            foreach (string current in dictionary.Keys)
                            {
                                dataTable.Columns.Add(current, dictionary[current].GetType());
                            }
                        }
                        DataRow dataRow = dataTable.NewRow();
                        foreach (string current in dictionary.Keys)
                        {
                            dataRow[current] = dictionary[current];
                        }

                        dataTable.Rows.Add(dataRow); //循环添加行到DataTable中
                    }
                }
            }
            catch
            {
                return null;
            }
            return dataTable;
        }
        private static string ToStr(object s, string format = "")
        {
            string result = "";
            try
            {
                if (format == "")
                {
                    result = s.ToString();
                }
                else
                {
                    result = string.Format("{0:" + format + "}", s);
                }
            }
            catch
            {
            }
            return result;
        }
        #endregion
        #endregion
        #region 基于Newtonsoft.Json的封装
        // 总结一些简单的高级用法
        //1.自定义序列化的字段名称 [JsonProperty(PropertyName = "CName")]
        //2. 忽略某些字段[JsonIgnore]
        //3.自定义类型转换 继承JsonConverter并重写 如[JsonConverter(typeof(BoolConvert))]

        /// <summary>
        /// 对象转换成json 基于Newtonsoft.Json的封装   converters: new IsoDateTimeConverter() { DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss" }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modle"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T model, params JsonConverter[] converters)
        {
            if (model == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(model, converters);
        }
        //返回流的序列化
        public static byte[] Serialize(object obj)
        {
            string json = SerializeObject(obj, new IsoDateTimeConverter() { DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss" });
            return Encoding.UTF8.GetBytes(json);
        }
        public static Object DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject(value);
        }
        public static T DeserializeObject<T>(string value)
        {
            if (value == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(value);
        }
        /// <summary>
        /// 将JSON字符串反序列化为一个JObject对象
        /// </summary>
        public static JObject DeserializeJObject(string json)
        {
            return JsonConvert.DeserializeObject(json) as JObject;
        }
        #endregion
        #region DataTable装换成json 第一个Web用的 直接eval解析
        /// <summary>
        /// datatable转json 第一个Web用的 直接eval解析
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>json数据</returns>      
        public static string DataTableToJson(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return "[]";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (DataRow dr in dt.Rows)
            {
                sb.Append("{");
                foreach (DataColumn dc in dr.Table.Columns)
                {
                    sb.Append("\"");
                    sb.Append(dc.ColumnName);
                    sb.Append("\":\"");
                    if (dr[dc] != null && dr[dc] != DBNull.Value && dr[dc].ToString() != "")
                        sb.Append(JsonCharFilter(dr[dc].ToString()));//特殊转移字符过滤
                    else
                        sb.Append("");
                    sb.Append("\",");
                }
                sb = sb.Remove(sb.Length - 1, 1);
                sb.Append("},");
            }
            sb = sb.Remove(sb.Length - 1, 1);
            sb.Append("]");
            return sb.ToString();

        }

        #endregion
        #region DataReader转换为Json
        /// <summary> 
        /// DataReader转换为Json 
        /// </summary> 
        /// <param name="dataReader">DataReader对象</param> 
        /// <returns>Json字符串</returns> 
        public static string DataReaderToJson(DbDataReader dataReader)
        {
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[");
            while (dataReader.Read())
            {
                jsonString.Append("{");
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    Type type = dataReader.GetFieldType(i);
                    string strKey = dataReader.GetName(i);
                    string strValue = dataReader[i].ToString();
                    jsonString.Append("\"" + strKey + "\":");
                    strValue = StringFormat(strValue, type);
                    if (i < dataReader.FieldCount - 1)
                    {
                        jsonString.Append(strValue + ",");
                    }
                    else
                    {
                        jsonString.Append(strValue);
                    }
                }
                jsonString.Append("},");
            }
            dataReader.Close();
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            return jsonString.ToString();
        }
        #endregion
    }
}
