using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.ObjectManage.PriorityManager
{
    /// <summary>
    /// IPriorityObject 具有优先级的对象的接口。
    /// </summary>
    public interface IPriorityObject
    {
        /// <summary>
        /// 优先级别是用int表示的，其值是从0开始连续的一串整数，整数值越小，表明优先级越高
        /// </summary>
        int PriorityLevel { get; }
    }
}
