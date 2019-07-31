using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PXLib.Threading.Engines
{
    /// <summary>
    /// BaseCycleEngine ICycleEngine的抽象实现，循环引擎直接继承它并实现DoDetect方法即可。
    ///    关于BaseCycleEngine的实现要注意以下几点：
    ///（1）循环引擎是在后台线程池的某个线程上运行的。
    ///（2）循环引擎可以无限次的启动、停止、启动、停止……
    ///（3）为了保证调用Stop方法时能迅速地停止引擎，我将间隔时间划分为多个BaseCycleEngine.SleepTime。而不是一次性地Sleep间隔时间。
    ///（4）为了保证循环引擎真正停止后，才返回Stop方法的调用，我使用了ManualResetEvent来进行控制。
    ///（5）DoDetect方法的返回值为false，则表示在该Action执行完后将停止循环引擎。此后，可以重新调用Start方法再次启动循环引擎。
    /// </summary>
    public abstract class BaseCycleEngine : ICycleEngine
    {
        private const int SleepTime = 1000;//默认时间ms

        private volatile bool isStop = true;
        //volatile多用于多线程的环境，当一个变量定义为volatile时，读取这个变量的值时候每次都是从momery里面读取而不是从cache读。这样做是为了保证读取该变量的信息都是最新的，而无论其他线程如何更新这个变量。

        private ManualResetEvent manualResetEvent4Stop = new ManualResetEvent(false);

        private int totalSleepCount = 0;

        private int detectSpanInSecs = 0;


        /// <summary>
        /// 监测时间 执行事件间隔s
        /// </summary>
        public virtual int DetectSpanInSecs
        {
            get
            {
                return this.detectSpanInSecs;
            }
            set
            {
                this.detectSpanInSecs = value;
            }
        }

        public bool IsRunning
        {
            get
            {
                return !this.isStop;
            }
        }

        public event Action<Exception> EngineStopped;
        public BaseCycleEngine()
        {
            this.EngineStopped += delegate { };
        }

        public virtual void Start()
        {
            this.totalSleepCount = this.detectSpanInSecs * 1000 / 1000;
            if (this.detectSpanInSecs >= 0)
            {
                if (this.isStop)
                {
                    this.manualResetEvent4Stop.Reset();
                    this.isStop = false;
                    Action cbSimple = new Action(this.Worker);
                    cbSimple.BeginInvoke(null, null);
                }
            }
        }

        /// <summary>
        /// 停止引擎。千万不要在DoDetect方法中调用该方法，会导致死锁，可以改用StopAsyn方法。
        /// </summary>
        public virtual void Stop()
        {
            if (!this.isStop)
            {
                this.isStop = true;
                this.manualResetEvent4Stop.WaitOne();
                this.manualResetEvent4Stop.Reset();
            }
        }

        /// <summary>
        /// 异步停止引擎。
        /// </summary>
        public void StopAsyn()
        {
            Action cbGeneric = new Action(this.Stop);
            cbGeneric.BeginInvoke(null, null);
        }

        protected virtual void Worker()
        {
            Exception obj = null;
            try
            {
                while (!this.isStop)
                {
                    for (int i = 0; i < this.totalSleepCount; i++)
                    {
                        if (this.isStop)
                        {
                            break;
                        }
                        Thread.Sleep(SleepTime);
                    }
                    if (!this.DoDetect())
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                obj = ex;
                throw;
            }
            finally
            {
                this.isStop = true;
                this.manualResetEvent4Stop.Set();
                this.EngineStopped(obj);
            }
        }

        /// <summary>
        ///(1) DoDetect 每次循环时，引擎需要执行的核心动作。要确保我们的Action（即派生类的DoDetect方法）不任何抛出异常，否则会导致循环引擎异常停止，并导致循环引擎的内部状态损坏而不可用。所以在派生类的DoDetect方法方法实现时捕捉所有的异常并加以处理。
        /// (2)该方法中不允许调用BaseCycleEngine.Stop()方法，否则会导致死锁。
        /// </summary>
        /// <returns>返回值如果为false，只执行一次，表示退出引擎循环线程</returns>
        protected abstract bool DoDetect();
    }
}
