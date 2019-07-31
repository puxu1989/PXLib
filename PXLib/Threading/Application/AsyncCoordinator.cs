using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PXLib.Threading.Application
{
    /// <summary>
    /// 乐观并发模型的协调类  异步执行模块
    /// </summary>
    public class AsyncCoordinator
    {
        private Action handler = null;
        /// <summary>
        /// 传入隔离执行的方法
        /// </summary>
        /// <param name="operater">隔离执行的方法</param>
        public AsyncCoordinator(Action operater)
        {
            handler = operater;
        }
        /// <summary>
        /// 操作状态，0是未操作，1是操作中
        /// </summary>
        private int OperaterStatus = 0;
        /// <summary>
        /// 需要操作的次数
        /// </summary>
        private long Target = 0;
        /// <summary>
        /// 启动线程池执行隔离方法
        /// </summary>
        public void StartHandlerOperater()
        {
            Interlocked.Increment(ref Target);
            if (Interlocked.CompareExchange(ref OperaterStatus, 1, 0) == 0)
            {
                //启动保存
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolOperater), null);
            }
        }
        private void ThreadPoolOperater(object obj)
        {
            long currentVal = Target, startVal;
            long desiredVal = 0;
            do
            {
                startVal = currentVal;//设置值
                //以下为业务逻辑，允许实现非常复杂的设置
                if (handler != null)
                    handler.Invoke();
                //需要清零值的时候必须用下面的原子操作
                currentVal = Interlocked.CompareExchange(ref Target, desiredVal, startVal);
            }
            while (startVal != currentVal);//更改失败就强制更新

            //退出保存状态
            Interlocked.Exchange(ref OperaterStatus, 0);
            //最终状态确认
            if (Target != desiredVal)
                StartHandlerOperater();
        }
    }
}
