using PXLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib
{
   public enum OperationType
    {
       /// <summary>
        /// 其他
        /// </summary>
        [EnumText("其他")]
        Other = 0,
        /// <summary>
        /// 登陆
        /// </summary>
        [EnumText("登录")]
        Login = 1,
        /// <summary>
        /// 退出登录
        /// </summary>
        [EnumText("退出")]
        Exit = 2,
        /// <summary>
        /// 访问
        /// </summary>
        [EnumText("访问")]
        Visit = 3,
        /// <summary>
        /// 离开
        /// </summary>
        [EnumText("离开")]
        Leave = 4,
        /// <summary>
        /// 新增
        /// </summary>
        [EnumText("新增")]
        Create = 5,
        /// <summary>
        /// 删除
        /// </summary>
        [EnumText("删除")]
        Delete = 6,
        /// <summary>
        /// 修改
        /// </summary>
        [EnumText("修改")]
        Update = 7,
        /// <summary>
        /// 提交
        /// </summary>
        [EnumText("提交")]
        Submit = 8,
        /// <summary>
        /// 异常
        /// </summary>
        [EnumText("异常")]
        Exception = 9,
        /// <summary>
        /// 异常
        /// </summary>
        [EnumText("移动登录")]
        AppLogin = 10,
    }
}
