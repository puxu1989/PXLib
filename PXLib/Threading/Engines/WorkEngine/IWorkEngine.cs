using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Threading.Engines
{
    /// <summary>
    /// IWorkerEngine 工作者引擎。用于在后台使用多线程不间断、连续地处理任务。
    /// 一群工作者线程轮流从工作队列中取出工作进行处理，模仿完成端口的机制。
    /// 实现这个IWorkerEngine接口的时候要注意以下几点：
    ///（1）AddWork方法会在多线程的环境中被调用，所以必须保证其是线程安全的。
    ///（2）每个工作者线程实际上就是一个我们前面介绍的循环引擎ICycleEngine，只不过将其DetectSpanInSecs设为0即可，表示不间断地执行任务。WorkerEngine便是使用了N个AgileCycleEngine实例来作为工作者的。这些AgileCycleEngine实例在Initialize方法中被实例化。
    ///（3）所有的工作者最终都是执行私有的DoWork方法，这个方法就是从任务队列中取出任务并且调用IWorkProcesser来处理任务，如果任务队列为空，则等待IdleSpanInMSecs秒钟后再重试。
    ///（4）MaxWaitWorkCount属性用于记录自从引擎运行以来最大的等待任务的数量，通过这个属性我们可以推测任务量与任务处理速度之间的差距。
    ///（5）通过Start、Stop方法我们可以随时停止、启动工作者引擎，并可重复调用。
    /// </summary>    
    public interface IWorkerEngine<T>
    {
        /// <summary>
        /// IdleSpanInMSecs 当没有工作要处理时，工作者线程休息的时间间隔。默认为10ms
        /// </summary>
        int IdleSpanInMSecs
        {
            get;
            set;
        }

        /// <summary>
        /// WorkerThreadCount 工作者线程的数量。默认值为1。
        /// </summary>
        int WorkerThreadCount
        {
            get;
            set;
        }

        /// <summary>
        /// WorkProcesser 用于处理任务的处理器。
        /// </summary>
        IWorkProcesser<T> WorkProcesser
        {
            set;
        }

        /// <summary>
        /// WorkCount 当前任务队列中的任务数。
        /// </summary>
        int WorkCount
        {
            get;
        }

        /// <summary>
        /// MaxWaitWorkCount 历史中最大的处于等待状态的任务数量。
        /// </summary>
        int MaxWaitWorkCount
        {
            get;
        }

        void Initialize();

        void Start();

        void Stop();

        /// <summary>
        /// AddWork 添加任务。
        /// </summary>       
        void AddWork(T work);
    }
    /// <summary>
    /// IWorkProcesser 任务处理器。由于任务的类型不是固定的，所以我们使用的泛型参数T来表示要处理任务的类型。有的任务的具体执行都是由IWorkProcesser完成的：    
    /// </summary>    
    public interface IWorkProcesser<T>
    {
        void Process(T work);
        // public void Process(T Work)
        //{
        //    MethodInfo mi = Work.GetType().GetMethod("ThreadFunc");
        //    mi.Invoke(Work, null);
        //}
    }
}
