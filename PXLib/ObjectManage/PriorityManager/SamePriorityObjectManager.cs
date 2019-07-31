using PXLib.Threading.Locker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.ObjectManage.PriorityManager
{
    /// <summary>
    /// 同一优先级对象管理器
    /// </summary>
    public class SamePriorityObjectManager<T> : ISamePriorityObjectManager<T>
    {
        private LinkedList<T> waiterList = new LinkedList<T>();
        private SmartRWLocker smartRWLocker = new SmartRWLocker();
        public event Action<T> WaiterDiscarded;

        private int capacity = int.MaxValue;
        /// <summary>
        ///  获取空间大小 默认21亿多个
        /// </summary>
        public int Capacity
        {
            get { return capacity; }
        }
        /// <summary>
        /// 管理器当前数量
        /// </summary>

        public int Count
        {
            get
            {
                using (this.smartRWLocker.Lock(AccessMode.Read))
                {
                    return this.waiterList.Count;
                }
            }
        }
        /// <summary>
        /// 是否满载
        /// </summary>
        public bool Full
        {
            get
            {
                return this.Count >= this.capacity;
            }
        }
        private ActionTypeOnAddOverflow actionTypeOnAddOverflow = ActionTypeOnAddOverflow.Wait;
        public ActionTypeOnAddOverflow ActionTypeOnAddOverflow
        {
            get { return actionTypeOnAddOverflow; }
        }
        private int detectSpanInMSecsOnWait = 10;
        public int DetectSpanInMSecsOnWait
        {
            get { return detectSpanInMSecsOnWait; }
            set
            {
                detectSpanInMSecsOnWait = value <= 0 ? 1: value;//默认10毫秒
            }
        }
        #region 构造
        public SamePriorityObjectManager()
        {
            this.WaiterDiscarded += delegate { };
        }
        public SamePriorityObjectManager(int _capacity, ActionTypeOnAddOverflow actionType, int _detectSpanInMSecsOnWait) : this()
        {
            this.capacity = _capacity;
            this.actionTypeOnAddOverflow = actionType;
            this.detectSpanInMSecsOnWait = _detectSpanInMSecsOnWait;
        }
        public SamePriorityObjectManager(int _capacity, ActionTypeOnAddOverflow actionType)
            : this(_capacity, actionType, 10)
        {
        }
        public SamePriorityObjectManager(PriorityManagerPara para)
            : this(para.Capacity, para.ActionTypeOnAddOverflow, para.DetectSpanInMSecsOnWait)
        {
        }
        #endregion
        /// <summary>
        /// 添加等待者
        /// </summary>
        /// <param name="waiter"></param>
        public void AddWaiter(T waiter)
        {
            if (this.waiterList.Count < this.capacity)
            {
                using (this.smartRWLocker.Lock(AccessMode.Write))
                {
                    this.waiterList.AddLast(waiter);
                    return;
                }
            }

            if (this.actionTypeOnAddOverflow == ActionTypeOnAddOverflow.DiscardCurrent)
            {
                this.WaiterDiscarded(waiter);
                return;
            }

            if (this.actionTypeOnAddOverflow == ActionTypeOnAddOverflow.DiscardLatest)
            {
                T discarded = default(T);
                using (this.smartRWLocker.Lock(AccessMode.Write))
                {
                    if (this.waiterList.Count > 0)
                    {
                        discarded = this.waiterList.Last.Value;
                        this.waiterList.RemoveLast();
                    }
                    this.waiterList.AddLast(waiter);
                }
                this.WaiterDiscarded(discarded);
                return;
            }

            if (this.actionTypeOnAddOverflow == ActionTypeOnAddOverflow.DiscardOldest)
            {
                T discarded = default(T);
                using (this.smartRWLocker.Lock(AccessMode.Write))
                {
                    if (this.waiterList.Count > 0)
                    {
                        discarded = this.waiterList.First.Value;
                        this.waiterList.RemoveFirst();
                    }
                    this.waiterList.AddLast(waiter);
                }
                this.WaiterDiscarded(discarded);
                return;
            }

            //this.actionTypeOnAddOverflow == ActionTypeOnAddOverflow.Wait
            while (this.waiterList.Count >= this.capacity)
            {
                System.Threading.Thread.Sleep(this.detectSpanInMSecsOnWait);
            }

            using (this.smartRWLocker.Lock(AccessMode.Write))
            {
                this.waiterList.AddLast(waiter);
            }
        }
        public bool Contains(T waiter)
        {
            using (this.smartRWLocker.Lock(AccessMode.Read))
            {
                return this.waiterList.Contains(waiter);
            }
        }
        public void RemoveWaiter(T waiter)
        {
            using (this.smartRWLocker.Lock(AccessMode.Write))
            {
                if (!this.waiterList.Contains(waiter))
                {
                    return;
                }

                this.waiterList.Remove(waiter);
            }
        }
        /// <summary>
        /// 获取下一个对象 就是第一个
        /// </summary>
        /// <returns></returns>
        public T GetNextWaiter()
        {
            using (this.smartRWLocker.Lock(AccessMode.Read))
            {
                if (this.waiterList.Count == 0)
                {
                    return default(T);
                }
                return this.waiterList.First.Value;
            }
        }
        /// <summary>
        /// 获取下一个对象 并弹出当前对象
        /// </summary>
        /// <returns></returns>
        public T PopNextWaiter()
        {
            using (this.smartRWLocker.Lock(AccessMode.Write))
            {
                if (this.waiterList.Count == 0)
                {
                    return default(T);
                }
                T target = this.waiterList.First.Value;
                this.waiterList.RemoveFirst();
                return target;
            }
        }
        public T[] GetWaitersByPriority()
        {
            using (this.smartRWLocker.Lock(AccessMode.Read))           
            {
                T[] waiters = new T[this.Count];
                if (waiters.Length == 0)
                {
                    return waiters;
                }
                LinkedListNode<T> firstNode = this.waiterList.First;
                waiters[0] = firstNode.Value;
                LinkedListNode<T> temp = firstNode;
                for (int i = 1; i < waiters.Length; i++)
                {
                    temp = temp.Next;
                    waiters[i] = temp.Value;
                }
                return waiters;
            }
        } 
        public void Clear()
        {
            using (this.smartRWLocker.Lock(AccessMode.Write))           
            {
                this.waiterList.Clear();
            }
        } 
    }
}
