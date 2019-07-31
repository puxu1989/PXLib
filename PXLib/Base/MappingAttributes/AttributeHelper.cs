using PXLib.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Attributes
{
   public static class AttributeHelper
    {
        #region 旧版属性读取 
        /// <summary>
        ///  获取实体对象表名 使用自定义的TableAttribute
        /// </summary>
        /// <returns></returns>
        public static string GetEntityTableName<T>()
        {
            Type entityType = typeof(T);
            string entityName = "";
            var tableAttribute = entityType.GetCustomAttributes(true).OfType<TableAttribute>();
            var descriptionAttributes = tableAttribute as TableAttribute[] ?? tableAttribute.ToArray();
            if (descriptionAttributes.Any())
                entityName = descriptionAttributes.ToList()[0].GetMappingName;//多个属性取第一个
            else
                entityName = entityType.Name;//否则就是Class名称
            return entityName;
        }
        /// <summary>
        /// 获取实体类名称 如果该类添加了DisplayName属性则返回该属性设置的名称
        /// </summary>
        /// <returns></returns>
        public static string GetClassName<T>()
        {
            Type type = typeof(T);
            string entityName = "";
            var busingessNames = type.GetCustomAttributes(true).OfType<DisplayNameAttribute>();
            var descriptionAttributes = busingessNames as DisplayNameAttribute[] ?? busingessNames.ToArray();
            if (descriptionAttributes.Any())
                entityName = descriptionAttributes.ToList()[0].DisplayName;
            else
                entityName = type.Name;
            return entityName;
        }
        /// <summary>
        ///  获取实体对象Key
        /// </summary>
        /// <returns></returns>
        public static string GetEntityPrimaryKey<T>()
        {
            Type type = typeof(T);
            foreach (PropertyInfo prop in type.GetProperties())
            {
                foreach (Attribute attr in prop.GetCustomAttributes(true))
                {
                    PrimaryKeyAttribute keyattribute = attr as PrimaryKeyAttribute;
                    if (keyattribute != null)
                    {
                        return prop.Name;
                    }
                }
            }
            throw new Exception("未对象设置设置[PrimaryKey]");
        }
        /// <summary>
        /// 获取实体类的属性名称 如果该类的字段添加了DisplayName属性则返回该字段属性设置的名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> GetFieldText<T>()
        {
            Type type = typeof(T);
            List<string> filedNames = new List<string>();
            foreach (var item in type.GetProperties())
            {
                var descAttrs = item.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                if (descAttrs.Any())
                {
                    var descAttr = descAttrs[0] as DisplayNameAttribute;
                    filedNames.Add(descAttr.DisplayName);
                }
                else
                {
                    filedNames.Add(item.Name);
                }           
            }
            return filedNames;
        }
        #endregion

        #region 新版属性读取
        /// <summary>
        /// 获取 自定义属性的共有方法
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string GetMappingAttributeName(this MemberInfo member)
        {
            if (member.IsDefined(typeof(BaseMappingAttribute), true))
            {
                var attr = member.GetCustomAttribute<BaseMappingAttribute>();
                return attr.GetMappingName;
            }
            else
            {
                return member.Name;
            }
        }
        #endregion
    }
}
