using System.Collections;
using System.Collections.Generic;
using AutoMapper.Configuration.Internal;
using Xunit;
using Shouldly;

namespace AutoMapper.UnitTests
{
    using Configuration;

    public class PrimitiveExtensionsTester
    {
        interface Interface
        {
            int Value { get; }
        }

        class DestinationClass : Interface
        {
            int Interface.Value { get { return 123; } }

            public int PrivateProperty { get; private set; }

            public int PublicProperty { get; set; }
        }

        class CustomCollection<T> : IList<T>
        {
            private List<T> _collection = new List<T>();

            public T this[int index] { get => ((IList<T>)_collection)[index]; set => ((IList<T>)_collection)[index] = value; }

            public int Count => ((IList<T>)_collection).Count;

            public bool IsReadOnly => ((IList<T>)_collection).IsReadOnly;

            public void Add(T item)
            {
                ((IList<T>)_collection).Add(item);
            }

            public void Clear()
            {
                ((IList<T>)_collection).Clear();
            }

            public bool Contains(T item)
            {
                return ((IList<T>)_collection).Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                ((IList<T>)_collection).CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ((IList<T>)_collection).GetEnumerator();
            }

            public int IndexOf(T item)
            {
                return ((IList<T>)_collection).IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                ((IList<T>)_collection).Insert(index, item);
            }

            public bool Remove(T item)
            {
                return ((IList<T>)_collection).Remove(item);
            }

            public void RemoveAt(int index)
            {
                ((IList<T>)_collection).RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IList<T>)_collection).GetEnumerator();
            }
        }

        [Fact]
        public void Should_find_explicitly_implemented_member()
        {
            PrimitiveHelper.GetFieldOrProperty(typeof(DestinationClass), "Value").ShouldNotBeNull();
        }

        [Fact]
        public void Should_not_flag_only_enumerable_type_as_writeable_collection()
        {
            PrimitiveHelper.IsListOrDictionaryType(typeof(string)).ShouldBeFalse();
        }

        [Fact]
        public void Should_flag_list_as_writable_collection()
        {
            PrimitiveHelper.IsListOrDictionaryType(typeof(int[])).ShouldBeTrue();
        }

        [Fact]
        public void Should_flag_generic_list_as_writeable_collection()
        {
            PrimitiveHelper.IsListOrDictionaryType(typeof(List<int>)).ShouldBeTrue();
        }

        [Fact]
        public void Should_flag_dictionary_as_writeable_collection()
        {
            PrimitiveHelper.IsListOrDictionaryType(typeof(Dictionary<string, int>)).ShouldBeTrue();
        }

        [Fact]
        public void Should_flag_custom_generic_list_type_as_writeable_collection()
        {
            PrimitiveHelper.IsListOrDictionaryType(typeof(CustomCollection<int>)).ShouldBeTrue();
        }
    }
}