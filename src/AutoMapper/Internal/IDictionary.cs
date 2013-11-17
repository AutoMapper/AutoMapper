using System;
using System.Collections.Generic;
using System.Threading;

namespace AutoMapper.Internal
{
    public interface IProxyGenerator
    {
        Type GetProxyType(Type interfaceType);
    }

    public interface IProxyGeneratorFactory
    {
        IProxyGenerator Create();
    }

    public interface ILazy<T>
    {
        T Value { get; }
    }

    public interface IReaderWriterLockSlim : IDisposable
    {
        void EnterWriteLock();
        void ExitWriteLock();
        void EnterUpgradeableReadLock();
        void ExitUpgradeableReadLock();
        void EnterReadLock();
        void ExitReadLock();
    }

    public interface IDictionary<TKey, TValue>
    {
        TValue AddOrUpdate(
            TKey key,
            TValue addValue,
            Func<TKey, TValue, TValue> updateValueFactory
            );

        bool TryGetValue(
            TKey key,
            out TValue value
            );

        TValue GetOrAdd(
            TKey key,
            Func<TKey, TValue> valueFactory
            );

        TValue this[TKey key] { get; set; }
        bool TryRemove(TKey key, out TValue value);
    }

    public interface INullableConverter
    {
        object ConvertFrom(object value);
        Type UnderlyingType { get; }
    }


    //public interface ISet<T> : IEnumerable<T>
    //{
    //    bool Add(T item);
    //}

    public interface INullableConverterFactory
    {
        INullableConverter Create(Type nullableType);
    }

    public interface IEnumNameValueMapperFactory
    {
        IEnumNameValueMapper Create();
    }

    public interface IEnumNameValueMapper
    {
        bool IsMatch(Type enumDestinationType, string sourceValue);
        object Convert(Type enumSourceType, Type enumDestinationType, ResolutionContext context);
    }

    public interface IDictionaryFactory
    {
        IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>();
    }

    public interface IReaderWriterLockSlimFactory
    {
        IReaderWriterLockSlim Create();
    }

    public static class LazyFactory
    {
        public static ILazy<T> Create<T>(Func<T> valueFactory) where T : class
        {
            return new LazyImpl<T>(valueFactory);
        }

        private sealed class LazyImpl<T> : ILazy<T>
            where T : class
        {
            private readonly Object _lockObj = new Object();
            private readonly Func<T> _valueFactory;
            private Boolean _isDelegateInvoked;

            private T m_value;

            public LazyImpl(Func<T> valueFactory)
            {
                _valueFactory = valueFactory;
            }

            public T Value
            {
                get
                {
                    if (!_isDelegateInvoked)
                    {
                        T temp = _valueFactory();
                        Interlocked.CompareExchange<T>(ref m_value, temp, null);

                        Boolean lockTaken = false;

                        try
                        {
                            Monitor.Enter(_lockObj); lockTaken = true;

                            _isDelegateInvoked = true;
                        }
                        finally
                        {
                            if (lockTaken) { Monitor.Exit(_lockObj); }
                        }
                    }

                    return m_value;
                }
            }
        }
    }

    public static class FeatureDetector
    {
        public static Func<Type, bool> IsIDataRecordType = t => false;
        private static bool? _isEnumGetNamesSupported;


        public static bool IsEnumGetNamesSupported
        {
            get
            {
                if (_isEnumGetNamesSupported == null)
                    _isEnumGetNamesSupported = ResolveIsEnumGetNamesSupported();

                return _isEnumGetNamesSupported.Value;
            }
        }

        private static bool ResolveIsEnumGetNamesSupported()
        {
            return typeof (Enum).GetMethod("GetNames") != null;
        }
    }
}
