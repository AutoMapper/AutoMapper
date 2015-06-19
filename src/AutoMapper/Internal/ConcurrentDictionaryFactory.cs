//TODO: they're all concurrent, with lock mechanisms
#if NET4 || NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || DNXCORE50

namespace AutoMapper.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public class ConcurrentDictionaryFactory : IDictionaryFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>()
        {
            return new PrivateConcurrentDictionary<TKey, TValue>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        private class PrivateConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            /// <summary>
            /// Dictionary backing field.
            /// </summary>
            private readonly System.Collections.Generic.IDictionary<TKey, TValue> _dictionary;

            /// <summary>
            /// Internal Default Constructor
            /// </summary>
            internal PrivateConcurrentDictionary()
                : this(new Dictionary<TKey, TValue>())
            {
            }

            /// <summary>
            /// Private Constructor
            /// </summary>
            /// <param name="dictionary"></param>
            private PrivateConcurrentDictionary(System.Collections.Generic.IDictionary<TKey, TValue> dictionary)
            {
                _dictionary = new ConcurrentDictionary<TKey, TValue>(dictionary);
            }

            #region Dictionary Members

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
                => _dictionary.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            public void Add(KeyValuePair<TKey, TValue> item)
                => _dictionary.Add(item.Key, item.Value);

            /// <summary>
            /// 
            /// </summary>
            public void Clear()
                => _dictionary.Clear();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Contains(KeyValuePair<TKey, TValue> item)
                => _dictionary.Contains(item);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="array"></param>
            /// <param name="arrayIndex"></param>
            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
                => _dictionary.CopyTo(array, arrayIndex);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Remove(KeyValuePair<TKey, TValue> item)
                => _dictionary.Remove(item);

            /// <summary>
            /// 
            /// </summary>
            public int Count => _dictionary.Count;

            /// <summary>
            /// 
            /// </summary>
            public bool IsReadOnly => _dictionary.IsReadOnly;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool TryGetValue(TKey key, out TValue value)
                => _dictionary.TryGetValue(key, out value);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public bool ContainsKey(TKey key)
                => _dictionary.ContainsKey(key);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void Add(TKey key, TValue value)
                => _dictionary.Add(key, value);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public bool Remove(TKey key)
                => _dictionary.Remove(key);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public TValue this[TKey key]
            {
                get { lock (_dictionary) return _dictionary[key]; }
                set { lock (_dictionary) _dictionary[key] = value; }
            }

            /// <summary>
            /// 
            /// </summary>
            public ICollection<TKey> Keys => _dictionary.Keys;

            /// <summary>
            /// 
            /// </summary>
            public ICollection<TValue> Values => _dictionary.Values;

            #endregion

            #region Enhanced Concurrent Dictionary Members

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="addValue"></param>
            /// <param name="updateValueFactory"></param>
            /// <returns></returns>
            public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
                => ((ConcurrentDictionary<TKey, TValue>) _dictionary).AddOrUpdate(key, addValue, updateValueFactory);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="valueFactory"></param>
            /// <returns></returns>
            public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
                => ((ConcurrentDictionary<TKey, TValue>) _dictionary).GetOrAdd(key, valueFactory);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool TryRemove(TKey key, out TValue value)
                => ((ConcurrentDictionary<TKey, TValue>)_dictionary).TryRemove(key, out value);

            #endregion
        }
    }
}

#endif