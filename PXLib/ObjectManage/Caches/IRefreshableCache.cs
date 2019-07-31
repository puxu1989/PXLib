using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage.Caches
{
    /// <summary>
    /// IRefreshableCache 能够进行刷新的缓存，被IRefreshableCacheManager统一管理。
    /// </summary>
    public interface IRefreshableCache
    {
        /// <summary>
        /// RefreshSpanInSecs 定时刷新的时间间隔（秒）。如果设置为0，则表示与IRefreshableCacheManager的刷新时间统一。
        /// </summary>
        int RefreshSpanInSecs
        {
            get;
        }

        /// <summary>
        /// LastRefreshTime 最后一次刷新时间。
        /// </summary>
        DateTime LastRefreshTime
        {
            get;
            set;
        }
        void Refresh();
    }
}
