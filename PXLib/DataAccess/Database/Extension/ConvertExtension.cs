using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PXLib.DataAccess.Database.Extension
{
    /// <summary>
    /// 数据转换
    /// </summary>
    public class ConvertExtension
    {
        /// <summary>
        /// 将DataReader数据转为Dynamic对象
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static dynamic DataFillDynamic(IDataReader reader)
        {
            using (reader)
            {
                dynamic d = new ExpandoObject();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    try
                    {
                        ((IDictionary<string, object>)d).Add(reader.GetName(i), reader.GetValue(i));
                    }
                    catch
                    {
                        ((IDictionary<string, object>)d).Add(reader.GetName(i), null);
                    }
                }
                return d;
            }
        }
        /// <summary>
        /// 获取模型对象集合
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<dynamic> DataFillDynamicList(IDataReader reader)
        {
            using (reader)
            {
                List<dynamic> list = new List<dynamic>();
                if (reader != null && !reader.IsClosed)
                {
                    while (reader.Read())
                    {
                        list.Add(DataFillDynamic(reader));
                    }
                    reader.Close();
                    reader.Dispose();
                }
                return list;
            }
        }
        /// <summary>
        /// 将IDataReader转换为 集合 使用了using
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<T> IDataReaderToList<T>(IDataReader reader)//与DataConvertDReadToList重复 暂不处理
        {
            if (reader == null)
            {
                return null;
            }
            using (reader)
            {
                List<string> field = new List<string>(reader.FieldCount);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    field.Add(reader.GetName(i).ToLower());
                }
                List<T> list = new List<T>();
                while (reader.Read())
                {
                    T model = Activator.CreateInstance<T>();
                    foreach (PropertyInfo property in model.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (field.Contains(property.Name.ToLower()))
                        {
                            if (!IsNullOrDBNull(reader[property.Name]))
                            {
                                property.SetValue(model, HackType(reader[property.Name], property.PropertyType), null);
                            }
                        }
                    }
                    list.Add(model);
                }
                reader.Close();
                reader.Dispose();//using了就没有必要加了 加了就立即释放了
                return list;
            }
        }
        /// <summary>
        ///  将IDataReader转换为DataTable
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static DataTable IDataReaderToDataTable(IDataReader reader)
        {
            if (reader == null)
                return null;
            using (reader)
            {

                DataTable objDataTable = new DataTable("Table");
                int intFieldCount = reader.FieldCount;
                for (int intCounter = 0; intCounter < intFieldCount; ++intCounter)
                {
                    objDataTable.Columns.Add(reader.GetName(intCounter), reader.GetFieldType(intCounter));//小写
                }
                objDataTable.BeginLoadData();
                object[] objValues = new object[intFieldCount];
                while (reader.Read())
                {
                    reader.GetValues(objValues);
                    objDataTable.LoadDataRow(objValues, true);
                }
                reader.Close();
                reader.Dispose();
                objDataTable.EndLoadData();
                return objDataTable;
            }
        }
        /// <summary>
        /// 将IDataReader转换为 哈希表
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static Hashtable ReaderToHashtable(IDataReader dr)
        {
            using (dr)
            {
                Hashtable ht = new Hashtable();
                while (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        string strfield = dr.GetName(i).ToLower();
                        ht[strfield] = dr[strfield];
                    }
                }
                dr.Close();
                return ht;
            }
        }

        /// <summary>
        /// 获取实体类键值（缓存）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Hashtable GetPropertyInfo<T>(T entity)
        {
            Type type = entity.GetType();
            //object CacheEntity = CacheHelper.GetCache("CacheEntity_" + EntityAttribute.GetEntityTable<T>());
            object CacheEntity = null;
            if (CacheEntity == null)
            {
                Hashtable ht = new Hashtable();
                PropertyInfo[] props = type.GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    string name = prop.Name;
                    object value = prop.GetValue(entity, null);
                    ht[name] = value;
                }
                //CacheHelper.SetCache("CacheEntity_" + EntityAttribute.GetEntityTable<T>(), ht);
                return ht;
            }
            else
            {
                return (Hashtable)CacheEntity;
            }
        }
        //这个类对可空类型进行判断转换，要不然会报错
        public static object HackType(object value, Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return null;
                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            if (conversionType.IsEnum)
            {
                return Enum.ToObject(conversionType, value);
            }
            return Convert.ChangeType(value, conversionType);
        }
        public static bool IsNullOrDBNull(object obj)
        {
            return ((obj is DBNull) || string.IsNullOrEmpty(obj.ToString())) ? true : false;
        }
    }
}
