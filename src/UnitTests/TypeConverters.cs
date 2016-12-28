using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.CustomMapping
{
    public class NullableConverter : AutoMapperSpecBase
    {
        public enum GreekLetters
        {
            Alpha = 11,
            Beta = 12,
            Gamma = 13
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c =>
        {
            c.CreateMap<int?, GreekLetters>().ConvertUsing(n => n == null ? GreekLetters.Beta : GreekLetters.Gamma);
        });

        [Fact]
        public void Should_map_nullable()
        {
            Mapper.Map<int?, GreekLetters>(null).ShouldEqual(GreekLetters.Beta);
            Mapper.Map<int?, GreekLetters>(42).ShouldEqual(GreekLetters.Gamma);
        }
    }

    public class MissingConverter : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(c =>
        {
            c.ConstructServicesUsing(t => null);
            c.CreateMap<int, int>().ConvertUsing<ITypeConverter<int, int>>();
        });

        [Fact]
        public void Should_report_the_missing_converter()
        {
            new Action(()=>Mapper.Map<int, int>(0))
                .ShouldThrow<AutoMapperMappingException>(e=>e.Message.ShouldEqual("Cannot create an instance of type AutoMapper.ITypeConverter`2[System.Int32,System.Int32]"));
        }
    }

    public class DecimalAndNullableDecimal : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public decimal Value1 { get; set; }
            public decimal? Value2 { get; set; }
            public decimal? Value3 { get; set; }
        }

        class Destination
        {
            public decimal? Value1 { get; set; }
            public decimal Value2 { get; set; }
            public decimal? Value3 { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<decimal?, decimal>().ConvertUsing(source => source ?? decimal.MaxValue);
            cfg.CreateMap<decimal, decimal?>().ConvertUsing(source => source == decimal.MaxValue ? new decimal?() : source);
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { Value1 = decimal.MaxValue });
        }


        [Fact]
        public void Should_treat_max_value_as_null()
        {
            _destination.Value1.ShouldBeNull();
            _destination.Value2.ShouldEqual(decimal.MaxValue);
            _destination.Value3.ShouldBeNull();
        }
    }

    public class When_converting_to_string : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public Id TheId { get; set; }
        }

        class Destination
        {
            public string TheId { get; set; }
        }

        interface IId
        {
            string Serialize();
        }

        class Id : IId
        {
            public string Prefix { get; set; }

            public string Value { get; set; }

            public string Serialize()
            {
                return Prefix + "_" + Value;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<IId, string>().ConvertUsing(id => id.Serialize());
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { TheId = new Id { Prefix = "p", Value = "v" } });
        }

        [Fact]
        public void Should_use_the_type_converter()
        {
            _destination.TheId.ShouldEqual("p_v");
        }
    }

    public class When_specifying_type_converters_for_object_mapper_types : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public IDictionary<int, int> Values { get; set; }
        }

        class Destination
        {
            public IDictionary<int, int> Values { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(IDictionary<,>), typeof(IDictionary<,>)).ConvertUsing(typeof(DictionaryConverter<,>));
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { Values = new Dictionary<int, int>() });
        }

        [Fact]
        public void Should_override_the_built_in_mapper()
        {
            _destination.Values.ShouldBeSameAs(DictionaryConverter<int, int>.Instance);
        }

        private class DictionaryConverter<TKey, TValue> : ITypeConverter<IDictionary<TKey, TValue>, IDictionary<TKey, TValue>>
        {
            public static readonly IDictionary<TKey, TValue> Instance = new Dictionary<TKey, TValue>();

            public IDictionary<TKey, TValue> Convert(IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> destination, ResolutionContext context)
            {
                return Instance;
            }
        }
    }

    public class When_specifying_type_converters : AutoMapperSpecBase
    {
        private Destination _result;

        public class Source
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }
            public string Value3 { get; set; }
        }

        public class Destination
        {
            public int Value1 { get; set; }
            public DateTime Value2 { get; set; }
            public Type Value3 { get; set; }
        }

        public class DateTimeTypeConverter : ITypeConverter<string, DateTime>
        {
            public DateTime Convert(string source, DateTime destination, ResolutionContext context)
            {
                return System.Convert.ToDateTime(source);
            }
        }

        public class TypeTypeConverter : ITypeConverter<string, Type>
        {
            public Type Convert(string source, Type destination, ResolutionContext context)
            {
                Type type = Assembly.GetExecutingAssembly().GetType(source);
                return type;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<string, int>().ConvertUsing((string arg) => Convert.ToInt32(arg));
            cfg.CreateMap<string, DateTime>().ConvertUsing(new DateTimeTypeConverter());
            cfg.CreateMap<string, Type>().ConvertUsing<TypeTypeConverter>();
            cfg.CreateMap<Source, Destination>();

        });

        protected override void Because_of()
        {
            var source = new Source
            {
                Value1 = "5",
                Value2 = "01/01/2000",
                Value3 = "AutoMapper.UnitTests.CustomMapping.When_specifying_type_converters+Destination"
            };

            _result = Mapper.Map<Source, Destination>(source);
        }

        [Fact]
        public void Should_convert_type_using_expression()
        {
            _result.Value1.ShouldEqual(5);
        }

        [Fact]
        public void Should_convert_type_using_instance()
        {
            _result.Value2.ShouldEqual(new DateTime(2000, 1, 1));
        }

        [Fact]
        public void Should_convert_type_using_Func_that_returns_instance()
        {
            _result.Value3.ShouldEqual(typeof(Destination));
        }
    }

    public class When_specifying_type_converters_on_types_with_incompatible_members : AutoMapperSpecBase
    {
        private ParentDestination _result;

        public class Source
        {
            public string Foo { get; set; }
        }

        public class Destination
        {
            public int Type { get; set; }
        }

        public class ParentSource
        {
            public Source Value { get; set; }
        }

        public class ParentDestination
        {
            public Destination Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().ConvertUsing(arg => new Destination {Type = Convert.ToInt32(arg.Foo)});
            cfg.CreateMap<ParentSource, ParentDestination>();

        });

        protected override void Because_of()
        {
            var source = new ParentSource
            {
                Value = new Source { Foo = "5", }
            };

            _result = Mapper.Map<ParentSource, ParentDestination>(source);
        }

        [Fact]
        public void Should_convert_type_using_expression()
        {
            _result.Value.Type.ShouldEqual(5);
        }
    }

    public class When_specifying_mapping_with_the_BCL_type_converter_class : NonValidatingSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        [TypeConverter(typeof(CustomTypeConverter))]
        public class Source
        {
            public int Value { get; set; }
        }

        public class Destination
        {
            public int OtherValue { get; set; }
        }

        public class CustomTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof (Destination);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                return new Destination
                    {
                        OtherValue = ((Source) value).Value + 10
                    };
            }
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(Destination);
            }
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                return new Source {Value = ((Destination) value).OtherValue - 10};
            }
        }

        [Fact]
        public void Should_convert_from_type_using_the_custom_type_converter()
        {
            var source = new Source
                {
                    Value = 5
                };
            var destination = Mapper.Map<Source, Destination>(source);

            destination.OtherValue.ShouldEqual(15);
        }

        [Fact]
        public void Should_convert_to_type_using_the_custom_type_converter()
        {
            var source = new Destination()
            {
                OtherValue = 15
            };
            var destination = Mapper.Map<Destination, Source>(source);

            destination.Value.ShouldEqual(5);
        }
    }

    public class When_specifying_a_type_converter_for_a_non_generic_configuration : NonValidatingSpecBase
    {
        private Destination _result;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Destination
        {
            public int OtherValue { get; set; }
        }

        public class CustomConverter : ITypeConverter<Source, Destination>
        {
            public Destination Convert(Source source, Destination destination, ResolutionContext context)
            {
                return new Destination
                    {
                        OtherValue = source.Value + 10
                    };
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof (Source), typeof (Destination)).ConvertUsing<CustomConverter>();
        });

        protected override void Because_of()
        {
            _result = Mapper.Map<Source, Destination>(new Source {Value = 5});
        }

        [Fact]
        public void Should_use_converter_specified()
        {
            _result.OtherValue.ShouldEqual(15);
        }
    }

    public class When_specifying_a_non_generic_type_converter_for_a_non_generic_configuration : AutoMapperSpecBase
    {
        private Destination _result;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Destination
        {
            public int OtherValue { get; set; }
        }

        public class CustomConverter : ITypeConverter<Source, Destination>
        {
            public Destination Convert(Source source, Destination destination, ResolutionContext context)
            {
                return new Destination
                    {
                        OtherValue = source.Value + 10
                    };
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof (Source), typeof (Destination)).ConvertUsing(typeof (CustomConverter));
        });

        protected override void Because_of()
        {
            _result = Mapper.Map<Source, Destination>(new Source {Value = 5});
        }

        [Fact]
        public void Should_use_converter_specified()
        {
            _result.OtherValue.ShouldEqual(15);
        }
    }
}