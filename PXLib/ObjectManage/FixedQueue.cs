using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage
{
    /// <summary>
    /// FixedQueue 固定大小的队列，当到达Capacity时，缓存新object，则最先缓存的object会被删除掉。
    /// FixedQueue是线程安全的。
    /// </summary>  
    public class FixedQueue<T>
    {
        private Queue<T> queue = new Queue<T>();

        private object locker = new object();

        private int capacity = int.MaxValue;

        public event Action<T> ObjectDiscarded;

        public int Capacity
        {
            get
            {
                return this.capacity;
            }
            set
            {
                if (value < 0)
                {
                    throw new Exception("The capacity of FixedQueue can't be less than 0 !");
                }
                lock (this.locker)
                {
                    if (value == 0)
                    {
                        this.queue.Clear();
                        this.capacity = value;
                    }
                    else if (value > 0)
                    {
                        this.capacity = value;
                        while (this.queue.Count > this.capacity)
                        {
                            this.ObjectDiscarded(this.queue.Dequeue());
                        }
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }
        public FixedQueue(int _capacity)
        {
            this.capacity = _capacity;
            this.ObjectDiscarded += delegate { };
        }

        public void Enqueue(T obj)
        {
            if (this.capacity > 0)
            {
                lock (this.locker)
                {
                    if (this.queue.Count >= this.capacity)
                    {
                        this.ObjectDiscarded(this.queue.Dequeue());
                    }
                    this.queue.Enqueue(obj);
                }
            }
        }

        public T Dequeue()
        {
            T result;
            lock (this.locker)
            {
                result = this.queue.Dequeue();
            }
            return result;
        }

        public T Peek()
        {
            T result;
            lock (this.locker)
            {
                result = this.queue.Peek();
            }
            return result;
        }

        public void Remove(T obj)
        {
            lock (this.locker)
            {
                Queue<T> queue = new Queue<T>();
                while (this.queue.Count > 0)
                {
                    T item = this.queue.Dequeue();
                    if (!item.Equals(obj))
                    {
                        queue.Enqueue(item);
                    }
                }
                this.queue = queue;
            }
        }

        public T[] GetObjectArrayCopy()
        {
            T[] result;
            lock (this.locker)
            {
                result = this.queue.ToArray();
            }
            return result;
        }

        public void Clear()
        {
            lock (this.locker)
            {
                this.queue.Clear();
            }
        }
    }
}
