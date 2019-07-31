using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib
{
    /// <summary>
    /// 表示Ajax操作结果 
    /// </summary>
    public class BaseAjaxResult
    {
        /// <summary>
        /// 获取 Ajax操作结果编码
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        /// 获取 消息内容
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 获取 返回数据
        /// </summary>
        public object resdata { get; set; }
    }
    
}
