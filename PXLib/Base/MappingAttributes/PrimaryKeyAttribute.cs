using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Attributes
{
    /// <summary>
    /// 主键字段属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PrimaryKeyAttribute : Attribute
    {
        private string _keyName;
        public PrimaryKeyAttribute()
        {
        }

        public PrimaryKeyAttribute(string keyName)
        {
            _keyName = keyName;
        }
    
        public virtual string KeyName { get { return _keyName; } set { _keyName = value; } }
    }
}
