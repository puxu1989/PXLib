using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Attributes
{
    //使用[EnumText("枚举描述文本")] 
    public class EnumText : Attribute
    {
        public string Description { get; set; }
        public EnumText(string value)
        {
            Description = value;
        }
    }
}
