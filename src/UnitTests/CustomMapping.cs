using System;
using System.Collections;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    namespace CustomMapping
    {
        public class When_mapping_to_a_dto_member_with_custom_mapping : AutoMapperSpecBase
        {
            private ModelDto _result;

            public class ModelObject
            {
                public int Value { get; set; }
                public int Value2fff { get; set; }
                public int Value3 { get; set; }
                public int Value4 { get; set; }
                public int Value5 { get; set; }
            }

            public class ModelDto
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
                public int Value3 { get; set; }
                public int Value4 { get; set; }
                public int Value5 { get; set; }
            }

            public class CustomResolver : IValueResolver
            {
                public ResolutionResult Resolve(ResolutionResult source)
                {
                    return source.New(((ModelObject)source.Value).Value + 1);
                }
            }

            public class CustomResolver2 : IValueResolver
            {
                public ResolutionResult Resolve(ResolutionResult source)
                {
                    return source.New(((ModelObject)source.Value).Value2fff + 2);
                }
            }

            public class CustomResolver3 : IValueResolver
            {
                public ResolutionResult Resolve(ResolutionResult source)
                {
                    return source.New(((ModelObject)source.Value).Value4 + 4);
                }

                public Type GetResolvedValueType()
                {
                    return typeof(int);
                }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<ModelObject, ModelDto>()
                    .ForMember(dto => dto.Value, opt => opt.ResolveUsing<CustomResolver>())
                    .ForMember(dto => dto.Value2, opt => opt.ResolveUsing(new CustomResolver2()))
                    .ForMember(dto => dto.Value4, opt => opt.ResolveUsing(typeof(CustomResolver3)))
                    .ForMember(dto => dto.Value5, opt => opt.ResolveUsing(src => src.Value5 + 5));

                var model = new ModelObject { Value = 42, Value2fff = 42, Value3 = 42, Value4 = 42, Value5 = 42 };
                _result = Mapper.Map<ModelObject, ModelDto>(model);
            }

            [Fact]
            public void Should_ignore_the_mapping_for_normal_members()
            {
                _result.Value3.ShouldEqual(42);
            }

            [Fact]
            public void Should_use_the_custom_generic_mapping_for_custom_dto_members()
            {
                _result.Value.ShouldEqual(43);
            }

            [Fact]
            public void Should_use_the_instance_based_mapping_for_custom_dto_members()
            {
                _result.Value2.ShouldEqual(44);
            }

            [Fact]
            public void Should_use_the_type_object_based_mapping_for_custom_dto_members()
            {
                _result.Value4.ShouldEqual(46);
            }

            [Fact]
            public void Should_use_the_func_based_mapping_for_custom_dto_members()
            {
                _result.Value5.ShouldEqual(47);
            }
        }

        public class When_using_a_custom_resolver_for_a_child_model_property_instead_of_the_model : AutoMapperSpecBase
        {
            private ModelDto _result;

            public class ModelObject
            {
                public ModelSubObject Sub { get; set; }
            }

            public class ModelSubObject
            {
                public int SomeValue { get; set; }
            }

            public class ModelDto
            {
                public int SomeValue { get; set; }
            }

            public class CustomResolver : IValueResolver
            {
                public ResolutionResult Resolve(ResolutionResult source)
                {
                    return source.New(((ModelSubObject)source.Value).SomeValue + 1);
                }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<ModelObject, ModelDto>()
                    .ForMember(dto => dto.SomeValue, opt => opt.ResolveUsing<CustomResolver>().FromMember(m => m.Sub));

                var model = new ModelObject
                    {
                        Sub = new ModelSubObject
                            {
                                SomeValue = 46
                            }
                    };

                _result = Mapper.Map<ModelObject, ModelDto>(model);
            }

            [Fact]
            public void Should_use_the_specified_model_member_to_resolve_from()
            {
                _result.SomeValue.ShouldEqual(47);
            }
        }

        public class When_reseting_a_mapping_to_use_a_resolver_to_a_different_member : AutoMapperSpecBase
        {
            private Dest _result;

            public class Source
            {
                public int SomeValue { get; set; }
                public int SomeOtherValue { get; set; }
            }

            public class Dest
            {
                public int SomeValue { get; set; }
            }

            public class CustomResolver : IValueResolver
            {
                public ResolutionResult Resolve(ResolutionResult source)
                {
                    return source.New(((int)source.Value) + 5);
                }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Dest>()
                    .ForMember(dto => dto.SomeValue, opt => opt.ResolveUsing<CustomResolver>().FromMember(m => m.SomeOtherValue));

                var model = new Source
                    {
                        SomeValue = 36,
                        SomeOtherValue = 53
                    };

                _result = Mapper.Map<Source, Dest>(model);
            }

            [Fact]
            public void Should_override_the_existing_match_to_the_new_custom_resolved_member()
            {
                _result.SomeValue.ShouldEqual(58);
            }
        }

        public class When_reseting_a_mapping_from_a_property_to_a_method : AutoMapperSpecBase
        {
            private Dest _result;

            public class Source
            {
                public int Type { get; set; }
            }

            public class Dest
            {
                public int Type { get; set; }
            }

            public class CustomResolver : IValueResolver
            {
                public ResolutionResult Resolve(ResolutionResult source)
                {
                    return source.New(((int)source.Value) + 5);
                }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Dest>()
                    .ForMember(dto => dto.Type, opt => opt.MapFrom(m => m.Type));

                var model = new Source
                    {
                        Type = 5
                    };

                _result = Mapper.Map<Source, Dest>(model);
            }

            [Fact]
            public void Should_override_the_existing_match_to_the_new_custom_resolved_member()
            {
                _result.Type.ShouldEqual(5);
            }
        }

        public class When_specifying_a_custom_constructor_and_member_resolver : AutoMapperSpecBase
        {
            private Source _source;
            private Destination _dest;

            public class Source
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            public class CustomResolver : ValueResolver<int, int>
            {
                private readonly int _toAdd;

                public CustomResolver(int toAdd)
                {
                    _toAdd = toAdd;
                }

                public CustomResolver()
                {
                    _toAdd = 10;
                }

                protected override int ResolveCore(int source)
                {
                    return source + _toAdd;
                }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ForMember(s => s.Value,
                               opt => opt.ResolveUsing<CustomResolver>()
                                        .FromMember(s => s.Value)
                                        .ConstructedBy(() => new CustomResolver(15)));

                _source = new Source
                    {
                        Value = 10
                    };
            }

            protected override void Because_of()
            {
                _dest = Mapper.Map<Source, Destination>(_source);
            }

            [Fact]
            public void Should_use_the_custom_constructor()
            {
                _dest.Value.ShouldEqual(25);
            }
        }

        public class When_specifying_a_member_resolver_and_custom_constructor : AutoMapperSpecBase
        {
            private Source _source;
            private Destination _dest;

            public class Source
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            public class CustomResolver : ValueResolver<int, int>
            {
                private readonly int _toAdd;

                public CustomResolver(int toAdd)
                {
                    _toAdd = toAdd;
                }

                public CustomResolver()
                {
                    _toAdd = 10;
                }

                protected override int ResolveCore(int source)
                {
                    return source + _toAdd;
                }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ForMember(s => s.Value,
                               opt => opt.ResolveUsing<CustomResolver>()
                                        .ConstructedBy(() => new CustomResolver(15))
                                        .FromMember(s => s.Value)
                    );

                _source = new Source
                    {
                        Value = 10
                    };
            }

            protected override void Because_of()
            {
                _dest = Mapper.Map<Source, Destination>(_source);
            }

            [Fact]
            public void Should_use_the_custom_constructor()
            {
                _dest.Value.ShouldEqual(25);
            }
        }

        public class When_specifying_a_custom_translator : AutoMapperSpecBase
        {
            private Source _source;
            private Destination _dest;

            public class Source
            {
                public int Value { get; set; }
                public int AnotherValue { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                base.Establish_context();

                _source = new Source
                {
                    Value = 10,
                    AnotherValue = 1000
                };
            }

            [Fact]
            public void Should_use_the_custom_translator()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ConvertUsing(s => new Destination { Value = s.Value + 10 });

                _dest = Mapper.Map<Source, Destination>(_source);
                _dest.Value.ShouldEqual(20);
            }

            [Fact]
            public void Should_ignore_other_mapping_rules()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AnotherValue))
                    .ConvertUsing(s => new Destination { Value = s.Value + 10 });

                _dest = Mapper.Map<Source, Destination>(_source);
                _dest.Value.ShouldEqual(20);
            }
        }

        public class When_specifying_a_custom_translator_using_projection : AutoMapperSpecBase
        {
            private Source _source;
            private Destination _dest;

            public class Source
            {
                public int Value { get; set; }
                public int AnotherValue { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                base.Establish_context();

                _source = new Source
                {
                    Value = 10,
                    AnotherValue = 1000
                };
            }

            [Fact]
            public void Should_use_the_custom_translator()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ProjectUsing(s => new Destination { Value = s.Value + 10 });

                _dest = Mapper.Map<Source, Destination>(_source);
                _dest.Value.ShouldEqual(20);
            }

            [Fact]
            public void Should_ignore_other_mapping_rules()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AnotherValue))
                    .ProjectUsing(s => new Destination { Value = s.Value + 10 });

                _dest = Mapper.Map<Source, Destination>(_source);
                _dest.Value.ShouldEqual(20);
            }
        }

        public class When_specifying_a_custom_translator_and_passing_in_the_destination_object : AutoMapperSpecBase
        {
            private Source _source;
            private Destination _dest;

            public class Source
            {
                public int Value { get; set; }
                public int AnotherValue { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                base.Establish_context();

                _source = new Source
                {
                    Value = 10,
                    AnotherValue = 1000
                };

                _dest = new Destination
                                  {
                                      Value = 2
                                  };
            }

            [Fact]
            public void Should_resolve_to_the_destination_object_from_the_custom_translator()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ConvertUsing(s => new Destination { Value = s.Value + 10 });

                _dest = Mapper.Map(_source, _dest);
                _dest.Value.ShouldEqual(20);
            }

            [Fact]
            public void Should_ignore_other_mapping_rules()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AnotherValue))
                    .ConvertUsing(s => new Destination { Value = s.Value + 10 });

                _dest = Mapper.Map(_source, _dest);
                _dest.Value.ShouldEqual(20);
            }
        }

        public class When_specifying_a_custom_translator_using_generics : AutoMapperSpecBase
        {
            private Source _source;
            private Destination _dest;

            public class Source
            {
                public int Value { get; set; }
                public int AnotherValue { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                base.Establish_context();

                _source = new Source
                {
                    Value = 10,
                    AnotherValue = 1000
                };
            }

            public class Converter : TypeConverter<Source, Destination>
            {
                protected override Destination ConvertCore(Source source)
                {
                    return new Destination { Value = source.Value + 10 };
                }
            }

            [Fact]
            public void Should_use_the_custom_translator()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ConvertUsing<Converter>();

                _dest = Mapper.Map<Source, Destination>(_source);
                _dest.Value.ShouldEqual(20);
            }

            [Fact]
            public void Should_ignore_other_mapping_rules()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AnotherValue))
                    .ConvertUsing(s => new Destination { Value = s.Value + 10 });

                _dest = Mapper.Map<Source, Destination>(_source);
                _dest.Value.ShouldEqual(20);
            }
        }

        public class When_specifying_a_custom_constructor_function_for_custom_converters : AutoMapperSpecBase
        {
            private Destination _result;

            public class Source
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            public class CustomConverter : TypeConverter<Source, Destination>
            {
                private readonly int _value;

                public CustomConverter()
                    : this(5)
                {
                }

                public CustomConverter(int value)
                {
                    _value = value;
                }

                protected override Destination ConvertCore(Source source)
                {
                    return new Destination { Value = source.Value + _value };
                }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(init => init.ConstructServicesUsing(t => new CustomConverter(10)));
                Mapper.CreateMap<Source, Destination>()
                    .ConvertUsing<CustomConverter>();
            }

            protected override void Because_of()
            {
                _result = Mapper.Map<Source, Destination>(new Source { Value = 5 });
            }

            [Fact]
            public void Should_use_the_custom_constructor_function()
            {
                _result.Value.ShouldEqual(15);
            }
        }


        public class When_specifying_a_custom_translator_with_mismatched_properties : AutoMapperSpecBase
        {
            public class Source
            {
                public int Value1 { get; set; }
                public int AnotherValue { get; set; }
            }

            public class Destination
            {
                public int Value2 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg => cfg.CreateMap<Source, Destination>()
                    .ConvertUsing(s => new Destination { Value2 = s.Value1 + 10 }));
            }

            [Fact]
            public void Should_pass_all_configuration_checks()
            {
                Exception thrown = null;
                try
                {
                    Mapper.AssertConfigurationIsValid();

                }
                catch (Exception ex)
                {
                    thrown = ex;
                }

                thrown.ShouldBeNull();
            }
        }

        public class When_configuring_a_global_constructor_function_for_resolvers : AutoMapperSpecBase
        {
            private Destination _result;

            public class Source
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            public class CustomValueResolver : ValueResolver<int, int>
            {
                private readonly int _toAdd;
                public CustomValueResolver() { _toAdd = 11; }

                public CustomValueResolver(int toAdd)
                {
                    _toAdd = toAdd;
                }

                protected override int ResolveCore(int source)
                {
                    return source + _toAdd;
                }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg => cfg.ConstructServicesUsing(type => new CustomValueResolver(5)));

                Mapper.CreateMap<Source, Destination>()
                    .ForMember(d => d.Value, opt => opt.ResolveUsing<CustomValueResolver>().FromMember(src => src.Value));
            }

            protected override void Because_of()
            {
                _result = Mapper.Map<Source, Destination>(new Source { Value = 5 });
            }

            [Fact]
            public void Should_use_the_specified_constructor()
            {
                _result.Value.ShouldEqual(10);
            }
        }


        public class When_custom_resolver_requests_property_to_be_ignored : AutoMapperSpecBase
        {
            private Destination _result = new Destination() { Value = 55 };

            public class Source
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            public class CustomValueResolver : IValueResolver
            {
                public ResolutionResult Resolve(ResolutionResult source)
                {
                    return source.Ignore();
                }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ForMember(d => d.Value, opt => opt.ResolveUsing<CustomValueResolver>().FromMember(src => src.Value));
            }

            protected override void Because_of()
            {
                _result = Mapper.Map(new Source { Value = 5 }, _result);
            }

            [Fact]
            public void Should_not_overwrite_destination_value()
            {
                _result.Value.ShouldEqual(55);
            }
        }


        public class When_specifying_member_and_member_resolver_using_string_property_names : AutoMapperSpecBase
        {
            private Destination _result;

            public class Source
            {
                public int SourceValue { get; set; }
            }

            public class Destination
            {
                public int DestinationValue { get; set; }
            }

            public class CustomValueResolver : ValueResolver<int, int>
            {
                public CustomValueResolver()
                {
                }

                protected override int ResolveCore(int source)
                {
                    return source + 5;
                }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg => cfg.ConstructServicesUsing(type => new CustomValueResolver()));

                Mapper.CreateMap<Source, Destination>()
                    .ForMember("DestinationValue", opt => opt.ResolveUsing<CustomValueResolver>().FromMember("SourceValue"));
            }

            protected override void Because_of()
            {
                _result = Mapper.Map<Source, Destination>(new Source { SourceValue = 5 });
            }

            [Fact]
            public void Should_translate_the_property()
            {
                _result.DestinationValue.ShouldEqual(10);
            }
        }

        public class When_specifying_a_custom_member_mapping_to_a_nested_object : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public SubDest Dest { get; set; }
            }

            public class SubDest
            {
                public int Value { get; set; }
            }

            [Fact]
            public void Should_fail_with_an_exception_during_configuration()
            {
                typeof(ArgumentException).ShouldBeThrownBy(() =>
                {
                    Mapper.CreateMap<Source, Destination>()
                        .ForMember(dest => dest.Dest.Value, opt => opt.MapFrom(src => src.Value));
                });
            }
        }

        public class When_specifying_a_custom_member_mapping_with_a_cast : NonValidatingSpecBase
        {
            private Source _source;
            private Destination _dest;

            public class Source
            {
                public string MyName { get; set; }
            }

            public class Destination : ISomeInterface
            {
                public string Name { get; set; }
            }

            public interface ISomeInterface
            {
                string Name { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ForMember(dest => ((ISomeInterface)dest).Name, opt => opt.MapFrom(src => src.MyName));

                _source = new Source { MyName = "jon" };
            }

            protected override void Because_of()
            {
                _dest = Mapper.Map<Source, Destination>(_source);
            }

            [Fact]
            public void Should_perform_the_translation()
            {
                _dest.Name.ShouldEqual("jon");
            }
        }

#if !SILVERLIGHT
        public class When_destination_property_does_not_have_a_setter : AutoMapperSpecBase
        {
            private Source _source;
            private Destination _dest;

            public class Source
            {
                public string Name { get; set; }
                public string Value { get; set; }
                public string Foo { get; set; }
            }

            public class Destination
            {
                private DateTime _today;

                public string Name { get; private set; }
                public string Foo { get; protected set; }
                public DateTime Today { get { return _today; } }
                public string Value { get; set; }

                public Destination()
                {
                    _today = DateTime.Today;
                    Name = "name";
                }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Destination>();

                _source = new Source { Name = "jon", Value = "value", Foo = "bar" };
                _dest = new Destination();
            }

            protected override void Because_of()
            {
                _dest = Mapper.Map<Source, Destination>(_source);
            }

            [Fact]
            public void Should_copy_to_properties_that_have_setters()
            {
                _dest.Value.ShouldEqual("value");
            }

            [Fact]
            public void Should_not_attempt_to_translate_to_properties_that_do_not_have_a_setter()
            {
                _dest.Today.ShouldEqual(DateTime.Today);
            }

            [Fact]
            public void Should_translate_to_properties_that_have_a_private_setters()
            {
                _dest.Name.ShouldEqual("jon");
            }

            [Fact]
            public void Should_translate_to_properties_that_have_a_protected_setters()
            {
                _dest.Foo.ShouldEqual("bar");
            }
        }
#endif

        public class When_destination_property_does_not_have_a_getter : AutoMapperSpecBase
        {
            private Source _source;
            private Destination _dest;
            private SourceWithList _sourceWithList;
            private DestinationWithList _destWithList;

            public class Source
            {
                public string Value { get; set; }

            }

            public class Destination
            {
                private string _value;

                public string Value
                {
                    set { _value = value; }
                }

                public string GetValue()
                {
                    return _value;
                }
            }

            public class SourceWithList
            {
                public IList SomeList { get; set; }
            }

            public class DestinationWithList
            {
                private IList _someList;

                public IList SomeList
                {
                    set { _someList = value; }
                }

                public IList GetSomeList()
                {
                    return _someList;
                }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Destination>();
                Mapper.CreateMap<SourceWithList, DestinationWithList>();

                _source = new Source { Value = "jon" };
                _dest = new Destination();


                _sourceWithList = new SourceWithList { SomeList = new[] { 1, 2 } };
                _destWithList = new DestinationWithList();
            }

            protected override void Because_of()
            {
                _dest = Mapper.Map<Source, Destination>(_source);
                _destWithList = Mapper.Map<SourceWithList, DestinationWithList>(_sourceWithList);
            }

            [Fact]
            public void Should_translate_to_properties_that_doesnt_have_a_getter()
            {
                _dest.GetValue().ShouldEqual("jon");
            }

            [Fact]
            public void Should_translate_to_enumerable_properties_that_doesnt_have_a_getter()
            {
                new[] { 1, 2 }.ShouldEqual(_destWithList.GetSomeList());
            }
        }


        public class When_destination_type_requires_a_constructor : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public Destination(int otherValue)
                {
                    OtherValue = otherValue;
                }

                public int Value { get; set; }
                public int OtherValue { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Destination>()
                    .ConstructUsing(src => new Destination(src.Value + 4))
                    .ForMember(dest => dest.OtherValue, opt => opt.Ignore());
            }

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source { Value = 5 });
            }

            [Fact]
            public void Should_use_supplied_constructor_to_map()
            {
                _destination.OtherValue.ShouldEqual(9);
            }

            [Fact]
            public void Should_map_other_members()
            {
                _destination.Value.ShouldEqual(5);
            }
        }

        public class When_mapping_from_a_constant_value : AutoMapperSpecBase
        {
            private Dest _dest;

            public class Source
            {

            }

            public class Dest
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Dest>()
                    .ForMember(dest => dest.Value, opt => opt.UseValue(5));
            }

            protected override void Because_of()
            {
                _dest = Mapper.Map<Source, Dest>(new Source());
            }

            [Fact]
            public void Should_map_from_that_constant_value()
            {
                _dest.Value.ShouldEqual(5);
            }
        }

        public class When_building_custom_configuration_mapping_to_itself : NonValidatingSpecBase
        {
            private Exception _e;

            public class Source
            {

            }

            public class Dest
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
            }

            protected override void Because_of()
            {
                try
                {
                    Mapper.CreateMap<Source, Dest>()
                        .ForMember(dest => dest, opt => opt.UseValue(5));
                }
                catch (Exception e)
                {
                    _e = e;
                }
            }

            [Fact]
            public void Should_map_from_that_constant_value()
            {
                _e.ShouldNotBeNull();
            }
        }


    }

    public class When_mapping_from_one_type_to_another : AutoMapperSpecBase
    {
        private Dest _dest;

        public class Source
        {
            public string Value { get; set; }
        }

        public class Dest
        {
            // AutoMapper tries to map Source.Value to this constructor's parameter,
            // but does not take its member configuration into account
            public Dest(int value)
            {
                Value = value;
            }
            public Dest()
            {
            }

            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.DisableConstructorMapping();
                cfg.CreateMap<Source, Dest>()
                    .ForMember(dest => dest.Value, opt => opt.MapFrom(s => ParseValue(s.Value)));
            });
        }

        protected override void Because_of()
        {
            var source = new Source { Value = "a1" };
            _dest = Mapper.Map<Source, Dest>(source);
        }

        [Fact]
        public void Should_use_member_configuration()
        {
            _dest.Value.ShouldEqual(1);
        }

        private static int ParseValue(string value)
        {
            return int.Parse(value.Substring(1));
        }
    }
}