namespace AutoMapper.Internal
{
    using System;
    using System.Threading;

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
                        Interlocked.CompareExchange(ref m_value, temp, null);

                        Boolean lockTaken = false;

                        try
                        {
                            Monitor.Enter(_lockObj);
                            lockTaken = true;

                            _isDelegateInvoked = true;
                        }
                        finally
                        {
                            if (lockTaken)
                            {
                                Monitor.Exit(_lockObj);
                            }
                        }
                    }

                    return m_value;
                }
            }
        }
    }
}