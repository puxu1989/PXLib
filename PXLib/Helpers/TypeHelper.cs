using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PXLib.Helpers
{
    public class TypeHelper
    {
        #region 基本类型判断
        /// <summary>
        /// IsSimpleType 是否为简单类型：数值、字符、字符串、日期、布尔、枚举、Type
        /// </summary>      
        public static bool IsSimpleType(Type t)
        {
            return TypeHelper.IsNumbericType(t) || t == typeof(char) || t == typeof(string) || t == typeof(bool) || t == typeof(DateTime) || t == typeof(Type) || t.IsEnum;
        }

        public static bool IsNumbericType(Type destDataType)
        {
            return destDataType == typeof(int) || destDataType == typeof(uint) || destDataType == typeof(double) || destDataType == typeof(short) || destDataType == typeof(ushort) || destDataType == typeof(decimal) || destDataType == typeof(long) || destDataType == typeof(ulong) || destDataType == typeof(float) || destDataType == typeof(byte) || destDataType == typeof(sbyte);
        }

        public static bool IsIntegerCompatibleType(Type destDataType)
        {
            return destDataType == typeof(int) || destDataType == typeof(uint) || destDataType == typeof(short) || destDataType == typeof(ushort) || destDataType == typeof(long) || destDataType == typeof(ulong) || destDataType == typeof(byte) || destDataType == typeof(sbyte);
        }
        /// <summary>
        /// GetClassSimpleName 获取class的声明名称，如 Person
        /// </summary>      
        #endregion
        public static string GetClassSimpleName(Type t)
        {
            string[] array = t.ToString().Split(new char[]
			{
				'.'
			});
            return array[array.Length - 1].ToString();
        }
        /// <summary>
        /// 获取命名空间路径返回字符串 如PXlib.Heplers.ClassName,ClassName
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetTypeFullName(Type t)
        {
            return t.FullName + "," + t.Assembly.FullName.Split(new char[]
			{
				','
			})[0];
        }
        ///<summary>
        ///根据字符串命名空间获取类型
        ///</summary>       
        public static Type GetTypeByRegularName(string typeAndAssName)
        {
            string[] array = typeAndAssName.Split(new char[] { ',' });
            Type type;
            if (array.Length < 2)
            {
                type = Type.GetType(typeAndAssName);
            }
            else
            {
                type = GetType(array[0].Trim(), array[1].Trim());
            }
            return type;
        }
        /// <summary>
        /// 获取个类型的默认值
        /// </summary>
        /// <param name="destType"></param>
        /// <returns></returns>
        public static object GetDefaultValue(Type destType)
        {
            object result;
            if (TypeHelper.IsNumbericType(destType))
            {
                result = 0;
            }
            else if (destType == typeof(string))
            {
                result = "";
            }
            else if (destType == typeof(bool))
            {
                result = false;
            }
            else if (destType == typeof(DateTime))
            {
                result = DateTime.Now;
            }
            else if (destType == typeof(Guid))
            {
                result = Guid.NewGuid();
            }
            else if (destType == typeof(TimeSpan))
            {
                result = TimeSpan.Zero;
            }
            else
            {
                result = null;
            }
            return result;
        }
        /// <summary>
        /// ChangeType 对System.Convert.ChangeType进行了增强，支持(0,1)到bool的转换,枚举、字符串
        /// </summary>       
        public static object ChangeType(Type targetType, object val)
        {
            object result;
            if (val == null)
            {
                result = null;
            }
            else if (targetType.IsAssignableFrom(val.GetType()))
            {
                result = val;
            }
            else if (targetType == val.GetType())
            {
                result = val;
            }
            else
            {
                if (targetType == typeof(bool))
                {
                    if (val.ToString() == "0")
                    {
                        result = false;
                        return result;
                    }
                    if (val.ToString() == "1")
                    {
                        result = true;
                        return result;
                    }
                }
                if (targetType.IsEnum)
                {
                    int num = 0;
                    if (!int.TryParse(val.ToString(), out num))
                    {
                        result = Enum.Parse(targetType, val.ToString());
                    }
                    else
                    {
                        result = val;
                    }
                }
                else if (targetType == typeof(Type))
                {
                    result = GetTypeByRegularName(val.ToString());
                }
                else if (targetType == typeof(IComparable))
                {
                    result = val;
                }
                else
                {
                    result = Convert.ChangeType(val, targetType);
                }
            }
            return result;
        }
        /// <summary>
        /// GetType  通过完全限定的类型名来加载对应的类型。typeAndAssName如"ESBasic.Filters.SourceFilter,ESBasic"。
        /// 如果为系统简单类型，则可以不带程序集名称。
        /// </summary>       
        public static Type GetType(string typeAndAssName)
        {
            string[] array = typeAndAssName.Split(new char[]
	{
		','
	});
            Type type;
            if (array.Length < 2)
            {
                type = Type.GetType(typeAndAssName);
            }
            else
            {
                type = GetType(array[0].Trim(), array[1].Trim());
            }
            return type;
        }

        /// <summary>
        /// GetType 加载assemblyName程序集中的名为typeFullName的类型。assemblyName不用带扩展名，如果目标类型在当前程序集中，assemblyName传入null	
        /// </summary>		
        public static Type GetType(string typeFullName, string assemblyName)
        {
            Type result;
            if (assemblyName == null)
            {
                result = Type.GetType(typeFullName);
            }
            else
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                Assembly[] array = assemblies;
                for (int i = 0; i < array.Length; i++)
                {
                    Assembly assembly = array[i];
                    string[] array2 = assembly.FullName.Split(new char[] { ',' });
                    if (array2[0].Trim() == assemblyName.Trim())
                    {
                        result = assembly.GetType(typeFullName);
                        return result;
                    }
                }
                Assembly assembly2 = Assembly.Load(assemblyName);
                if (assembly2 != null)
                {
                    result = assembly2.GetType(typeFullName);
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        #region 设置获取属性
        /// <summary>
        /// SetProperty 如果list中的object具有指定的propertyName属性，则设置该属性的值为proValue
        /// </summary>		
        public static void SetProperty(IList<object> objs, string propertyName, object proValue)
        {
            object[] array = new object[]
			{
				proValue
			};
            foreach (object current in objs)
            {
                SetProperty(current, propertyName, proValue);
            }
        }
        /// <summary>
        /// 此方法是设置公有属性的（get;set)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="proValue"></param>
        public static void SetProperty(object obj, string propertyName, object proValue)
        {
            SetProperty(obj, propertyName, proValue, true);
        }

        /// <summary>
        /// SetProperty 如果object具有指定的propertyName属性，则设置该属性的值为proValue
        /// </summary>		
        public static void SetProperty(object obj, string propertyName, object proValue, bool ignoreError)
        {
            Type type = obj.GetType();
            PropertyInfo property = type.GetProperty(propertyName);
            if (property == null || !property.CanWrite)
            {
                if (!ignoreError)
                {
                    string message = string.Format("The setter of property named '{0}' not found in '{1}'.", propertyName, type);
                    throw new Exception(message);
                }
            }
            else
            {
                try
                {
                    proValue = TypeHelper.ChangeType(property.PropertyType, proValue);
                }
                catch
                {
                }
                object[] args = new object[]
				{
					proValue
				};
                type.InvokeMember(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, null, obj, args);
            }
        }

        /// <summary>
        /// GetProperty 根据指定的属性名获取目标对象该属性的值
        /// </summary>
        public static object GetProperty(object obj, string propertyName)
        {
            Type type = obj.GetType();
            return type.InvokeMember(propertyName, BindingFlags.GetProperty, null, obj, null);
        }

        /// <summary>
        /// GetFieldValue 取得目标对象的指定field的值，field可以是private
        /// </summary>      
        public static object GetFieldValue(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
            if (field == null)
            {
                string message = string.Format("The field named '{0}' not found in '{1}'.", fieldName, type);
                throw new Exception(message);
            }
            return field.GetValue(obj);
        }

        /// <summary>
        /// SetFieldValue 设置目标对象的指定field的值，field可以是private （直接可以设置私有字段）
        /// </summary>      
        public static void SetFieldValue(object obj, string fieldName, object val)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField);
            if (field == null)
            {
                string message = string.Format("The field named '{0}' not found in '{1}'.", fieldName, type);
                throw new Exception(message);
            }
            field.SetValue(obj, val);
        }
        #endregion
        #region 获取接口的所有方法 包括继承的
        /// <summary>
        /// GetAllMethods 获取接口的所有方法信息，包括继承的
        /// </summary>       
        public static IList<MethodInfo> GetAllMethods(params Type[] interfaceTypes)
        {
            for (int i = 0; i < interfaceTypes.Length; i++)
            {
                Type type = interfaceTypes[i];
                if (!type.IsInterface)
                {
                    throw new Exception("Target Type must be interface!");
                }
            }
            IList<MethodInfo> result = new List<MethodInfo>();
            for (int i = 0; i < interfaceTypes.Length; i++)
            {
                Type type = interfaceTypes[i];
                DistillMethods(type, ref result);
            }
            return result;
        }

        private static void DistillMethods(Type interfaceType, ref IList<MethodInfo> methodList)
        {
            MethodInfo[] methods = interfaceType.GetMethods();
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo methodInfo = methods[i];
                bool flag = false;
                foreach (MethodInfo current in methodList)
                {
                    if (current.Name == methodInfo.Name && current.ReturnType == methodInfo.ReturnType)
                    {
                        ParameterInfo[] parameters = current.GetParameters();
                        ParameterInfo[] parameters2 = methodInfo.GetParameters();
                        if (parameters.Length == parameters2.Length)
                        {
                            bool flag2 = true;
                            for (int j = 0; j < parameters.Length; j++)
                            {
                                if (parameters[j].ParameterType != parameters2[j].ParameterType)
                                {
                                    flag2 = false;
                                }
                            }
                            if (flag2)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
                if (!flag)
                {
                    methodList.Add(methodInfo);
                }
            }
            Type[] interfaces = interfaceType.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                Type interfaceType2 = interfaces[i];
                DistillMethods(interfaceType2, ref methodList);
            }
        }
        #endregion
    }
}
