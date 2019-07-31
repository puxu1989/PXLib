using PXLib.Helpers;
using PXLib.Threading.Locker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage
{
    /// <summary>
    /// BidirectionalMapping 双向映射。即Key和Value都是唯一的，在这种情况下使用BidirectionalMapping可提升依据Value查找Key的速度。
    /// 该实现是线程安全的。2008.08.20
    /// </summary>    
    public class BidirectionalMapping<T1, T2>
    {
        private IDictionary<T1, T2> dictionary = new Dictionary<T1, T2>();

        private IDictionary<T2, T1> reversedDictionary = new Dictionary<T2, T1>();

        private SmartRWLocker smartRWLocker = new SmartRWLocker();

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }
        /// <summary>
        /// Add 添加映射对。如果已经有相同的key/value存在，则会覆盖。
        /// </summary>    
        public void Add(T1 t1, T2 t2)
        {
            using (this.smartRWLocker.Lock(AccessMode.Write))
            {
                if (this.dictionary.ContainsKey(t1))
                {
                    this.dictionary.Remove(t1);
                }
                this.dictionary.Add(t1, t2);
                if (this.reversedDictionary.ContainsKey(t2))
                {
                    this.reversedDictionary.Remove(t2);
                }
                this.reversedDictionary.Add(t2, t1);
            }
        }

        public void RemoveByT1(T1 t1)
        {
            using (this.smartRWLocker.Lock(AccessMode.Write))
            {
                if (this.dictionary.ContainsKey(t1))
                {
                    T2 key = this.dictionary[t1];
                    this.dictionary.Remove(t1);
                    this.reversedDictionary.Remove(key);
                }
            }
        }

        public void RemoveByT2(T2 t2)
        {
            using (this.smartRWLocker.Lock(AccessMode.Write))
            {
                if (this.reversedDictionary.ContainsKey(t2))
                {
                    T1 key = this.reversedDictionary[t2];
                    this.reversedDictionary.Remove(t2);
                    this.dictionary.Remove(key);
                }
            }
        }

        public T2 GetT2(T1 t1)
        {
            T2 result;
            using (this.smartRWLocker.Lock(AccessMode.Read))
            {
                if (!this.dictionary.ContainsKey(t1))
                {
                    result = default(T2);
                }
                else
                {
                    result = this.dictionary[t1];
                }
            }
            return result;
        }

        public T1 GetT1(T2 t2)
        {
            T1 result;
            using (this.smartRWLocker.Lock(AccessMode.Read))
            {
                if (!this.reversedDictionary.ContainsKey(t2))
                {
                    result = default(T1);
                }
                else
                {
                    result = this.reversedDictionary[t2];
                }
            }
            return result;
        }

        public bool ContainsT1(T1 t1)
        {
            return this.dictionary.ContainsKey(t1);
        }

        public bool ContainsT2(T2 t2)
        {
            return this.reversedDictionary.ContainsKey(t2);
        }

        /// <summary>
        /// GetAllT1ListCopy 返回T1类型元素列表的拷贝。
        /// </summary>  
        public IList<T1> GetAllT1ListCopy()
        {
            IList<T1> result;
            using (this.smartRWLocker.Lock(AccessMode.Read))
            {
                result = CollectionHelper.CopyAllToList<T1>(this.dictionary.Keys);
            }
            return result;
        }

        /// <summary>
        /// GetAllT2ListCopy 返回T2类型元素列表的拷贝。
        /// </summary>    
        public IList<T2> GetAllT2ListCopy()
        {
            IList<T2> result;
            using (this.smartRWLocker.Lock(AccessMode.Read))
            {
                result = CollectionHelper.CopyAllToList<T2>(this.reversedDictionary.Keys);
            }
            return result;
        }
    }
}
