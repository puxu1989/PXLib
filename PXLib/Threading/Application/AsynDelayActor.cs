using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PXLib.Threading.Application
{
    /// <summary>
    /// 异步延迟执行器。
    /// </summary>
    public class AsynDelayActor
    {
        private Action<object> handler;

        private object argument;

        private int delayInMSecs = 0;
        /// <summary>
        /// 延迟多少毫秒执行 delegate(object arg){//干的事};
        /// </summary>
        /// <param name="delayMSecs"></param>
        /// <param name="action"></param>
        /// <param name="arg"></param>
        public AsynDelayActor(int delayMSecs, Action<object> action, object arg)
        {
            if (this.delayInMSecs < 0)
            {
                throw new ArgumentException("The value of delayMSecs is invalid. ");
            }
            this.handler = action;
            this.delayInMSecs = delayMSecs;
            this.argument = arg;
            Action cbGeneric = new Action(this.Action);
            cbGeneric.BeginInvoke(null, null);
        }

        private void Action()
        {
            Thread.Sleep(this.delayInMSecs);
            this.handler(this.argument);
        }
    }
}
