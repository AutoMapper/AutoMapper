using AutoMapper.Configuration.Annotations;

namespace AutoMapper.UnitTests
{
    namespace AttributeBasedMaps
    {
        public class When_specifying_map_with_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_map_with_attribute));
            });

            [Fact]
            public void Should_map()
            {
                var source = new Source {Value = 5};
                var dest = Mapper.Map<Dest>(source);

                dest.Value.ShouldBe(5);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Dest>());
            }
        }

        public class When_specifying_map_and_reverse_map_with_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source), ReverseMap = true)]
            public class Dest
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_map_and_reverse_map_with_attribute));
            });

            [Fact]
            public void Should_reverse_map()
            {
                var dest = new Dest {Value = 5};
                var source = Mapper.Map<Source>(dest);

                source.Value.ShouldBe(5);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Dest>());
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Dest, Source>());
            }
        }

        public class When_specifying_reverse_map_with_sourcemember_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source), ReverseMap = true)]
            public class Destination
            {
                [SourceMember("Value")]
                public int Value2 { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_reverse_map_with_sourcemember_attribute));
            });

            [Fact]
            public void Should_reverse_map()
            {
                var destination = new Destination { Value2 = 5 };
                var source = Mapper.Map<Source>(destination);
                source.Value.ShouldBe(5);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Destination>());
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Destination, Source>());
            }
        }

        public class When_specifying_generic_reverse_map_with_sourcemember_attribute : NonValidatingSpecBase
        {
            public class Source<T>
            {
                public T Value { get; set; }

                public string StringValue { get; set; }
            }

            [AutoMap(typeof(Source<>), ReverseMap = true)]
            public class Destination<T>
            {
                [SourceMember("Value")]
                public int Value2 { get; set; }

                [SourceMember("StringValue")]
                public string StringValue2 { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_generic_reverse_map_with_sourcemember_attribute));
            });

            [Fact]
            public void Should_reverse_map()
            {
                Destination<int> destination = new Destination<int> { Value2 = 5, StringValue2 = "Joe" };
                Source<int> source = Mapper.Map<Source<int>>(destination);
                source.Value.ShouldBe(5);
                source.StringValue.ShouldBe("Joe");
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid(typeof(Source<>), typeof(Destination<>)));
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid(typeof(Destination<>), typeof(Source<>)));
            }
        }

        public class When_duplicating_map_configuration_with_code_and_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_map_and_reverse_map_with_attribute));
                cfg.CreateMap<Source, Dest>();
            });

            [Fact]
            public void Should_not_validate_successfully()
            {
                typeof(DuplicateTypeMapConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);

            }
        }

        public class When_specifying_source_member_name_via_attributes : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [SourceMember("Value")]
                public int OtherValue { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_source_member_name_via_attributes));
            });

            [Fact]
            public void Should_map_attribute_value()
            {
                var source = new Source
                {
                    Value = 5
                };

                var dest = Mapper.Map<Dest>(source);

                dest.OtherValue.ShouldBe(source.Value);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Dest>());
            }
        }

        public class When_specifying_source_member_name_via_attributes_using_nameof_operator : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [SourceMember(nameof(Source.Value))]
                public int OtherValue { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_source_member_name_via_attributes));
            });

            [Fact]
            public void Should_map_attribute_value()
            {
                var source = new Source
                {
                    Value = 5
                };

                var dest = Mapper.Map<Dest>(source);

                dest.OtherValue.ShouldBe(source.Value);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Dest>());
            }
        }

        public class When_specifying_null_substitute_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public string Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [NullSubstitute("Value")]
                [SourceMember(nameof(Source.Value))]
                public string OtherValue { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_source_member_name_via_attributes));
            });

            [Fact]
            public void Should_map_attribute_value()
            {
                var source = new Source
                {
                    Value = null
                };

                var dest = Mapper.Map<Dest>(source);

                dest.OtherValue.ShouldBe("Value");
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Dest>());
            }
        }

        public class When_specifying_value_resolver_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [ValueResolver(typeof(MyValueResolver))]
                public int OtherValue { get; set; }
            }

            public class MyValueResolver : IValueResolver<Source, Dest, int>
            {
                public int Resolve(Source source, Dest destination, int destMember, ResolutionContext context)
                {
                    return source.Value + 5;
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_source_member_name_via_attributes));
            });

            [Fact]
            public void Should_map_attribute_value()
            {
                var source = new Source
                {
                    Value = 6
                };

                var dest = Mapper.Map<Dest>(source);

                dest.OtherValue.ShouldBe(11);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Dest>());
            }
        }

        public class When_specifying_member_value_resolver_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [ValueResolver(typeof(MyMemberValueResolver))]
                [SourceMember(nameof(Source.Value))]
                public int Value { get; set; }
            }

            public class MyMemberValueResolver : IMemberValueResolver<Source, Dest, int, int>
            {
                public int Resolve(Source source, Dest destination, int sourceMember, int destMember, ResolutionContext context)
                {
                    return sourceMember + 5;
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_value_converter_via_attribute));
            });

            [Fact]
            public void Should_map_attribute_value()
            {
                var source = new Source
                {
                    Value = 6
                };

                var dest = Mapper.Map<Dest>(source);

                dest.Value.ShouldBe(11);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Dest>());
            }
        }

        public class When_specifying_value_converter_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [ValueConverter(typeof(MyValueConverter))]
                public int Value { get; set; }
            }

            public class MyValueConverter : IValueConverter<int, int>
            {
                public int Convert(int sourceMember, ResolutionContext context)
                {
                    return sourceMember + 5;
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_value_converter_via_attribute));
            });

            [Fact]
            public void Should_map_attribute_value()
            {
                var source = new Source
                {
                    Value = 6
                };

                var dest = Mapper.Map<Dest>(source);

                dest.Value.ShouldBe(11);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Dest>());
            }
        }

        public class When_specifying_value_converter_with_different_member_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [ValueConverter(typeof(MyValueConverter))]
                [SourceMember(nameof(Source.Value))]
                public int OtherValue { get; set; }
            }

            public class MyValueConverter : IValueConverter<int, int>
            {
                public int Convert(int sourceMember, ResolutionContext context)
                {
                    return sourceMember + 5;
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_value_converter_via_attribute));
            });

            [Fact]
            public void Should_map_attribute_value()
            {
                var source = new Source
                {
                    Value = 6
                };

                var dest = Mapper.Map<Dest>(source);

                dest.OtherValue.ShouldBe(11);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Dest>());
            }
        }

        public class When_ignoring_members_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [Ignore]
                public int Value { get; set; }

                [Ignore]
                public int OtherValue { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_value_converter_via_attribute));
            });

            [Fact]
            public void Should_map_attribute_value()
            {
                var source = new Source
                {
                    Value = 6
                };

                var dest = Mapper.Map<Dest>(source);

                dest.Value.ShouldBe(default);
                dest.OtherValue.ShouldBe(default);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Dest>());
            }
        }

        public class When_using_existing_value_via_attribute : NonValidatingSpecBase
        {
            private Source _source;
            private Destination _originalDest;
            private Destination _dest;

            public class Source
            {
                public int Value { get; set; }
                public ChildSource Child { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Destination
            {
                public int Value { get; set; }
                [Ignore]
                public string Name { get; set; }
                [UseExistingValue]
                public ChildDestination Child { get; set; }
            }

            public class ChildSource
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(ChildSource))]
            public class ChildDestination
            {
                public int Value { get; set; }
                [Ignore]
                public string Name { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_value_converter_via_attribute));
            });

            protected override void Because_of()
            {
                _source = new Source
                {
                    Value = 10,
                    Child = new ChildSource
                    {
                        Value = 20
                    }
                };
                _originalDest = new Destination
                {
                    Value = 1111,
                    Name = "foo",
                    Child = new ChildDestination
                    {
                        Name = "bar"
                    }
                };
                _dest = Mapper.Map(_source, _originalDest);
            }

            [Fact]
            public void Should_do_the_translation()
            {
                _dest.Value.ShouldBe(10);
                _dest.Child.Value.ShouldBe(20);
            }

            [Fact]
            public void Should_return_the_destination_object_that_was_passed_in()
            {
                _dest.Name.ShouldBe("foo");
                _dest.Child.Name.ShouldBe("bar");
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Destination>());
            }
        }

        public class When_specifying_a_mapping_order_with_attributes : NonValidatingSpecBase
        {
            private Destination _result;

            public class Source
            {
                private int _startValue;

                public Source(int startValue)
                {
                    _startValue = startValue;
                }

                public int GetValue1()
                {
                    _startValue += 10;
                    return _startValue;
                }

                public int GetValue2()
                {
                    _startValue += 5;
                    return _startValue;
                }
            }

            [AutoMap(typeof(Source))]
            public class Destination
            {
                [MappingOrder(2)]
                public int Value1 { get; set; }
                [MappingOrder(1)]
                public int Value2 { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_value_converter_via_attribute));
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<Source, Destination>(new Source(10));
            }

            [Fact]
            public void Should_perform_the_mapping_in_the_order_specified()
            {
                _result.Value2.ShouldBe(15);
                _result.Value1.ShouldBe(25);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<Source, Destination>());
            }
        }

        public class When_specifying_to_construct_using_service_locator_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source), ConstructUsingServiceLocator = true)]
            public class Dest
            {
                private int _value;
                private readonly int _addend;

                public int Value
                {
                    get { return _value + _addend; }
                    set { _value = value; }
                }

                public Dest(int addend)
                {
                    _addend = addend;
                }

                public Dest() : this(0)
                {
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_to_construct_using_service_locator_via_attribute));
                cfg.ConstructServicesUsing(t => new Dest(10));
            });

            [Fact]
            public void Should_map_with_the_custom_constructor()
            {
                var source = new Source { Value = 6 };
                var dest = Mapper.Map<Dest>(source);
                dest.Value.ShouldBe(16);
            }
        }

        public class When_specifying_max_depth_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Level { get; set; }
                public IList<Source> Children { get; set; }
                public Source Parent { get; set; }

                public Source(int level)
                {
                    Children = new List<Source>();
                    Level = level;
                }

                public void AddChild(Source child)
                {
                    Children.Add(child);
                    child.Parent = this;
                }
            }

            [AutoMap(typeof(Source), MaxDepth = 2)]
            public class Dest
            {
                public int Level { get; set; }
                public IList<Dest> Children { get; set; }
                public Dest Parent { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_max_depth_via_attribute));
            });

            [Fact]
            public void Third_level_children_are_null_with_max_depth_2()
            {
                var source = new Source(1);
                source.AddChild(new Source(2));
                source.AddChild(new Source(3));
                source.Children[0].AddChild(new Source(4));
                source.Children[1].AddChild(new Source(5));

                var dest = Mapper.Map<Dest>(source);
                dest.Level.ShouldBe(1);
                dest.Children[0].Level.ShouldBe(2);
                dest.Children[0].Children.ShouldAllBe(d => d == null);
                dest.Children[1].Level.ShouldBe(3);
                dest.Children[1].Children.ShouldAllBe(d => d == null);
            }
        }

        public class When_specifying_to_preserve_references_via_attribute : NonValidatingSpecBase
        {
            public class Parent
            {
                public int Id { get; set; }
            }

            public class Child
            {
                public int Id { get; set; }
                public Parent Parent { get; set; }
            }

            [AutoMap(typeof(Parent), PreserveReferences = true)]
            public class ParentDto
            {
                public int Id { get; set; }

                [SourceMember(nameof(Parent.Id))]
                public List<ChildDto> Children { get; set; }
            }

            [AutoMap(typeof(Child))]
            public class ChildDto
            {
                public int Id { get; set; }

                public ParentDto Parent { get; set; }
            }

            public class ParentIdToChildDtoListConverter : ITypeConverter<int, List<ChildDto>>
            {
                private readonly IList<Child> _childModels;

                public ParentIdToChildDtoListConverter(IList<Child> childModels)
                {
                    _childModels = childModels;
                }

                public List<ChildDto> Convert(int source, List<ChildDto> destination, ResolutionContext resolutionContext)
                {
                    var childModels = _childModels.Where(x => x.Parent.Id == source).ToList();
                    return (List<ChildDto>)resolutionContext.Mapper.Map(childModels, destination, typeof(List<Child>), typeof(List<ChildDto>));
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                _parent = new Parent
                {
                    Id = 2
                };

                var childModels = new List<Child>
                {
                    new Child
                    {
                        Id = 1,
                        Parent = _parent
                    }
                };
                cfg.AddMaps(typeof(When_specifying_to_preserve_references_via_attribute));
                cfg.CreateMap<int, List<ChildDto>>().ConvertUsing(new ParentIdToChildDtoListConverter(childModels));
            });

            private static Parent _parent;

            [Fact]
            public void Should_preserve_parent_relationship()
            {
                var dto = Mapper.Map<Parent, ParentDto>(_parent);
                dto.Children[0].Parent.ShouldBe(dto);
            }
        }

        public class When_specifying_to_include_all_derived_via_attribute : NonValidatingSpecBase
        {
            public class Foo { }
            public class FooBar : Foo { }

            [AutoMap(typeof(Foo), IncludeAllDerived = true)]
            public class FooDto { }

            [AutoMap(typeof(FooBar))]
            public class FooBarDto : FooDto { }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_to_include_all_derived_via_attribute));
            });

            [Fact]
            public void Should_convert_to_derived_correctly()
            {
                var source = new FooBar();
                var dest = Mapper.Map(source, source.GetType(), typeof(FooDto));
                dest.ShouldBeOfType<FooBarDto>();
            }
        }

        public class When_specifying_type_of_converter_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source), TypeConverter = typeof(CustomConverter))]
            public class Dest
            {
                public int OtherValue { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_type_of_converter_via_attribute));
            });

            public class CustomConverter : ITypeConverter<Source, Dest>
            {
                public Dest Convert(Source source, Dest destination, ResolutionContext context)
                {
                    return new Dest
                    {
                        OtherValue = source.Value + 10
                    };
                }
            }

            [Fact]
            public void Should_convert_using_custom_converter()
            {
                var source = new Source { Value = 15 };
                var dest = Mapper.Map<Dest>(source);
                dest.OtherValue.ShouldBe(25);
            }
        }

        public class When_specifying_map_with__multiple_attribute_sources : NonValidatingSpecBase
        {
            public class FirstSource
            {
                public int Value { get; set; }
            }

            public class SecondSource
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(FirstSource))]
            [AutoMap(typeof(SecondSource))]
            public class Dest
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_map_with_attribute));
            });

            [Fact]
            public void Should_map_from_both_sources()
            {
                Dest dest;
                var firstSource = new FirstSource { Value = 5 };
                dest = Mapper.Map<Dest>(firstSource);

                dest.Value.ShouldBe(5);

                var secondSource = new SecondSource { Value = 10 };
                dest = Mapper.Map<Dest>(secondSource);

                dest.Value.ShouldBe(10);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<FirstSource, Dest>());
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid<SecondSource, Dest>());
            }
        }

        public class When_specifying_as_proxy_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source), AsProxy = true)]
            public interface IDest
            {
                int Value { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_specifying_as_proxy_via_attribute));
            });

            [Fact]
            public void Should_convert_to_interface()
            {
                var source = new Source { Value = 15 };
                IDest dest = Mapper.Map<IDest>(source);
                dest.Value.ShouldBe(15);
            }
        }

        public class When_not_specifying_as_proxy_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source), AsProxy = false)]
            public interface IDest
            {
                int Value { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(typeof(When_not_specifying_as_proxy_via_attribute));
            });

            [Fact]
            public void Should_not_convert_to_interface()
            {
                var source = new Source { Value = 15 };
                Should.Throw<AutoMapperMappingException>(() => Mapper.Map<IDest>(source))
                    .Message.ShouldStartWith("Cannot create interface " + typeof(IDest).FullName);
            }
        }
    }
}