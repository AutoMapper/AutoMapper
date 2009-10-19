using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.Mappers;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
	class One
	{
		public IEnumerable<string> Stuff { get; set; }
	}

	class Two
	{
		public IEnumerable<Item> Stuff { get; set; }
	}

	class Item
	{
		public string Value { get; set; }
	}

	class StringToItemConverter : ITypeConverter<IEnumerable<string>, IEnumerable<Item>>
	{
		public IEnumerable<Item> Convert(IEnumerable<string> source)
		{
			var result = new List<Item>();
			foreach (string s in source)
				if (!String.IsNullOrEmpty(s))
					result.Add(new Item { Value = s });
			return result;
		}
	}

	[TestFixture]
	public class AutoMapperBugTest
	{
		[Test]
		public void ShouldMapOneToTwo()
		{
			var config = new Configuration(new TypeMapFactory(), MapperRegistry.AllMappers());
			config.CreateMap<One, Two>();

			config.CreateMap<IEnumerable<string>, IEnumerable<Item>>().ConvertUsing<StringToItemConverter>();

			config.AssertConfigurationIsValid();

			var engine = new MappingEngine(config);
			var one = new One
			{
				Stuff = new List<string> { "hi", "", "mom" }
			};

			var two = engine.Map<One, Two>(one);

			two.ShouldNotBeNull();
			two.Stuff.Count().ShouldEqual(2);
		}
	}
}
