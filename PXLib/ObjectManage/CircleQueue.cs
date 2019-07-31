using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage
{
    /// <summary>
    /// 内部使用Circle的固定大小的Queue，当装满后，再加入对象，则将最老的那个对象替换掉。该类的实现是线程安全的。
    /// </summary>
    /// <typeparam name="T"></typeparam>
  
    public class CircleQueue<T>
    {
        private object locker = new object();

        private T[] array;

        private int headIndex = 0;

        private int tailIndex = 0;

        private int maxCount = 0;

        private int count = 0;

        public event Action<T> ObjectBeDiscarded;

        /// <summary>
        /// 历史中最大的元素个数。
        /// </summary>
        public int MaxCount
        {
            get
            {
                return this.maxCount;
            }
        }

        /// <summary>
        /// 最大容量。
        /// </summary>
        public int Size
        {
            get
            {
                return this.array.Length;
            }
        }

        /// <summary>
        /// 队列中的元素个数。
        /// </summary>
        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public bool Full
        {
            get
            {
                return this.count >= this.array.Length;
            }
        }

        public CircleQueue(int size)
        {
            this.array = new T[size];
        }

        /// <summary>
        /// 当装满后，再加入对象，则将最老的那个对象覆盖掉。
        /// </summary>        
        public void Enqueue(T obj)
        {
            lock (this.locker)
            {
                if (this.count == 0)
                {
                    this.array[this.headIndex] = obj;
                    this.tailIndex = this.headIndex;
                    this.count++;
                }
                else
                {
                    T obj3 = this.array[this.headIndex];
                    this.tailIndex = (this.tailIndex + 1) % this.array.Length;
                    this.array[this.tailIndex] = obj;
                    this.count++;
                    if (this.tailIndex == this.headIndex)
                    {
                        this.headIndex = (this.headIndex + 1) % this.array.Length;
                        this.count--;
                        if (this.ObjectBeDiscarded != null)
                        {
                            this.ObjectBeDiscarded(obj3);
                        }
                    }
                    if (this.count > this.maxCount)
                    {
                        this.maxCount = this.count;
                    }
                }
            }
        }

        /// <summary>
        /// 查看队列首部的元素，但是不从队列中移除。
        /// </summary>       
        public T Peek()
        {
            T result;
            lock (this.locker)
            {
                T t = default(T);
                if (this.count == 0)
                {
                    result = t;
                }
                else
                {
                    result = this.array[this.headIndex];
                }
            }
            return result;
        }

        /// <summary>
        /// 查看指定位置的元素，但是不从队列中移除。
        /// </summary>       
        public T PeekAt(int index)
        {
            T result;
            lock (this.locker)
            {
                T t = default(T);
                if (this.count < index + 1)
                {
                    result = t;
                }
                else
                {
                    int num = (this.headIndex + index) % this.array.Length;
                    result = this.array[num];
                }
            }
            return result;
        }

        public bool Dequeue(out T obj)
        {
            bool result;
            lock (this.locker)
            {
                obj = default(T);
                if (this.count == 0)
                {
                    result = false;
                }
                else
                {
                    obj = this.array[this.headIndex];
                    this.array[this.headIndex] = default(T);
                    this.headIndex = (this.headIndex + 1) % this.array.Length;
                    this.count--;
                    result = true;
                }
            }
            return result;
        }

        public T Dequeue()
        {
            T result;
            this.Dequeue(out result);
            return result;
        }

        public void Clear()
        {
            lock (this.locker)
            {
                this.count = 0;
            }
        }

        /// <summary>
        /// 更改大小。如果当前队列中元素个数大于新的尺寸，则丢弃部分老的元素。
        /// </summary>        
        public void ChangeSize(int newSize)
        {
            if (newSize != this.Size)
            {
                lock (this.locker)
                {
                    T[] array = new T[newSize];
                    int num = this.count - newSize;
                    if (num < 0)
                    {
                        num = 0;
                    }
                    int num2 = this.count - num;
                    for (int i = 0; i < num2; i++)
                    {
                        int num3 = (this.headIndex + i) % this.array.Length;
                        array[i] = this.array[num3];
                    }
                    this.array = array;
                    this.count = num2;
                    this.headIndex = 0;
                    this.tailIndex = this.headIndex + num2 - 1;
                }
            }
        }

        public List<T> GetAll()
        {
            List<T> result;
            lock (this.locker)
            {
                List<T> list = new List<T>();
                for (int i = 0; i < this.count; i++)
                {
                    list.Add(this.array[(this.headIndex + i) % this.array.Length]);
                }
                result = list;
            }
            return result;
        }

        public override string ToString()
        {
            return string.Format("Count:{0},Size:{1}", this.count, this.Size);
        }
    }
}
