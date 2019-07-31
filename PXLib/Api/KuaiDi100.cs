using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PXLib.Helpers;

namespace PXLib.Api
{
   public class KuaiDi100
    {
        public static string Query(string key,string nu,string com)
        {
            string url = "http://api.kuaidi100.com/api?id={0}&com={1}&nu={2}&show=0&valicode=无意义&muti=1]&order=desc".FormatWith(key,nu,com);
            return RequestHelper.GetWebRequest(url);
        }
    }
}
