using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage.Caches
{

    /// <summary>
    /// CircleCache 循环缓存。循环使用缓存中的每个对象。线程安全。
    /// </summary>  
    public class CircleCache<T>
    {
        private object locker = new object();

        private Circle<T> circle = new Circle<T>();

        public CircleCache()
        {
        }

        public CircleCache(ICollection<T> collection)
        {
            if (collection != null && collection.Count > 0)
            {
                foreach (T current in collection)
                {
                    this.circle.Append(current);
                }
            }
        }

        public void Add(T t)
        {
            lock (this.locker)
            {
                this.circle.Append(t);
            }
        }

        public T Get()
        {
            T current;
            lock (this.locker)
            {
                this.circle.MoveNext();
                current = this.circle.Current;
            }
            return current;
        }
    }

}
