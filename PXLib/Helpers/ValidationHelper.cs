using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Helpers
{
   public class ValidationHelper
    {
        
        public static void ArgumentNotNull(object value, string parameterNameDesc) 
        {
            if(value.IsNullEmpty())
            throw new ArgumentNullException(parameterNameDesc, "不能为空");
        }

    }
}
