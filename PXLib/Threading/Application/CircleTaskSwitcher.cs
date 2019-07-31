using PXLib.ObjectManage;
using PXLib.Threading.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Threading.Application
{
    /// <summary>
    /// CircleTaskSwitcher 循环任务切换器。将一天24小时分为多个时段，在不同的时段，会有不同的任务。当到达任务切换点时，切换器会触发切换事件。
    /// </summary>
    /// <typeparam name="TaskType">任务的类型</typeparam> 
    public class CircleTaskSwitcher<TaskType> : ICycleEngineActor
    {
        private CycleEngine agileCycleEngine;

        private Circle<ShortTime> taskTimeCircle = new Circle<ShortTime>();

        private IDictionary<ShortTime, TaskType> taskDictionary = new Dictionary<ShortTime, TaskType>();

        public event Action<TaskType> TaskSwitched;

        /// <summary>
        /// TaskDictionary key为任务的起始点hour，value为对应的任务。
        /// </summary>
        public IDictionary<ShortTime, TaskType> TaskDictionary
        {
            get
            {
                return this.taskDictionary;
            }
            set
            {
                this.taskDictionary = value;
            }
        }

        public TaskType CurrentTask
        {
            get
            {
                return this.taskDictionary[this.taskTimeCircle.Current];
            }
        }

        public CircleTaskSwitcher()
        {
            this.TaskSwitched += delegate
            {
            };
        }

        public void Initialize()
        {
            if (this.taskDictionary.Count < 2)
            {
                throw new Exception("Count of StartHour must >= 2 !");
            }
            List<ShortTime> list = new List<ShortTime>();
            foreach (ShortTime current in this.taskDictionary.Keys)
            {
                list.Add(current);
            }
            list.Sort();
            this.taskTimeCircle = new Circle<ShortTime>(list);
            ShortTime shortTime = new ShortTime(DateTime.Now);
            if (shortTime.CompareTo(this.taskTimeCircle.Tail) >= 0 || shortTime.CompareTo(this.taskTimeCircle.Header) < 0)
            {
                this.taskTimeCircle.SetCurrent(this.taskTimeCircle.Tail);
            }
            else
            {
                this.taskTimeCircle.SetCurrent(this.taskTimeCircle.Header);
                while (shortTime.CompareTo(this.taskTimeCircle.PeekNext()) >= 0)
                {
                    this.taskTimeCircle.MoveNext();
                }
            }
            this.agileCycleEngine = new CycleEngine(this);
            this.agileCycleEngine.DetectSpanInSecs = 1;
            this.agileCycleEngine.Start();
        }

        public bool EngineAction()
        {
            if (this.taskTimeCircle.PeekNext().IsOnTime(DateTime.Now, this.agileCycleEngine.DetectSpanInSecs))
            {
                this.taskTimeCircle.MoveNext();
                this.TaskSwitched(this.CurrentTask);
            }
            return true;
        }
    }
}
