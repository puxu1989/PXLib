using PXLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Threading.Timers.TimingTask
{
    /// <summary>
    /// TimingTaskType 定时任务的类型 
    /// </summary>
    [EnumText("定时任务的类型")]
    public enum TimingTaskType
    {
        [EnumText("每小时一次")]//经测试为整点执行
        PerHour,
        [EnumText("每天一次")]
        PerDay,
        [EnumText("每周一次")]
        PerWeek,
        [EnumText("每月一次")]
        PerMonth
    }
   public class TimingTask
    {
        private DateTime lastRightTime = DateTime.Parse("2000-01-01 00:00:00");//用于记录上次执行时间

        private ITimingTaskExcuter timingTaskExcuter;

        private TimingTaskType timingTaskType = TimingTaskType.PerDay;

        private ShortTime excuteTime = new ShortTime();

        private DayOfWeek dayOfWeek = DayOfWeek.Monday;

        private int day = 1;

        public ITimingTaskExcuter TimingTaskExcuter
        {
            get
            {
                return this.timingTaskExcuter;
            }
            set
            {
                this.timingTaskExcuter = value;
            }
        }

        public TimingTaskType TimingTaskType
        {
            get
            {
                return this.timingTaskType;
            }
            set
            {
                this.timingTaskType = value;
            }
        }

        /// <summary>
        /// ExcuteTime 任务执行的具体时刻。如果TimingTaskType为PerHour，则将忽略ExcuteTime的Hour属性。
        /// </summary>
        public ShortTime ExcuteTime
        {
            get
            {
                return this.excuteTime;
            }
            set
            {
                this.excuteTime = value;
            }
        }

        /// <summary>
        /// DayOfWeek 该属性只有在TimingTaskType为PerWeek时才有效，表示在周几执行。
        /// </summary>
        public DayOfWeek DayOfWeek
        {
            get
            {
                return this.dayOfWeek;
            }
            set
            {
                this.dayOfWeek = value;
            }
        }

        /// <summary>
        /// Day 该属性只有在TimingTaskType为PerMonth时才有效，表示在每月的几号执行。
        /// </summary>
        public int Day
        {
            get
            {
                return this.day;
            }
            set
            {
                this.day = value;
            }
        }

        public bool IsOnTime(int checkSpanSeconds, DateTime now)
        {
            bool result;
            //如果lastRightTime与当前时间的差值2倍的扫描间隔以内，则将认为当前时间不符合条件
            if ((now - this.lastRightTime).TotalMilliseconds < (double)(checkSpanSeconds * 1000 * 2))
            {
                result = false;
            }
            else
            {
                bool flag = false;
                switch (this.timingTaskType)
                {
                    case TimingTaskType.PerHour:
                        {
                            ShortTime shortTime = new ShortTime(now.Hour, this.excuteTime.Minute, this.excuteTime.Second);
                            flag = shortTime.IsOnTime(now, checkSpanSeconds);
                            if (!flag)
                            {
                                ShortTime shortTime2 = new ShortTime(now.AddHours(1.0).Hour, this.excuteTime.Minute, this.excuteTime.Second);
                                flag = shortTime2.IsOnTime(now, checkSpanSeconds);
                            }
                            if (!flag)
                            {
                                ShortTime shortTime3 = new ShortTime(now.AddHours(-1.0).Hour, this.excuteTime.Minute, this.excuteTime.Second);
                                flag = shortTime3.IsOnTime(now, checkSpanSeconds);
                            }
                            break;
                        }
                    case TimingTaskType.PerDay:
                        flag = this.excuteTime.IsOnTime(now, checkSpanSeconds);
                        break;
                    case TimingTaskType.PerWeek:
                        flag = (now.DayOfWeek == this.dayOfWeek && this.excuteTime.IsOnTime(now, checkSpanSeconds));
                        break;
                    case TimingTaskType.PerMonth:
                        flag = (now.Day == this.day && this.excuteTime.IsOnTime(now, checkSpanSeconds));
                        break;
                }
                if (flag)
                {
                    this.lastRightTime = now;
                }
                result = flag;
            }
            return result;
        }
    }
}
