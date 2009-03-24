using System;
using System.Reflection;
using NBehave.Spec.NUnit;
using NUnit.Framework;

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

			private static TypeTypeConverter GetResolver()
			{
				return new TypeTypeConverter();
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<string, int>().ConvertUsing(arg => Convert.ToInt32(arg));
				Mapper.CreateMap<string, DateTime>().ConvertUsing(new DateTimeTypeConverter());
				Mapper.CreateMap<string, Type>().ConvertUsing(GetResolver);

				Mapper.CreateMap<Source, Destination>();
				Mapper.AssertConfigurationIsValid();

				var source = new Source
				{
					Value1 = "5",
					Value2 = "01/01/2000",
					Value3 = "AutoMapper.UnitTests.CustomMapping.When_specifying_type_converters+Destination"
				};

				_result = Mapper.Map<Source, Destination>(source);
			}

			[Test]
			public void Should_convert_type_using_expression()
			{
				_result.Value1.ShouldEqual(5);
			}

			[Test]
			public void Should_convert_type_using_instance()
			{
				_result.Value2.ShouldEqual(new DateTime(2000, 1, 1));
			}

			[Test]
			public void Should_convert_type_using_Func_that_returns_instance()
			{
				_result.Value3.ShouldEqual(typeof(Destination));
			}
		}
	}
}