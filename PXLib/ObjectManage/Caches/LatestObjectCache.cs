using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage.Caches
{
    /// <summary>
    /// 固定大小的最新N个对象缓存器。线程安全。
    /// 当容器满时，新加入的对象会挤掉最老的对象。
    /// </summary>
    public class LatestObjectCache<T>
    {
        private T[] array = null;
        private int latestObjectIndex = -1;
        private object locker = new object();
        private int capacity = 10;
        private bool isFull = false;
        private DateTime lastObjectAddedTime = DateTime.Now;
        /// <summary>
        /// 获取最多容纳多少个最新对象。
        /// </summary>
        public int Capacity
        {
            get
            {
                return this.capacity;
            }
        }

        /// <summary>
        /// 缓存是否已经满了？
        /// </summary>
        public bool IsFull
        {
            get
            {
                return this.isFull;
            }
        }

        /// <summary>
        /// 最后一个对象加入的时间。
        /// </summary>
        public DateTime LastObjectAddedTime
        {
            get
            {
                return this.lastObjectAddedTime;
            }
        }

        /// <summary>
        /// 缓存中对象的个数。
        /// </summary>
        private int ValidObjectCount
        {
            get
            {
                int result;
                if (this.array == null)
                {
                    result = 0;
                }
                else if (this.isFull)
                {
                    result = this.array.Length;
                }
                else
                {
                    result = this.latestObjectIndex + 1;
                }
                return result;
            }
        }
        public LatestObjectCache(int _capacity)
        {
            this.capacity = _capacity;
        }
        public void Initialize(int _capacity)
        {
            if (this.capacity <= 0)
            {
                throw new Exception("Capacity must be greater than 0.");
            }
            this.array = new T[this.capacity];
            this.latestObjectIndex = -1;
        }

        /// <summary>
        /// 向缓存中添加最新的对象。
        /// </summary>        
        public void Add(T t)
        {
            lock (this.locker)
            {
                this.lastObjectAddedTime = DateTime.Now;
                this.latestObjectIndex = (this.latestObjectIndex + 1) % this.array.Length;
                this.array[this.latestObjectIndex] = t;
                if (!this.isFull && this.latestObjectIndex == this.array.Length - 1)
                {
                    this.isFull = true;
                }
            }
        }
        /// <summary>
        /// 按照由老到新的顺序获取所有缓存内的对象。
        /// </summary>       
        public List<T> GetLatestObjects()
        {
            List<T> result;
            lock (this.locker)
            {
                List<T> list = new List<T>();
                if (!this.isFull)
                {
                    for (int i = 0; i <= this.latestObjectIndex; i++)
                    {
                        list.Add(this.array[i]);
                    }
                    result = list;
                }
                else
                {
                    int num = (this.latestObjectIndex + 1) % this.array.Length;
                    for (int i = 0; i < this.array.Length; i++)
                    {
                        int num2 = (num + i) % this.array.Length;
                        list.Add(this.array[num2]);
                    }
                    result = list;
                }
            }
            return result;
        }
    }
}
