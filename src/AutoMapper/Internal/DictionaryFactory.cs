namespace AutoMapper.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public class DictionaryFactory : IDictionaryFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>()
        {
            return new PrivateDictionary<TKey, TValue>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public class PrivateDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            /// <summary>
            /// Dictionary backing field.
            /// </summary>
            private readonly System.Collections.Generic.IDictionary<TKey, TValue> _dictionary;

            /// <summary>
            /// Internal Default Constructor
            /// </summary>
            internal PrivateDictionary()
                : this(new Dictionary<TKey, TValue>())
            {
            }

            /// <summary>
            /// Private Constructor
            /// </summary>
            /// <param name="dictionary"></param>
            private PrivateDictionary(System.Collections.Generic.IDictionary<TKey, TValue> dictionary)
            {
                _dictionary = new Dictionary<TKey, TValue>(dictionary);
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
                => _dictionary.Add(item);

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
                get { return _dictionary[key]; }
                set { _dictionary[key] = value; }
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
            {
                // Very important to use the existing value for the update factory.
                var value = _dictionary.ContainsKey(key) ? updateValueFactory(key, _dictionary[key]) : addValue;
                _dictionary[key] = value;
                return value;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="valueFactory"></param>
            /// <returns></returns>
            public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
            {
                if (_dictionary.ContainsKey(key))
                    return _dictionary[key];

                var value = valueFactory(key);

                _dictionary[key] = value;

                return value;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool TryRemove(TKey key, out TValue value)
            {
                if (!_dictionary.ContainsKey(key))
                {
                    value = default(TValue);
                    return false;
                }

                value = _dictionary[key];

                _dictionary.Remove(key);

                return true;
            }

            #endregion
        }
    }
}