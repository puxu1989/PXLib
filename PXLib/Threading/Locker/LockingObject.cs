using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PXLib.Threading.Locker
{
    /// <summary>
    /// LockingObject SmartReaderWriterLocker的Lock方法返回的锁对象。仅仅通过using来使用该对象，如：using(this.smartLocker.Lock(AccessMode.Read)){...}
    /// </summary>
    public class LockingObject : IDisposable
    {
        private ReaderWriterLock readerWriterLock;

        private AccessMode accessMode = AccessMode.Read;

        private LockCookie lockCookie;

        public LockingObject(ReaderWriterLock _lock, AccessMode _lockMode)
        {
            this.readerWriterLock = _lock;
            this.accessMode = _lockMode;
            if (this.accessMode == AccessMode.Read)
            {
                this.readerWriterLock.AcquireReaderLock(-1);
            }
            else if (this.accessMode == AccessMode.Write)
            {
                this.readerWriterLock.AcquireWriterLock(-1);
            }
            else
            {
                this.lockCookie = this.readerWriterLock.UpgradeToWriterLock(-1);
            }
        }

        public void Dispose()
        {
            if (this.accessMode == AccessMode.Read)
            {
                this.readerWriterLock.ReleaseReaderLock();
            }
            else if (this.accessMode == AccessMode.Write)
            {
                this.readerWriterLock.ReleaseWriterLock();
            }
            else
            {
                this.readerWriterLock.DowngradeFromWriterLock(ref this.lockCookie);
            }
        }
    }
}
