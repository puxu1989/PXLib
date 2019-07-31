using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Threading.Timers.CallbackTimer
{
    /// <summary>
    /// CallbackTask 用于封装一个定时回调任务。
    /// </summary>    
    public class CallbackTask<T>
    {
        public CallbackTask()
        {
        }

        public CallbackTask(int _id, int iniSecs, Action<T> _callback, T _callbackPara)
        {
            this.iD = _id;
            this.leftSeconds = iniSecs;
            this.callbackPara = _callbackPara;
            this.callback = _callback;

            if (this.leftSeconds <= 0)
            {
                throw new ArgumentException("The Left Seconds must greater than 0!");
            }
        }

        #region ID
        private int iD = 0;
        public int ID
        {
            get { return iD; }
            set { iD = value; }
        }
        #endregion

        #region CallbackPara
        private T callbackPara;
        public T CallbackPara
        {
            get { return callbackPara; }
            set { callbackPara = value; }
        }

        #endregion

        #region LeftSeconds
        private int leftSeconds = 10;
        public int LeftSeconds
        {
            set { leftSeconds = value; }
            get { return leftSeconds; }
        }
        #endregion

        #region Callback
        private Action<T> callback;
        /// <summary>
        /// Callback 回调执行时，不允许抛出任何异常
        /// </summary>
        public Action<T> Callback
        {
            get { return callback; }
            set { callback = value; }
        }
        #endregion

        #region SecondsPassed
        public bool SecondsPassed(int seconds)
        {
            this.leftSeconds -= seconds;

            return this.leftSeconds <= 0;
        }
        #endregion
    }
}
