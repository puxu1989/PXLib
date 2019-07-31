using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage
{
    /// <summary>
    /// UniqueObjectList 保证list中的每个object都是唯一的，不会重复。该接口的实现保证是线程安全的。
    /// </summary>   
    public class UniqueObjectList<T>
    {
        private IList<T> innerList = new List<T>();

        private object locker = new object();

        public void Add(T obj)
        {
            lock (this.locker)
            {
                if (!this.innerList.Contains(obj))
                {
                    this.innerList.Add(obj);
                }
            }
        }

        public void Remove(T obj)
        {
            lock (this.locker)
            {
                this.innerList.Remove(obj);
            }
        }
        public void Clear()
        {
            lock (this.locker)
            {
                this.innerList.Clear();
            }
        }
        public IList<T> GetListCopy()
        {
            IList<T> result;
            lock (this.locker)
            {
                result = CollectionHelper.CopyAllToList<T>(this.innerList);
            }
            return result;
        }
    }
}
