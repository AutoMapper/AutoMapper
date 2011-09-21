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
using System.Threading;
using TvdP.Threading;


namespace TvdP.Collections
{
    /// <summary>
    /// A 'single writer - multi reader' threaded segment in a hashtable. 
    /// </summary>
    /// <typeparam name="TStored"></typeparam>
    /// <typeparam name="TSearch"></typeparam>
    /// <remarks>
    /// Though each segment can be accessed by 1 writer thread simultaneously, the hashtable becomes concurrent 
    /// for writing by containing many segments so that collisions are rare. The table will be fully concurrent 
    /// for read operations as far as they are not colliding with write operations.
    /// Each segment is itself a small hashtable that can grow and shrink individualy. This prevents blocking of
    /// the entire hashtable when growing or shrinking is needed. Because each segment is relatively small (depending on
    /// the quality of the hash) resizing of the individual segments should not take much time.
    /// </remarks>
    internal class Segment<TStored, TSearch>
    {
        #region Construction

        protected Segment()
        { }

        public static Segment<TStored, TSearch> Create(Int32 initialSize)
        {
            var instance = new Segment<TStored, TSearch>();
            instance.Initialize(initialSize);
            return instance;
        }

        /// <summary>
        /// Initialize the segment.
        /// </summary>
        /// <param name="initialSize"></param>
        protected virtual void Initialize(Int32 initialSize)
        { _List = new TStored[Math.Max(4, initialSize)]; }

        /// <summary>
        /// When segment gets introduced into hashtable then its allocated space should be added to the
        /// total allocated space.
        /// Single threaded access or locking is needed
        /// </summary>
        /// <param name="traits"></param>
        public void Welcome(ConcurrentHashtable<TStored, TSearch> traits)
        { traits.EffectTotalAllocatedSpace(_List.Length); }

        /// <summary>
        /// When segment gets removed from hashtable then its allocated space should be subtracted to the
        /// total allocated space.
        /// Single threaded access or locking is needed
        /// </summary>
        /// <param name="traits"></param>
        public void Bye(ConcurrentHashtable<TStored, TSearch> traits)
        {
            traits.EffectTotalAllocatedSpace(-_List.Length);
            _List = null;
        }


        #endregion

        #region Locking

        TinyReaderWriterLock _Lock;

        internal void LockForWriting()
        { _Lock.LockForWriting(); }

        internal void LockForReading()
        { _Lock.LockForReading(); }

        internal bool Lock(bool forReading)
        {
            return forReading ? _Lock.LockForReading(false) : _Lock.LockForWriting(false);
        }

        internal void ReleaseForWriting()
        { _Lock.ReleaseForWriting(); }

        internal void ReleaseForReading()
        { _Lock.ReleaseForReading(); }

        internal void Release(bool forReading)
        {
            if (forReading)
                _Lock.ReleaseForReading();
            else
                _Lock.ReleaseForWriting();
        }

        #endregion

        /// <summary>
        /// Array with 'slots'. Each slot can be filled or empty.
        /// </summary>
        internal TStored[] _List;

        /// <summary>
        /// Boolean value indicating if this segment has not been trashed yet.
        /// </summary>
        internal bool IsAlive { get { return _List != null; } }


        #region Item Manipulation methods

        /// <summary>
        /// Inserts an item into a *not empty* spot given by position i. It moves items forward until an empty spot is found.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="i"></param>
        /// <param name="itemCopy"></param>
        /// <param name="traits"></param>
        private void InsertItemAtIndex(UInt32 mask, UInt32 i, TStored itemCopy, ConcurrentHashtable<TStored, TSearch> traits)
        {
            do
            {
                //swap
                TStored temp = _List[i];
                _List[i] = itemCopy;
                itemCopy = temp;

                i = (i + 1) & mask;
            }
            while (!traits.IsEmpty(ref _List[i]));

            _List[i] = itemCopy;
        }

        /// <summary>
        /// Find item in segment.
        /// </summary>
        /// <param name="key">Reference to the search key to use.</param>
        /// <param name="item">Out reference to store the found item in.</param>
        /// <param name="traits">Object that tells this segment how to treat items and keys.</param>
        /// <returns>True if an item could be found, otherwise false.</returns>
        public bool FindItem(ref TSearch key, out TStored item, ConcurrentHashtable<TStored, TSearch> traits)
        {
            var searchHash = traits.GetKeyHashCode(ref key);
            var mask = (UInt32)(_List.Length - 1);
            var i = searchHash & mask;

            if (!traits.IsEmpty(ref _List[i]))
            {
                var firstHash = traits.GetItemHashCode(ref _List[i]);
                var storedItemHash = firstHash;
                var searchHashDiff = (searchHash - firstHash) & mask;

                do
                {
                    if (storedItemHash == searchHash && traits.ItemEqualsKey(ref _List[i], ref key))
                    {
                        item = _List[i];
                        return true;
                    }

                    i = (i + 1) & mask;

                    if (traits.IsEmpty(ref _List[i]))
                        break;

                    storedItemHash = traits.GetItemHashCode(ref _List[i]);
                }
                while (((storedItemHash - firstHash) & mask) <= searchHashDiff);
            }

            item = default(TStored);
            return false;
        }

        /// <summary>
        /// Find an existing item or, if it can't be found, insert a new item.
        /// </summary>
        /// <param name="key">Reference to the item that will be inserted if an existing item can't be found. It will also be used to search with.</param>
        /// <param name="item">Out reference to store the found item or, if it can not be found, the new inserted item.</param>
        /// <param name="traits">Object that tells this segment how to treat items and keys.</param>
        /// <returns>True if an existing item could be found, otherwise false.</returns>
        public bool GetOldestItem(ref TStored key, out TStored item, ConcurrentHashtable<TStored, TSearch> traits)
        {
            var searchHash = traits.GetItemHashCode(ref key);
            var mask = (UInt32)(_List.Length - 1);
            var i = searchHash & mask;

            if (!traits.IsEmpty(ref _List[i]))
            {
                var firstHash = traits.GetItemHashCode(ref _List[i]);
                var storedItemHash = firstHash;
                var searchHashDiff = (searchHash - firstHash) & mask;

                while (true)
                {
                    if (storedItemHash == searchHash && traits.ItemEqualsItem(ref _List[i], ref key))
                    {
                        item = _List[i];
                        return true;
                    }

                    i = (i + 1) & mask;

                    if (traits.IsEmpty(ref _List[i]))
                        break;

                    storedItemHash = traits.GetItemHashCode(ref _List[i]);

                    if (((storedItemHash - firstHash) & mask) > searchHashDiff)
                    {
                        //insert
                        InsertItemAtIndex(mask, i, key, traits);
                        IncrementCount(traits);
                        item = key;
                        return false;
                    }
                }
            }

            item = _List[i] = key;
            IncrementCount(traits);
            return false;
        }

        /// <summary>
        /// Inserts an item in the segment, possibly replacing an equal existing item.
        /// </summary>
        /// <param name="key">A reference to the item to insert.</param>
        /// <param name="item">An out reference where any replaced item will be written to, if no item was replaced the new item will be written to this reference.</param>
        /// <param name="traits">Object that tells this segment how to treat items and keys.</param>
        /// <returns>True if an existing item could be found and is replaced, otherwise false.</returns>
        public bool InsertItem(ref TStored key, out TStored item, ConcurrentHashtable<TStored, TSearch> traits)
        {
            var searchHash = traits.GetItemHashCode(ref key);
            var mask = (UInt32)(_List.Length - 1);
            var i = searchHash & mask;

            if (!traits.IsEmpty(ref _List[i]))
            {
                var firstHash = traits.GetItemHashCode(ref _List[i]);
                var storedItemHash = firstHash;
                var searchHashDiff = (searchHash - firstHash) & mask;

                while (true)
                {
                    if (storedItemHash == searchHash && traits.ItemEqualsItem(ref _List[i], ref key))
                    {
                        item = _List[i];
                        _List[i] = key;
                        return true;
                    }

                    i = (i + 1) & mask;

                    if (traits.IsEmpty(ref _List[i]))
                        break;

                    storedItemHash = traits.GetItemHashCode(ref _List[i]);

                    if (((storedItemHash - firstHash) & mask) > searchHashDiff)
                    {
                        //insert                   
                        InsertItemAtIndex(mask, i, key, traits);
                        IncrementCount(traits);
                        item = key;
                        return false;
                    }
                }
            }

            item = _List[i] = key;
            IncrementCount(traits);
            return false;
        }

        /// <summary>
        /// Removes an item from the segment.
        /// </summary>
        /// <param name="key">A reference to the key to search with.</param>
        /// <param name="item">An out reference where the removed item will be stored or default(<typeparamref name="TStored"/>) if no item to remove can be found.</param>
        /// <param name="traits">Object that tells this segment how to treat items and keys.</param>
        /// <returns>True if an item could be found and is removed, false otherwise.</returns>
        public bool RemoveItem(ref TSearch key, out TStored item, ConcurrentHashtable<TStored, TSearch> traits)
        {
            var searchHash = traits.GetKeyHashCode(ref key);
            var mask = (UInt32)(_List.Length - 1);
            var i = searchHash & mask;

            if (!traits.IsEmpty(ref _List[i]))
            {
                var firstHash = traits.GetItemHashCode(ref _List[i]);
                var storedItemHash = firstHash;
                var searchHashDiff = (searchHash - firstHash) & mask;

                do
                {
                    if (storedItemHash == searchHash && traits.ItemEqualsKey(ref _List[i], ref key))
                    {
                        item = _List[i];
                        RemoveAtIndex(i, traits);
                        DecrementCount(traits);
                        return true;
                    }

                    i = (i + 1) & mask;

                    if (traits.IsEmpty(ref _List[i]))
                        break;

                    storedItemHash = traits.GetItemHashCode(ref _List[i]);
                }
                while (((storedItemHash - firstHash) & mask) <= searchHashDiff);
            }

            item = default(TStored);
            return false;
        }

        protected void RemoveAtIndex(UInt32 index, ConcurrentHashtable<TStored, TSearch> traits)
        {
            var mask = (UInt32)(_List.Length - 1);
            var i = index;
            var j = (index + 1) & mask;

            while (!traits.IsEmpty(ref _List[j]) && (traits.GetItemHashCode(ref _List[j]) & mask) != j)
            {
                _List[i] = _List[j];

                i = j;
                j = (j + 1) & mask;
            }

            _List[i] = default(TStored);
        }

        public void Clear(ConcurrentHashtable<TStored, TSearch> traits)
        {
            var oldList = _List;
            _List = new TStored[4];

            var effect = _List.Length - oldList.Length;

            if (effect != 0)
                traits.EffectTotalAllocatedSpace(effect);

            _Count = 0;
        }

        /// <summary>
        /// Iterate over items in the segment. 
        /// </summary>
        /// <param name="beyond">Position beyond which the next filled slot will be found and the item in that slot returned. (Starting with -1)</param>
        /// <param name="item">Out reference where the next item will be stored or default if the end of the segment is reached.</param>
        /// <param name="traits">Object that tells this segment how to treat items and keys.</param>
        /// <returns>The index position the next item has been found or -1 otherwise.</returns>
        public int GetNextItem(int beyond, out TStored item, ConcurrentHashtable<TStored, TSearch> traits)
        {
            for (int end = _List.Length; ++beyond < end; )
            {
                if (!traits.IsEmpty(ref _List[beyond]))
                {
                    item = _List[beyond];
                    return beyond;
                }
            }

            item = default(TStored);
            return -1;
        }

        #endregion

        #region Resizing

        protected virtual void ResizeList(ConcurrentHashtable<TStored, TSearch> traits)
        {
            var oldList = _List;
            var oldListLength = oldList.Length;

            var newListLength = 2;

            while (newListLength < _Count)
                newListLength <<= 1;

            newListLength <<= 1;

            if (newListLength != oldListLength)
            {
                _List = new TStored[newListLength];

                var mask = (UInt32)(newListLength - 1);

                for (int i = 0; i != oldListLength; ++i)
                    if (!traits.IsEmpty(ref oldList[i]))
                    {
                        var searchHash = traits.GetItemHashCode(ref oldList[i]);

                        //j is prefered insertion pos in new list.
                        var j = searchHash & mask;

                        if (traits.IsEmpty(ref _List[j]))
                            _List[j] = oldList[i];
                        else
                        {
                            var firstHash = traits.GetItemHashCode(ref _List[j]);
                            var storedItemHash = firstHash;
                            var searchHashDiff = (searchHash - firstHash) & mask;

                            while (true)
                            {
                                j = (j + 1) & mask;

                                if (traits.IsEmpty(ref _List[j]))
                                {
                                    _List[j] = oldList[i];
                                    break;
                                }

                                storedItemHash = traits.GetItemHashCode(ref _List[j]);

                                if (((storedItemHash - firstHash) & mask) > searchHashDiff)
                                {
                                    InsertItemAtIndex(mask, j, oldList[i], traits);
                                    break;
                                }
                            }
                        }
                    }

                traits.EffectTotalAllocatedSpace(newListLength - oldListLength);
            }
        }

        /// <summary>
        /// Total numer of filled slots in _List.
        /// </summary>
        internal Int32 _Count;

        protected void DecrementCount(ConcurrentHashtable<TStored, TSearch> traits, int amount)
        {
            var oldListLength = _List.Length;
            _Count -= amount;

            if (oldListLength > 4 && _Count < (oldListLength >> 2))
                //Shrink
                ResizeList(traits);
        }

        protected void DecrementCount(ConcurrentHashtable<TStored, TSearch> traits)
        { DecrementCount(traits, 1); }

        private void IncrementCount(ConcurrentHashtable<TStored, TSearch> traits)
        {
            var oldListLength = _List.Length;

            if (++_Count >= (oldListLength - (oldListLength >> 2)))
                //Grow
                ResizeList(traits);
        }

        /// <summary>
        /// Remove any excess allocated space
        /// </summary>
        /// <param name="traits"></param>
        internal void Trim(ConcurrentHashtable<TStored, TSearch> traits)
        { DecrementCount(traits, 0); }

        #endregion
    }
}
