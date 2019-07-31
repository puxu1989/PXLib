using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Threading.Engines
{
    /// <summary>
    /// 通过组合使用的循环引擎 sealed:此类不能不继承
    /// 有些系统需要每隔一段时间就执行一下某个动作，比如，一个监控系统每隔10秒钟就要检测一下被监控对象的状态是否正常，那这时我们就可以用到循环引擎了
    /// 使用注意 此引擎只有一个线程 不是精准的定时任务 比如它会隔10秒执行一个Action，执行完后再隔10秒再执行Action。间隔时间的等待与Action的执行都是在同一个线程中处理的
    /// 与Timer的区别 Timer每次定时事件触发都会用到一个线程，如果定时的时间间隔小于事件处理的时间，则后台线程池中将会有越来越多的线程被Timer使用掉，直至线程池中再无空闲的线程。
    /// </summary>
    public sealed class CycleEngine : BaseCycleEngine
    {
        private ICycleEngineActor engineActor;
        /// <summary>
        /// 循环引擎
        /// </summary>
        /// <param name="_engineActor"></param>
        public CycleEngine(ICycleEngineActor _engineActor)
        {
            this.engineActor = _engineActor;
        }

        protected override bool DoDetect()
        {
            return this.engineActor.EngineAction();
        }
    }
}
