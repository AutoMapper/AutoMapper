namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

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
        void Clear();
        ICollection<TValue> Values { get; }
        ICollection<TKey> Keys { get; }
        bool ContainsKey(TKey key);
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