namespace AutoMapper.Internal
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IDictionary<TKey, TValue> : System.Collections.Generic.IDictionary<TKey, TValue>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="addValue"></param>
        /// <param name="updateValueFactory"></param>
        /// <returns></returns>
        TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryRemove(TKey key, out TValue value);
    }

    /// <summary>
    /// 
    /// </summary>
    public static class FeatureDetector
    {
        /// <summary>
        /// 
        /// </summary>
        public static Func<Type, bool> IsIDataRecordType = t => false;

        /// <summary>
        /// 
        /// </summary>
        private static bool? _isEnumGetNamesSupported;

        /// <summary>
        /// 
        /// </summary>
        public static bool IsEnumGetNamesSupported
        {
            get
            {
                if (_isEnumGetNamesSupported == null)
                    _isEnumGetNamesSupported = ResolveIsEnumGetNamesSupported();

                return _isEnumGetNamesSupported.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static bool ResolveIsEnumGetNamesSupported()
        {
            return typeof (Enum).GetMethod("GetNames") != null;
        }
    }
}
