using PXLib.Helpers;
using PXLib.Threading.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage.Caches
{

     
    /// <summary>
    ///  用于缓存那些活跃的对象，并定时删除不活跃的对象。该接口的实现必须是线程安全的。
    ///  
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TObject"></typeparam>
    public class HotCache<TKey, TObject> : ICycleEngineActor where TObject : class
    {

        #region 包
        private sealed class CachePackage<TKey, TObject>
        {
            private TKey iD;

            private TObject target;

            private DateTime lastAccessTime = DateTime.Now;

            public TKey ID
            {
                get
                {
                    return this.iD;
                }
            }

            public TObject Target
            {
                get
                {
                    this.lastAccessTime = DateTime.Now;
                    return this.target;
                }
            }

            public DateTime LastAccessTime
            {
                get
                {
                    return this.lastAccessTime;
                }
            }

            public CachePackage(TKey _iD, TObject _target)
            {
                this.iD = _iD;
                this.target = _target;
            }
        }
        #endregion
        private IDictionary<TKey, HotCache<TKey, TObject>.CachePackage<TKey, TObject>> dictionary = new Dictionary<TKey, HotCache<TKey, TObject>.CachePackage<TKey, TObject>>();

        private object locker = new object();

        private CycleEngine agileCycleEngine;

        private IObjectRetriever<TKey, TObject> objectRetriever;

        private int maxCachedCount = int.MaxValue;

        private int maxMuteSpanInMinutes = 10;

        private int detectSpanInSecs = 600;

        private long requestCount = 0L;

        private long hitCount = 0L;

        private long nonexistentCount = 0L;

        private DateTime lastReadTime = DateTime.Now;

        public event Action CacheContentChanged;

        public IObjectRetriever<TKey, TObject> ObjectRetriever
        {
            set
            {
                this.objectRetriever = value;
            }
        }
        /// <summary>
        /// MaxCachedCount 最多缓存的对象个数。当超过此个数时，不再缓存新的对象。
        /// </summary>
        public int MaxCachedCount
        {
            get
            {
                return this.maxCachedCount;
            }
            set
            {
                this.maxCachedCount = value;
            }
        }
        /// <summary>
        /// MaxMuteSpanInMinutes 对象最大的沉默时间（分钟）。如果一个对象在MaxMuteSpanInMinutes时间间隔内都不被访问，则将被从缓存中清除。
        /// 如果该属性的值被设置为小于或等于0，则表示永远不会从缓存中清除。
        /// </summary>
        public int MaxMuteSpanInMinutes
        {
            set
            {
                this.maxMuteSpanInMinutes = value;
            }
        }
        /// <summary>
        /// DetectSpanInSecs 多长时间检测一次对象是否活跃，单位：秒。
        /// </summary>

        public int DetectSpanInSecs
        {
            set
            {
                this.detectSpanInSecs = value;
            }
        }

       
        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }
        //表示通过Get获取对象的次数
        public long RequestCount
        {
            get
            {
                return this.requestCount;
            }
        }
        //Get方法后除了不通过ObjectRetriever获取对象存在的次数
        public long HitCount
        {
            get
            {
                return this.hitCount;
            }
        }

        /// <summary>
        /// NonexistentCount Get方法返回为不存在对象的次数。
        /// </summary>
        public long NonExistentCount
        {
            get
            {
                return this.nonexistentCount;
            }
        }

        public DateTime LastReadTime
        {
            get
            {
                return this.lastReadTime;
            }
        }

        public HotCache()
        {
            this.CacheContentChanged += delegate
            {
            };
        }
        /// <summary>
        /// 初始化之前先初始化其他Set属性
        /// </summary>
        public void Initialize()
        {
            if (this.maxMuteSpanInMinutes > 0)
            {
                this.agileCycleEngine = new CycleEngine(this);
                this.agileCycleEngine.DetectSpanInSecs = this.detectSpanInSecs;
                this.agileCycleEngine.Start();
            }
        }
        /// <summary>
        /// Get 如果缓存中存在目标则直接返回，否则通过ObjectRetriever提取对象并缓存。
        /// </summary>      
        public TObject Get(TKey id)
        {
            TObject result;
            lock (this.locker)
            {
                this.lastReadTime = DateTime.Now;
                ++this.requestCount;
                if (this.dictionary.ContainsKey(id))
                {
                    ++this.hitCount;
                    result = this.dictionary[id].Target;
                }
                else
                {
                    TObject tObject = this.objectRetriever.Retrieve(id);
                    if (tObject == null)
                    {
                        ++this.nonexistentCount;
                    }
                    if (tObject != null && this.dictionary.Count < this.maxCachedCount)
                    {
                        this.dictionary.Add(id, new HotCache<TKey, TObject>.CachePackage<TKey, TObject>(id, tObject));
                        this.CacheContentChanged();
                    }
                    result = tObject;
                }
            }
            return result;
        }

        public IList<TObject> GetAll()
        {
            this.lastReadTime = DateTime.Now;
            return CollectionHelper.ConvertAll<HotCache<TKey, TObject>.CachePackage<TKey, TObject>, TObject>(this.dictionary.Values, (HotCache<TKey, TObject>.CachePackage<TKey, TObject> package) => package.Target);
        }

        public void Clear()
        {
            lock (this.locker)
            {
                this.dictionary.Clear();
                this.CacheContentChanged();
            }
        }

        public void Add(TKey id, TObject obj)
        {
            lock (this.locker)
            {
                if (this.dictionary.ContainsKey(id))
                {
                    this.dictionary.Remove(id);
                }
                this.dictionary.Add(id, new HotCache<TKey, TObject>.CachePackage<TKey, TObject>(id, obj));
                this.CacheContentChanged();
            }
        }

        public void Remove(TKey id)
        {
            lock (this.locker)
            {
                if (this.dictionary.ContainsKey(id))
                {
                    this.dictionary.Remove(id);
                    this.CacheContentChanged();
                }
            }
        }

        public bool EngineAction()
        {
            lock (this.locker)
            {
                DateTime now = DateTime.Now;
                IList<TKey> list = new List<TKey>();
                foreach (HotCache<TKey, TObject>.CachePackage<TKey, TObject> current in this.dictionary.Values)
                {
                    if ((now - current.LastAccessTime).TotalMinutes > (double)this.maxMuteSpanInMinutes)
                    {
                        list.Add(current.ID);
                    }
                }
                foreach (TKey current2 in list)
                {
                    this.dictionary.Remove(current2);
                }
                if (list.Count > 0)
                {
                    this.CacheContentChanged();
                }
            }
            return true;
        }
    }
}
