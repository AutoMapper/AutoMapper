using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using NUnit.Framework;
using Should;

namespace AutoMapperSamples
{
	namespace GlobalTypeConverters
	{
		[TestFixture]
		public class GlobalTypeConverters
		{
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

			[Test]
			public void Example()
			{
				Mapper.CreateMap<string, int>().ConvertUsing((string s) => Convert.ToInt32(s));
				Mapper.CreateMap<string, DateTime>().ConvertUsing(new DateTimeTypeConverter());
				Mapper.CreateMap<string, Type>().ConvertUsing<TypeTypeConverter>();
				Mapper.CreateMap<Source, Destination>();
				Mapper.AssertConfigurationIsValid();

				var source = new Source
				             	{
				             		Value1 = "5",
				             		Value2 = "01/01/2000",
									Value3 = "AutoMapperSamples.GlobalTypeConverters.GlobalTypeConverters+Destination"
				             	};

				Destination result = Mapper.Map<Source, Destination>(source);
				result.Value3.ShouldEqual(typeof(Destination));

				Expression<Func<Source, object>> func = x => x.Value1;

			}

			[SetUp]
			public void SetUp()
			{
				Mapper.Reset();
			}
		}
	}
}