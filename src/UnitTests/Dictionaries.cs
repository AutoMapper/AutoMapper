namespace AutoMapper.UnitTests
{
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
        //        _result.Values.Count.ShouldBe(2);

        //        _result.Values["Key1"].ShouldBe("Value1");
        //        _result.Values["Key2"].ShouldBe(4);
        //    }
        //}

        public class When_mapping_to_a_generic_dictionary_with_mapped_value_pairs : AutoMapperSpecBase
        {
            private Destination _result;

            public class Source
            {
                public Dictionary<string, SourceValue> Values { get; set; }
            }

            public class SourceValue
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public Dictionary<string, DestinationValue> Values { get; set; }
            }

            public class DestinationValue
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg => 
            {
                cfg.CreateMap<Source, Destination>();
                cfg.CreateMap<SourceValue, DestinationValue>();
            });

            protected override void Because_of()
            {
                var source = new Source
                    {
                        Values = new Dictionary<string, SourceValue>
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
                _result.Values.Count.ShouldBe(2);

                _result.Values["Key1"].Value.ShouldBe(5);
                _result.Values["Key2"].Value.ShouldBe(10);
            }
        }

        public class When_mapping_to_a_generic_dictionary_interface_with_mapped_value_pairs : AutoMapperSpecBase
        {
            private Destination _result;

            public class Source
            {
                public Dictionary<string, SourceValue> Values { get; set; }
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

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
                cfg.CreateMap<SourceValue, DestinationValue>();
            });

            protected override void Because_of()
            {
                var source = new Source
                    {
                        Values = new Dictionary<string, SourceValue>
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
                _result.Values.Count.ShouldBe(2);

                _result.Values["Key1"].Value.ShouldBe(5);
                _result.Values["Key2"].Value.ShouldBe(10);
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

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AllowNullDestinationValues = false;
                cfg.CreateMap<Foo, FooDto>();
                cfg.CreateMap<FooDto, Foo>();
            });

            protected override void Because_of()
            {
                var foo1 = new Foo
                {
                    Bar = new Dictionary<string, Foo>
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
                _result.Bar["lol"].Bar.ShouldBeOfType<Dictionary<string, FooDto>>();
            }
        }


        public class When_mapping_to_a_generic_dictionary_that_does_not_use_keyvaluepairs : AutoMapperSpecBase
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

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<SourceDto, DestDto>()
                    .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));
            });

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


                _dest = Mapper.Map<System.Collections.Generic.IDictionary<string, string>, System.Collections.Generic.IDictionary<string, string>>(source.Items);
            }

            [Fact]
            public void Should_map_using_the_nongeneric_dictionaryentry()
            {
                _dest.Values.Count.ShouldBe(3);
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
                System.Collections.IDictionary inner = new Dictionary<TKey, TValue>();

                public void Add(TKey key, TValue value)
                {
                    inner.Add(key, value);
                }

                public bool ContainsKey(TKey key)
                {
                    throw new NotImplementedException();
                }

                public ICollection<TKey> Keys
                {
                    get { return inner.Keys.Cast<TKey>().ToList(); }
                }

                public bool Remove(TKey key)
                {
                    throw new NotImplementedException();
                }

                public bool TryGetValue(TKey key, out TValue value)
                {
                    throw new NotImplementedException();
                }

                public ICollection<TValue> Values
                {
                    get { return inner.Values.Cast<TValue>().ToList(); }
                }

                public TValue this[TKey key]
                {
                    get
                    {
                        return (TValue)inner[key];
                    }
                    set
                    {
                        inner[key] = value;
                    }
                }

                public void Add(KeyValuePair<TKey, TValue> item)
                {
                    throw new NotImplementedException();
                }

                public void Clear()
                {
                    throw new NotImplementedException();
                }

                public bool Contains(KeyValuePair<TKey, TValue> item)
                {
                    throw new NotImplementedException();
                }

                public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
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

                public bool Remove(KeyValuePair<TKey, TValue> item)
                {
                    throw new NotImplementedException();
                }

                public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
                {
                    return inner.GetEnumerator() as IEnumerator<KeyValuePair<TKey, TValue>>;
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return inner.GetEnumerator();
                }

                public void Add(object key, object value)
                {
                    inner.Add(key, value);
                }

                public bool Contains(object key)
                {
                    throw new NotImplementedException();
                }

                IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator()
                {
                    return ((System.Collections.IDictionary)inner).GetEnumerator();
                }

                public bool IsFixedSize
                {
                    get { throw new NotImplementedException(); }
                }

                ICollection System.Collections.IDictionary.Keys
                {
                    get { return inner.Keys; }
                }

                public void Remove(object key)
                {
                    throw new NotImplementedException();
                }

                ICollection System.Collections.IDictionary.Values
                {
                    get { return inner.Values; }
                }

                public object this[object key]
                {
                    get
                    {
                        return inner[key];
                    }
                    set
                    {
                        inner[key] = value;
                    }
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

        public class When_mapping_from_a_list_of_object_to_generic_dictionary : AutoMapperSpecBase    
        {
            private FooObject _result;

            public class FooDto
            {
                public DestinationValuePair[] Values { get; set; }
            }

            public class FooObject
            {
                public IDictionary<string, string> Values { get; set; }
            }

            public class DestinationValuePair
            {
                public string Key { get; set; }
                public string Value { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<FooDto, FooObject>();
                cfg.CreateMap<DestinationValuePair, KeyValuePair<string, string>>()
                    .ConvertUsing(src => new KeyValuePair<string, string>(src.Key, src.Value));
            });

            protected override void Because_of()
            {
                var source = new FooDto
                {
                    Values = new List<DestinationValuePair>
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
                _result.Values.Count.ShouldBe(2);

                _result.Values["Key1"].ShouldBe("Value1");
                _result.Values["Key2"].ShouldBe("Value2");
            }
        }

        public class When_mapping_nongeneric_type_inherited_from_dictionary : AutoMapperSpecBase
        {
            public class BaseClassWithDictionary
            {
                public DataDictionary Data { get; set; }
            }

            public class DerivedClassWithDictionary : BaseClassWithDictionary { }

            public class DataDictionary : Dictionary<string, object>
            {
                public string GetString(string name, string @default)
                {
                    return null;
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<BaseClassWithDictionary, DerivedClassWithDictionary>());
            [Fact]
            public void Validate() => AssertConfigurationIsValid();
        }
    }

    public class When_mapping_from_a_list_of_object_to_IReadOnly_dictionary : AutoMapperSpecBase
    {
        private FooObject _result;

        public class FooDto
        {
            public DestinationValuePair[] Values { get; set; }
        }

        public class FooObject
        {
            public IReadOnlyDictionary<string, string> Values { get; set; }
        }

        public class DestinationValuePair
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<FooDto, FooObject>();
            cfg.CreateMap<DestinationValuePair, KeyValuePair<string, string>>()
                .ConvertUsing(src => new KeyValuePair<string, string>(src.Key, src.Value));
        });

        protected override void Because_of()
        {
            var source = new FooDto
            {
                Values = new List<DestinationValuePair>
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
            _result.Values.Count.ShouldBe(2);

            _result.Values["Key1"].ShouldBe("Value1");
            _result.Values["Key2"].ShouldBe("Value2");
        }
    }

    public class When_mapping_from_a_list_of_object_to_readonly_dictionary : AutoMapperSpecBase
    {
        private FooObject _result;

        public class FooDto
        {
            public DestinationValuePair[] Values { get; set; }
        }

        public class FooObject
        {
            public ReadOnlyDictionary<string, string> Values { get; set; }
        }

        public class DestinationValuePair
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<FooDto, FooObject>();
            cfg.CreateMap<DestinationValuePair, KeyValuePair<string, string>>()
                .ConvertUsing(src => new KeyValuePair<string, string>(src.Key, src.Value));
        });

        protected override void Because_of()
        {
            var source = new FooDto
            {
                Values = new List<DestinationValuePair>
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
            _result.Values.Count.ShouldBe(2);

            _result.Values["Key1"].ShouldBe("Value1");
            _result.Values["Key2"].ShouldBe("Value2");
        }
    }

}