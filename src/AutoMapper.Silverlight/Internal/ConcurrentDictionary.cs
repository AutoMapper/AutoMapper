/*  
 Copyright 2008 The 'A Concurrent Hashtable' development team  
 (http://www.codeplex.com/CH/People/ProjectPeople.aspx)

 This library is licensed under the GNU Library General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.codeplex.com/CH/license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Collections;

namespace TvdP.Collections
{
    /// <summary>
    /// Search key structure for <see cref="ConcurrentDictionary{TKey,TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    public struct ConcurrentDictionaryKey<TKey, TValue>
    {
        internal TKey _Key;
        internal TValue _Value;
        internal bool _IgnoreValue;

        internal ConcurrentDictionaryKey(TKey key)
        {
            _Key = key;
            _IgnoreValue = true;
            _Value = default(TValue);
        }

        internal ConcurrentDictionaryKey(TKey key, TValue value)
        {
            _Key = key;
            _IgnoreValue = false;
            _Value = value;
        }
    }

    /// <summary>
    /// A Concurrent <see cref="IDictionary{TKey,TValue}"/> implementation.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    /// <remarks>
    /// This class is threadsafe and highly concurrent. This means that multiple threads can do lookup and insert operations
    /// on this dictionary simultaneously. 
    /// It is not guaranteed that collisions will not occur. The dictionary is partitioned in segments. A segment contains
    /// a set of items based on a hash of those items. The more segments there are and the beter the hash, the fewer collisions will occur.
    /// This means that a nearly empty ConcurrentDictionary is not as concurrent as one containing many items. 
    /// </remarks>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ConcurrentDictionary<TKey, TValue>
        : ConcurrentHashtable<KeyValuePair<TKey, TValue>?, ConcurrentDictionaryKey<TKey, TValue>>
            , IDictionary<TKey, TValue>, IDictionary
#if !SILVERLIGHT
            , ISerializable 
#endif
    {
        #region Constructors

        /// <summary>
        /// Constructs a <see cref="ConcurrentDictionary{TKey,TValue}"/> instance using the default <see cref="IEqualityComparer{TKey}"/> to compare keys.
        /// </summary>
        public ConcurrentDictionary()
            : this(EqualityComparer<TKey>.Default)
        { }

        /// <summary>
        /// Constructs a <see cref="ConcurrentDictionary{TKey,TValue}"/> instance using the specified <see cref="IEqualityComparer{TKey}"/> to compare keys.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/> tp compare keys with.</param>
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
        public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
            : base()
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            _Comparer = comparer;

            Initialize();
        }

#if !SILVERLIGHT
        ConcurrentDictionary(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            _Comparer = (IEqualityComparer<TKey>)serializationInfo.GetValue("Comparer", typeof(IEqualityComparer<TKey>));

            var items = (List<KeyValuePair<TKey, TValue>>)serializationInfo.GetValue("Items", typeof(List<KeyValuePair<TKey, TValue>>));

            if (_Comparer == null || items == null)
                throw new SerializationException();

            Initialize();

            foreach (var kvp in items)
                this.Add(kvp);        
        }
#endif

        #endregion

        #region Traits

        readonly IEqualityComparer<TKey> _Comparer;

        /// <summary>
        /// Gives the <see cref="IEqualityComparer{TKey}"/> of TKey that is used to compare keys.
        /// </summary>
        public IEqualityComparer<TKey> Comparer { get { return _Comparer; } }

        /// <summary>
        /// Get a hashcode for given storeable item.
        /// </summary>
        /// <param name="item">Reference to the item to get a hash value for.</param>
        /// <returns>The hash value as an <see cref="UInt32"/>.</returns>
        /// <remarks>
        /// The hash returned should be properly randomized hash. The standard GetItemHashCode methods are usually not good enough.
        /// A storeable item and a matching search key should return the same hash code.
        /// So the statement <code>ItemEqualsItem(storeableItem, searchKey) ? GetItemHashCode(storeableItem) == GetItemHashCode(searchKey) : true </code> should always be true;
        /// </remarks>
        internal protected override UInt32 GetItemHashCode(ref KeyValuePair<TKey, TValue>? item)
        { return item.HasValue ? Hasher.Rehash(_Comparer.GetHashCode(item.Value.Key)) : 0; }

        /// <summary>
        /// Get a hashcode for given search key.
        /// </summary>
        /// <param name="key">Reference to the key to get a hash value for.</param>
        /// <returns>The hash value as an <see cref="UInt32"/>.</returns>
        /// <remarks>
        /// The hash returned should be properly randomized hash. The standard GetItemHashCode methods are usually not good enough.
        /// A storeable item and a matching search key should return the same hash code.
        /// So the statement <code>ItemEqualsItem(storeableItem, searchKey) ? GetItemHashCode(storeableItem) == GetItemHashCode(searchKey) : true </code> should always be true;
        /// </remarks>
        internal protected override UInt32 GetKeyHashCode(ref ConcurrentDictionaryKey<TKey, TValue> key)
        { return Hasher.Rehash(_Comparer.GetHashCode(key._Key)); }

        /// <summary>
        /// Compares a storeable item to a search key. Should return true if they match.
        /// </summary>
        /// <param name="item">Reference to the storeable item to compare.</param>
        /// <param name="key">Reference to the search key to compare.</param>
        /// <returns>True if the storeable item and search key match; false otherwise.</returns>
        internal protected override bool ItemEqualsKey(ref KeyValuePair<TKey, TValue>? item, ref ConcurrentDictionaryKey<TKey, TValue> key)
        { return item.HasValue && _Comparer.Equals(item.Value.Key, key._Key) && (key._IgnoreValue || EqualityComparer<TValue>.Default.Equals(item.Value.Value, key._Value)); }

        /// <summary>
        /// Compares two storeable items for equality.
        /// </summary>
        /// <param name="item1">Reference to the first storeable item to compare.</param>
        /// <param name="item2">Reference to the second storeable item to compare.</param>
        /// <returns>True if the two soreable items should be regarded as equal.</returns>
        internal protected override bool ItemEqualsItem(ref KeyValuePair<TKey, TValue>? item1, ref KeyValuePair<TKey, TValue>? item2)
        { return item1.HasValue && item2.HasValue && _Comparer.Equals(item1.Value.Key, item2.Value.Key); }

        /// <summary>
        /// Indicates if a specific item reference contains a valid item.
        /// </summary>
        /// <param name="item">The storeable item reference to check.</param>
        /// <returns>True if the reference doesn't refer to a valid item; false otherwise.</returns>
        /// <remarks>The statement <code>IsEmpty(default(TStoredI))</code> should always be true.</remarks>
        internal protected override bool IsEmpty(ref KeyValuePair<TKey, TValue>? item)
        { return !item.HasValue; }

        protected internal override Type GetKeyType(ref KeyValuePair<TKey, TValue>? item)
        { return !item.HasValue || item.Value.Key == null ? null : item.Value.Key.GetType(); }

        #endregion

        #region IDictionary<TKey,TValue> Members

        /// <summary>
        /// Adds an element with the provided key and value to the dictionary.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="ArgumentException">An element with the same key already exists in the dictionary.</exception>
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        { ((ICollection<KeyValuePair<TKey, TValue>>)this).Add(new KeyValuePair<TKey, TValue>(key, value)); }

        /// <summary>
        /// Determines whether the dictionary
        /// contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>true if the dictionary contains
        /// an element with the key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            KeyValuePair<TKey, TValue>? presentItem;
            ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            return FindItem(ref searchKey, out presentItem);
        }

        /// <summary>
        /// Gets an <see cref="ICollection{TKey}"/>  containing the keys of
        /// the dictionary.           
        /// </summary>
        /// <returns>An <see cref="ICollection{TKey}"/> containing the keys of the dictionary.</returns>
        /// <remarks>This property takes a snapshot of the current keys collection of the dictionary at the moment of invocation.</remarks>
        public ICollection<TKey> Keys
        {
            get
            {
                lock (SyncRoot)
                    return base.Items.Select(kvp => kvp.Value.Key).ToList();
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false. This method
        /// also returns false if key was not found in the original dictionary.</returns>
        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            KeyValuePair<TKey, TValue>? oldItem;
            ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            return base.RemoveItem(ref searchKey, out oldItem);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if
        /// the key is found; otherwise, the default value for the type of the value
        /// parameter. This parameter is passed uninitialized.
        ///</param>
        /// <returns>
        /// true if the dictionary contains an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            KeyValuePair<TKey, TValue>? presentItem;
            ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);

            var res = FindItem(ref searchKey, out presentItem);

            if (res)
            {
                value = presentItem.Value.Value;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Gets an <see cref="ICollection{TKey}"/> containing the values in
        ///     the dictionary.
        /// </summary>
        /// <returns>
        /// An <see cref="ICollection{TKey}"/> containing the values in the dictionary.
        /// </returns>
        /// <remarks>This property takes a snapshot of the current keys collection of the dictionary at the moment of invocation.</remarks>
        public ICollection<TValue> Values
        {
            get
            {
                lock (SyncRoot)
                    return base.Items.Select(kvp => kvp.Value.Value).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a KeyNotFoundException, and a set operation creates a new element with the specified key.</returns>
        /// <remarks>
        /// When working with multiple threads, that can each potentialy remove the searched for item, a <see cref="KeyNotFoundException"/> can always be expected.
        /// </remarks>
        public TValue this[TKey key]
        {
            get
            {
                KeyValuePair<TKey, TValue>? presentItem;
                ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);

                if (!FindItem(ref searchKey, out presentItem))
                    throw new KeyNotFoundException("The property is retrieved and key is not found.");
                return presentItem.Value.Value;
            }
            set
            {
                KeyValuePair<TKey, TValue>? newItem = new KeyValuePair<TKey, TValue>(key, value);
                KeyValuePair<TKey, TValue>? presentItem;
                InsertItem(ref newItem, out presentItem);
            }
        }

        #endregion

        #region IDictionary Members

        void IDictionary.Add(object key, object value)
        { ((IDictionary<TKey, TValue>)this).Add((TKey)key, (TValue)value); }

        void IDictionary.Clear()
        { ((IDictionary<TKey, TValue>)this).Clear(); }

        bool IDictionary.Contains(object key)
        { return ((IDictionary<TKey, TValue>)this).ContainsKey((TKey)key); }

        class DictionaryEnumerator : IDictionaryEnumerator
        {
            public IEnumerator<KeyValuePair<TKey, TValue>> _source;

            #region IDictionaryEnumerator Members

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    var current = _source.Current;
                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            { get { return _source.Current.Key; } }

            object IDictionaryEnumerator.Value
            { get { return _source.Current.Value; } }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            { get { return ((IDictionaryEnumerator)this).Entry; } }

            bool IEnumerator.MoveNext()
            { return _source.MoveNext(); }

            void IEnumerator.Reset()
            { _source.Reset(); }

            #endregion
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        { return new DictionaryEnumerator { _source = ((IDictionary<TKey, TValue>)this).GetEnumerator() }; }

        bool IDictionary.IsFixedSize
        { get { return false; } }

        bool IDictionary.IsReadOnly
        { get { return false; } }

        ICollection IDictionary.Keys
        { get { return (ICollection)((IDictionary<TKey, TValue>)this).Keys; } }

        void IDictionary.Remove(object key)
        { ((IDictionary<TKey, TValue>)this).Remove((TKey)key); }

        ICollection IDictionary.Values
        { get { return (ICollection)((IDictionary<TKey, TValue>)this).Values; } }

        object IDictionary.this[object key]
        {
            get { return ((IDictionary<TKey, TValue>)this)[(TKey)key]; }
            set { ((IDictionary<TKey, TValue>)this)[(TKey)key] = (TValue)value; }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Adds an association to the dictionary.
        /// </summary>
        /// <param name="item">A <see cref="KeyValuePair{TKey,TValue}"/> that represents the association to add.</param>
        /// <exception cref="ArgumentException">An association with an equal key already exists in the dicitonary.</exception>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            KeyValuePair<TKey, TValue>? newItem = item;
            KeyValuePair<TKey, TValue>? presentItem;

            if (GetOldestItem(ref newItem, out presentItem))
                throw new ArgumentException("An element with the same key already exists.");
        }

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        /// <remarks>WHen working with multiple threads, that each can add items to this dictionary, it is not guaranteed that the dictionary will be empty when this method returns.</remarks>
        public new void Clear()
        { base.Clear(); }

        /// <summary>
        /// Determines whether the specified association exists in the dictionary.
        /// </summary>
        /// <param name="item">The key-value association to search fo in the dicionary.</param>
        /// <returns>True if item is found in the dictionary; otherwise, false.</returns>
        /// <remarks>
        /// This method compares both key and value. It uses the default equality comparer to compare values.
        /// </remarks>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            KeyValuePair<TKey, TValue>? presentItem;
            ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(item.Key, item.Value);

            return
                FindItem(ref searchKey, out presentItem);
        }

        /// <summary>
        /// Copies all associations of the dictionary to an
        ///    <see cref="System.Array"/>, starting at a particular <see cref="System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="System.Array"/> that is the destination of the associations
        ///     copied from <see cref="ConcurrentDictionaryKey{TKey,TValue}"/>. The <see cref="System.Array"/> must
        ///     have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException"><paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.</exception>
        /// <exception cref="ArgumentException">The number of associations to be copied
        /// is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination
        /// <paramref name="array"/>.</exception>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (SyncRoot)
                Items.Select(nkvp => nkvp.Value).ToList().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/>.
        /// </summary>
        public new int Count
        { get { return base.Count; } }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/> is read-only, which is always false.
        /// </summary>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        { get { return false; } }

        /// <summary>
        /// Removes the specified association from the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/>, comparing both key and value.
        /// </summary>
        /// <param name="item">A <see cref="KeyValuePair{TKey,TValue}"/> representing the association to remove.</param>
        /// <returns>true if the association was successfully removed from the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/>;
        /// otherwise, false. This method also returns false if the association is not found in
        /// the original <see cref="ConcurrentDictionaryKey{TKey,TValue}"/>.
        ///</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            KeyValuePair<TKey, TValue>? oldItem;
            ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(item.Key, item.Value);
            return base.RemoveItem(ref searchKey, out oldItem);
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        { ((ICollection<KeyValuePair<TKey, TValue>>)this).CopyTo((KeyValuePair<TKey, TValue>[])array, index); }

        int ICollection.Count
        { get { return ((ICollection<KeyValuePair<TKey, TValue>>)this).Count; } }

        bool ICollection.IsSynchronized
        { get { return true; } }

        object ICollection.SyncRoot
        { get { return this; } }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Returns an enumerator that iterates through all associations in the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/> at the moment of invocation.
        /// </summary>
        /// <returns>A <see cref="IEnumerator{T}"/> that can be used to iterate through the associations.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (SyncRoot)
                return Items.Select(nkvp => nkvp.Value).ToList().GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through all associations in the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/> at the moment of invocation.
        /// </summary>
        /// <returns>A <see cref="System.Collections.IEnumerator"/> that can be used to iterate through the associations.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        { return GetEnumerator(); }

        #endregion

#if !SILVERLIGHT
        #region ISerializable Members

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        { 
            info.AddValue("Items", (object)Items.Select(item => item.Value).ToList());
            info.AddValue("Comparer", _Comparer);
        }
        #endregion
#endif

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (null == addValueFactory)
                throw new ArgumentNullException("addValueFactory");

            if (null == updateValueFactory)
                throw new ArgumentNullException("updateValueFactory");

            var searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            KeyValuePair<TKey, TValue>? latestItem;

            while (true)
                if (this.FindItem(ref searchKey, out latestItem))
                {
                    TValue storedValue = latestItem.Value.Value;
                    TValue newValue = updateValueFactory(key, storedValue);

                    if (TryUpdate(key, newValue, storedValue))
                        return newValue;
                }
                else
                    return AddOrUpdate(key, addValueFactory(key), updateValueFactory);
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (null == updateValueFactory)
                throw new ArgumentNullException("updateValueFactory");

            KeyValuePair<TKey, TValue>? latestItem;
            KeyValuePair<TKey, TValue>? addItem = new KeyValuePair<TKey, TValue>(key, addValue);

            while (true)
                if (this.GetOldestItem(ref addItem, out latestItem))
                {
                    TValue storedValue = latestItem.Value.Value;
                    TValue newValue = updateValueFactory(key, storedValue);

                    if (TryUpdate(key, newValue, storedValue))
                        return newValue;
                }
                else
                    return latestItem.Value.Value;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            KeyValuePair<TKey, TValue>? addKey = new KeyValuePair<TKey, TValue>(key, value);
            KeyValuePair<TKey, TValue>? oldItem;

            return !this.GetOldestItem(ref addKey, out oldItem);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            var searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            KeyValuePair<TKey, TValue>? oldItem;

            var res = base.RemoveItem(ref searchKey, out oldItem);

            value = res ? oldItem.Value.Value : default(TValue);

            return res;
        }

        public bool TryUpdate(
            TKey key,
            TValue newValue,
            TValue comparisonValue
        )
        {
            var searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            KeyValuePair<TKey, TValue>? newItem = new KeyValuePair<TKey, TValue>(key, newValue);
            KeyValuePair<TKey, TValue>? dummy;

            return base.ReplaceItem(ref searchKey, ref newItem, out dummy, item => EqualityComparer<TValue>.Default.Equals(item.Value.Value, comparisonValue));
        }

        public TValue GetOrAdd(
            TKey key,
            TValue value
        )
        {
            KeyValuePair<TKey, TValue>? newItem = new KeyValuePair<TKey, TValue>(key, value);
            KeyValuePair<TKey, TValue>? oldItem;

            return base.GetOldestItem(ref newItem, out oldItem) ? oldItem.Value.Value : value;
        }

        public TValue GetOrAdd(
            TKey key,
            Func<TKey, TValue> valueFactory
        )
        {
            if (null == valueFactory)
                throw new ArgumentNullException("valueFactory");

            var searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            KeyValuePair<TKey, TValue>? oldItem;

            if (base.FindItem(ref searchKey, out oldItem))
                return oldItem.Value.Value;

            KeyValuePair<TKey, TValue>? newItem = new KeyValuePair<TKey, TValue>(key, valueFactory(key));

            base.GetOldestItem(ref newItem, out oldItem);

            return oldItem.Value.Value;
        }
    }
}
