using PXLib.Threading.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage.Caches
{
    public delegate void CbCacheException(IRefreshableCache cache, Exception ee);
    /// <summary>
    /// RefreshableCacheManager 用于管理多个可被刷新的缓存对象，并能定时刷新所管理的缓存。
    /// （1）系统中需要使用一个或多个需要进行定时刷新的缓存。（2）每个缓存所要求的刷新时间间隔可能都不一样。（3）刷新的时间间隔并不要求精确。
    /// </summary>
    public class RefreshableCacheManager :  ICycleEngineActor
    {
        private CycleEngine agileCycleEngine;

        private object locker = new object();

        private IList<IRefreshableCache> cacheList = new List<IRefreshableCache>();

        private int refreshSpanInSecs = 60;
        /// <summary>
        /// 当某个缓存在刷新时，抛出异常，则IRefreshableCacheManager会触发CacheRefreshFailed事件，事件参数包含了出现异常的缓存和异常对象
        /// </summary>
        public event CbCacheException CacheRefreshFailed;


        public IList<IRefreshableCache> CacheList
        {
            set
            {
                this.cacheList = (value ?? new List<IRefreshableCache>());
            }
        }

        public int RefreshSpanInSecs
        {
            set
            {
                this.refreshSpanInSecs = value;
            }
        }

        public RefreshableCacheManager()
        {
            this.CacheRefreshFailed += delegate
            {
            };
        }

        public void Initialize()
        {
            if (this.refreshSpanInSecs <= 0)
            {
                throw new Exception("RefreshSpanInSecs Property must be greater than 0 !");
            }
            foreach (IRefreshableCache current in this.cacheList)
            {
                current.LastRefreshTime = DateTime.Now;
            }
            this.agileCycleEngine = new CycleEngine(this);
            this.agileCycleEngine.DetectSpanInSecs = 1;
            this.agileCycleEngine.Start();
        }

        public void RefreshNow()
        {
            this.EngineAction();
        }

        public void AddCache(IRefreshableCache cache)
        {
            lock (this.locker)
            {
                this.cacheList.Add(cache);
            }
        }

        public void RemoveCache(IRefreshableCache cache)
        {
            lock (this.locker)
            {
                this.cacheList.Remove(cache);
            }
        }

        public bool EngineAction()
        {
            bool result;
            lock (this.locker)
            {
                foreach (IRefreshableCache current in this.cacheList)
                {
                    try
                    {
                        int num = current.RefreshSpanInSecs;
                        if (num <= 0)
                        {
                            num = this.refreshSpanInSecs;
                        }
                        if ((DateTime.Now - current.LastRefreshTime).TotalSeconds >= (double)num)
                        {
                            current.Refresh();
                            current.LastRefreshTime = DateTime.Now;
                        }
                    }
                    catch (Exception ee)
                    {
                        this.CacheRefreshFailed(current, ee);
                    }
                }
                result = true;
            }
            return result;
        }
    }
}
