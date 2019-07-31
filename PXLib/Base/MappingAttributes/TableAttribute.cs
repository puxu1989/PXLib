using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Attributes
{
    // 摘要: 
    //     指定类将映射到的数据库表。
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : BaseMappingAttribute
    {
        public TableAttribute(string attrName) : base(attrName)
        {
        }
    }
}
