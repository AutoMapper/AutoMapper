using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AutoMapper
{
    public class ThreadSafeList<T> : IEnumerable<T>
        where T : class
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly IList<T> _propertyMaps = new List<T>();

        public void Add(T propertyMap)
        {
            _lock.EnterWriteLock();
            try
            {
                _propertyMaps.Add(propertyMap);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T GetOrCreate(Predicate<T> predicate, Func<T> creatorFunc)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                var propertyMap = _propertyMaps.FirstOrDefault(pm => predicate(pm));

                if (propertyMap == null)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        propertyMap = creatorFunc();

                        _propertyMaps.Add(propertyMap);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
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
            _lock.EnterWriteLock();
            try
            {
                _propertyMaps.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
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
            _lock.EnterReadLock();
            try
            {
                return _propertyMaps.ToList().GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}