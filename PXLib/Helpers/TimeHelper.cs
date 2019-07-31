using PXLib.Caches;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PXLib.Helpers
{
    public class TimeHelper
    {
        #region Stopwatch计时器
        /// <summary>
        /// 计时器开始
        /// </summary>
        /// <returns></returns>
        public static Stopwatch TimerStart()
        {
            Stopwatch watch = new Stopwatch();
            watch.Reset();
            watch.Start();
            return watch;
        }
        /// <summary>
        /// 计时器结束
        /// </summary>
        /// <param name="watch"></param>
        /// <returns></returns>
        public static string TimerEnd(Stopwatch watch)
        {
            watch.Stop();
            double costtime = watch.ElapsedMilliseconds;
            return costtime.ToString();
        }
        #endregion

        /// <summary>
        /// 获取这个格式的日期yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <returns></returns>
        public static string GetDate()
        {
            DateTime dt = DateTime.Now;
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /// <summary>  
        /// 获取当前时间戳  
        /// </summary>  
        /// <param name="isShort">为真时获取10位时间戳,为假时获取13位时间戳.</param>  
        /// <returns></returns>  
        public static long GetTimeStamp(bool isShort = true)
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            if (isShort)
                return Convert.ToInt64(ts.TotalSeconds);
            else
                return Convert.ToInt64(ts.TotalMilliseconds);
        }
        /// <summary>
        /// 判断指定时间targetTime是否在startTime-endTime之间 
        /// </summary>
        /// <param name="targetTime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static bool IsTimeInTimeSpan(DateTime targetTime,string startTime,string endTime)
        {
            //判断当前时间是否在工作时间段内
            //string _strWorkingDayAM = "08:30";
            //string _strWorkingDayPM = "17:30";
            TimeSpan startTimeSpan = DateTime.Parse(startTime).TimeOfDay;
            TimeSpan endTimeSpan = DateTime.Parse(endTime).TimeOfDay;
            TimeSpan dspNow = targetTime.TimeOfDay;
            if (dspNow > startTimeSpan && dspNow < endTimeSpan)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// IsOnTime 时间val与requiredTime之间的差值是否在maxToleranceInSecs范围之内。
        /// </summary>        
        public static bool IsOnTime(DateTime requiredTime, DateTime val, int maxToleranceInSecs)
        {
            TimeSpan timeSpan = val - requiredTime;
            double num = (timeSpan.TotalMilliseconds < 0.0) ? (-timeSpan.TotalMilliseconds) : timeSpan.TotalMilliseconds;
            return num <= (double)(maxToleranceInSecs * 1000);
        }

        /// <summary>
        /// IsOnTime 对于循环调用，时间val与startTime之间的差值(&gt;0)对cycleSpanInSecs求余数的结果是否在maxToleranceInSecs范围之内。
        /// </summary>        
        public static bool IsOnTime(DateTime startTime, DateTime val, int cycleSpanInSecs, int maxToleranceInSecs)
        {
            double totalMilliseconds = (val - startTime).TotalMilliseconds;
            double num = totalMilliseconds % (double)(cycleSpanInSecs * 1000);
            return num <= (double)(maxToleranceInSecs * 1000);
        }
        /// <summary>
        /// 获取格式如20170518
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int ConvertToDateInteger(DateTime dt)
        {
            return dt.Year * 10000 + dt.Month * 100 + dt.Day;
        }
        /// <summary>
        /// 将20170518数字日期转换成标准的DateTime类型
        /// </summary>

        public static DateTime ConvertFromDateInteger(int theDate)
        {
            return ConvertFromDateInteger(theDate, 0, 0, 0);
        }
        public static DateTime ConvertFromDateInteger(int theDate, int hour, int minute, int second)
        {
            int num = theDate / 10000;
            int num2 = theDate % 10000 / 100;
            int num3 = theDate % 10000 % 100;
            return new DateTime(num, num2, num3, hour, minute, second);
        }
        /// <summary>
        /// 获取本月有多少天
        /// </summary>
        public static int GetDaysOfMonth(int iYear, int Month)
        {
            var days = 0;
            switch (Month)
            {
                case 1:
                    days = 31;
                    break;
                case 2:
                    days = IsRuYear(iYear) ? 29 : 28;
                    break;
                case 3:
                    days = 31;
                    break;
                case 4:
                    days = 30;
                    break;
                case 5:
                    days = 31;
                    break;
                case 6:
                    days = 30;
                    break;
                case 7:
                    days = 31;
                    break;
                case 8:
                    days = 31;
                    break;
                case 9:
                    days = 30;
                    break;
                case 10:
                    days = 31;
                    break;
                case 11:
                    days = 30;
                    break;
                case 12:
                    days = 31;
                    break;
            }

            return days;
        }
        /// <summary>
        /// /计算两个时间差 距离当前时间有多长的总时间描述
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public static string GetDateTotalDesc(DateTime startTime)
        {
            string dateDiff = null;
            TimeSpan ts = DateTime.Now.Subtract(startTime).Duration();
            dateDiff = ts.Days.ToString() + "天" + ts.Hours.ToString() + "小时" + ts.Minutes.ToString() + "分钟" + ts.Seconds.ToString() + "秒";
            return dateDiff;
        }
        /// <summary>
        /// 获取过去的时间模糊描述  如微信qq朋友圈发布等时间描述 如果是今天的就只显示月.日 否则显示中文描述的具体时间
        /// </summary>
        public static string GetDateStringDesc(DateTime dt)
        {
            string dateDesc = null;
            TimeSpan ts = DateTime.Now - dt;
            if (ts.Days > 6)
            {
                if (dt.Year == DateTime.Now.Year)
                    dateDesc = dt.Month.ToString() + "月" + dt.Day.ToString() + "日";
                else
                    dateDesc = dt.ToString("U");//中文具体时间

            }
            else if (ts.Days >= 1 && ts.Days <= 6)
            {

                if (DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd") == dt.ToString("yyyy-MM-dd"))
                {
                    dateDesc = "前天";
                }
                else if (DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") == dt.ToString("yyyy-MM-dd"))
                {
                    dateDesc = "昨天";
                }
                else
                    dateDesc = ts.Days + "天前";
            }
            else
            {
                if (ts.Hours >= 1)
                    dateDesc = ts.Hours.ToString() + "小时前";
                else if (ts.Minutes >= 1 && ts.Minutes < 60)
                    dateDesc = ts.Minutes.ToString() + "分钟前";
                else if (ts.Seconds <= 60)
                    dateDesc = "刚刚";
            }
            return dateDesc;
        }
        #region 获取/转换成中国具体时间描述 2017-07-22(星期六) 上午 11:08:48
        public static string GetChineseWeekDay(int y, int m, int d)
        {
            string[] weekstr = { "日", "一", "二", "三", "四", "五", "六" };
            if (m < 3)
            {
                m += 12;
                y--;
            }
            if (y % 400 == 0 || y % 100 != 0 && y % 4 == 0)
                d--;
            else
                d += 1;
            return "星期" + weekstr[(d + 2 * m + 3 * (m + 1) / 5 + y + y / 4 - y / 100 + y / 400) % 7];
        }
        public static string GetChineseDateTime(DateTime dt)
        {
            string dateTime = dt.ToString("yyyy-MM-dd");
            dateTime = dateTime + string.Format("({0})", GetChineseWeekDay(dt.Year, dt.Month, dt.Day));
            string time = string.Format("{0:  tt hh:mm:ss }", dt);
            time = time.Replace("am", "上午").Replace("pm", "下午");
            dateTime = dateTime + time;
            return dateTime;
        }
        #endregion
        #region 返回两个日期之间相差的天数
        /// <summary>
        /// 返回两个日期之间相差的天数
        /// </summary>
        /// <param name="dtfrm">两个日期参数</param>
        /// <param name="dtto">两个日期参数</param>
        /// <returns>天数</returns>
        public static int DiffDays(DateTime dtfrm, DateTime dtto)
        {
            TimeSpan tsDiffer = dtto.Date - dtfrm.Date;
            return tsDiffer.Days;
        }
        #endregion
        #region 获取某一日期是该年中的第几周
        public static int GetWeekOfYear(DateTime date)
        {
            int dayOfYear = date.DayOfYear;
            DateTime tempDate = new DateTime(date.Year, 1, 1);
            int tempDayOfWeek = (int)tempDate.DayOfWeek;
            tempDayOfWeek = tempDayOfWeek == 0 ? 7 : tempDayOfWeek;
            int index = (int)date.DayOfWeek;
            index = index == 0 ? 7 : index;
            DateTime retStartDay = date.AddDays(-(index - 1));
            DateTime retEndDay = date.AddDays(6 - index);
            int weekIndex = (int)Math.Ceiling(((double)dayOfYear + tempDayOfWeek - 1) / 7);
            if (retStartDay.Year < retEndDay.Year)
            {
                weekIndex = 1;
            }
            return weekIndex;
        }
        //public static int GetWeekOfYear(DateTime dt)
        //{
        //    var gc = new GregorianCalendar();
        //    return gc.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        //}
        #endregion
        #region 判断某个日期是否为今天
        public static bool IsToday(DateTime dt)
        {
            DateTime today = DateTime.Today;
            DateTime tempToday = new DateTime(dt.Year, dt.Month, dt.Day);
            if (today == tempToday)
                return true;
            else
                return false;
        }
        #endregion
        #region 判断当前年份是否是闰年
        /// <summary>判断当前年份是否是闰年，私有函数</summary>
        /// <param name="iYear">年份</param>
        /// <returns>是闰年：True ，不是闰年：False</returns>
        private static bool IsRuYear(int iYear)
        {
            //形式参数为年份
            //例如：2003
            int n = iYear;
            return (n % 400 == 0) || (n % 4 == 0 && n % 100 != 0);
        }
        #endregion

        #region 在某个时间内能做某事 使用缓存
        /// <summary>
        ///  在某个时间段秒数表示能做某事 IsSlidingExpiration=true访问后时间往后延 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="overTimeSecond">过期秒数</param>
        /// <param name="IsSlidingExpiration"></param>
        /// <returns></returns>
        public static bool IsEnabledToDoSomething(string cacheKey, int overTimeSecond, bool IsSlidingExpiration = true)
        {
            const string fexd = "@@@@rfwe";
            string key = fexd + cacheKey;
            string value = CacheFactory.Cache().GetCache<string>(key);
            if (value != null)
            {
                return false;
            }
            else
            {
                if (IsSlidingExpiration)
                {
                    TimeSpan t = new TimeSpan(0, 0, overTimeSecond);
                    CacheFactory.Cache().WriteCache(key, "0", t);
                }
                else
                {
                    CacheFactory.Cache().WriteCache(key, "0", DateTime.Now.AddSeconds(overTimeSecond));
                }
                return true;
            }        
        }
        #endregion
    }
}
