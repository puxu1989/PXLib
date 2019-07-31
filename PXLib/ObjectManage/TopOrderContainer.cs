
using PXLib.Threading.Locker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage
{
  
    /// <summary>
    ///  用于始终保持排行榜前N名的Object。该实现是线程安全的。
    ///  排行榜容器可以在多线程的环境中使用。TopNOrderedContainer使用SmartRWLocker来对Add方法进行同步，之所以选择读写锁而不是简单的lock，是因为使排行榜容器在应对多读/少写的状况时能支持更大的并发。
    ///  适用场合：TopNOrderedContainer用于对巨大数量的对象进行TopN排序。其适用场合有如下特点：
    /// （1）需要被排序的对象的数量非常巨大（如几百万、甚至几千万）。对系统有价值的排序结果只有前N名。N远小于总的对象数量。
    ///  如果要排序的对象的数量与TopN的N值的差距并不大，那么使用TopNOrderedContainer并不一定是最佳的选择，这时我们可以采用一些高效的完全排序算法对所有的对象进行排序，然后再取出前N名，可能速度会更快。
　　///  当然，我们也可以使用最大最小堆的算法来实现TopN的排序，也是完全可行的。
    /// </summary>
    public  class TopOrderContainer<TObj> where TObj : IOrdered<TObj>
    {
       private TObj[] orderedArray = null;

		private int validObjCount = 0;

        private SmartRWLocker smartRWLocker = new SmartRWLocker();

		private int _topNumber = 10;//默认Top10

		public int TopNumber{get{return this._topNumber;}set{this._topNumber = value;}}
        public TopOrderContainer()
        {
            this.orderedArray = new TObj[this._topNumber];
        }
        public TopOrderContainer(int topNumber)
		{
			this._topNumber = topNumber;
            if (this._topNumber < 1)
            {
                throw new Exception("The value of TopNumber must greater than 0 ");
            }
            this.orderedArray = new TObj[this._topNumber];
		}

		public void Add(IEnumerable<TObj> list)
		{
			if (list != null)
			{
				using (this.smartRWLocker.Lock(AccessMode.Write))
				{
					foreach (TObj current in list)
					{
						this.DoAdd(current);
					}
				}
			}
		}

		public void Add(TObj obj)
		{
			using (this.smartRWLocker.Lock(AccessMode.Write))
			{
				this.DoAdd(obj);
			}
		}

		public TObj[] GetTopN()
		{
			TObj[] result;
			using (this.smartRWLocker.Lock(AccessMode.Read))
			{
				result = (TObj[])this.orderedArray.Clone();//GetTopN方法用于返回当前的排行榜的拷贝。之所以返回一个拷贝，是因为外部对返回的数组进行任何操作都不会影响到TopNOrderedContainer的内部集合。
			}
			return result;
		}
        #region Private
        private void DoAdd(TObj obj)
        {
            if (obj != null)
            {
                if (this.validObjCount < this._topNumber)
                {
                    this.orderedArray[this.validObjCount] = obj;
                    this.Adjust(this.validObjCount);
                    this.validObjCount++;
                }
                else if (!this.orderedArray[this._topNumber - 1].IsTopThan(obj))
                {
                    this.orderedArray[this._topNumber - 1] = obj;
                    this.Adjust(this._topNumber - 1);
                }
            }
        }

        /// <summary>
        /// Adjust 二分查找 新加入的对象初始时被放置于最后一个有效位置即posIndex，需要将其调整到正确的位置。
        /// </summary>        
        private void Adjust(int posIndex)
        {
            TObj target = this.orderedArray[posIndex];
            int targetPosIndex = -1;
            if (target.IsTopThan(this.orderedArray[0]))
            {
                targetPosIndex = 0;
            }
            else
            {
                int left = 0;
                int right = posIndex;

                while (right - left > 1)
                {
                    int middle = (left + right) / 2;

                    if (target.IsTopThan(this.orderedArray[middle]))
                    {
                        right = middle;
                    }
                    else
                    {
                        left = middle;
                    }
                }

                targetPosIndex = left;
                if (right != left)
                {
                    if (!target.IsTopThan(this.orderedArray[left]))
                    {
                        targetPosIndex = right;
                    }
                }
            }
            for (int i = posIndex; i > targetPosIndex; i--)
            {
                this.orderedArray[i] = this.orderedArray[i - 1];
            }

            this.orderedArray[targetPosIndex] = target;
        }

        #endregion
    }
    /// <summary>
    /// IOrdered 参与排行榜排序的对象必须实现的接口。
    /// </summary>
    /// <typeparam name="TOrderedObj">参与排行榜排序的对象的类型</typeparam>
    public interface IOrdered<TOrderedObj>
    {
        bool IsTopThan(TOrderedObj other);//实现接口 this.score>other.score
    }
}
