namespace AutoMapper.Mappers
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public class ObjectMapperCollection : IObjectMapperCollection
    {
        /// <summary>
        /// Mappers backing field.
        /// </summary>
        private IList<IObjectMapper> _collection;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<IObjectMapper> GetDefaultMappers()
        {
            yield return new ExpressionMapper();
            yield return new TypeMapMapper(TypeMapObjectMapperRegistry.Mappers);
            yield return new AssignableArrayMapper();
            yield return new FlagsEnumMapper();
            yield return new EnumMapper();
            yield return new PrimitiveArrayMapper();
            yield return new ArrayMapper();
            yield return new EnumerableToDictionaryMapper();
#if NET4 || MONODROID || MONOTOUCH || __IOS__ || DNXCORE50
            yield return new NameValueCollectionMapper();
#endif
            yield return new DictionaryMapper();
            yield return new ReadOnlyCollectionMapper();
#if NET4 || NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || SILVERLIGHT || DNXCORE50
            yield return new HashSetMapper();
#endif
            yield return new CollectionMapper();
            yield return new EnumerableMapper();
#if MONODROID || MONOTOUCH || __IOS__ || NET4
            yield return new ListSourceMapper();
#endif
#if SILVERLIGHT || NETFX_CORE
            new StringMapper();
#endif
            yield return new AssignableMapper();
#if NET4 || MONODROID || MONOTOUCH || __IOS__ || SILVERLIGHT || DNXCORE50
            yield return new TypeConverterMapper();
#endif
            yield return new NullableSourceMapper();
            yield return new NullableMapper();
            yield return new ImplicitConversionOperatorMapper();
            yield return new ExplicitConversionOperatorMapper();
            yield return new OpenGenericMapper();
        }

        /// <summary>
        /// 
        /// </summary>
        public ObjectMapperCollection()
            : this(GetDefaultMappers())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappers"></param>
        public ObjectMapperCollection(IEnumerable<IObjectMapper> mappers)
        {
            _collection = new List<IObjectMapper>(mappers);
        }

        #region Collection Members

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IObjectMapper> GetEnumerator()
        {
            lock (_collection) return _collection.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(IObjectMapper item)
        {
            lock (_collection) _collection.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            lock (_collection) _collection.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(IObjectMapper item)
        {
            lock (_collection) return _collection.Contains(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(IObjectMapper[] array, int arrayIndex)
        {
            lock (_collection) _collection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(IObjectMapper item)
        {
            lock (_collection) return _collection.Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get { lock (_collection) return _collection.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly
        {
            get { lock (_collection) return _collection.IsReadOnly; }
        }

        #endregion

        #region Mapper Collection Members

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            _collection = new List<IObjectMapper>(GetDefaultMappers());
        }

        #endregion

        #region List Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(IObjectMapper item)
        {
            lock (_collection) return _collection.IndexOf(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, IObjectMapper item)
        {
            lock (_collection) _collection.Insert(index, item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            lock (_collection) _collection.RemoveAt(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IObjectMapper this[int index]
        {
            get { lock (_collection) return _collection[index]; }
            set { lock (_collection) _collection[index] = value; }
        }

        #endregion
    }
}
