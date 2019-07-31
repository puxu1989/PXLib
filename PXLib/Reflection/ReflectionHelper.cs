using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Reflection
{
    /// <summary>
    /// 反射帮助类
    /// </summary>
   public class ReflectionHelper
    {

        /*
            TypeOf() 和GetType()的区别:  
            (1)TypeOf():运算符 得到一个Class的Type
            (2)GetType():得到一个Class的实例的Type
             */
        public static bool IsNullableType(Type t)
        {
            ValidationHelper.ArgumentNotNull(t, "Type");
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
