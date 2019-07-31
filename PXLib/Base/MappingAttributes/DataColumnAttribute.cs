using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Attributes
{
   public class DataColumnAttribute:BaseMappingAttribute
    {
        //属性的映射 父类抽象
        public DataColumnAttribute(string attrName) : base(attrName)
        {
        }
    }
}
