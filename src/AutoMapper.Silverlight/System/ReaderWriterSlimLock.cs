
namespace System.Threading
{

    using System;

    //[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    public class ReaderWriterLockSlim : IDisposable
    {
        private bool fIsReentrant;
        private bool fNoWaiters;
        private bool fUpgradeThreadHoldingRead;
        private const int hashTableSize = 0xff;
        private const int LockSleep0Count = 5;
        private const int LockSpinCount = 10;
        private const int LockSpinCycles = 20;
        private const uint MAX_READER = 0xffffffe;
        private const int MaxSpinCount = 20;
        private int myLock;
        private uint numReadWaiters;
        private uint numUpgradeWaiters;
        private uint numWriteUpgradeWaiters;
        private uint numWriteWaiters;
        private uint owners;
        private const uint READER_MASK = 0xfffffff;
        private EventWaitHandle readEvent;
        private ReaderWriterCount[] rwc;
        private EventWaitHandle upgradeEvent;
        private int upgradeLockOwnerId;
        private const uint WAITING_UPGRADER = 0x20000000;
        private const uint WAITING_WRITERS = 0x40000000;
        private EventWaitHandle waitUpgradeEvent;
        private EventWaitHandle writeEvent;
        private int writeLockOwnerId;
        private const uint WRITER_HELD = 0x80000000;

        public ReaderWriterLockSlim()
            : this(LockRecursionPolicy.NoRecursion)
        {
        }

        public ReaderWriterLockSlim(LockRecursionPolicy recursionPolicy)
        {
            if (recursionPolicy == LockRecursionPolicy.SupportsRecursion)
            {
                this.fIsReentrant = true;
            }
            this.InitializeThreadCounts();
        }

        private void ClearUpgraderWaiting()
        {
            this.owners &= 0xdfffffff;
        }

        private void ClearWriterAcquired()
        {
            this.owners &= 0x7fffffff;
        }

        private void ClearWritersWaiting()
        {
            this.owners &= 0xbfffffff;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.writeEvent != null)
                {
                    this.writeEvent.Close();
                    this.writeEvent = null;
                }
                if (this.readEvent != null)
                {
                    this.readEvent.Close();
                    this.readEvent = null;
                }
                if (this.upgradeEvent != null)
                {
                    this.upgradeEvent.Close();
                    this.upgradeEvent = null;
                }
                if (this.waitUpgradeEvent != null)
                {
                    this.waitUpgradeEvent.Close();
                    this.waitUpgradeEvent = null;
                }
            }
        }

        private void EnterMyLock()
        {
            if (Interlocked.CompareExchange(ref this.myLock, 1, 0) != 0)
            {
                this.EnterMyLockSpin();
            }
        }

        private void EnterMyLockSpin()
        {
            int processorCount = Environment.ProcessorCount;
            int num2 = 0;
            while (true)
            {
                if ((num2 < 10) && (processorCount > 1))
                {
                    Thread.SpinWait(20 * (num2 + 1));
                }
                else if (num2 < 15)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    Thread.Sleep(1);
                }
                if ((this.myLock == 0) && (Interlocked.CompareExchange(ref this.myLock, 1, 0) == 0))
                {
                    return;
                }
                num2++;
            }
        }

        public void EnterReadLock()
        {
            this.TryEnterReadLock(-1);
        }

        public void EnterUpgradeableReadLock()
        {
            this.TryEnterUpgradeableReadLock(-1);
        }

        public void EnterWriteLock()
        {
            this.TryEnterWriteLock(-1);
        }

        private void ExitAndWakeUpAppropriateWaiters()
        {
            if (this.fNoWaiters)
            {
                this.ExitMyLock();
            }
            else
            {
                this.ExitAndWakeUpAppropriateWaitersPreferringWriters();
            }
        }

        private void ExitAndWakeUpAppropriateWaitersPreferringWriters()
        {
            bool flag = false;
            bool flag2 = false;
            uint numReaders = this.GetNumReaders();
            if ((this.fIsReentrant && (this.numWriteUpgradeWaiters > 0)) && (this.fUpgradeThreadHoldingRead && (numReaders == 2)))
            {
                this.ExitMyLock();
                this.waitUpgradeEvent.Set();
            }
            else if ((numReaders == 1) && (this.numWriteUpgradeWaiters > 0))
            {
                this.ExitMyLock();
                this.waitUpgradeEvent.Set();
            }
            else if ((numReaders == 0) && (this.numWriteWaiters > 0))
            {
                this.ExitMyLock();
                this.writeEvent.Set();
            }
            else if (numReaders >= 0)
            {
                if ((this.numReadWaiters == 0) && (this.numUpgradeWaiters == 0))
                {
                    this.ExitMyLock();
                }
                else
                {
                    if (this.numReadWaiters != 0)
                    {
                        flag2 = true;
                    }
                    if ((this.numUpgradeWaiters != 0) && (this.upgradeLockOwnerId == -1))
                    {
                        flag = true;
                    }
                    this.ExitMyLock();
                    if (flag2)
                    {
                        this.readEvent.Set();
                    }
                    if (flag)
                    {
                        this.upgradeEvent.Set();
                    }
                }
            }
            else
            {
                this.ExitMyLock();
            }
        }

        private void ExitMyLock()
        {
            this.myLock = 0;
        }

        public void ExitReadLock()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            ReaderWriterCount threadRWCount = null;
            this.EnterMyLock();
            threadRWCount = this.GetThreadRWCount(id, true);
            if (!this.fIsReentrant)
            {
                if (threadRWCount == null)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockExceptionEx("SynchronizationLockException_MisMatchedRead");
                }
            }
            else
            {
                if ((threadRWCount == null) || (threadRWCount.readercount < 1))
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockExceptionEx("SynchronizationLockException_MisMatchedRead");
                }
                if (threadRWCount.readercount > 1)
                {
                    threadRWCount.readercount--;
                    this.ExitMyLock();
                    return;
                }
                if (id == this.upgradeLockOwnerId)
                {
                    this.fUpgradeThreadHoldingRead = false;
                }
            }
            this.owners--;
            threadRWCount.readercount--;
            this.ExitAndWakeUpAppropriateWaiters();
        }

        public void ExitUpgradeableReadLock()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (id != this.upgradeLockOwnerId)
                {
                    throw new SynchronizationLockExceptionEx("SynchronizationLockException_MisMatchedUpgrade");
                }
                this.EnterMyLock();
            }
            else
            {
                this.EnterMyLock();
                ReaderWriterCount threadRWCount = this.GetThreadRWCount(id, true);
                if (threadRWCount == null)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockExceptionEx("SynchronizationLockException_MisMatchedUpgrade");
                }
                RecursiveCounts rc = threadRWCount.rc;
                if (rc.upgradecount < 1)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockExceptionEx("SynchronizationLockException_MisMatchedUpgrade");
                }
                rc.upgradecount--;
                if (rc.upgradecount > 0)
                {
                    this.ExitMyLock();
                    return;
                }
                this.fUpgradeThreadHoldingRead = false;
            }
            this.owners--;
            this.upgradeLockOwnerId = -1;
            this.ExitAndWakeUpAppropriateWaiters();
        }

        public void ExitWriteLock()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (id != this.writeLockOwnerId)
                {
                    throw new SynchronizationLockExceptionEx("SynchronizationLockException_MisMatchedWrite");
                }
                this.EnterMyLock();
            }
            else
            {
                this.EnterMyLock();
                ReaderWriterCount threadRWCount = this.GetThreadRWCount(id, false);
                if (threadRWCount == null)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockExceptionEx("SynchronizationLockException_MisMatchedWrite");
                }
                RecursiveCounts rc = threadRWCount.rc;
                if (rc.writercount < 1)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockExceptionEx("SynchronizationLockException_MisMatchedWrite");
                }
                rc.writercount--;
                if (rc.writercount > 0)
                {
                    this.ExitMyLock();
                    return;
                }
            }
            this.ClearWriterAcquired();
            this.writeLockOwnerId = -1;
            this.ExitAndWakeUpAppropriateWaiters();
        }

        private uint GetNumReaders()
        {
            return (this.owners & 0xfffffff);
        }

        private ReaderWriterCount GetThreadRWCount(int id, bool DontAllocate)
        {
            ReaderWriterCount rwc;
            int index = id & 0xff;
            ReaderWriterCount count = null;
            if (this.rwc[index].threadid == id)
            {
                return this.rwc[index];
            }
            if (IsRWEntryEmpty(this.rwc[index]) && !DontAllocate)
            {
                if (this.rwc[index].next == null)
                {
                    this.rwc[index].threadid = id;
                    return this.rwc[index];
                }
                count = this.rwc[index];
            }
            for (rwc = this.rwc[index].next; rwc != null; rwc = rwc.next)
            {
                if (rwc.threadid == id)
                {
                    return rwc;
                }
                if ((count == null) && IsRWEntryEmpty(rwc))
                {
                    count = rwc;
                }
            }
            if (DontAllocate)
            {
                return null;
            }
            if (count == null)
            {
                rwc = new ReaderWriterCount(this.fIsReentrant);
                rwc.threadid = id;
                rwc.next = this.rwc[index].next;
                this.rwc[index].next = rwc;
                return rwc;
            }
            count.threadid = id;
            return count;
        }

        private void InitializeThreadCounts()
        {
            this.rwc = new ReaderWriterCount[0x100];
            for (int i = 0; i < this.rwc.Length; i++)
            {
                this.rwc[i] = new ReaderWriterCount(this.fIsReentrant);
            }
            this.upgradeLockOwnerId = -1;
            this.writeLockOwnerId = -1;
        }

        private static bool IsRWEntryEmpty(ReaderWriterCount rwc)
        {
            return ((rwc.threadid == -1) || (((rwc.readercount == 0) && (rwc.rc == null)) || (((rwc.readercount == 0) && (rwc.rc.writercount == 0)) && (rwc.rc.upgradecount == 0))));
        }

        private static bool IsRwHashEntryChanged(ReaderWriterCount lrwc, int id)
        {
            return (lrwc.threadid != id);
        }

        private bool IsWriterAcquired()
        {
            return ((this.owners & 0xbfffffff) == 0);
        }

        private void LazyCreateEvent(ref EventWaitHandle waitEvent, bool makeAutoResetEvent)
        {
            EventWaitHandle handle;
            this.ExitMyLock();
            if (makeAutoResetEvent)
            {
                handle = new AutoResetEvent(false);
            }
            else
            {
                handle = new ManualResetEvent(false);
            }
            this.EnterMyLock();
            if (waitEvent == null)
            {
                waitEvent = handle;
            }
            else
            {
                handle.Close();
            }
        }

        private void SetUpgraderWaiting()
        {
            this.owners |= 0x20000000;
        }

        private void SetWriterAcquired()
        {
            this.owners |= 0x80000000;
        }

        private void SetWritersWaiting()
        {
            this.owners |= 0x40000000;
        }

        private static void SpinWait(int SpinCount)
        {
            if ((SpinCount < 5) && (Environment.ProcessorCount > 1))
            {
                Thread.SpinWait(20 * SpinCount);
            }
            else if (SpinCount < 0x11)
            {
                Thread.Sleep(0);
            }
            else
            {
                Thread.Sleep(1);
            }
        }

        public bool TryEnterReadLock(int millisecondsTimeout)
        {
            ReaderWriterCount lrwc = null;
            int id = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (id == this.writeLockOwnerId)
                {
                    throw new LockRecursionException("LockRecursionException_ReadAfterWriteNotAllowed");
                }
                this.EnterMyLock();
                lrwc = this.GetThreadRWCount(id, false);
                if (lrwc.readercount > 0)
                {
                    this.ExitMyLock();
                    throw new LockRecursionException("LockRecursionException_RecursiveReadNotAllowed");
                }
                if (id == this.upgradeLockOwnerId)
                {
                    lrwc.readercount++;
                    this.owners++;
                    this.ExitMyLock();
                    return true;
                }
            }
            else
            {
                this.EnterMyLock();
                lrwc = this.GetThreadRWCount(id, false);
                if (lrwc.readercount > 0)
                {
                    lrwc.readercount++;
                    this.ExitMyLock();
                    return true;
                }
                if (id == this.upgradeLockOwnerId)
                {
                    lrwc.readercount++;
                    this.owners++;
                    this.ExitMyLock();
                    this.fUpgradeThreadHoldingRead = true;
                    return true;
                }
                if (id == this.writeLockOwnerId)
                {
                    lrwc.readercount++;
                    this.owners++;
                    this.ExitMyLock();
                    return true;
                }
            }
            bool flag = true;
            int spinCount = 0;
        Label_011F:
            if (this.owners < 0xffffffe)
            {
                this.owners++;
                lrwc.readercount++;
            }
            else
            {
                if (spinCount < 20)
                {
                    this.ExitMyLock();
                    if (millisecondsTimeout == 0)
                    {
                        return false;
                    }
                    spinCount++;
                    SpinWait(spinCount);
                    this.EnterMyLock();
                    if (IsRwHashEntryChanged(lrwc, id))
                    {
                        lrwc = this.GetThreadRWCount(id, false);
                    }
                }
                else if (this.readEvent == null)
                {
                    this.LazyCreateEvent(ref this.readEvent, false);
                    if (IsRwHashEntryChanged(lrwc, id))
                    {
                        lrwc = this.GetThreadRWCount(id, false);
                    }
                }
                else
                {
                    flag = this.WaitOnEvent(this.readEvent, ref this.numReadWaiters, millisecondsTimeout);
                    if (!flag)
                    {
                        return false;
                    }
                    if (IsRwHashEntryChanged(lrwc, id))
                    {
                        lrwc = this.GetThreadRWCount(id, false);
                    }
                }
                goto Label_011F;
            }
            this.ExitMyLock();
            return flag;
        }

        public bool TryEnterReadLock(TimeSpan timeout)
        {
            int millisecondsTimeout = (int)timeout.TotalMilliseconds;
            if ((millisecondsTimeout < -1) || (millisecondsTimeout > 0x7fffffff))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            return this.TryEnterReadLock(millisecondsTimeout);
        }

        public bool TryEnterUpgradeableReadLock(int millisecondsTimeout)
        {
            ReaderWriterCount lrwc;
            int id = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (id == this.upgradeLockOwnerId)
                {
                    throw new LockRecursionException("LockRecursionException_RecursiveUpgradeNotAllowed");
                }
                if (id == this.writeLockOwnerId)
                {
                    throw new LockRecursionException("LockRecursionException_UpgradeAfterWriteNotAllowed");
                }
                this.EnterMyLock();
                lrwc = this.GetThreadRWCount(id, true);
                if ((lrwc != null) && (lrwc.readercount > 0))
                {
                    this.ExitMyLock();
                    throw new LockRecursionException("LockRecursionException_UpgradeAfterReadNotAllowed");
                }
            }
            else
            {
                this.EnterMyLock();
                lrwc = this.GetThreadRWCount(id, false);
                if (id == this.upgradeLockOwnerId)
                {
                    lrwc.rc.upgradecount++;
                    this.ExitMyLock();
                    return true;
                }
                if (id == this.writeLockOwnerId)
                {
                    this.owners++;
                    this.upgradeLockOwnerId = id;
                    lrwc.rc.upgradecount++;
                    if (lrwc.readercount > 0)
                    {
                        this.fUpgradeThreadHoldingRead = true;
                    }
                    this.ExitMyLock();
                    return true;
                }
                if (lrwc.readercount > 0)
                {
                    this.ExitMyLock();
                    throw new LockRecursionException("LockRecursionException_UpgradeAfterReadNotAllowed");
                }
            }
            int spinCount = 0;
        Label_011B:
            if ((this.upgradeLockOwnerId == -1) && (this.owners < 0xffffffe))
            {
                this.owners++;
                this.upgradeLockOwnerId = id;
            }
            else
            {
                if (spinCount < 20)
                {
                    this.ExitMyLock();
                    if (millisecondsTimeout == 0)
                    {
                        return false;
                    }
                    spinCount++;
                    SpinWait(spinCount);
                    this.EnterMyLock();
                    goto Label_011B;
                }
                if (this.upgradeEvent == null)
                {
                    this.LazyCreateEvent(ref this.upgradeEvent, true);
                    goto Label_011B;
                }
                if (this.WaitOnEvent(this.upgradeEvent, ref this.numUpgradeWaiters, millisecondsTimeout))
                {
                    goto Label_011B;
                }
                return false;
            }
            if (this.fIsReentrant)
            {
                if (IsRwHashEntryChanged(lrwc, id))
                {
                    lrwc = this.GetThreadRWCount(id, false);
                }
                lrwc.rc.upgradecount++;
            }
            this.ExitMyLock();
            return true;
        }

        public bool TryEnterUpgradeableReadLock(TimeSpan timeout)
        {
            int millisecondsTimeout = (int)timeout.TotalMilliseconds;
            if ((millisecondsTimeout < -1) || (millisecondsTimeout > 0x7fffffff))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            return this.TryEnterUpgradeableReadLock(millisecondsTimeout);
        }

        public bool TryEnterWriteLock(int millisecondsTimeout)
        {
            ReaderWriterCount lrwc;
            int id = Thread.CurrentThread.ManagedThreadId;
            bool flag = false;
            if (!this.fIsReentrant)
            {
                if (id == this.writeLockOwnerId)
                {
                    throw new LockRecursionException("LockRecursionException_RecursiveWriteNotAllowed");
                }
                if (id == this.upgradeLockOwnerId)
                {
                    flag = true;
                }
                this.EnterMyLock();
                lrwc = this.GetThreadRWCount(id, true);
                if ((lrwc != null) && (lrwc.readercount > 0))
                {
                    this.ExitMyLock();
                    throw new LockRecursionException("LockRecursionException_WriteAfterReadNotAllowed");
                }
            }
            else
            {
                this.EnterMyLock();
                lrwc = this.GetThreadRWCount(id, false);
                if (id == this.writeLockOwnerId)
                {
                    lrwc.rc.writercount++;
                    this.ExitMyLock();
                    return true;
                }
                if (id == this.upgradeLockOwnerId)
                {
                    flag = true;
                }
                else if (lrwc.readercount > 0)
                {
                    this.ExitMyLock();
                    throw new LockRecursionException("LockRecursionException_WriteAfterReadNotAllowed");
                }
            }
            int spinCount = 0;
        Label_00CE:
            if (this.IsWriterAcquired())
            {
                this.SetWriterAcquired();
            }
            else
            {
                if (flag)
                {
                    uint numReaders = this.GetNumReaders();
                    if (numReaders == 1)
                    {
                        this.SetWriterAcquired();
                        goto Label_01BF;
                    }
                    if ((numReaders == 2) && (lrwc != null))
                    {
                        if (IsRwHashEntryChanged(lrwc, id))
                        {
                            lrwc = this.GetThreadRWCount(id, false);
                        }
                        if (lrwc.readercount > 0)
                        {
                            this.SetWriterAcquired();
                            goto Label_01BF;
                        }
                    }
                }
                if (spinCount < 20)
                {
                    this.ExitMyLock();
                    if (millisecondsTimeout == 0)
                    {
                        return false;
                    }
                    spinCount++;
                    SpinWait(spinCount);
                    this.EnterMyLock();
                    goto Label_00CE;
                }
                if (flag)
                {
                    if (this.waitUpgradeEvent != null)
                    {
                        if (!this.WaitOnEvent(this.waitUpgradeEvent, ref this.numWriteUpgradeWaiters, millisecondsTimeout))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        this.LazyCreateEvent(ref this.waitUpgradeEvent, true);
                    }
                    goto Label_00CE;
                }
                if (this.writeEvent == null)
                {
                    this.LazyCreateEvent(ref this.writeEvent, true);
                    goto Label_00CE;
                }
                if (this.WaitOnEvent(this.writeEvent, ref this.numWriteWaiters, millisecondsTimeout))
                {
                    goto Label_00CE;
                }
                return false;
            }
        Label_01BF:
            if (this.fIsReentrant)
            {
                if (IsRwHashEntryChanged(lrwc, id))
                {
                    lrwc = this.GetThreadRWCount(id, false);
                }
                lrwc.rc.writercount++;
            }
            this.ExitMyLock();
            this.writeLockOwnerId = id;
            return true;
        }

        public bool TryEnterWriteLock(TimeSpan timeout)
        {
            int millisecondsTimeout = (int)timeout.TotalMilliseconds;
            if ((millisecondsTimeout < -1) || (millisecondsTimeout > 0x7fffffff))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            return this.TryEnterWriteLock(millisecondsTimeout);
        }

        private bool WaitOnEvent(EventWaitHandle waitEvent, ref uint numWaiters, int millisecondsTimeout)
        {
            waitEvent.Reset();
            numWaiters++;
            this.fNoWaiters = false;
            if (this.numWriteWaiters == 1)
            {
                this.SetWritersWaiting();
            }
            if (this.numWriteUpgradeWaiters == 1)
            {
                this.SetUpgraderWaiting();
            }
            bool flag = false;
            this.ExitMyLock();
            try
            {
                flag = waitEvent.WaitOne(millisecondsTimeout);//, false);
            }
            finally
            {
                this.EnterMyLock();
                numWaiters--;
                if (((this.numWriteWaiters == 0) && (this.numWriteUpgradeWaiters == 0)) && ((this.numUpgradeWaiters == 0) && (this.numReadWaiters == 0)))
                {
                    this.fNoWaiters = true;
                }
                if (this.numWriteWaiters == 0)
                {
                    this.ClearWritersWaiting();
                }
                if (this.numWriteUpgradeWaiters == 0)
                {
                    this.ClearUpgraderWaiting();
                }
                if (!flag)
                {
                    this.ExitMyLock();
                }
            }
            return flag;
        }

        public int CurrentReadCount
        {
            get
            {
                int numReaders = (int)this.GetNumReaders();
                if (this.upgradeLockOwnerId != -1)
                {
                    return (numReaders - 1);
                }
                return numReaders;
            }
        }

        public bool IsReadLockHeld
        {
            get
            {
                return (this.RecursiveReadCount > 0);
            }
        }

        public bool IsUpgradeableReadLockHeld
        {
            get
            {
                return (this.RecursiveUpgradeCount > 0);
            }
        }

        public bool IsWriteLockHeld
        {
            get
            {
                return (this.RecursiveWriteCount > 0);
            }
        }

        private bool MyLockHeld
        {
            get
            {
                return (this.myLock != 0);
            }
        }

        public LockRecursionPolicy RecursionPolicy
        {
            get
            {
                if (this.fIsReentrant)
                {
                    return LockRecursionPolicy.SupportsRecursion;
                }
                return LockRecursionPolicy.NoRecursion;
            }
        }

        public int RecursiveReadCount
        {
            get
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                int readercount = 0;
                this.EnterMyLock();
                ReaderWriterCount threadRWCount = this.GetThreadRWCount(id, true);
                if (threadRWCount != null)
                {
                    readercount = threadRWCount.readercount;
                }
                this.ExitMyLock();
                return readercount;
            }
        }

        public int RecursiveUpgradeCount
        {
            get
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                if (this.fIsReentrant)
                {
                    int upgradecount = 0;
                    this.EnterMyLock();
                    ReaderWriterCount threadRWCount = this.GetThreadRWCount(id, true);
                    if (threadRWCount != null)
                    {
                        upgradecount = threadRWCount.rc.upgradecount;
                    }
                    this.ExitMyLock();
                    return upgradecount;
                }
                if (id == this.upgradeLockOwnerId)
                {
                    return 1;
                }
                return 0;
            }
        }

        public int RecursiveWriteCount
        {
            get
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                int writercount = 0;
                if (this.fIsReentrant)
                {
                    this.EnterMyLock();
                    ReaderWriterCount threadRWCount = this.GetThreadRWCount(id, true);
                    if (threadRWCount != null)
                    {
                        writercount = threadRWCount.rc.writercount;
                    }
                    this.ExitMyLock();
                    return writercount;
                }
                if (id == this.writeLockOwnerId)
                {
                    return 1;
                }
                return 0;
            }
        }

        public int WaitingReadCount
        {
            get
            {
                return (int)this.numReadWaiters;
            }
        }

        public int WaitingUpgradeCount
        {
            get
            {
                return (int)this.numUpgradeWaiters;
            }
        }

        public int WaitingWriteCount
        {
            get
            {
                return (int)this.numWriteWaiters;
            }
        }
    }

    internal class ReaderWriterCount
    {
        public ReaderWriterCount next;
        public RecursiveCounts rc;
        public int readercount;
        public int threadid = -1;

        public ReaderWriterCount(bool fIsReentrant)
        {
            if (fIsReentrant)
            {
                this.rc = new RecursiveCounts();
            }
        }
    }

    internal class RecursiveCounts
    {
        public int upgradecount;
        public int writercount;
    }

}
