using System;
using AutoMapper;
using Should;
using NUnit.Framework;

namespace AutoMapperSamples.Mappers
{
	namespace Strings
	{
		[TestFixture]
		public class FormattingExample
		{
			public class MoneyFormatter : ValueFormatter<decimal>
			{
				protected override string FormatValueCore(decimal value)
				{
					return value.ToString("c");
				}
			}

			[Test]
			public void Example()
			{
				Mapper.Initialize(cfg =>
				{
					cfg.ForSourceType<decimal>().AddFormatter<MoneyFormatter>();
				});

				var value = 5343.15m;

				var result = Mapper.Map<decimal, string>(value);

				result.ShouldEqual(value.ToString("c"));
			}
		}
	}
}