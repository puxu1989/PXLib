using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PXLib.Threading.Timers.TimingTask
{
    /// <summary>
    /// TimingTaskManager 用于管理所有的定时任务，并在时间到达时，异步执行任务。误差1秒，如果需要精确时间点执行，这个并不适合。
    /// </summary>
   public class TimingTaskManager
    {
        private object locker = new object();

        private Timer timer;//TimingTaskManager内部采用的Timer定时器

        private int timerSpanInSecs = 1;//默认1s

        private IList<TimingTask> taskList = new List<TimingTask>();
        /// <summary>
        /// 多个定时任务集合 如：一个在每周二的中午12:00:00执行，另一个在每周四的中午12:00:00执行
        /// </summary>
        public IList<TimingTask> TaskList
        {
            get
            {
                return this.taskList;
            }
            set
            {
                this.taskList = (value ?? new List<TimingTask>());
            }
        }

        public int TimerSpanInSecs
        {
            get
            {
                return this.timerSpanInSecs;
            }
            set
            {
                this.timerSpanInSecs = value;
                if (this.timerSpanInSecs < 1)
                {
                    this.timerSpanInSecs = 1;
                }
            }
        }

        public void Initialize()
        {
            this.timer = new Timer(new TimerCallback(this.Worker), null, this.timerSpanInSecs * 1000, this.timerSpanInSecs * 1000);
        }

        private void Worker(object state)
        {
            DateTime now = DateTime.Now;
            lock (this.locker)
            {
                foreach (TimingTask current in this.taskList)
                {
                    if (current.IsOnTime(this.timerSpanInSecs, now))
                    {
                        Action<DateTime> cbDateTime = new Action<DateTime>(current.TimingTaskExcuter.ExcuteOnTime);
                        cbDateTime.BeginInvoke(now, null,null);
                    }
                }
            }
        }

        public void RegisterTask(TimingTask task)
        {
            lock (this.locker)
            {
                this.taskList.Add(task);
            }
        }

        public void UnRegisterTask(TimingTask task)
        {
            lock (this.locker)
            {
                this.taskList.Remove(task);
            }
        }
    }
}
