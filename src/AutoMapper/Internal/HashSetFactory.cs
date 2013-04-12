using System.Collections;
using System.Collections.Generic;

namespace AutoMapper.Internal
{
    public class SetFactoryOverride : ISetFactory
    {
        public ISet<T> CreateSet<T>()
        {
            return new HashSetImpl<T>(new HashSet<T>());
        }

        private class HashSetImpl<T> : ISet<T>
        {
            private readonly HashSet<T> _hashSet;

            public HashSetImpl(HashSet<T> hashSet)
            {
                _hashSet = hashSet;
            }

            public bool Add(T item)
            {
                return _hashSet.Add(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _hashSet.GetEnumerator();
            }
        }
    }
}