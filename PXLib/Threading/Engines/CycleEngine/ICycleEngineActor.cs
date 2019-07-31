using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Threading.Engines
{
    /// <summary>
    /// 要循环执行某个动作的对象必须实现该接口
    /// </summary>
    public interface ICycleEngineActor
    {
        /// <summary>
        /// IEngineCycleActor 执行引擎动作，返回false表示停止引擎。
        /// 注意，该方法不能抛出异常，否则会导致引擎停止运行（循环线程遭遇异常退出）。
        /// </summary>       
        bool EngineAction();
    }
}
