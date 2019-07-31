using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace PXLib.Helpers
{
    /// <summary>
    /// 执行js脚本帮助类
    /// </summary>
   public class ExecuteScriptHelper
    {
        /// <summary>
        /// 后台调用JS函数
        /// </summary>
        /// <param name="obj"></param>
        public static void ShowScript(string strobj)
        {
            Page p = HttpContext.Current.Handler as Page;
            p.ClientScript.RegisterStartupScript(p.ClientScript.GetType(), "myscript", "<script>" + strobj + "</script>");
        }
        public static void ExecuteScript(string scriptBody)
        {
            Page p = HttpContext.Current.Handler as Page;
            p.ClientScript.RegisterStartupScript(typeof(string), "SomeKey", scriptBody, true);
        }
    }
}
