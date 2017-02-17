using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class When_implementing_multiple_IValueResolver_interfaces : AutoMapperSpecBase
    {
        public class Source1 { }

        public class Source2 { }

        public class Destination
        {
            public string Value { get; set; }
        }

        public class MyTestResolver : IValueResolver<Source1, Destination, string>, IValueResolver<Source2, Destination, string>
        {
            public string Resolve(Source1 source, Destination destination, string destMember, ResolutionContext context)
            {
                return "source1";
            }

            public string Resolve(Source2 source, Destination destination, string destMember, ResolutionContext context)
            {
                return "source2";
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source1, Destination>().ForMember(dest => dest.Value, opt => opt.ResolveUsing<MyTestResolver>());
            cfg.CreateMap<Source2, Destination>().ForMember(dest => dest.Value, opt => opt.ResolveUsing<MyTestResolver>());
        });

        [Fact]
        public void Should_map_ok()
        {
            Mapper.Map<Destination>(new Source1()).Value.ShouldEqual("source1");
            Mapper.Map<Destination>(new Source2()).Value.ShouldEqual("source2");
        }
    }

    public class When_using_IMemberResolver_derived_interface : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public string SValue { get; set; }
        }

        class Destination
        {
            public string Value { get; set; }
        }

        interface IResolver : IMemberValueResolver<Source, Destination, string, string>
        {
        }

        class Resolver : IResolver
        {
            public string Resolve(Source source, Destination destination, string sourceMember, string destMember, ResolutionContext context)
            {
                return "Resolved";
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.ResolveUsing(new Resolver(), s=>s.SValue));
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Value.ShouldEqual("Resolved");
        }
    }

    public class OpenGenericMapForMember : AutoMapperSpecBase
    {
        ModelPager<int> _destination;
        int[] _items = Enumerable.Range(1, 10).ToArray();

        public interface IPager<out TItem> : IEnumerable<TItem>
        {
            int CurrentPage { get; set; }

            int PageCount { get; set; }

            int PageSize { get; set; }

            int TotalItems { get; set; }
        }
        public class ModelPager<TItem>
        {
            public int CurrentPage { get; set; }

            public IEnumerable<TItem> Items { get; set; }

            public int PageCount { get; set; }

            public int PageSize { get; set; }

            public int TotalItems { get; set; }
        }
        public class Pager<TItem> : IPager<TItem>
        {
            private readonly IEnumerable<TItem> _items;

            public Pager(IEnumerable<TItem> items) :this(items, 0, 0, 0)
            {
            }
            public Pager(IEnumerable<TItem> items,
                         int currentPage,
                         int pageSize,
                         int totalItems)
            {
                _items = items ?? Enumerable.Empty<TItem>();
                CurrentPage = currentPage;
                PageSize = pageSize;
                TotalItems = totalItems;
            }

            public int CurrentPage { get; set; }

            public int PageCount { get; set; }

            public int PageSize { get; set; }

            public int TotalItems { get; set; }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            public IEnumerator<TItem> GetEnumerator() { return _items.GetEnumerator(); }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(IPager<>), typeof(ModelPager<>)).ForMember("Items", e => e.MapFrom(o => (IEnumerable)o));
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<ModelPager<int>>(new Pager<int>(_items));
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Items.SequenceEqual(_items).ShouldBeTrue();
        } 
    }

    public class IntToNullableIntConverter : AutoMapperSpecBase
    {
        Destination _destination;

        public class IntToNullableConverter : ITypeConverter<int, int?>
        {
            public int? Convert(int source, int? destination, ResolutionContext context)
            {
                if(source == default(int))
                {
                    return null;
                }
                return source;
            }
        }

        public class Source
        {
            public int Id { get; set; }
        }

        public class Destination
        {
            public int? Id { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<int, int?>().ConvertUsing<IntToNullableConverter>();
            cfg.CreateMap<Source, Destination>();
        });
        
        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_use_the_converter()
        {
            _destination.Id.ShouldBeNull();
        }
    }

    public class When_throwing_NRE_from_MapFrom_value_types : AutoMapperSpecBase
    {
        ViewModel _viewModel;

        public class Model
        {
            public List<SubModel> SubModels { get; set; }
        }

        public class SubModel
        {
            public List<SubSubModel> SubSubModels { get; set; }
        }

        public class SubSubModel
        {
            public int Id { get; set; }
        }

        public class ViewModel
        {
            public int SubModelId { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Model, ViewModel>()
                .ForMember(x => x.SubModelId,
                    opts => opts.MapFrom(src => src.SubModels.FirstOrDefault().SubSubModels.FirstOrDefault().Id));
        });

        protected override void Because_of()
        {
            var model = new Model
            {
                SubModels = new List<SubModel>()
            };
            _viewModel = Mapper.Map<ViewModel>(model);
        }

        [Fact]
        public void Should_map_ok()
        {
            _viewModel.SubModelId.ShouldEqual(0);
        }
    }

    public class When_throwing_NRE_from_MapFrom : AutoMapperSpecBase
    {
        class Source
        {
        }

        class Destination
        {
            public string Value { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            string x = null;
            cfg.CreateMap<Source, Destination>().ForMember(d=>d.Value, o=>o.MapFrom(s=>x.ToString()));
        });

        [Fact]
        public void We_should_catch_it()
        {
            Mapper.Map<Destination>(new Source()).Value.ShouldBeNull();
        }
    }

    public class When_using_value_with_mismatched_properties : AutoMapperSpecBase
    {
        Destination _destination;
        static Guid _guid = Guid.NewGuid();

        class Source
        {
            public int Value { get; set; }
        }

        class Destination
        {
            public Guid Value { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(c =>
                {
                    c.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.UseValue(_guid));
                });
            }
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Value.ShouldEqual(_guid);
        }
    }

    public class When_custom_resolving_mismatched_properties : AutoMapperSpecBase
    {
        Destination _destination;
        static Guid _guid = Guid.NewGuid();

        class Source
        {
            public int Value { get; set; }
        }

        class Destination
        {
            public Guid Value { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(c =>
                {
                    c.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.ResolveUsing<Resolver>());//.ForMember(d => d.Value, o => o.ResolveUsing(s=>_guid));
                });
            }
        }

        class Resolver : IValueResolver<Source, Destination, Guid>
        {
            public Guid Resolve(Source model, Destination d, Guid dest, ResolutionContext context)
            {
                return _guid;
            }
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Value.ShouldEqual(_guid);
        }
    }

    public class When_resolve_throws : NonValidatingSpecBase
    {
        Exception _ex = new Exception();

        class Source
        {
        }

        class Destination
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(c =>
                {
                    c.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.ResolveUsing(s => { Throw(); return 0; }));
                });
            }
        }

        private void Throw()
        {
            throw _ex;
        }

        [Fact]
        public void Should_propagate_exception()
        {
            new Action(()=>Mapper.Map<Destination>(new Source())).ShouldThrow<AutoMapperMappingException>(e=>e.InnerException.ShouldEqual(_ex));
        }
    }

    public class When_mapping_different_types_with_UseValue : AutoMapperSpecBase
    {
        Destination _destination;

        class InnerSource
        {
            public int IntValue { get; set; }
        }

        class InnerDestination
        {
            public int IntValue { get; set; }
        }

        class Source
        {
        }

        class Destination
        {
            public InnerDestination Value { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(c =>
                {
                    c.CreateMap<InnerSource, InnerDestination>();
                    c.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.UseValue(new InnerSource { IntValue = 15 }));
                });
            }
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_work()
        {
            _destination.Value.IntValue.ShouldEqual(15);
        }
    }

    public class When_mapping_different_types_with_ResolveUsing : AutoMapperSpecBase
    {
        Destination _destination;

        class InnerSource
        {
            public int IntValue { get; set; }
        }

        class InnerDestination
        {
            public int IntValue { get; set; }
        }

        class Source
        {
            public InnerSource ObjectValue { get; set; }
        }

        class Destination
        {
            public InnerDestination Value { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(c =>
                {
                    c.CreateMap<InnerSource, InnerDestination>();
                    c.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.ResolveUsing(s => s.ObjectValue));
                });
            }
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { ObjectValue = new InnerSource { IntValue = 15 } });
        }

        [Fact]
        public void Should_work()
        {
            _destination.Value.IntValue.ShouldEqual(15);
        }
    }

    public class When_mapping_from_object_to_string_with_use_value : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
        }

        class Destination
        {
            public string Value { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(c => c.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.UseValue(new object())));
            }
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { });
        }

        [Fact]
        public void Should_use_to_string()
        {
            _destination.Value.ShouldEqual("System.Object");
        }
    }

    public class When_mapping_from_object_to_string : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public object ObjectValue { get; set; }
        }

        class Destination
        {
            public string Value { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(c => c.CreateMap<Source, Destination>().ForMember(d=>d.Value, o=>o.ResolveUsing(s=>s.ObjectValue)));
            }
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { ObjectValue = new object() });
        }

        [Fact]
        public void Should_use_to_string()
        {
            _destination.Value.ShouldEqual("System.Object");
        }
    }

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

        public class CustomResolver : IValueResolver<ModelObject, ModelDto, int>
        {
            public int Resolve(ModelObject source, ModelDto d, int dest, ResolutionContext context)
            {
                return source.Value + 1;
            }
        }

        public class CustomResolver2 : IValueResolver<ModelObject, ModelDto, int>
        {
            public int Resolve(ModelObject source, ModelDto d, int dest, ResolutionContext context)
            {
                return source.Value2fff + 2;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ModelObject, ModelDto>()
                .ForMember(dto => dto.Value, opt => opt.ResolveUsing<CustomResolver>())
                .ForMember(dto => dto.Value2, opt => opt.ResolveUsing(new CustomResolver2()))
                .ForMember(dto => dto.Value5, opt => opt.ResolveUsing(src => src.Value5 + 5));

        });

        protected override void Because_of()
        {
            var model = new ModelObject {Value = 42, Value2fff = 42, Value3 = 42, Value4 = 42, Value5 = 42};
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

        public class CustomResolver : IMemberValueResolver<object, object, ModelSubObject, int>
        {
            public int Resolve(object s, object d, ModelSubObject source, int ignored, ResolutionContext context)
            {
                return source.SomeValue + 1;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ModelObject, ModelDto>()
                .ForMember(dto => dto.SomeValue, opt => opt.ResolveUsing<CustomResolver, ModelSubObject>(m => m.Sub));
        });

        [Fact]
        public void Should_use_the_specified_model_member_to_resolve_from()
        {
            var model = new ModelObject
            {
                Sub = new ModelSubObject
                {
                    SomeValue = 46
                }
            };

            _result = Mapper.Map<ModelObject, ModelDto>(model);
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

        public class CustomResolver : IMemberValueResolver<object, object, int, int>
        {
            public int Resolve(object s, object d, int source, int dest, ResolutionContext context)
            {
                return source + 5;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(dto => dto.SomeValue,
                    opt => opt.ResolveUsing<CustomResolver, int>(m => m.SomeOtherValue));

        });

        protected override void Because_of()
        {
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(dto => dto.Type, opt => opt.MapFrom(m => m.Type));

        });

        protected override void Because_of()
        {
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

        public class CustomResolver : IMemberValueResolver<object, object, int, int>
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

            public int Resolve(object s, object d, int source, int dest, ResolutionContext context)
            {
                return source + _toAdd;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ForMember(s => s.Value,
                    opt => opt.ResolveUsing(new CustomResolver(15), src => src.Value));

        });


        protected override void Because_of()
        {
            _source = new Source
            {
                Value = 10
            };
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

        public class CustomResolver : IMemberValueResolver<object, object, int, int>
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

            public int Resolve(object s, object d, int source, int dest, ResolutionContext context)
            {
                return source + _toAdd;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ForMember(s => s.Value,
                    opt => opt.ResolveUsing(new CustomResolver(15), s => s.Value)
                );

        });

        protected override void Because_of()
        {
                _source = new Source
                {
                    Value = 10
                };
            _dest = Mapper.Map<Source, Destination>(_source);
        }

        [Fact]
        public void Should_use_the_custom_constructor()
        {
            _dest.Value.ShouldEqual(25);
        }
    }

    public class When_specifying_a_custom_translator
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

        public When_specifying_a_custom_translator()
        {
            _source = new Source
            {
                Value = 10,
                AnotherValue = 1000
            };
        }

        [Fact]
        public void Should_use_the_custom_translator()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                .ConvertUsing(s => new Destination { Value = s.Value + 10 }));

            _dest = config.CreateMapper().Map<Source, Destination>(_source);
            _dest.Value.ShouldEqual(20);
        }

        [Fact]
        public void Should_ignore_other_mapping_rules()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AnotherValue))
                .ConvertUsing(s => new Destination { Value = s.Value + 10 }));

            _dest = config.CreateMapper().Map<Source, Destination>(_source);
            _dest.Value.ShouldEqual(20);
        }
    }

    public class When_specifying_a_custom_translator_using_projection
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

        public When_specifying_a_custom_translator_using_projection()
        {
            _source = new Source
            {
                Value = 10,
                AnotherValue = 1000
            };
        }

        [Fact]
        public void Should_use_the_custom_translator()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                .ProjectUsing(s => new Destination { Value = s.Value + 10 }));

            _dest = config.CreateMapper().Map<Source, Destination>(_source);
            _dest.Value.ShouldEqual(20);
        }

        [Fact]
        public void Should_ignore_other_mapping_rules()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AnotherValue))
                .ProjectUsing(s => new Destination { Value = s.Value + 10 }));

            _dest = config.CreateMapper().Map<Source, Destination>(_source);
            _dest.Value.ShouldEqual(20);
        }
    }

    public class When_specifying_a_custom_translator_and_passing_in_the_destination_object
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

        public When_specifying_a_custom_translator_and_passing_in_the_destination_object()
        {
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
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                .ConvertUsing(s => new Destination { Value = s.Value + 10 }));

            _dest = config.CreateMapper().Map(_source, _dest);
            _dest.Value.ShouldEqual(20);
        }

        [Fact]
        public void Should_ignore_other_mapping_rules()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AnotherValue))
                .ConvertUsing(s => new Destination { Value = s.Value + 10 }));

            _dest = config.CreateMapper().Map(_source, _dest);
            _dest.Value.ShouldEqual(20);
        }
    }

    public class When_specifying_a_custom_translator_using_generics
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

        public When_specifying_a_custom_translator_using_generics()
        {
            _source = new Source
            {
                Value = 10,
                AnotherValue = 1000
            };
        }

        public class Converter : ITypeConverter<Source, Destination>
        {
            public Destination Convert(Source source, Destination destination, ResolutionContext context)
            {
                return new Destination { Value = source.Value + 10 };
            }
        }

        [Fact]
        public void Should_use_the_custom_translator()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                .ConvertUsing<Converter>());

            _dest = config.CreateMapper().Map<Source, Destination>(_source);
            _dest.Value.ShouldEqual(20);
        }

        [Fact]
        public void Should_ignore_other_mapping_rules()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AnotherValue))
                .ConvertUsing(s => new Destination { Value = s.Value + 10 }));

            _dest = config.CreateMapper().Map<Source, Destination>(_source);
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

        public class CustomConverter : ITypeConverter<Source, Destination>
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

            public Destination Convert(Source source, Destination destination, ResolutionContext context)
            {
                return new Destination { Value = source.Value + _value };
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.ConstructServicesUsing(t => new CustomConverter(10));
            cfg.CreateMap<Source, Destination>()
                .ConvertUsing<CustomConverter>();
        });

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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConvertUsing(s => new Destination {Value2 = s.Value1 + 10});
        });

        [Fact]
        public void Should_pass_all_configuration_checks()
        {
            Exception thrown = null;
            try
            {
                Configuration.AssertConfigurationIsValid();

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

        public class CustomValueResolver : IMemberValueResolver<object, object, int, int>
        {
            private readonly int _toAdd;
            public CustomValueResolver() { _toAdd = 11; }

            public CustomValueResolver(int toAdd)
            {
                _toAdd = toAdd;
            }

            public int Resolve(object s, object d, int source, int dest, ResolutionContext context)
            {
                return source + _toAdd;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.ConstructServicesUsing(type => new CustomValueResolver(5));

            cfg.CreateMap<Source, Destination>()
                .ForMember(d => d.Value, opt => opt.ResolveUsing<CustomValueResolver, int>(src => src.Value));
        });

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

        public class CustomValueResolver : IMemberValueResolver<object, object, int, int>
        {
            public int Resolve(object s, object d, int source, int dest, ResolutionContext context)
            {
                return dest;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ForMember(d => d.Value, opt => opt.ResolveUsing<CustomValueResolver, int>(src => src.Value));
        });

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

    public class When_using_inheritance_with_value_resoluvers : AutoMapperSpecBase
    {
        public class SourceDto
        {
            public int Id { get; set; }
            public string NumberValue { get; set; }
        }

        public class SourceChildDto : SourceDto
        {
            public string ChildField { get; set; }
        }

        public class DestinationDto
        {
            public int Ident { get; set; }
            public int Number { get; set; }
        }

        public class DestinationChildDto : DestinationDto
        {
            public string ChildField { get; set; }
        }

        public class CustomResolver : IMemberValueResolver<SourceDto, object, string, int>
        {
            public int Resolve(SourceDto src, object dest, string source, int member, ResolutionContext context)
            {
                return int.Parse(source);
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => {
            cfg.CreateMap<SourceDto, DestinationDto>()
                .ForMember(dest => dest.Ident, opt => opt.MapFrom(x => x.Id))
                .ForMember(dest => dest.Number, opt => opt.ResolveUsing<CustomResolver, string>(src => src.NumberValue))
                ;
            cfg.CreateMap<SourceChildDto, DestinationChildDto>()
                .IncludeBase<SourceDto, DestinationDto>()
                //.ForMember(dest => dest.Number, opt => opt.ResolveUsing<CustomResolver, string>(src => src.NumberValue))
                ;
        });

        [Fact]
        public void Should_inherit_value_resolver()
        {
            var sourceChild = new SourceChildDto
            {
                Id = 1,
                NumberValue = "13",
                ChildField = "alpha"
            };

            // destination = { Ident: 1, Number: 0 /* should be 13 */, ChildField: "alpha" }
            var destination = Mapper.Map<DestinationChildDto>(sourceChild);
            destination.Number.ShouldEqual(13);
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

        public class CustomValueResolver : IMemberValueResolver<object, object, int, object>
        {
            public CustomValueResolver()
            {
            }

            public object Resolve(object s, object d, int source, object dest, ResolutionContext context)
            {
                return source + 5;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.ConstructServicesUsing(type => new CustomValueResolver());

            cfg.CreateMap<Source, Destination>()
                .ForMember("DestinationValue",
                    opt => opt.ResolveUsing<CustomValueResolver, int>("SourceValue"));
        });

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

    public class When_specifying_a_custom_member_mapping_to_a_nested_object
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
                var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Dest.Value, opt => opt.MapFrom(src => src.Value)));
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ForMember(dest => ((ISomeInterface) dest).Name, opt => opt.MapFrom(src => src.MyName));

        });

        protected override void Because_of()
        {
            _source = new Source {MyName = "jon"};
            _dest = Mapper.Map<Source, Destination>(_source);
        }

        [Fact]
        public void Should_perform_the_translation()
        {
            _dest.Name.ShouldEqual("jon");
        }
    }

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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();

        });

        protected override void Because_of()
        {
            _source = new Source {Name = "jon", Value = "value", Foo = "bar"};
            _dest = new Destination();
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<SourceWithList, DestinationWithList>();

        });

        protected override void Because_of()
        {
            _source = new Source { Value = "jon" };
            _dest = new Destination();


            _sourceWithList = new SourceWithList { SomeList = new[] { 1, 2 } };
            _destWithList = new DestinationWithList();
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConstructUsing(src => new Destination(src.Value + 4))
                .ForMember(dest => dest.OtherValue, opt => opt.Ignore());
        });

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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Value, opt => opt.UseValue(5));
        });

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

    public class When_building_custom_configuration_mapping_to_itself
    {
        private Exception _e;

        public class Source
        {

        }

        public class Dest
        {
            public int Value { get; set; }
        }

        [Fact]
        public void Should_map_from_that_constant_value()
        {
            try
            {
                var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>()
                    .ForMember(dest => dest, opt => opt.UseValue(5)));
            }
            catch (Exception e)
            {
                _e = e;
            }
            _e.ShouldNotBeNull();
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
            // AutoMapper tries to map source to this constructor's parameter,
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.DisableConstructorMapping();
            cfg.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(s => ParseValue(s.Value)));
        });

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