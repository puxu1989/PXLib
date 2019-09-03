using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.ObjectManage.ObjectManager
{

    [Serializable]
    /// <summary>
    /// 线程安全的字典对象管理器
    /// </summary>
    /// <typeparam name="TPKey"></typeparam>
    /// <typeparam name="TObject"></typeparam>
    public class ObjectManager<TPKey, TObject> : IObjectManager<TPKey, TObject>
    {
        protected IDictionary<TPKey, TObject> objectDictionary = new Dictionary<TPKey, TObject>();//管理器的容器
        [NonSerialized]
        private List<TObject> readonlyCopy = null;//只读对象列表
        [NonSerialized]
        protected object locker = new object();//访问锁，用于线程安全

        public event Action<TObject> ObjectRegistered;
        public event Action<TObject> ObjectUnregistered;

        public ObjectManager() 
        { 
             //注册事件（匿名）
            this.ObjectRegistered += delegate { };
            this.ObjectUnregistered += delegate { };
        }

        public int Count
        {
            get
            {
                return this.objectDictionary.Count;
            }      
        }
        /// <summary>
        /// 返回的列表不能被修改。【使用缓存】
        /// </summary>  
        public List<TObject> GetAllReadonly()
        {
            lock (this.locker)
            {
                if (this.readonlyCopy == null)
                {
                    return CollectionHelper.CopyAllToList<TObject>(this.objectDictionary.Values);
                }

                return this.readonlyCopy;
            }
        }
        public virtual void Add(TPKey key, TObject obj)
        {
            lock (this.locker)
            {
                if (this.objectDictionary.ContainsKey(key))
                {
                    this.objectDictionary.Remove(key);
                }

                this.objectDictionary.Add(key, obj);

                this.readonlyCopy = null;
            }
            this.ObjectRegistered(obj);
        }

        public virtual void Remove(TPKey id)
        {
            TObject target = default(TObject);
            lock (this.locker)
            {
                if (this.objectDictionary.ContainsKey(id))
                {
                    target = this.objectDictionary[id];
                    this.objectDictionary.Remove(id);
                    this.readonlyCopy = null;
                }
            }

            if (target != null)
            {
                this.ObjectUnregistered(target);
            }
        }

        public virtual void RemoveByValue(TObject val)
        {
            lock (this.locker)
            {
                List<TPKey> keyList = new List<TPKey>(this.objectDictionary.Keys);
                foreach (TPKey key in keyList)
                {
                    if (this.objectDictionary[key].Equals(val))
                    {
                        this.objectDictionary.Remove(key);
                    }
                }
                this.readonlyCopy = null;
            }
        }

        public void RemoveByPredication(Predicate<TObject> predicate)
        {
            lock (this.locker)
            {
                List<TPKey> keyList = new List<TPKey>(this.objectDictionary.Keys);
                foreach (TPKey key in keyList)
                {
                    if (predicate(this.objectDictionary[key]))
                    {
                        this.objectDictionary.Remove(key);
                    }
                }

                this.readonlyCopy = null;
            }
        }

        public virtual void Clear()
        {
            lock (this.locker)
            {
                this.objectDictionary.Clear();
                this.readonlyCopy = null;
            }
        }

        public TObject Get(TPKey id)
        {
            lock (this.locker)
            {
                if (this.objectDictionary.ContainsKey(id))
                {
                    return this.objectDictionary[id];
                }
            }

            return default(TObject);
        }

        public bool Contains(TPKey id)
        {
            if (id == null) 
            {
                return false;
            }
            lock (this.locker)
            {
                return this.objectDictionary.ContainsKey(id);
            }
        }

        public List<TObject> GetAll()
        {
            lock (this.locker)
            {
                return objectDictionary.Values.ToList();
            }
        }

        public List<TPKey> GetKeyList()
        {
            lock (this.locker)
            {
                return CollectionHelper.CopyAllToList<TPKey>(this.objectDictionary.Keys);
            }
        }

        public List<TPKey> GetKeyListByObj(TObject obj)
        {
            lock (this.locker)
            {
                List<TPKey> list = new List<TPKey>();
                foreach (TPKey key in this.GetKeyList())
                {
                    if (this.objectDictionary[key].Equals(obj))
                    {
                        list.Add(key);
                    }
                }

                return list;
            }
        }

        public Dictionary<TPKey, TObject> ToDictionary()
        {
            lock (this.locker)
            {
                return new Dictionary<TPKey, TObject>(this.objectDictionary);
            }
        }
    }
}
