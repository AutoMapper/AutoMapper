using System;
using AutoMapper;
using NBehave.Spec.NUnit;
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

				var result = Mapper.Map<decimal, string>(5343.15m);

				result.ShouldEqual("$5,343.15");
			}
		}
	}
}