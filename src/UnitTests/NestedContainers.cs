using System;
using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests
{
    namespace NestedContainers
    {
        public class When_specifying_a_custom_contextual_constructor : AutoMapperSpecBase
        {
            private Dest _dest;

            public class FooResolver : ValueResolver<int, int>
            {
                private readonly int _value;

                public FooResolver()
                    : this(1)
                {
                }

                public FooResolver(int value)
                {
                    _value = value;
                }

                protected override int ResolveCore(int source)
                {
                    return source + _value;
                }
            }

            public class BarResolver : ValueResolver<int, int>
            {
                protected override int ResolveCore(int source)
                {
                    return source + 1;
                }
            }

            public class Source
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Dest>()
                        .ForMember(x => x.Value, opt => opt.ResolveUsing<FooResolver>().FromMember(x => x.Value))
                        .ForMember(x => x.Value2, opt => opt.ResolveUsing<BarResolver>().FromMember(x => x.Value2));
                });
            }

            protected override void Because_of()
            {
                _dest = Mapper.Map<Source, Dest>(new Source { Value = 5, Value2 = 6 },
                    opt => opt.ConstructServicesUsing(type => type == typeof(FooResolver) ? new FooResolver(2) : null));
            }

            [Test]
            public void Should_use_the_new_ctor()
            {
                _dest.Value.ShouldEqual(7);
            }

            [Test]
            public void Should_use_the_existing_ctor_for_non_overridden_ctors()
            {
                _dest.Value2.ShouldEqual(7);
            }
        }

        public class When_specifying_a_custom_contextual_constructor_for_type_converters : AutoMapperSpecBase
        {
            private Dest _dest;

            public class FooTypeConverter : TypeConverter<Source, Dest>
            {
                private readonly int _value;

                public FooTypeConverter()
                    : this(1)
                {
                }

                public FooTypeConverter(int value)
                {
                    _value = value;
                }

                protected override Dest ConvertCore(Source source)
                {
                    return new Dest
                    {
                        Value = source.Value + _value,
                        Value2 = source.Value2 + _value
                    };
                }
            }

            public class Source
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Dest>()
                        .ConvertUsing<FooTypeConverter>();
                });
            }

            protected override void Because_of()
            {
                _dest = Mapper.Map<Source, Dest>(new Source { Value = 5, Value2 = 6 },
                    opt => opt.ConstructServicesUsing(type => type == typeof(FooTypeConverter) ? new FooTypeConverter(2) : null));
            }

            [Test]
            public void Should_use_the_new_ctor()
            {
                _dest.Value.ShouldEqual(7);
                _dest.Value2.ShouldEqual(8);
            }

        }
    }
}