using PXLib.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PXLib.Helpers
{
    public class DataConvertHelper
    {
        public static List<T> ReaderToList<T>(IDataReader dr)
        {
            using (dr)
            {
                List<string> field = new List<string>(dr.FieldCount);
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    field.Add(dr.GetName(i).ToLower());
                }
                List<T> list = new List<T>();
                while (dr.Read())
                {
                    T model = Activator.CreateInstance<T>();
                    foreach (PropertyInfo property in model.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (field.Contains(property.Name.ToLower()))
                        {
                            if (!Object.Equals(dr[property.Name], DBNull.Value) || !string.IsNullOrEmpty(dr[property.Name].ToString()))
                            {
                                property.SetValue(model, ChangeType(dr[property.Name], property.PropertyType), null);
                            }
                        }
                    }
                    list.Add(model);
                }
                return list;
            }
        }
        /// <summary>
        /// DataTable 转 对象要IList
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>行中是对象的类，类的属性与数据字段一致</returns>
        public static IList DataTableToIList<T>(DataTable dt)
        {
            // 定义集合    
            IList list = new List<T>();
            // 获得此模型的类型    
            // Type type = typeof(T);
            string tempName;
            foreach (DataRow dr in dt.Rows)
            {
                //T t = new T();
                T obj = Activator.CreateInstance<T>();
                // 获得此模型的公共属性    
                PropertyInfo[] propertys = obj.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;
                    // 检查DataTable是否包含此列    
                    if (dt.Columns.Contains(tempName))
                    {
                        // 判断此属性是否有Setter    
                        if (!pi.CanWrite) continue;
                        object value = dr[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(obj, value, null);
                    }
                }
                list.Add(obj);
            }
            return list;
        }
        /// <summary>
        /// 将泛类型集合List类转换成DataTable
        /// </summary>
        /// <param name="list">泛类型集合</param>
        /// <returns></returns>
        public static DataTable ListToDataTable<T>(List<T> entitys)
        {
            //检查实体集合不能为空
            if (entitys == null || entitys.Count < 1)
            {
                throw new Exception("需转换的集合为空");
            }
            //取出第一个实体的所有Propertie
            Type entityType = entitys[0].GetType();
            PropertyInfo[] entityProperties = entityType.GetProperties();

            //生成DataTable的structure
            //生产代码中，应将生成的DataTable结构Cache起来，此处略
            DataTable dt = new DataTable();
            for (int i = 0; i < entityProperties.Length; i++)
            {
                //dt.Columns.Add(entityProperties[i].Name, entityProperties[i].PropertyType);
                dt.Columns.Add(entityProperties[i].Name);
            }
            //将所有entity添加到DataTable中
            foreach (object entity in entitys)
            {
                //检查所有的的实体都为同一类型
                if (entity.GetType() != entityType)
                {
                    throw new Exception("要转换的集合元素类型不一致");
                }
                object[] entityValues = new object[entityProperties.Length];
                for (int i = 0; i < entityProperties.Length; i++)
                {
                    entityValues[i] = entityProperties[i].GetValue(entity, null);
                }
                dt.Rows.Add(entityValues);
            }
            return dt;
        }
        /// <summary>
        /// DataTable转换成hashTable 键名小写
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static Hashtable DataTableToHashTable(DataTable dt)
        {
            try
            {
                DataRow row = dt.Rows[0];
                Hashtable hash = new Hashtable();
                for (int i = 0; i < row.Table.Columns.Count; i++)
                {
                    hash.Add(row.Table.Columns[i].ColumnName, Convert.ToString(row[i]));
                }
                return hash;

            }
            catch
            {
                return null;
            }


        }
        public static Hashtable ModelToHashtable<T>(T model)
        {
            Hashtable hashtable = new Hashtable();

            T obj = Activator.CreateInstance<T>();
            Type type = obj.GetType();
            PropertyInfo[] pis = type.GetProperties();
            for (int j = 0; j < pis.Length; j++)
            {
                hashtable[pis[j].Name.ToString()] = pis[j].GetValue(model, null);

            }
            return hashtable;
        }
        /// <summary>
        /// IList转换成List （用途：IList直接包含了模型列表，List有更多方法，如FinfAll(t => t.Name == Name)）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> IListToList<T>(IList list)
        {
            T[] array = new T[list.Count];
            list.CopyTo(array, 0);
            return new List<T>(array);
        }
        public static T DataRowToModel<T>(DataRow dr)//将DataRow转换为 实体类
        {
            T model = Activator.CreateInstance<T>();
            foreach (PropertyInfo pi in model.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
            {
                string piName = pi.Name;
                if (dr.Table.Columns.Contains(piName) && dr[piName] != null)
                {
                    if (!(dr[piName] is DBNull) || !string.IsNullOrEmpty(dr[piName].ToString()))
                    {
                        pi.SetValue(model, ChangeType(dr[piName], pi.PropertyType), null);
                    }
                }
            }
            return model;
        }
        //这个类对可空类型进行判断转换，要不然会报错
        public static object ChangeType(object value, Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null||value.ToString()=="")
                    return null;
                System.ComponentModel.NullableConverter nullableConverter = new System.ComponentModel.NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            return Convert.ChangeType(value, conversionType);
        }
        #region 结构体和byte之间转换
        /// <summary>
        /// 由结构体转换为byte数组
        /// </summary>
        public static byte[] StructureToByte<T>(T structure)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            IntPtr bufferIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, bufferIntPtr, true);
                Marshal.Copy(bufferIntPtr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferIntPtr);
            }
            return buffer;
        }

        /// <summary>
        /// 由byte数组转换为结构体
        /// </summary>
        public static T ByteToStructure<T>(byte[] dataBuffer)
        {
            object structure = null;
            int size = dataBuffer.Length;
            IntPtr allocIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(dataBuffer, 0, allocIntPtr, size);
                structure = Marshal.PtrToStructure(allocIntPtr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(allocIntPtr);
            }
            return (T)structure;
        }
        #endregion

        /// <summary>
        /// 枚举描述转换成IList 将Enum的所有枚举值放到IList中，以绑定到如ComoboBox等控件
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns>返回的描述字符串数组 如["第一","第二","第三"]</returns>
        public static IList<string> ConvertEnumToList(Type enumType)
        {
            IList<string> list = new List<string>();
            FieldInfo[] array =  enumType.GetFields();
            for (int i = 0; i < array.Length; i++)
            {
                FieldInfo fieldInfo = array[i];
                object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumText), false);
                if (customAttributes != null && customAttributes.Length > 0)
                {
                    EnumText enumDescription = (EnumText)customAttributes[0];
                    list.Add(enumDescription.Description);
                }
                else if (fieldInfo.Name != "value__")
                {
                    list.Add(fieldInfo.Name);
                }
            }
            return list;
        }
        /// <summary>  
        /// 枚举转字典集合  
        /// </summary>  
        /// <typeparam name="T">枚举类名称</typeparam>  
        /// <param name="keyDefault">默认key值</param>  
        /// <param name="valueDefault">默认value值</param>  
        /// <returns>返回生成的字典集合</returns>  
        public static Dictionary<string, object> EnumListToDic(Type enumType, string keyDefault = "", string valueDefault = "")
        {
            
            //MVC应用 ViewBag.dropList = new SelectList(dropDic,"value","key");  
            //@Html.DropDownList("dropList", null, new { })      //dropList与 ViewBag.dropList对应自动装载
            //Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new InvalidOperationException("转换对象必须是Enum");
            Dictionary<string, object> dicEnum = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(keyDefault)) //判断是否添加默认选项  
            {
                dicEnum.Add(keyDefault, valueDefault);
            }
            string[] fieldstrs = Enum.GetNames(enumType); //获取枚举字段数组  
            foreach (var item in fieldstrs)
            {
                string description = string.Empty;
                var field = enumType.GetField(item);
                object[] arr = field.GetCustomAttributes(typeof(EnumText), true); //获取属性字段数组  
                if (arr != null && arr.Length > 0)
                {
                    description = ((EnumText)arr[0]).Description;   //属性描述  
                }
                else
                {
                    description = item;  //描述不存在取字段名称  
                }
                dicEnum.Add(description, (int)Enum.Parse(enumType, item));  //不用枚举的value值作为字典key值的原因从枚举例子能看出来，其实这边应该判断他的值不存在，默认取字段名称  
            }
            return dicEnum;
        }  
    }
}