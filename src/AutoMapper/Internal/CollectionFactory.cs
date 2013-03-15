﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoMapper.Internal
{
    public class CollectionFactory : ICollectionFactory
    {
        public IDictionary<TKey, TValue> CreateConcurrentDictionary<TKey, TValue>()
        {
            return new ConcurrentDictionaryImpl<TKey, TValue>(new ConcurrentDictionary<TKey, TValue>());
        }

        public ISet<T> CreateSet<T>()
        {
            return new HashSetImpl<T>(new HashSet<T>());
        }

        private class ConcurrentDictionaryImpl<TKey, TValue> : IDictionary<TKey, TValue>
        {
            private readonly ConcurrentDictionary<TKey, TValue> _dictionary;

            public ConcurrentDictionaryImpl(ConcurrentDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }


            public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
            {
                return _dictionary.AddOrUpdate(key, addValue, updateValueFactory);
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return _dictionary.TryGetValue(key, out value);
            }

            public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
            {
                return _dictionary.GetOrAdd(key, valueFactory);
            }

            public TValue this[TKey key]
            {
                get { return _dictionary[key]; }
                set { _dictionary[key] = value; }
            }

            public bool TryRemove(TKey key, out TValue value)
            {
                return _dictionary.TryRemove(key, out value);
            }
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
