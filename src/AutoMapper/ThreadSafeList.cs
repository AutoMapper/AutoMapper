using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Internal;

namespace AutoMapper
{
    public class ThreadSafeList<T> : IList<T>, IDisposable
        where T : class
    {
        private static readonly IReaderWriterLockSlimFactory ReaderWriterLockSlimFactory =
            PlatformAdapter.Resolve<IReaderWriterLockSlimFactory>();

        private IReaderWriterLockSlim _lock = ReaderWriterLockSlimFactory.Create();
        private readonly IList<T> _propertyMaps = new List<T>();
        private bool _disposed;

        #region Synchronized Wrappers
        /// <summary>
        /// Synchronized update function
        /// </summary>
        /// <param name="changeFunc">Update function</param>
        public TR SyncChange<TR>(Func<IList<T>, TR> changeFunc)
        {
            if (changeFunc == null)
                throw new ArgumentNullException();

            _lock.EnterWriteLock();
            try
            {
                return changeFunc(_propertyMaps);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Synchronized update action
        /// </summary>
        /// <param name="changeAction">Update action</param>
        public void SyncChange(Action<IList<T>> changeAction)
        {
            if (changeAction == null)
                throw new ArgumentNullException();
            SyncChange(list =>
            {
                changeAction(list);
                return 0;
            });
        }

        /// <summary>
        /// Synchronized get function
        /// </summary>
        /// <param name="getFunc">Get function</param>
        public TR SyncGet<TR>(Func<IList<T>, TR> getFunc)
        {
            if (getFunc == null)
                throw new ArgumentNullException();

            _lock.EnterReadLock();
            try
            {
                return getFunc(_propertyMaps);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Synchronized action
        /// </summary>
        /// <param name="action">Action</param>
        public void SyncGet(Action<IList<T>> action)
        {
            if (action == null)
                throw new ArgumentNullException();
            SyncGet(list =>
            {
                action(list);
                return 0;
            });
        }
        #endregion

        public void Add(T propertyMap)
        {
            SyncChange(list => list.Add(propertyMap));
        }

        public T GetOrCreate(Predicate<T> predicate, Func<T> creatorFunc)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                var propertyMap = _propertyMaps.FirstOrDefault(pm => predicate(pm));

                if (propertyMap == null)
                {
                    propertyMap = SyncChange(list =>
                     {
                         var tpropertyMap = creatorFunc();
                         list.Add(tpropertyMap);
                         return tpropertyMap;
                     });
                }

                return propertyMap;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }

        }

        public void Clear()
        {
            SyncChange(list => list.Clear());
        }

        public bool Contains(T item)
        {
            return SyncGet(list => list.Contains(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            SyncGet(list => list.CopyTo(array, arrayIndex));
        }

        public bool Remove(T item)
        {
            return SyncChange(list => list.Remove(item));
        }

        public int Count
        {
            get { return SyncGet(list => list.Count); }
        }

        public bool IsReadOnly
        {
            get { return SyncGet(list => list.IsReadOnly); }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumeratorImpl();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorImpl();
        }

        private IEnumerator<T> GetEnumeratorImpl()
        {
            return SyncGet(list => list.ToList().GetEnumerator());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_lock != null)
                        _lock.Dispose();
                }

                _lock = null;
                _disposed = true;
            }
        }

        public int IndexOf(T item)
        {
            return SyncGet(list => list.IndexOf(item));
        }

        public void Insert(int index, T item)
        {
            SyncChange(list => list.Insert(index, item));
        }

        public void RemoveAt(int index)
        {
            SyncChange(list => list.RemoveAt(index));
        }

        public T this[int index]
        {
            get { return SyncGet(list => list[index]); }
            set { SyncChange(list => list[index] = value); }
        }
    }
}