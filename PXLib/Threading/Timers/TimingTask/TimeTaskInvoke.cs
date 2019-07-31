using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Threading.Timers.TimingTask
{
    /// <summary>
    /// 定时任务简单组合调用类 此对象作为调用者的属性 调用者实现接口ITimingTaskExcuter的ExcuteOnTime方法
    /// </summary>
   public class TimeTaskInvoke
    {
       public TimeTaskInvoke(ITimingTaskExcuter taskExcuter, TimingTaskType timingTaskType)
        {
            TimingTask task = new TimingTask();//如果多种定时任务 多创建几个TimeingTask 注册到TimingTaskManager
            task.TimingTaskType = timingTaskType;
            task.TimingTaskExcuter = taskExcuter;
            task.ExcuteTime = new ShortTime(0, 0, 1);
            TimingTaskManager timeTask = new TimingTaskManager();
            timeTask.RegisterTask(task);
            timeTask.Initialize();
        }
    }
}
