using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if NET7_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

#if LIGHT_EXPRESSION
namespace FastExpressionCompiler.LightExpression.ImTools;
#else
namespace FastExpressionCompiler.ImTools;
#endif

using static FHashMap;

/// <summary>Wrapper for the array and count</summary>
public struct SmallList<T>
{
    /// <summary>Array of items</summary>
    public T[] Items;
    /// <summary>The count of used items</summary>
    public int Count;

    /// <summary>Creating this stuff</summary>
    public SmallList(T[] items, int count)
    {
        Items = items;
        Count = count;
    }

    /// <summary>Creates the wrapper out of the items</summary>
    public SmallList(T[] items) : this(items, items.Length) { }

    /// <summary>Popping candy</summary>
    public void Pop() => --Count;
}

/// <summary>SmallList module he-he</summary>
public static class SmallList
{
    internal const int ForLoopCopyCount = 4;
    internal const int InitialCapacity = 4;

    [MethodImpl((MethodImplOptions)256)]
    internal static void Expand<TItem>(ref TItem[] items)
    {
        // `| 1` is for the case when the length is 0
        var newItems = new TItem[(items.Length << 1) | 1]; // have fun to guess the new length, haha ;-P
        if (items.Length > ForLoopCopyCount)
            Array.Copy(items, newItems, items.Length);
        else
            for (var i = 0; i < items.Length; ++i)
                newItems[i] = items[i];
        items = newItems;
    }

    /// <summary>Appends the new default item at the end of the items. Assumes that `index lte items.Length`! 
    /// `items` should be not null</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem AppendDefaultToNotNullItemsAndGetRef<TItem>(ref TItem[] items, int index)
    {
        Debug.Assert(index <= items.Length);
        if (index == items.Length)
            Expand(ref items);
        return ref items[index];
    }

    /// <summary>Appends the new default item at the end of the items. Assumes that `index lte items.Length`, `items` may be null</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem AppendDefaultAndGetRef<TItem>(ref TItem[] items, int index, int initialCapacity = InitialCapacity)
    {
        if (items == null)
        {
            Debug.Assert(index == 0);
            items = new TItem[initialCapacity];
            return ref items[index];
        }

        Debug.Assert(index <= items.Length);
        if (index == items.Length)
            Expand(ref items);
        return ref items[index];
    }

    /// <summary>Returns surely present item ref by its index</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem GetSurePresentItemRef<TItem>(this ref SmallList<TItem> source, int index) =>
        ref source.Items[index];

    // todo: @perf add the not null variant
    /// <summary>Appends the new default item to the list and returns ref to it for write or read</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem Append<TItem>(ref this SmallList<TItem> source, int initialCapacity = InitialCapacity) =>
        ref AppendDefaultAndGetRef(ref source.Items, source.Count++, initialCapacity);

    /// <summary>Appends the new item to the list</summary>
    // todo: @perf add the not null variant
    [MethodImpl((MethodImplOptions)256)]
    public static void Append<TItem>(ref this SmallList<TItem> source, in TItem item, int initialCapacity = InitialCapacity) =>
        AppendDefaultAndGetRef(ref source.Items, source.Count++, initialCapacity) = item;

    /// <summary>Looks for the item in the list and return its index if found or -1 for the absent item</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static int TryGetIndex<TItem, TEq>(this ref SmallList<TItem> source, TItem it, TEq eq = default)
        where TEq : struct, IEq<TItem>
    {
        var count = source.Count;
        var items = source.Items;
        for (var i = 0; i < count; ++i)
        {
            ref var di = ref items[i]; // todo: @perf Marshall?
            if (eq.Equals(it, di))
                return i;
        }
        return -1;
    }

    /// <summary>Returns the ref of the found item or appends the item to the end of the list, and returns ref to it</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static int GetIndexOrAppend<TItem, TEq>(this ref SmallList<TItem> source, in TItem item, TEq eq)
        where TEq : struct, IEq<TItem>
    {
        var count = source.Count;
        var items = source.Items;
        for (var i = 0; i < count; ++i)
        {
            ref var di = ref items[i]; // todo: @perf Marshall?
            if (eq.Equals(item, di))
                return i;
        }
        source.Append() = item;
        return -1;
    }

    /// <summary>Returns surely present item ref by its index</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem GetSurePresentItemRef<TItem>(this ref SmallList4<TItem> source, int index)
    {
        Debug.Assert(source.Count != 0);
        Debug.Assert(index < source.Count);
        switch (index)
        {
            case 0: return ref source._it0;
            case 1: return ref source._it1;
            case 2: return ref source._it2;
            case 3: return ref source._it3;
            default:
                Debug.Assert(source._rest != null, $"Expecting deeper items are already existing on stack at index: {index}");
                return ref source._rest[index - SmallList4<TItem>.OnStackItemCount];
        }
    }

    /// <summary>Returns last present item ref, assumes that the list is not empty!</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem GetLastSurePresentItem<TItem>(this ref SmallList4<TItem> source) =>
        ref source.GetSurePresentItemRef(source._count - 1);

    /// <summary>Returns the ref to tombstone indicating the missing item.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem NotFound<TItem>(this ref SmallList4<TItem> _) => ref SmallList4<TItem>.Missing;

    /// <summary>Appends the default item to the end of the list and returns the reference to it.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem AppendDefaultAndGetRef<TItem>(this ref SmallList4<TItem> source)
    {
        var index = source._count++;
        switch (index)
        {
            case 0: return ref source._it0;
            case 1: return ref source._it1;
            case 2: return ref source._it2;
            case 3: return ref source._it3;
            default:
                return ref AppendDefaultAndGetRef(ref source._rest, index - SmallList4<TItem>.OnStackItemCount, SmallList4<TItem>.OnStackItemCount);
        }
    }

    /// <summary>Looks for the item in the list and return its index if found or -1 for the absent item</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static int TryGetIndex<TItem, TEq>(this ref SmallList4<TItem> source, TItem it, TEq eq = default)
        where TEq : struct, IEq<TItem>
    {
        switch (source._count)
        {
            case 1:
                if (eq.Equals(it, source._it0)) return 0;
                break;

            case 2:
                if (eq.Equals(it, source._it0)) return 0;
                if (eq.Equals(it, source._it1)) return 1;
                break;

            case 3:
                if (eq.Equals(it, source._it0)) return 0;
                if (eq.Equals(it, source._it1)) return 1;
                if (eq.Equals(it, source._it2)) return 2;
                break;

            default:
                if (eq.Equals(it, source._it0)) return 0;
                if (eq.Equals(it, source._it1)) return 1;
                if (eq.Equals(it, source._it2)) return 2;
                if (eq.Equals(it, source._it3)) return 3;
                if (source._rest != null)
                {
                    var count = source._count - SmallList4<TItem>.OnStackItemCount;
                    var items = source._rest;
                    for (var i = 0; i < count; ++i)
                    {
                        ref var di = ref items[i]; // todo: @perf Marshall?
                        if (eq.Equals(it, di))
                            return i + SmallList4<TItem>.OnStackItemCount;
                    }
                }
                break;
        }
        return -1;
    }

    /// <summary>Returns the ref of the found item or appends the item to the end of the list, and returns ref to it</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static int GetIndexOrAppend<TItem, TEq>(this ref SmallList4<TItem> source, in TItem item, TEq eq)
        where TEq : struct, IEq<TItem>
    {
        switch (source._count)
        {
            case 0:
                source._count = 1;
                source._it0 = item;
                return -1;

            case 1:
                if (eq.Equals(item, source._it0)) return 0;
                source._count = 2;
                source._it1 = item;
                return -1;

            case 2:
                if (eq.Equals(item, source._it0)) return 0;
                if (eq.Equals(item, source._it1)) return 1;
                source._count = 3;
                source._it2 = item;
                return -1;

            case 3:
                if (eq.Equals(item, source._it0)) return 0;
                if (eq.Equals(item, source._it1)) return 1;
                if (eq.Equals(item, source._it2)) return 2;
                source._count = 4;
                source._it3 = item;
                return -1;

            default:
                if (eq.Equals(item, source._it0)) return 0;
                if (eq.Equals(item, source._it1)) return 1;
                if (eq.Equals(item, source._it2)) return 2;
                if (eq.Equals(item, source._it3)) return 3;

                var restCount = source._count - SmallList4<TItem>.OnStackItemCount;
                if (restCount != 0)
                {
                    var items = source._rest;
                    for (var i = 0; i < restCount; ++i)
                    {
                        ref var di = ref items[i]; // todo: @perf Marshall?
                        if (eq.Equals(item, di))
                            return i + SmallList4<TItem>.OnStackItemCount;
                    }
                }
                AppendDefaultAndGetRef(ref source._rest, restCount, SmallList4<TItem>.OnStackItemCount) = item;
                ++source._count;
                return -1;
        }
    }

    /// <summary>Returns surely present item ref by its index</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem GetSurePresentItemRef<TItem>(this ref SmallList2<TItem> source, int index)
    {
        Debug.Assert(source.Count != 0);
        Debug.Assert(index < source.Count);
        switch (index)
        {
            case 0: return ref source._it0;
            case 1: return ref source._it1;
            default:
                Debug.Assert(source._rest != null, $"Expecting deeper items are already existing on stack at index: {index}");
                return ref source._rest[index - SmallList2<TItem>.OnStackItemCount];
        }
    }

    /// <summary>Returns last present item ref, assumes that the list is not empty!</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem GetLastSurePresentItem<TItem>(this ref SmallList2<TItem> source) =>
        ref source.GetSurePresentItemRef(source._count - 1);

    /// <summary>Returns the ref to tombstone indicating the missing item.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem NotFound<TItem>(this ref SmallList2<TItem> _) => ref SmallList2<TItem>.Missing;

    /// <summary>Appends the default item to the end of the list and returns the reference to it.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref TItem AppendDefaultAndGetRef<TItem>(this ref SmallList2<TItem> source)
    {
        var index = source._count++;
        switch (index)
        {
            case 0: return ref source._it0;
            case 1: return ref source._it1;
            default:
                return ref AppendDefaultAndGetRef(ref source._rest, index - SmallList2<TItem>.OnStackItemCount, SmallList2<TItem>.OnStackItemCount);
        }
    }

    /// <summary>Looks for the item in the list and return its index if found or -1 for the absent item</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static int TryGetIndex<TItem, TEq>(this ref SmallList2<TItem> source, TItem it, TEq eq = default)
        where TEq : struct, IEq<TItem>
    {
        switch (source._count)
        {
            case 1:
                if (eq.Equals(it, source._it0)) return 0;
                break;

            default:
                if (eq.Equals(it, source._it0)) return 0;
                if (eq.Equals(it, source._it1)) return 1;
                if (source._rest != null)
                {
                    var count = source._count - SmallList2<TItem>.OnStackItemCount;
                    var items = source._rest;
                    for (var i = 0; i < count; ++i)
                    {
                        ref var di = ref items[i]; // todo: @perf Marshall?
                        if (eq.Equals(it, di))
                            return i + SmallList2<TItem>.OnStackItemCount;
                    }
                }
                break;
        }
        return -1;
    }

    /// <summary>Returns the ref of the found item or appends the item to the end of the list, and returns ref to it</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static int GetIndexOrAppend<TItem, TEq>(this ref SmallList2<TItem> source, TItem item, TEq eq)
        where TEq : struct, IEq<TItem>
    {
        switch (source._count)
        {
            case 0:
                source._count = 1;
                source._it0 = item;
                return -1;

            case 1:
                if (eq.Equals(item, source._it0)) return 0;
                source._count = 2;
                source._it1 = item;
                return -1;

            default:
                if (eq.Equals(item, source._it0)) return 0;
                if (eq.Equals(item, source._it1)) return 1;

                var restCount = source._count - SmallList2<TItem>.OnStackItemCount;
                if (restCount != 0)
                {
                    var items = source._rest;
                    for (var i = 0; i < restCount; ++i)
                    {
                        ref var di = ref items[i]; // todo: @perf Marshall?
                        if (eq.Equals(item, di))
                            return i + SmallList2<TItem>.OnStackItemCount;
                    }
                }
                AppendDefaultAndGetRef(ref source._rest, restCount, SmallList2<TItem>.OnStackItemCount) = item;
                ++source._count;
                return -1;
        }
    }
}

/// <summary>List with the number of first items (4) stored inside its struct and the rest in the growable array.
/// Supports addition and removal (removel is without resize) only at the end of the list, aka Stack behavior</summary>
[DebuggerDisplay("{Count} of {_it0?.ToString()}, {_it1?.ToString()}, {_it2?.ToString()}, {_it3?.ToString()}, ...")]
public struct SmallList4<TItem>
{
    /// <summary>The number of entries stored inside the map itself without moving them to array on heap</summary>
    public const int OnStackItemCount = 4;

    // todo: @check what if someone stores something in it, it would be a memory leak, but isn't it the same as using `out var` in the returning`false` Try...methods?
    internal static TItem Missing; // return the ref to Tombstone when nothing found

    internal int _count;
    internal TItem _it0, _it1, _it2, _it3;
    internal TItem[] _rest;

    /// <summary>Gets the number of items in the list</summary>
    public int Count
    {
        [MethodImpl((MethodImplOptions)256)]
        get => _count;
    }

    /// <summary>Returns surely present item by its index</summary>
    public TItem this[int index]
    {
        [MethodImpl((MethodImplOptions)256)]
        get
        {
            Debug.Assert(_count != 0);
            Debug.Assert(index < _count);
            switch (index)
            {
                case 0: return _it0;
                case 1: return _it1;
                case 2: return _it2;
                case 3: return _it3;
                default:
                    Debug.Assert(_rest != null, $"Expecting deeper items are already existing on stack at index: {index}");
                    return _rest[index - OnStackItemCount];
            }
        }
    }

    /// <summary>Adds the item to the end of the list aka the Stack.Push</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void Append(in TItem item)
    {
        var index = _count++;
        switch (index)
        {
            case 0: _it0 = item; break;
            case 1: _it1 = item; break;
            case 2: _it2 = item; break;
            case 3: _it3 = item; break;
            default:
                SmallList.AppendDefaultAndGetRef(ref _rest, index - OnStackItemCount, OnStackItemCount) = item;
                break;
        }
    }

    /// <summary>Bridge to go from the List.Add</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void Add(in TItem item) => Append(in item);

    /// <summary>Adds the default item to the end of the list aka the Stack.Push default</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void AppendDefault()
    {
        if (++_count >= OnStackItemCount)
            SmallList.AppendDefaultAndGetRef(ref _rest, _count - OnStackItemCount, OnStackItemCount);
    }

    /// <summary>Removes the last item from the list aka the Stack Pop. Assumes that the list is not empty!</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void RemoveLastSurePresentItem()
    {
        Debug.Assert(Count != 0);
        var index = --_count;
        switch (index)
        {
            case 0: _it0 = default; break;
            case 1: _it1 = default; break;
            case 2: _it2 = default; break;
            case 3: _it3 = default; break;
            default:
                Debug.Assert(_rest != null, $"Expecting a deeper parent stack created before accessing it here at level {index}");
                _rest[index - OnStackItemCount] = default;
                break;
        }
    }

    /// <summary>Copy items to new the array</summary>
    public TItem[] ToArray()
    {
        switch (Count)
        {
            case 0: return Tools.Empty<TItem>();
            case 1: return new[] { _it0 };
            case 2: return new[] { _it0, _it1 };
            case 3: return new[] { _it0, _it1, _it2 };
            case 4: return new[] { _it0, _it1, _it2, _it3 };
            default:
                var items = new TItem[Count];
                items[0] = _it0;
                items[1] = _it1;
                items[2] = _it2;
                items[3] = _it3;
                Array.Copy(_rest, 0, items, 4, Count - OnStackItemCount);
                return items;
        }
    }

    /// <summary>Exposing as list</summary>
    [MethodImpl((MethodImplOptions)256)]
    public IList<TItem> AsList() => ToArray();
}

/// <summary>List with the number of first items (2) stored inside its struct and the rest in the growable array.
/// Supports addition and removal (removel is without resize) only at the end of the list, aka Stack behavior</summary>
[DebuggerDisplay("{Count} of {_it0?.ToString()}, {_it1?.ToString()}, ...")]
public struct SmallList2<TItem>
{
    /// <summary>The number of entries stored inside the map itself without moving them to array on heap</summary>
    public const int OnStackItemCount = 2;

    // todo: @check what if someone stores something in it, it would be a memory leak, but isn't it the same as using `out var` in the returning`false` Try...methods?
    internal static TItem Missing; // return the ref to Tombstone when nothing found

    internal int _count;
    internal TItem _it0, _it1;
    internal TItem[] _rest;

    /// <summary>Good stuff</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void InitCount(int count)
    {
        _count = count;
        if (count > OnStackItemCount)
            _rest = new TItem[count - OnStackItemCount];
    }

    /// <summary>Good stiff</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void Init(TItem it0)
    {
        _count = 1;
        _it0 = it0;
    }

    /// <summary>Good steff</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void Init(TItem it0, TItem it1)
    {
        _count = 2;
        _it0 = it0;
        _it1 = it1;
    }

    /// <summary>Good staff</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void Init(TItem it0, TItem it1, params TItem[] rest)
    {
        _count = 2 + rest.Length;
        _it0 = it0;
        _it1 = it1;
        _rest = rest;
    }

    /// <summary>Good styff</summary>
    public void Init(params TItem[] items)
    {
        switch (items.Length)
        {
            case 0:
                break;
            case 1:
                Init(items[0]);
                break;
            case 2:
                Init(items[0], items[1]);
                break;
            default:
                _count = items.Length;
                _it0 = items[0];
                _it1 = items[1];
                for (var i = OnStackItemCount; i < items.Length; ++i)
                    items[i - OnStackItemCount] = items[i];
                _rest = items;
                break;
        }
    }

    /// <summary>Good staff</summary>
    public void Init<TList>(TList items) where TList : IReadOnlyList<TItem>
    {
        if (items is TItem[] arr)
        {
            Init(arr);
            return;
        }
        switch (items.Count)
        {
            case 0:
                break;
            case 1:
                Init(items[0]);
                break;
            case 2:
                Init(items[0], items[1]);
                break;
            default:
                var count = items.Count;
                var rest = new TItem[count - OnStackItemCount];
                _count = count;
                _it0 = items[0];
                _it1 = items[1];
                for (var i = OnStackItemCount; i < count; ++i)
                    rest[i - OnStackItemCount] = items[i];
                _rest = rest;
                break;
        }
    }

    /// <summary>Gets the number of items in the list</summary>
    public int Count
    {
        [MethodImpl((MethodImplOptions)256)]
        get => _count;
    }

    /// <summary>Returns surely present item by its index</summary>
    public TItem this[int index]
    {
        [MethodImpl((MethodImplOptions)256)]
        get
        {
            Debug.Assert(_count != 0);
            Debug.Assert(index < _count);
            switch (index)
            {
                case 0: return _it0;
                case 1: return _it1;
                default:
                    Debug.Assert(_rest != null, $"Expecting deeper items are already existing on stack at index: {index}");
                    return _rest[index - OnStackItemCount];
            }
        }
    }

    /// <summary>Adds the item to the end of the list aka the Stack.Push</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void Append(in TItem item)
    {
        var index = _count++;
        switch (index)
        {
            case 0: _it0 = item; break;
            case 1: _it1 = item; break;
            default:
                SmallList.AppendDefaultAndGetRef(ref _rest, index - OnStackItemCount, OnStackItemCount) = item;
                break;
        }
    }

    /// <summary>Sugar bridge from the List.Add</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void Add(in TItem item) => Append(in item);

    /// <summary>Adds the default item to the end of the list aka the Stack.Push default</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void AppendDefault()
    {
        if (++_count >= OnStackItemCount)
            SmallList.AppendDefaultAndGetRef(ref _rest, _count - OnStackItemCount, OnStackItemCount);
    }

    /// <summary>Removes the last item from the list aka the Stack Pop. Assumes that the list is not empty!</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void RemoveLastSurePresentItem()
    {
        Debug.Assert(Count != 0);
        var index = --_count;
        switch (index)
        {
            case 0: _it0 = default; break;
            case 1: _it1 = default; break;
            default:
                Debug.Assert(_rest != null, $"Expecting a deeper parent stack created before accessing it here at level {index}");
                _rest[index - OnStackItemCount] = default;
                break;
        }
    }

    /// <summary>Copy items to new the array</summary>
    [MethodImpl((MethodImplOptions)256)]
    public TItem[] ToArray()
    {
        switch (Count)
        {
            case 0: return Tools.Empty<TItem>();
            case 1: return new[] { _it0 };
            case 2: return new[] { _it0, _it1 };
            default:
                var items = new TItem[Count];
                items[0] = _it0;
                items[1] = _it1;
                Array.Copy(_rest, 0, items, 2, Count - OnStackItemCount);
                return items;
        }
    }

    /// <summary>Exposing as list</summary>
    [MethodImpl((MethodImplOptions)256)]
    public IList<TItem> AsList() => ToArray();
}

/// <summary>Configiration and the tools for the FHashMap map data structure</summary>
public static class FHashMap
{
    // todo: @improve for the future me
    // <summary>2^32 / phi for the Fibonacci hashing, where phi is the golden ratio ~1.61803</summary>
    // public const uint GoldenRatio32 = 2654435769;

    internal const byte MinFreeCapacityShift = 3; // e.g. for the capacity 16: 16 >> 3 => 2, 12.5% of the free hash slots (it does not mean the entries free slot)
    internal const byte MinHashesCapacityBitShift = 4; // 1 << 4 == 16

    /// <summary>Upper hash bits spent on storing the probes, e.g. 5 bits mean 31 probes max.</summary>
    public const byte MaxProbeBits = 5;
    internal const byte MaxProbeCount = (1 << MaxProbeBits) - 1;
    internal const byte ProbeCountShift = 32 - MaxProbeBits;
    internal const int HashAndIndexMask = ~(MaxProbeCount << ProbeCountShift);

    /// <summary>The number of entries stored inside the map itself without moving them to array on heap</summary>
    public const int StackEntriesCount = 4;

    /// <summary>Creates the map with the <see cref="SingleArrayEntries{K, V, TEq}"/> storage</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static FHashMap<K, V, TEq, SingleArrayEntries<K, V, TEq>> New<K, V, TEq>(byte capacityBitShift = 0)
        where TEq : struct, IEq<K> => new(capacityBitShift);

    /// <summary>Holds a single entry consisting of key and value. 
    /// Value may be set or changed but the key is set in stone (by construction).</summary>
    [DebuggerDisplay("{Key?.ToString()}->{Value}")]
    public struct Entry<K, V>
    {
        /// <summary>The readonly key</summary>
        public K Key;
        /// <summary>The mutable value</summary>
        public V Value;
        /// <summary>Construct with the key and default value</summary>
        public Entry(K key) => Key = key;
        /// <summary>Construct with the key and value</summary>
        public Entry(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }

    /// binary reprsentation of the `int`
    public static string ToB(int x) => System.Convert.ToString(x, 2).PadLeft(32, '0');

    [MethodImpl((MethodImplOptions)256)]
#if NET7_0_OR_GREATER
    internal static ref int GetHashRef(ref int start, int distance) => ref Unsafe.Add(ref start, distance);
#else
    internal static ref int GetHashRef(ref int[] start, int distance) => ref start[distance];
#endif

    [MethodImpl((MethodImplOptions)256)]
#if NET7_0_OR_GREATER
    internal static int GetHash(ref int start, int distance) => Unsafe.Add(ref start, distance);
#else
    internal static int GetHash(ref int[] start, int distance) => start[distance];
#endif

    /// <summary>Configures removed key tombstone, equality and hash function for the FHashMap</summary>
    public interface IEq<K>
    {
        /// <summary>Defines the value of the key indicating the removed entry</summary>
        K GetTombstone();

        /// <summary>Equals keys</summary>
        bool Equals(K x, K y);

        /// <summary>Calculates and returns the hash of the key</summary>
        int GetHashCode(K key);
    }

    /// <summary>Default comparer using the `object.GetHashCode` and `object.Equals` oveloads</summary>
    public struct DefaultEq<K> : IEq<K>
    {
        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public K GetTombstone() => default;

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public bool Equals(K x, K y) => x.Equals(y);

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public int GetHashCode(K key) => key.GetHashCode();
    }

    /// <summary>Uses the `object.GetHashCode` and `object.ReferenceEquals`</summary>
    public struct RefEq<K> : IEq<K> where K : class
    {
        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public K GetTombstone() => null;

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public bool Equals(K x, K y) => ReferenceEquals(x, y);

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public int GetHashCode(K key) => RuntimeHelpers.GetHashCode(key);
    }

    /// <summary>Compares via `ReferenceEquals` and gets the hash faster via `RuntimeHelpers.GetHashCode`</summary>
    public struct RefEq<A, B> : IEq<(A, B)>
        where A : class
        where B : class
    {
        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public (A, B) GetTombstone() => (null, null);

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public bool Equals((A, B) x, (A, B) y) =>
            ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public int GetHashCode((A, B) key) =>
            Hasher.Combine(RuntimeHelpers.GetHashCode(key.Item1), RuntimeHelpers.GetHashCode(key.Item2));
    }

    /// <summary>Compares via `ReferenceEquals` and gets the hash faster via `RuntimeHelpers.GetHashCode`</summary>
    public struct RefEq<A, B, C> : IEq<(A, B, C)>
        where A : class
        where B : class
        where C : class
    {
        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public (A, B, C) GetTombstone() => (null, null, null);

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public bool Equals((A, B, C) x, (A, B, C) y) =>
            ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2) && ReferenceEquals(x.Item3, y.Item3);

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public int GetHashCode((A, B, C) key) =>
            Hasher.Combine(RuntimeHelpers.GetHashCode(key.Item1), Hasher.Combine(RuntimeHelpers.GetHashCode(key.Item2), RuntimeHelpers.GetHashCode(key.Item3)));
    }

    /// <summary>Combines the hashes of 2 keys</summary>
    internal static class Hasher
    {
        /// <summary>Combines the hashes of 2 keys</summary>
        [MethodImpl((MethodImplOptions)256)]
        public static int Combine(int h1, int h2) => unchecked((h1 * (int)0xA5555529) + h2);
    }

    // todo: @improve can we move the Entry into the type parameter to configure and possibly save the memory e.g. for the sets? 
    /// <summary>Abstraction to configure your own entries data structure. Check the derivitives for the examples</summary>
    public interface IEntries<K, V, TEq> where TEq : IEq<K>
    {
        /// <summary>Initializes the entries storage to the specified capacity via the number of <paramref name="capacityBitShift"/> bits in the capacity</summary>
        void Init(byte capacityBitShift);

        /// <summary>Returns the reference to entry by its index, index should map to the present/non-removed entry</summary>
        ref Entry<K, V> GetSurePresentEntryRef(int index);

        /// <summary>Adds the key at the "end" of entriesc- so the order of addition is preserved.</summary>
        ref V AddKeyAndGetValueRef(K key, int index);
    }

    internal const int MinEntriesCapacity = 2;

    /// <summary>For now to use in the Set as a value</summary>
    public readonly struct NoValue {}

    /// <summary>Stores the entries in a single dynamically reallocated array</summary>
    public struct SingleArrayEntries<K, V, TEq> : IEntries<K, V, TEq> where TEq : struct, IEq<K>
    {
        internal Entry<K, V>[] _entries;

        /// <inheritdoc/>
        public void Init(byte capacityBitShift) =>
            _entries = new Entry<K, V>[1 << capacityBitShift];

        /// <inheritdoc/>
        [MethodImpl((MethodImplOptions)256)]
        public ref Entry<K, V> GetSurePresentEntryRef(int index)
        {
#if NET7_0_OR_GREATER
            return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_entries), index);
#else
            return ref _entries[index];
#endif
        }

        /// <inheritdoc/>
        [MethodImpl((MethodImplOptions)256)]
        public ref V AddKeyAndGetValueRef(K key, int index)
        {
            if (index == _entries.Length)
                Array.Resize(ref _entries, index << 1);
#if NET7_0_OR_GREATER
            ref var e = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_entries), index);
#else
            ref var e = ref _entries[index];
#endif
            e.Key = key;
            return ref e.Value;
        }
    }

    /// <summary>Finds the stored value by key. If found returns ref to the value it can be modified in place.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref V TryGetValueRef<K, V, TEq, TEntries>(this ref FHashMap<K, V, TEq, TEntries> map, K key, out bool found)
        where TEq : struct, IEq<K>
        where TEntries : struct, IEntries<K, V, TEq>
    {
        if (map._count > StackEntriesCount)
            return ref map.TryGetValueRefByHash(key, out found);
        switch (map._count)
        {
            case 1:
                if (found = default(TEq).Equals(key, map._e0.Key)) return ref map._e0.Value;
                break;
            case 2:
                if (found = default(TEq).Equals(key, map._e0.Key)) return ref map._e0.Value;
                if (found = default(TEq).Equals(key, map._e1.Key)) return ref map._e1.Value;
                break;
            case 3:
                if (found = default(TEq).Equals(key, map._e0.Key)) return ref map._e0.Value;
                if (found = default(TEq).Equals(key, map._e1.Key)) return ref map._e1.Value;
                if (found = default(TEq).Equals(key, map._e2.Key)) return ref map._e2.Value;
                break;
            case 4:
                if (found = default(TEq).Equals(key, map._e0.Key)) return ref map._e0.Value;
                if (found = default(TEq).Equals(key, map._e1.Key)) return ref map._e1.Value;
                if (found = default(TEq).Equals(key, map._e2.Key)) return ref map._e2.Value;
                if (found = default(TEq).Equals(key, map._e3.Key)) return ref map._e3.Value;
                break;
        }
        found = false;
        return ref FHashMap<K, V, TEq, TEntries>._missing.Value;
    }

    /// <summary>Finds the stored value by key. If found returns ref to the value it can be modified in place.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static bool Contains<K, V, TEq, TEntries>(this ref FHashMap<K, V, TEq, TEntries> map, K key)
        where TEq : struct, IEq<K>
        where TEntries : struct, IEntries<K, V, TEq>
    {
        if (map._count > StackEntriesCount)
        {
            map.TryGetValueRefByHash(key, out var found);
            return found;
        }

        // for small counts just compare the keys without calculating the hashes
        var eq = default(TEq);
        return map._count switch
        {
            1 => eq.Equals(key, map._e0.Key),
            2 => eq.Equals(key, map._e0.Key) || eq.Equals(key, map._e1.Key),
            3 => eq.Equals(key, map._e0.Key) || eq.Equals(key, map._e1.Key) || eq.Equals(key, map._e2.Key),
            4 => eq.Equals(key, map._e0.Key) || eq.Equals(key, map._e1.Key) || eq.Equals(key, map._e2.Key) || eq.Equals(key, map._e3.Key),
            _ => false,
        };
    }

    /// <summary>Gets the reference to the existing value of the provided key, or the default value to set for the newly added key.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref V AddOrGetValueRef<K, V, TEq, TEntries>(this ref FHashMap<K, V, TEq, TEntries> map, K key, out bool found)
        where TEq : struct, IEq<K>
        where TEntries : struct, IEntries<K, V, TEq>
    {
        if (map._count > StackEntriesCount)
            return ref map.AddOrGetValueRefByHash(key, out found);
        found = true;
        switch (map._count)
        {
            case 0:
                found = false;
                map._count = 1;
                map._e0.Key = key;
                return ref map._e0.Value;

            case 1:
                if (default(TEq).Equals(key, map._e0.Key)) return ref map._e0.Value;
                found = false;
                map._count = 2;
                map._e1.Key = key;
                return ref map._e1.Value;

            case 2:
                if (default(TEq).Equals(key, map._e0.Key)) return ref map._e0.Value;
                if (default(TEq).Equals(key, map._e1.Key)) return ref map._e1.Value;
                found = false;
                map._count = 3;
                map._e2.Key = key;
                return ref map._e2.Value;

            case 3:
                if (default(TEq).Equals(key, map._e0.Key)) return ref map._e0.Value;
                if (default(TEq).Equals(key, map._e1.Key)) return ref map._e1.Value;
                if (default(TEq).Equals(key, map._e2.Key)) return ref map._e2.Value;
                found = false;
                map._count = 4;
                map._e3.Key = key;
                return ref map._e3.Value;

            default:
                if (default(TEq).Equals(key, map._e0.Key)) return ref map._e0.Value;
                if (default(TEq).Equals(key, map._e1.Key)) return ref map._e1.Value;
                if (default(TEq).Equals(key, map._e2.Key)) return ref map._e2.Value;
                if (default(TEq).Equals(key, map._e3.Key)) return ref map._e3.Value;
                found = false;

                map._capacityBitShift = MinHashesCapacityBitShift;
                map._packedHashesAndIndexes = new int[1 << MinHashesCapacityBitShift];

                var indexMask = (1 << MinHashesCapacityBitShift) - 1;

                // todo: @perf optimize by calculating the keys hashes and putting them into the span and iterating over them inside a single method

                map.AddInitialHashWithoutResizing(map._e0.Key, 0, indexMask);
                map.AddInitialHashWithoutResizing(map._e1.Key, 1, indexMask);
                map.AddInitialHashWithoutResizing(map._e2.Key, 2, indexMask);
                map.AddInitialHashWithoutResizing(map._e3.Key, 3, indexMask);
                map.AddInitialHashWithoutResizing(key, StackEntriesCount, indexMask);

                map._count = 5;
                map._entries.Init(2);

                // we do not copying the entries because we provide the stable value reference guaranties
                return ref map._entries.AddKeyAndGetValueRef(key, 0);
        }
    }

    private static void AddInitialHashWithoutResizing<K, V, TEq, TEntries>(this ref FHashMap<K, V, TEq, TEntries> map, K key, int index, int indexMask)
        where TEq : struct, IEq<K>
        where TEntries : struct, IEntries<K, V, TEq>
    {
#if NET7_0_OR_GREATER
        ref var hashesAndIndexes = ref MemoryMarshal.GetArrayDataReference(map._packedHashesAndIndexes);
#else
        var hashesAndIndexes = map._packedHashesAndIndexes;
#endif
        var hash = default(TEq).GetHashCode(key);
        var hashIndex = hash & indexMask;

        // 1. Skip over hashes with the bigger and equal probes. The hashes with bigger probes overlapping from the earlier ideal positions
        ref var h = ref GetHashRef(ref hashesAndIndexes, hashIndex);
        var probes = 1;
        while ((h >>> ProbeCountShift) >= probes)
        {
            h = ref GetHashRef(ref hashesAndIndexes, ++hashIndex & indexMask);
            ++probes;
        }

        // 3. We did not find the hash and therefore the key, so insert the new entry
        var hRobinHooded = h;
        h = (probes << ProbeCountShift) | (hash & HashAndIndexMask & ~indexMask) | index;

        // 4. If the robin hooded hash is empty then we stop
        // 5. Otherwise we steal the slot with the smaller probes
        probes = hRobinHooded >>> ProbeCountShift;
        while (hRobinHooded != 0)
        {
            h = ref GetHashRef(ref hashesAndIndexes, ++hashIndex & indexMask);
            if ((h >>> ProbeCountShift) < ++probes)
            {
                var tmp = h;
                h = (probes << ProbeCountShift) | (hRobinHooded & HashAndIndexMask);
                hRobinHooded = tmp;
                probes = hRobinHooded >>> ProbeCountShift;
            }
        }
    }

    /// <summary>Adds the sure absent key entry. 
    /// Provides the performance in scenarios where you look for present key, and using it, and if ABSENT then add the new one.
    /// So this method optimized NOT to look for the present item for the second time in SEQUENCE</summary>
    public static ref V AddSureAbsentDefaultAndGetRef<K, V, TEq, TEntries>(this ref FHashMap<K, V, TEq, TEntries> map, K key)
        where TEq : struct, IEq<K>
        where TEntries : struct, IEntries<K, V, TEq>
    {
        if (map._count > StackEntriesCount)
            return ref map.AddSureAbsentDefaultAndGetRefByHash(key);
        switch (map._count)
        {
            case 0:
                map._count = 1;
                map._e0.Key = key;
                return ref map._e0.Value;

            case 1:
                map._count = 2;
                map._e1.Key = key;
                return ref map._e1.Value;

            case 2:
                map._count = 3;
                map._e2.Key = key;
                return ref map._e2.Value;

            case 3:
                map._count = 4;
                map._e3.Key = key;
                return ref map._e3.Value;

            default:
                map._capacityBitShift = MinHashesCapacityBitShift;
                map._packedHashesAndIndexes = new int[1 << MinHashesCapacityBitShift];

                var indexMask = (1 << MinHashesCapacityBitShift) - 1;

                map.AddInitialHashWithoutResizing(map._e0.Key, 0, indexMask);
                map.AddInitialHashWithoutResizing(map._e1.Key, 1, indexMask);
                map.AddInitialHashWithoutResizing(map._e2.Key, 2, indexMask);
                map.AddInitialHashWithoutResizing(map._e3.Key, 3, indexMask);
                map.AddInitialHashWithoutResizing(key, StackEntriesCount, indexMask);

                map._count = 5;
                map._entries.Init(2);
                return ref map._entries.AddKeyAndGetValueRef(key, 0);
        }
    }

    [MethodImpl((MethodImplOptions)256)]
    private static ref V AddSureAbsentDefaultAndGetRefByHash<K, V, TEq, TEntries>(this ref FHashMap<K, V, TEq, TEntries> map, K key)
        where TEq : struct, IEq<K>
        where TEntries : struct, IEntries<K, V, TEq>
    {
        // if the free space is less than 1/8 of capacity (12.5%) then Resize
        var indexMask = (1 << map._capacityBitShift) - 1;
        if (indexMask - map._count <= (indexMask >>> MinFreeCapacityShift))
            indexMask = map.ResizeHashes(indexMask);

        var hash = default(TEq).GetHashCode(key);
        var hashIndex = hash & indexMask;

#if NET7_0_OR_GREATER
        ref var hashesAndIndexes = ref MemoryMarshal.GetArrayDataReference(map._packedHashesAndIndexes);
#else
        var hashesAndIndexes = map._packedHashesAndIndexes;
#endif
        ref var h = ref GetHashRef(ref hashesAndIndexes, hashIndex);

        // 1. Skip over hashes with the bigger and equal probes. The hashes with bigger probes overlapping from the earlier ideal positions
        var probes = 1;
        while ((h >>> ProbeCountShift) >= probes)
        {
            h = ref GetHashRef(ref hashesAndIndexes, ++hashIndex & indexMask);
            ++probes;
        }

        // 3. We did not find the hash and therefore the key, so insert the new entry
        var hRobinHooded = h;
        h = (probes << ProbeCountShift) | (hash & HashAndIndexMask & ~indexMask) | map._count;

        // 4. If the robin hooded hash is empty then we stop
        // 5. Otherwise we steal the slot with the smaller probes
        probes = hRobinHooded >>> ProbeCountShift;
        while (hRobinHooded != 0)
        {
            h = ref GetHashRef(ref hashesAndIndexes, ++hashIndex & indexMask);
            if ((h >>> ProbeCountShift) < ++probes)
            {
                var tmp = h;
                h = (probes << ProbeCountShift) | (hRobinHooded & HashAndIndexMask);
                hRobinHooded = tmp;
                probes = hRobinHooded >>> ProbeCountShift;
            }
        }

        return ref map._entries.AddKeyAndGetValueRef(key, (map._count++) - StackEntriesCount);
    }

    ///<summary>Get the value ref by the entry index. Also the index corresponds to entry adding order.
    ///Improtant: it does not checks the index bounds, so you need to check that the index is from 0 to map.Count-1</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ref Entry<K, V> GetSurePresentEntryRef<K, V, TEq, TEntries>(this ref FHashMap<K, V, TEq, TEntries> map, int index)
        where TEq : struct, IEq<K>
        where TEntries : struct, IEntries<K, V, TEq>
    {
        Debug.Assert(index >= 0);
        Debug.Assert(index < map._count);
        if (index >= StackEntriesCount)
            return ref map._entries.GetSurePresentEntryRef(index - StackEntriesCount);
        switch (index)
        {
            case 0: return ref map._e0;
            case 1: return ref map._e1;
            case 2: return ref map._e2;
            case 3: return ref map._e3;
        }
        return ref FHashMap<K, V, TEq, TEntries>._missing;
    }

    [MethodImpl((MethodImplOptions)256)]
    internal static ref V TryGetValueRefByHash<K, V, TEq, TEntries>(this ref FHashMap<K, V, TEq, TEntries> map, K key, out bool found)
        where TEq : struct, IEq<K>
        where TEntries : struct, IEntries<K, V, TEq>
    {
        var hash = default(TEq).GetHashCode(key);

        var indexMask = (1 << map._capacityBitShift) - 1;
        var hashMiddleMask = HashAndIndexMask & ~indexMask;
        var hashMiddle = hash & hashMiddleMask;
        var hashIndex = hash & indexMask;

#if NET7_0_OR_GREATER
        ref var hashesAndIndexes = ref MemoryMarshal.GetArrayDataReference(map._packedHashesAndIndexes);
#else
        var hashesAndIndexes = map._packedHashesAndIndexes;
#endif

        var h = GetHash(ref hashesAndIndexes, hashIndex);

        // 1. Skip over hashes with the bigger and equal probes. The hashes with bigger probes overlapping from the earlier ideal positions
        var probes = 1;
        while ((h >>> ProbeCountShift) >= probes)
        {
            // 2. For the equal probes check for equality the hash middle part, and update the entry if the keys are equal too 
            if (((h >>> ProbeCountShift) == probes) & ((h & hashMiddleMask) == hashMiddle))
            {
                ref var e = ref map.GetSurePresentEntryRef(h & indexMask);
                if (default(TEq).Equals(e.Key, key))
                {
                    found = true;
                    return ref e.Value;
                }
            }

            h = GetHash(ref hashesAndIndexes, ++hashIndex & indexMask);
            ++probes;
        }

        found = false;
        return ref FHashMap<K, V, TEq, TEntries>._missing.Value;
    }

    [MethodImpl((MethodImplOptions)256)]
    private static ref V AddOrGetValueRefByHash<K, V, TEq, TEntries>(this ref FHashMap<K, V, TEq, TEntries> map, K key, out bool found)
        where TEq : struct, IEq<K>
        where TEntries : struct, IEntries<K, V, TEq>
    {
        // if the free space is less than 1/8 of capacity (12.5%) then Resize
        var indexMask = (1 << map._capacityBitShift) - 1;
        if (indexMask - map._count <= (indexMask >>> MinFreeCapacityShift))
            indexMask = map.ResizeHashes(indexMask);

        var hash = default(TEq).GetHashCode(key);
        var hashMiddleMask = HashAndIndexMask & ~indexMask;
        var hashMiddle = hash & hashMiddleMask;
        var hashIndex = hash & indexMask;

#if NET7_0_OR_GREATER
        ref var hashesAndIndexes = ref MemoryMarshal.GetArrayDataReference(map._packedHashesAndIndexes);
#else
        var hashesAndIndexes = map._packedHashesAndIndexes;
#endif
        ref var h = ref GetHashRef(ref hashesAndIndexes, hashIndex);

        // 1. Skip over hashes with the bigger and equal probes. The hashes with bigger probes overlapping from the earlier ideal positions
        var probes = 1;
        while ((h >>> ProbeCountShift) >= probes)
        {
            // 2. For the equal probes check for equality the hash middle part, and update the entry if the keys are equal too 
            if (((h >>> ProbeCountShift) == probes) & ((h & hashMiddleMask) == hashMiddle))
            {
                ref var e = ref map.GetSurePresentEntryRef(h & indexMask);
                if (default(TEq).Equals(e.Key, key))
                {
                    found = true;
                    return ref e.Value;
                }
            }
            h = ref GetHashRef(ref hashesAndIndexes, ++hashIndex & indexMask);
            ++probes;
        }

        // 3. We did not find the hash and therefore the key, so insert the new entry
        var hRobinHooded = h;
        h = (probes << ProbeCountShift) | hashMiddle | map._count;

        // 4. If the robin hooded hash is empty then we stop
        // 5. Otherwise we steal the slot with the smaller probes
        probes = hRobinHooded >>> ProbeCountShift;
        while (hRobinHooded != 0)
        {
            h = ref GetHashRef(ref hashesAndIndexes, ++hashIndex & indexMask);
            if ((h >>> ProbeCountShift) < ++probes)
            {
                var tmp = h;
                h = (probes << ProbeCountShift) | (hRobinHooded & HashAndIndexMask);
                hRobinHooded = tmp;
                probes = hRobinHooded >>> ProbeCountShift;
            }
        }
        found = false;
        return ref map._entries.AddKeyAndGetValueRef(key, (map._count++) - StackEntriesCount);
    }
}

// todo: @improve ? how/where to add SIMD to improve CPU utilization but not losing perf for smaller sizes
/// <summary>
/// Fast and less-allocating hash map without thread safety nets. Please measure it in your own use case before use.
/// It is configurable in regard of hash calculation/equality via `TEq` type paremeter and 
/// in regard of key-value storage via `TEntries` type parameter.
/// 
/// Details:
/// - Implemented as a struct so that the empty/default map does not allocate on heap
/// - Hashes and key-values are the separate collections enabling better cash locality and faster performance (data-oriented design)
/// - No SIMD for now to avoid complexity and costs for the smaller maps, so the map is more fit for the smaller sizes.
/// - Provides the "stable" enumeration of the entries in the added order
/// - The TryRemove method removes the hash but replaces the key-value entry with the tombstone key and the default value.
/// For instance, for the `RefEq` the tombstone is <see langword="null"/>. You may redefine it in the `IEq{K}.GetTombstone()` implementation.
/// 
/// </summary>
[DebuggerDisplay("{Count} of {_e0}, {_e1}, {_e2}, {_e3}, ...")]
public struct FHashMap<K, V, TEq, TEntries>
    where TEq : struct, IEq<K>
    where TEntries : struct, IEntries<K, V, TEq>
{
    internal static Entry<K, V> _missing;

    internal byte _capacityBitShift;
    internal int _count;

    // The _packedHashesAndIndexes elements are of `Int32` with the bits split as following:
    // 00010|000...110|01101
    // |     |         |- The index into the _entries structure, 0-based. The index bit count (indexMask) is the hashes capacity - 1.
    // |     |         | This part of the erased hash is used to get the ideal index into the hashes array, so later this part of hash may be restored from the hash index and its probes.
    // |     |- The remaining middle bits of the original hash
    // |- 5 (MaxProbeBits) high bits of the Probe count, with the minimal value of b00001 indicating the non-empty slot.
    internal int[] _packedHashesAndIndexes;

#pragma warning disable IDE0044 // it tries to make entries readonly but they should stay modifyable to prevent its defensive struct copying  
    internal TEntries _entries;
#pragma warning restore IDE0044

    // todo: @improve how to configure how much we store on stack
    internal Entry<K, V> _e0, _e1, _e2, _e3;

    /// <summary>Capacity bits</summary>
    public int CapacityBitShift => _capacityBitShift;

    /// <summary>Access to the hashes and indexes</summary>
    public int[] PackedHashesAndIndexes => _packedHashesAndIndexes;

    /// <summary>Number of entries in the map</summary>
    public int Count => _count;

    /// <summary>Access to the key-value entries</summary>
    public TEntries Entries => _entries;

    /// <summary>Capacity calculates as `1 leftShift capacityBitShift`</summary>
    public FHashMap(byte capacityBitShift)
    {
        _capacityBitShift = capacityBitShift;

        // the overflow tail to the hashes is the size of log2N where N==capacityBitShift, 
        // it is probably fine to have the check for the overlow of capacity because it will be mispredicted only once at the end of loop (it even rarely for the lookup)
        _packedHashesAndIndexes = new int[1 << capacityBitShift];
        _entries = default;
        _entries.Init(capacityBitShift);
    }

    internal int ResizeHashes(int indexMask)
    {
        var oldCapacity = indexMask + 1;
        var newHashAndIndexMask = HashAndIndexMask & ~oldCapacity;
        var newIndexMask = (indexMask << 1) | 1;

        var newHashesAndIndexes = new int[oldCapacity << 1];

#if NET7_0_OR_GREATER
        ref var newHashes = ref MemoryMarshal.GetArrayDataReference(newHashesAndIndexes);
        ref var oldHashes = ref MemoryMarshal.GetArrayDataReference(_packedHashesAndIndexes);
        var oldHash = oldHashes;
#else
        var newHashes = newHashesAndIndexes;
        var oldHashes = _packedHashesAndIndexes;
        var oldHash = oldHashes[0];
#endif
        // Overflow segment is wrapped-around hashes and! the hashes at the beginning robin hooded by the wrapped-around hashes
        var i = 0;
        while ((oldHash >>> ProbeCountShift) > 1)
            oldHash = GetHash(ref oldHashes, ++i);

        var oldCapacityWithOverflowSegment = i + oldCapacity;
        while (true)
        {
            if (oldHash != 0)
            {
                // get the new hash index from the old one with the next bit equal to the `oldCapacity`
                var indexWithNextBit = (oldHash & oldCapacity) | (((i + 1) - (oldHash >>> ProbeCountShift)) & indexMask);

                // no need for robinhooding because we already did it for the old hashes and now just sparcing the hashes into the new array which are already in order
                var probes = 1;
                ref var newHash = ref GetHashRef(ref newHashes, indexWithNextBit);
                while (newHash != 0)
                {
                    newHash = ref GetHashRef(ref newHashes, ++indexWithNextBit & newIndexMask);
                    ++probes;
                }
                newHash = (probes << ProbeCountShift) | (oldHash & newHashAndIndexMask);
            }
            if (++i >= oldCapacityWithOverflowSegment)
                break;

            oldHash = GetHash(ref oldHashes, i & indexMask);
        }
        ++_capacityBitShift;
        _packedHashesAndIndexes = newHashesAndIndexes;
        return newIndexMask;
    }
}
