using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.ObjectManage.PriorityManager
{
    /// <summary>
    /// 管理器的默认配置参数
    /// </summary>
    [Serializable]
    public class PriorityManagerPara
    {
        #region Ctor
        public PriorityManagerPara() { }
        public PriorityManagerPara(int _capacity, ActionTypeOnAddOverflow actionType, int _detectSpanInMSecsOnWait)
        {
            this.capacity = _capacity;
            this.actionTypeOnAddOverflow = actionType;
            this.detectSpanInMSecsOnWait = _detectSpanInMSecsOnWait;
        }
        public PriorityManagerPara(int _capacity, ActionTypeOnAddOverflow actionType)
        {
            this.capacity = _capacity;
            this.actionTypeOnAddOverflow = actionType;
        }
        #endregion

        #region Capacity
        private int capacity = int.MaxValue;
        public int Capacity
        {
            get { return capacity; }
        }
        #endregion

        #region ActionTypeOnAddOverflow
        private ActionTypeOnAddOverflow actionTypeOnAddOverflow = ActionTypeOnAddOverflow.Wait;
        public ActionTypeOnAddOverflow ActionTypeOnAddOverflow
        {
            get { return actionTypeOnAddOverflow; }
        }
        #endregion

        #region DetectSpanInMSecsOnWait
        private int detectSpanInMSecsOnWait = 10;
        public int DetectSpanInMSecsOnWait
        {
            get { return detectSpanInMSecsOnWait; }
            set
            {
                detectSpanInMSecsOnWait = value <= 0 ? 1 : value;
            }
        }
        #endregion
    }
}
