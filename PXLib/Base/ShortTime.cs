using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib
{
    /// <summary>
    /// ShortTime 用于指定小时、分钟、秒。比如上班时间、下班时间。
    /// </summary>
    [Serializable]
    public class ShortTime : IComparable<ShortTime>
    {
        private int hour = 0;

        private int minute = 0;

        private int second = 0;

        public int Hour
        {
            get
            {
                return this.hour;
            }
            set
            {
                this.hour = value;
                this.hour = ((this.hour > 23) ? 23 : this.hour);
                this.hour = ((this.hour < 0) ? 0 : this.hour);
            }
        }

        public int Minute
        {
            get
            {
                return this.minute;
            }
            set
            {
                this.minute = value;
                this.minute = ((this.minute > 59) ? 59 : this.minute);
                this.minute = ((this.minute < 0) ? 0 : this.minute);
            }
        }

        public int Second
        {
            get
            {
                return this.second;
            }
            set
            {
                this.second = value;
                this.second = ((this.second > 59) ? 59 : this.second);
                this.second = ((this.second < 0) ? 0 : this.second);
            }
        }
        /// <summary>
        /// 只写 标准：短时间形式
        /// </summary>
        public string ShortTimeString
        {
            set
            {
                string[] ary = value.Split(':');
                this.Hour = int.Parse(ary[0]);
                this.Minute = int.Parse(ary[1]);
                this.Second = int.Parse(ary[2]);
            }

        }
        #region 构造
        public ShortTime()
        {
            DateTime time = DateTime.Now;
            this.Hour = time.Hour;
            this.Minute = time.Minute;
            this.Second = time.Second;
        }

        public ShortTime(int h, int m, int s)
        {
            this.Hour = h;
            this.Minute = m;
            this.Second = s;
        }
        public ShortTime(DateTime time)
        {
            this.Hour = time.Hour;
            this.Minute = time.Minute;
            this.Second = time.Second;
        }
        #endregion

       /// <summary>
       /// 返回当前时间的年月日+ShortTime的时分秒
       /// </summary>
       /// <returns></returns>
        public DateTime GetDateTime()
        {
            DateTime now = DateTime.Now;
            return this.GetDateTime(now.Year, now.Month, now.Day);
        }

        public DateTime GetDateTime(int year, int month, int day)
        {
            return new DateTime(year, month, day, this.hour, this.minute, this.second);
        }

        /// <summary>
        /// IsOnTime 目标时间是否与当前对象所表示的时间的差值是否在maxToleranceInSecs范围之内。
        /// </summary>       
        public bool IsOnTime(DateTime target, int maxToleranceInSecs)
        {
            DateTime dateTime = this.GetDateTime(target.Year, target.Month, target.Day);
            bool flag = TimeHelper.IsOnTime(dateTime, target, maxToleranceInSecs);
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                flag = TimeHelper.IsOnTime(dateTime.AddDays(1.0), target, maxToleranceInSecs);
                if (flag)
                {
                    result = true;
                }
                else
                {
                    flag = TimeHelper.IsOnTime(dateTime.AddDays(-1.0), target, maxToleranceInSecs);
                    result = flag;
                }
            }
            return result;
        }
        //ShortTime所表示的时刻的值越大，则ShortTime就越大，这是完全一致的。
        public int CompareTo(ShortTime other)
        {
            int result;
            if (this.hour == other.hour && this.minute == other.minute && this.second == other.second)
            {
                result = 0;
            }
            else
            {
                int num = this.hour - other.hour;
                int num2 = this.minute - other.minute;
                int num3 = this.second - other.second;
                if (num > 0)
                {
                    result = 1;
                }
                else if (num < 0)
                {
                    result = -1;
                }
                else if (num2 > 0)
                {
                    result = 1;
                }
                else if (num2 < 0)
                {
                    result = -1;
                }
                else if (num3 > 0)
                {
                    result = 1;
                }
                else if (num3 < 0)
                {
                    result = -1;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", this.hour, this.minute, this.second);
        }
    }
}
