namespace AutoMapper.UnitTests
{
    using System;
    using System.Linq;
    using Should;
    using Xunit;

    // ReSharper disable ConvertPropertyToExpressionBody
    namespace Dictionaries
    {
        //[Explicit("Need to resolve the assignable collection bug as well")]
        //public class When_mapping_to_a_non_generic_dictionary : AutoMapperSpecBase
        //{
        //    private Destination _result;

        //    public class Source
        //    {
        //        public Hashtable Values { get; set; }
        //    }

        //    public class Destination
        //    {
        //        public IDictionary Values { get; set; }
        //    }

        //    protected override void Establish_context()
        //    {
        //        Mapper.CreateMap<Source, Destination>();
        //    }

        //    protected override void Because_of()
        //    {
        //        var source = new Source
        //            {
        //                Values = new Hashtable
        //                    {
        //                        {"Key1", "Value1"},
        //                        {"Key2", 4}
        //                    }
        //            };

        //        _result = Mapper.Map<Source, Destination>(source);
        //    }

        //    [Fact]
        //    public void Should_map_the_source_dictionary_with_all_keys_and_values_preserved()
        //    {
        //        _result.Values.Count.ShouldEqual(2);

        //        _result.Values["Key1"].ShouldEqual("Value1");
        //        _result.Values["Key2"].ShouldEqual(4);
        //    }
        //}

        public class When_mapping_to_a_generic_dictionary_with_mapped_value_pairs : SpecBase
        {
            private Destination _result;

            public class Source
            {
                public System.Collections.Generic.Dictionary<string, SourceValue> Values { get; set; }
            }

            public class SourceValue
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public System.Collections.Generic.Dictionary<string, DestinationValue> Values { get; set; }
            }

            public class DestinationValue
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Destination>();
                Mapper.CreateMap<SourceValue, DestinationValue>();
            }

            protected override void Because_of()
            {
                var source = new Source
                {
                    Values = new System.Collections.Generic.Dictionary<string, SourceValue>
                    {
                        {"Key1", new SourceValue {Value = 5}},
                        {"Key2", new SourceValue {Value = 10}},
                    }
                };

                _result = Mapper.Map<Source, Destination>(source);
            }

            [Fact]
            public void Should_perform_mapping_for_individual_values()
            {
                _result.Values.Count.ShouldEqual(2);

                _result.Values["Key1"].Value.ShouldEqual(5);
                _result.Values["Key2"].Value.ShouldEqual(10);
            }
        }

        public class When_mapping_to_a_generic_dictionary_interface_with_mapped_value_pairs : SpecBase
        {
            private Destination _result;

            public class Source
            {
                public System.Collections.Generic.Dictionary<string, SourceValue> Values { get; set; }
            }

            public class SourceValue
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public System.Collections.Generic.IDictionary<string, DestinationValue> Values { get; set; }
            }

            public class DestinationValue
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Destination>();
                Mapper.CreateMap<SourceValue, DestinationValue>();
            }

            protected override void Because_of()
            {
                var source = new Source
                {
                    Values = new System.Collections.Generic.Dictionary<string, SourceValue>
                    {
                        {"Key1", new SourceValue {Value = 5}},
                        {"Key2", new SourceValue {Value = 10}},
                    }
                };

                _result = Mapper.Map<Source, Destination>(source);
            }

            [Fact]
            public void Should_perform_mapping_for_individual_values()
            {
                _result.Values.Count.ShouldEqual(2);

                _result.Values["Key1"].Value.ShouldEqual(5);
                _result.Values["Key2"].Value.ShouldEqual(10);
            }
        }

        public class When_mapping_from_a_source_with_a_null_dictionary_member : AutoMapperSpecBase
        {
            private FooDto _result;

            public class Foo
            {
                public System.Collections.Generic.IDictionary<string, Foo> Bar { get; set; }
            }

            public class FooDto
            {
                public System.Collections.Generic.IDictionary<string, FooDto> Bar { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.AllowNullDestinationValues = false;
                Mapper.CreateMap<Foo, FooDto>();
                Mapper.CreateMap<FooDto, Foo>();
            }

            protected override void Because_of()
            {
                var foo1 = new Foo
                {
                    Bar = new System.Collections.Generic.Dictionary<string, Foo>
                    {
                        {"lol", new Foo()}
                    }
                };

                _result = Mapper.Map<Foo, FooDto>(foo1);
            }

            [Fact]
            public void Should_fill_the_destination_with_an_empty_dictionary()
            {
                _result.Bar["lol"].Bar.ShouldNotBeNull();
                _result.Bar["lol"].Bar.ShouldBeType<System.Collections.Generic.Dictionary<string, FooDto>>();
            }
        }


        public class When_mapping_to_a_generic_dictionary_that_does_not_use_keyvaluepairs : SpecBase
        {
            private System.Collections.Generic.IDictionary<string, string> _dest;

            public class SourceDto
            {
                public System.Collections.Generic.IDictionary<string, string> Items { get; set; }
            }

            public class DestDto
            {
                public System.Collections.Generic.IDictionary<string, string> Items { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<SourceDto, DestDto>()
                    .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));
            }

            protected override void Because_of()
            {
                var source = new SourceDto()
                {
                    Items = new GenericWrappedDictionary<string, string>
                    {
                        {"A", "AAA"},
                        {"B", "BBB"},
                        {"C", "CCC"}
                    }
                };

                _dest = Mapper.Map<System.Collections.Generic.IDictionary<string, string>,
                    System.Collections.Generic.IDictionary<string, string>>(source.Items);
            }

            [Fact]
            public void Should_map_using_the_nongeneric_dictionaryentry()
            {
                _dest.Values.Count.ShouldEqual(3);
            }

            // A wrapper for an IDictionary that implements IDictionary<TKey, TValue>
            //
            // The important difference from a standard generic BCL dictionary is that:
            //
            // ((IEnumerable)GenericWrappedDictionary).GetEnumerator() returns DictionaryEntrys
            // GenericWrappedDictionary.GetEnumerator() returns KeyValuePairs
            //
            // This behaviour is demonstrated by NHibernate's PersistentGenericMap
            // (which wraps a nongeneric PersistentMap).
            public class GenericWrappedDictionary<TKey, TValue> :
                System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.IDictionary
            {
                private readonly System.Collections.IDictionary _dictionary
                    = new System.Collections.Generic.Dictionary<TKey, TValue>();

                public void Add(TKey key, TValue value)
                {
                    _dictionary.Add(key, value);
                }

                public bool ContainsKey(TKey key)
                {
                    throw new NotImplementedException();
                }

                public System.Collections.Generic.ICollection<TKey> Keys
                {
                    get { return _dictionary.Keys.Cast<TKey>().ToList(); }
                }

                public bool Remove(TKey key)
                {
                    throw new NotImplementedException();
                }

                public bool TryGetValue(TKey key, out TValue value)
                {
                    throw new NotImplementedException();
                }

                public System.Collections.Generic.ICollection<TValue> Values
                {
                    get { return _dictionary.Values.Cast<TValue>().ToList(); }
                }

                public TValue this[TKey key]
                {
                    get { return (TValue) _dictionary[key]; }
                    set { _dictionary[key] = value; }
                }

                public void Add(System.Collections.Generic.KeyValuePair<TKey, TValue> item)
                {
                    throw new NotImplementedException();
                }

                public void Clear()
                {
                    throw new NotImplementedException();
                }

                public bool Contains(System.Collections.Generic.KeyValuePair<TKey, TValue> item)
                {
                    throw new NotImplementedException();
                }

                public void CopyTo(System.Collections.Generic.KeyValuePair<TKey, TValue>[] array, int arrayIndex)
                {
                    throw new NotImplementedException();
                }

                public int Count
                {
                    get { throw new NotImplementedException(); }
                }

                public bool IsReadOnly
                {
                    get { throw new NotImplementedException(); }
                }

                public bool Remove(System.Collections.Generic.KeyValuePair<TKey, TValue> item)
                {
                    throw new NotImplementedException();
                }

                public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> GetEnumerator()
                {
                    return _dictionary.OfType<System.Collections.DictionaryEntry>()
                        .Select(e => new System.Collections.Generic.KeyValuePair<TKey, TValue>((TKey) e.Key, (TValue) e.Value))
                        .GetEnumerator();
                }

                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                {
                    return _dictionary.GetEnumerator();
                }

                public void Add(object key, object value)
                {
                    _dictionary.Add(key, value);
                }

                public bool Contains(object key)
                {
                    throw new NotImplementedException();
                }

                System.Collections.IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator()
                {
                    return _dictionary.GetEnumerator();
                }

                public bool IsFixedSize
                {
                    get { throw new NotImplementedException(); }
                }

                System.Collections.ICollection System.Collections.IDictionary.Keys
                {
                    get { return _dictionary.Keys; }
                }

                public void Remove(object key)
                {
                    throw new NotImplementedException();
                }

                System.Collections.ICollection System.Collections.IDictionary.Values
                {
                    get { return _dictionary.Values; }
                }

                public object this[object key]
                {
                    get { return _dictionary[key]; }
                    set { _dictionary[key] = value; }
                }

                public void CopyTo(Array array, int index)
                {
                    throw new NotImplementedException();
                }

                public bool IsSynchronized
                {
                    get { throw new NotImplementedException(); }
                }

                public object SyncRoot
                {
                    get { throw new NotImplementedException(); }
                }
            }
        }

        public class When_mapping_from_a_list_of_object_to_generic_dictionary : SpecBase
        {
            private FooObject _result;

            public class FooDto
            {
                public DestinationValuePair[] Values { get; set; }
            }

            public class FooObject
            {
                public System.Collections.Generic.Dictionary<string, string> Values { get; set; }
            }

            public class DestinationValuePair
            {
                public string Key { get; set; }
                public string Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<FooDto, FooObject>();
                Mapper.CreateMap<DestinationValuePair, System.Collections.Generic.KeyValuePair<string, string>>()
                    .ConvertUsing(src => new System.Collections.Generic.KeyValuePair<string, string>(src.Key, src.Value));
            }

            protected override void Because_of()
            {
                var source = new FooDto
                {
                    Values = new System.Collections.Generic.List<DestinationValuePair>
                    {
                        new DestinationValuePair {Key = "Key1", Value = "Value1"},
                        new DestinationValuePair {Key = "Key2", Value = "Value2"}
                    }.ToArray()
                };

                _result = Mapper.Map<FooDto, FooObject>(source);
            }

            [Fact]
            public void Should_perform_mapping_for_individual_values()
            {
                _result.Values.Count.ShouldEqual(2);

                _result.Values["Key1"].ShouldEqual("Value1");
                _result.Values["Key2"].ShouldEqual("Value2");
            }
        }
    }
}