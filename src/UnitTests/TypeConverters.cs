#if !NETFX_CORE
using System;
using System.ComponentModel;
using System.Reflection;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
	namespace CustomMapping
	{
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

			public class DateTimeTypeConverter : TypeConverter<string, DateTime>
			{
				protected override DateTime ConvertCore(string source)
				{
					return System.Convert.ToDateTime(source);
				}
			}

			public class TypeTypeConverter : TypeConverter<string, Type>
			{
				protected override Type ConvertCore(string source)
				{
					Type type = Assembly.GetExecutingAssembly().GetType(source);
					return type;
				}
			}

			protected override void Establish_context()
			{
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<string, int>().ConvertUsing((string arg) => Convert.ToInt32(arg));
                    cfg.CreateMap<string, DateTime>().ConvertUsing(new DateTimeTypeConverter());
                    cfg.CreateMap<string, Type>().ConvertUsing<TypeTypeConverter>();
                    cfg.CreateMap<Source, Destination>();
                });

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

			protected override void Establish_context()
			{
			    Mapper.CreateMap<Source, Destination>().ConvertUsing(arg => new Destination {Type = Convert.ToInt32(arg.Foo)});
			    Mapper.CreateMap<ParentSource, ParentDestination>();

                var source = new ParentSource
				{
					Value = new Source { Foo = "5",}
				};

                _result = Mapper.Map<ParentSource, ParentDestination>(source);
			}

			[Fact]
			public void Should_convert_type_using_expression()
			{
                _result.Value.Type.ShouldEqual(5);
			}
		}

		public class When_specifying_mapping_with_the_BCL_type_converter_class : AutoMapperSpecBase
		{
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

		public class When_specifying_a_type_converter_for_a_non_generic_configuration : SpecBase
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

			public class CustomConverter : TypeConverter<Source, Destination>
			{
				protected override Destination ConvertCore(Source source)
				{
					return new Destination
						{
							OtherValue = source.Value + 10
						};
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap(typeof(Source), typeof(Destination)).ConvertUsing<CustomConverter>();
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Source, Destination>(new Source {Value = 5});
			}

			[Fact]
			public void Should_use_converter_specified()
			{
				_result.OtherValue.ShouldEqual(15);
			}

			[Fact]
			public void Should_pass_configuration_validation()
			{
				Mapper.AssertConfigurationIsValid();
			}
		}

		public class When_specifying_a_non_generic_type_converter_for_a_non_generic_configuration : SpecBase
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

			public class CustomConverter : TypeConverter<Source, Destination>
			{
				protected override Destination ConvertCore(Source source)
				{
					return new Destination
						{
							OtherValue = source.Value + 10
						};
				}
			}

			protected override void Establish_context()
			{
				Mapper.Initialize(cfg => cfg.CreateMap(typeof(Source), typeof(Destination)).ConvertUsing(typeof(CustomConverter)));
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Source, Destination>(new Source {Value = 5});
			}

			[Fact]
			public void Should_use_converter_specified()
			{
				_result.OtherValue.ShouldEqual(15);
			}

			[Fact]
			public void Should_pass_configuration_validation()
			{
				Mapper.AssertConfigurationIsValid();
			}
		}

	}
}
#endif
