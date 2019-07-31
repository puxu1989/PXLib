
using PXLib.ObjectManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PXLib.Threading.Engines
{
    /// <summary>
    /// 假设我们的系统在运行的过程中，源源不断的有新的任务需要处理（比如订单处理），而且这些任务的处理是相互独立的，没有前后顺序依赖性（顺序依赖性是指，必须在任务A处理结束后才可开始B任务），那么我们就可以使用多个线程来同时处理多个任务。每个处理任务的线程称为“工作者（线程）”，其目的就是使用多个线程来并行处理任务，提高系统的吞吐能力。
    ///适用场合： 对于突发的大批量的任务（比如订单系统经常在其它时段接受的订单很少，但在某高峰期会有突发性的大量的订单进来）进行缓冲处理，并最大限度地利用现有资源进行处理。
    ///设计思路：使用一个队列来存放需要处理的任务，新来的任务都会排队到这个队列中，然后有N个工作者线程不断地从队列中取出任务去处理，每个线程处理完当前任务后，又从队列中取出下一个任务……，如此循环。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WorkerEngine<T> : ICycleEngineActor, IWorkerEngine<T>
    {
        private CycleEngine[] agileCycleEngines;

        private CircleQueue<T> queueOfWork;

        protected IWorkProcesser<T> workProcesser;

        private int workerThreadCount = 1;//工作者线程的数量。默认值为1。

        private int idleSpanInMSecs = 10;

        /// <summary>
        /// MaxWaitWorkCount 历史中最大的处于等待状态的任务数量。
        /// </summary>
        public int MaxWaitWorkCount
        {
            get
            {
                int result;
                if (this.queueOfWork == null)
                {
                    result = 0;
                }
                else
                {
                    result = this.queueOfWork.MaxCount;
                }
                return result;
            }
        }

        /// <summary>
        /// WorkProcesser 用于处理任务的处理器。
        /// </summary>
        public IWorkProcesser<T> WorkProcesser
        {
            set
            {
                this.workProcesser = value;
            }
        }

        /// <summary>
        /// WorkerThreadCount 工作者线程的数量。默认值为1。
        /// </summary>
        public int WorkerThreadCount
        {
            get
            {
                return this.workerThreadCount;
            }
            set
            {
                if (this.workerThreadCount < 1)
                {
                    throw new Exception("The number of worker must be > 0 !");
                }
                this.workerThreadCount = value;
            }
        }

        /// <summary>
        /// WorkCount 当前任务队列中的任务数。
        /// </summary>
        public int WorkCount
        {
            get
            {
                return this.queueOfWork.Count;
            }
        }

        /// <summary>
        /// IdleSpanInMSecs 当没有工作要处理时，工作者线程休息的时间间隔。默认为10ms
        /// </summary>
        public int IdleSpanInMSecs
        {
            get
            {
                return this.idleSpanInMSecs;
            }
            set
            {
                this.idleSpanInMSecs = value;
            }
        }
        /// <summary>
        /// 初始化 默认为10000
        /// </summary>
        public void Initialize()
        {
            this.Initialize(10000);
        }

        public void Initialize(int capacity)
        {
            this.queueOfWork = new CircleQueue<T>(capacity);
            this.agileCycleEngines = new CycleEngine[this.workerThreadCount];
            for (int i = 0; i < this.agileCycleEngines.Length; i++)
            {
                this.agileCycleEngines[i] = new CycleEngine(this);
                this.agileCycleEngines[i].DetectSpanInSecs = 0;
            }
        }

        public void Start()
        {
            CycleEngine[] array = this.agileCycleEngines;
            for (int i = 0; i < array.Length; i++)
            {
                CycleEngine agileCycleEngine = array[i];
                agileCycleEngine.Start();
            }
        }

        public void Stop()
        {
            CycleEngine[] array = this.agileCycleEngines;
            for (int i = 0; i < array.Length; i++)
            {
                CycleEngine agileCycleEngine = array[i];
                agileCycleEngine.Stop();
            }
        }

        /// <summary>
        /// AddWork 添加任务。
        /// </summary>
        public void AddWork(T work)
        {
            this.queueOfWork.Enqueue(work);
        }

        private void DoWork()
        {
            T work = default(T);
            if (this.queueOfWork.Dequeue(out work))
            {
                this.workProcesser.Process(work);
            }
            else
            {
                Thread.Sleep(this.idleSpanInMSecs);
            }
        }

        public bool EngineAction()
        {
            this.DoWork();
            return true;
        }
    }
}
