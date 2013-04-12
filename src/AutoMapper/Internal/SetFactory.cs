using System.Collections;
using System.Collections.Generic;

namespace AutoMapper.Internal
{
    public class SetFactory : ISetFactory
    {
        public ISet<T> CreateSet<T>()
        {
            return new HashSetImpl<T>();
        }

        private class HashSetImpl<T> : ISet<T>
        {
            private readonly Dictionary<T, short> _dict;

            public HashSetImpl()
            {
                _dict = new Dictionary<T, short>();
            }

            public bool Add(T item)
            {
                if (_dict.ContainsKey(item))
                    return false;
                _dict.Add(item, 0);
                return true;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _dict.Keys.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _dict.Keys.GetEnumerator();
            }
        }
    }
}