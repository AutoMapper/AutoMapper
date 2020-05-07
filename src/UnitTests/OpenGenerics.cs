namespace AutoMapper.UnitTests
{
    using AutoMapper;
    using MappingInheritance;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class GenericValueResolver : AutoMapperSpecBase
    {
        class Destination
        {
            public string MyKey;
            public string MyValue;
        }
        class Destination<TKey, TValue>
        {
            public TKey MyKey;
            public TValue MyValue;
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(KeyValuePair<,>), typeof(Destination))
                .ForMember("MyKey", o => o.MapFrom(typeof(KeyResolver<>)))
                .ForMember("MyValue", o => o.MapFrom(typeof(ValueResolver<,>)));
            cfg.CreateMap(typeof(KeyValuePair<,>), typeof(Destination<,>))
                .ForMember("MyKey", o => o.MapFrom(typeof(KeyResolver<,,>)))
                .ForMember("MyValue", o => o.MapFrom(typeof(ValueResolver<,,,>)));
        });
        private class KeyResolver<TKey> : IValueResolver<KeyValuePair<TKey, int>, Destination, string>
        {
            public string Resolve(KeyValuePair<TKey, int> source, Destination destination, string destMember, ResolutionContext context) => source.Key.ToString();
        }
        private class ValueResolver<TKey, TValue> : IValueResolver<KeyValuePair<TKey, TValue>, Destination, string>
        {
            public string Resolve(KeyValuePair<TKey, TValue> source, Destination destination, string destMember, ResolutionContext context) => source.Value.ToString();
        }
        private class KeyResolver<TKeySource, TValueSource, TKeyDestination>
            : IValueResolver<KeyValuePair<TKeySource, TValueSource>, Destination<TKeyDestination, string>, string>
        {
            public string Resolve(KeyValuePair<TKeySource, TValueSource> source, Destination<TKeyDestination, string> destination, string destMember, ResolutionContext context)
                => source.Key.ToString();
        }
        private class ValueResolver<TKeySource, TValueSource, TKeyDestination, TValueDestination>
            : IValueResolver<KeyValuePair<TKeySource, TValueSource>, Destination<TKeyDestination, TValueDestination>, string>
        {
            public string Resolve(KeyValuePair<TKeySource, TValueSource> source, Destination<TKeyDestination, TValueDestination> destination, string destMember, ResolutionContext context)
                => source.Value.ToString();
        }
        [Fact]
        public void Should_map_non_generic_destination()
        {
            var destination = Map<Destination>(new KeyValuePair<int, int>(1,2));
            destination.MyKey.ShouldBe("1");
            destination.MyValue.ShouldBe("2");
        }
        [Fact]
        public void Should_map_generic_destination()
        {
            var destination = Map<Destination<string, string>>(new KeyValuePair<int, int>(1, 2));
            destination.MyKey.ShouldBe("1");
            destination.MyValue.ShouldBe("2");
        }
    }

    public class GenericMemberValueResolver : AutoMapperSpecBase
    {
        class Destination
        {
            public string MyKey;
            public string MyValue;
        }
        class Destination<TKey, TValue>
        {
            public TKey MyKey;
            public TValue MyValue;
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(KeyValuePair<,>), typeof(Destination))
                .ForMember("MyKey", o => o.MapFrom(typeof(Resolver<>), "Key"))
                .ForMember("MyValue", o => o.MapFrom(typeof(Resolver<,>), "Value"));
            cfg.CreateMap(typeof(KeyValuePair<,>), typeof(Destination<,>))
                .ForMember("MyKey", o => o.MapFrom(typeof(Resolver<,,>), "Key"))
                .ForMember("MyValue", o => o.MapFrom(typeof(Resolver<,,,>), "Value"));
        });
        private class Resolver<TKey> : IMemberValueResolver<KeyValuePair<TKey, int>, Destination, int, string>
        {
            public string Resolve(KeyValuePair<TKey, int> source, Destination destination, int sourceMember, string destMember, ResolutionContext context) => sourceMember.ToString();
        }
        private class Resolver<TKey, TValue> : IMemberValueResolver<KeyValuePair<TKey, TValue>, Destination, int, string>
        {
            public string Resolve(KeyValuePair<TKey, TValue> source, Destination destination, int sourceMember, string destMember, ResolutionContext context) => sourceMember.ToString();
        }
        private class Resolver<TKey, TValue, TDestinatonKey> : IMemberValueResolver<KeyValuePair<TKey, TValue>, Destination<TDestinatonKey, string>, int, string>
        {
            public string Resolve(KeyValuePair<TKey, TValue> source, Destination<TDestinatonKey, string> destination, int sourceMember, string destMember, ResolutionContext context) => sourceMember.ToString();
        }
        private class Resolver<TKey, TValue, TDestinatonKey, TDestinatonValue> : IMemberValueResolver<KeyValuePair<TKey, TValue>, Destination<TDestinatonKey, TDestinatonValue>, int, string>
        {
            public string Resolve(KeyValuePair<TKey, TValue> source, Destination<TDestinatonKey, TDestinatonValue> destination, int sourceMember, string destMember, ResolutionContext context) => sourceMember.ToString();
        }
        [Fact]
        public void Should_map_non_generic_destination()
        {
            var destination = Map<Destination>(new KeyValuePair<int, int>(1, 2));
            destination.MyKey.ShouldBe("1");
            destination.MyValue.ShouldBe("2");
        }
        [Fact]
        public void Should_map_generic_destination()
        {
            var destination = Map<Destination<string, string>>(new KeyValuePair<int, int>(1, 2));
            destination.MyKey.ShouldBe("1");
            destination.MyValue.ShouldBe("2");
        }
    }

    public class RecursiveOpenGenerics : AutoMapperSpecBase
    {
        public class SourceTree<T>
        {
            public SourceTree(T value, SourceTree<T>[] children)
            {
                Value = value;
                Children = children;
            }

            public T Value { get; }

            public SourceTree<T>[] Children { get; }
        }

        public class DestinationTree<T>
        {
            public DestinationTree(T value, DestinationTree<T>[] children)
            {
                Value = value;
                Children = children;
            }

            public T Value { get; }

            public DestinationTree<T>[] Children { get; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap(typeof(SourceTree<>), typeof(DestinationTree<>)));

        [Fact]
        public void Should_work()
        {
            var source = new SourceTree<string>("value", new SourceTree<string>[0]);
            Mapper.Map<DestinationTree<string>>(source).Value.ShouldBe("value");
        }
    }

    public class OpenGenericsValidation : NonValidatingSpecBase
    {
        public class Source<T>
        {
            public T Value { get; set; }
        }

        public class Dest<T>
        {
            public int A { get; set; }
            public T Value { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

        [Fact]
        public void Should_report_unmapped_property()
        {
            new Action(()=> Mapper.Map<Dest<int>>(new Source<int>{ Value = 5 }))
                .ShouldThrow<AutoMapperConfigurationException>()
                .Errors.Single().UnmappedPropertyNames.Single().ShouldBe("A");
        }
    }

    public class OpenGenericsProfileValidationNonGenericMembers : NonValidatingSpecBase
    {
        public class Source<T>
        {
            public T[] Value { get; set; }
        }

        public class Dest<T>
        {
            public int A { get; set; }
            public T[] Value { get; set; }
        }

        class MyProfile : Profile
        {
            public MyProfile()
            {
                CreateMap(typeof(Source<>), typeof(Dest<>));
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.AddProfile<MyProfile>());

        [Fact]
        public void Should_report_unmapped_property()
        {
            new Action(()=> Configuration.AssertConfigurationIsValid<MyProfile>())
                .ShouldThrow<AutoMapperConfigurationException>()
                .Errors.Single().UnmappedPropertyNames.Single().ShouldBe("A"); ;
        }
    }

    public class OpenGenericsProfileValidation : AutoMapperSpecBase
    {
        public class Source<T>
        {
            public T[] Value { get; set; }
        }

        public class Dest<T>
        {
            public T[] Value { get; set; }
        }

        class MyProfile : Profile
        {
            public MyProfile()
            {
                CreateMap(typeof(Source<>), typeof(Dest<>));
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.AddProfile<MyProfile>());
    }

    public class OpenGenerics
    {
        public class Source<T>
        {
            public int A { get; set; }
            public T Value { get; set; }
        }

        public class Dest<T>
        {
            public int A { get; set; }
            public T Value { get; set; }
        }

        [Fact]
        public void Can_map_simple_generic_types()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

            var source = new Source<int>
            {
                Value = 5
            };

            var dest = config.CreateMapper().Map<Source<int>, Dest<int>>(source);

            dest.Value.ShouldBe(5);
        }

        [Fact]
        public void Can_map_non_generic_members()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

            var source = new Source<int>
            {
                A = 5
            };

            var dest = config.CreateMapper().Map<Source<int>, Dest<int>>(source);

            dest.A.ShouldBe(5);
        }

        [Fact]
        public void Can_map_recursive_generic_types()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

            var source = new Source<Source<int>>
            {
                Value = new Source<int>
                {
                    Value = 5,
                }
            };

            var dest = config.CreateMapper().Map<Source<Source<int>>, Dest<Dest<double>>>(source);

            dest.Value.Value.ShouldBe(5);
        }
    }

    public class OpenGenerics_With_MemberConfiguration : AutoMapperSpecBase
    {
        public class Foo<T>
        {
            public int A { get; set; }
            public int B { get; set; }
        }

        public class Bar<T>
        {
            public int C { get; set; }
            public int D { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(mapper => {
            mapper.CreateMap(typeof(Foo<>), typeof(Bar<>))
            .ForMember("C", to => to.MapFrom("A"))
            .ForMember("D", to => to.MapFrom("B"));
        });

        [Fact]
        public void Can_remap_explicit_members()
        {
            var source = new Foo<int>
            {
                A = 5,
                B = 10
            };

            var dest = Mapper.Map<Foo<int>, Bar<int>>(source);

            dest.C.ShouldBe(5);
            dest.D.ShouldBe(10);
        }
    }

    public class OpenGenerics_With_UntypedMapFrom : AutoMapperSpecBase
    {
        public class Foo<T>
        {
            public T Value1 { get; set; }
        }

        public class Bar<T>
        {
            public T Value2 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(mapper => {
            mapper.CreateMap(typeof(Foo<>), typeof(Bar<>)).ForMember("Value2", to => to.MapFrom("Value1"));
        });

        [Fact]
        public void Can_remap_explicit_members()
        {
            var dest = Mapper.Map<Bar<int>>(new Foo<int> { Value1 = 5 });
            dest.Value2.ShouldBe(5);
        }
    }

    public class OpenGenerics_With_UntypedMapFromStructs : AutoMapperSpecBase
    {
        public class Foo<T> where T : struct
        {
            public T Value1 { get; set; }
        }

        public class Bar<T> where T : struct
        {
            public T Value2 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(mapper => {
            mapper.CreateMap(typeof(Foo<>), typeof(Bar<>)).ForMember("Value2", to => to.MapFrom("Value1"));
        });

        [Fact]
        public void Can_remap_explicit_members()
        {
            var dest = Mapper.Map<Bar<int>>(new Foo<int> { Value1 = 5 });
            dest.Value2.ShouldBe(5);
        }
    }
}