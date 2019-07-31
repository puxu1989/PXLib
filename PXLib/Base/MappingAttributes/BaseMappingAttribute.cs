using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Attributes
{
   public abstract class BaseMappingAttribute:Attribute
    {
        private string _attrName;
        public BaseMappingAttribute(string attrName)
        {
            this._attrName = attrName;
        }
        /// <summary>
        /// 获取设置属性的参数名称
        /// </summary>
        /// <returns></returns>
        public string GetMappingName { get { return _attrName; } }

    }
}
