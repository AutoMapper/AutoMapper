using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.Mappers;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
	public class One
	{
		public IEnumerable<string> Stuff { get; set; }
	}

	public class Two
	{
		public IEnumerable<Item> Stuff { get; set; }
	}

	public class Item
	{
		public string Value { get; set; }
	}

	public class StringToItemConverter : TypeConverter<IEnumerable<string>, IEnumerable<Item>>
	{
		protected override IEnumerable<Item> ConvertCore(IEnumerable<string> source)
		{
			var result = new List<Item>();
			foreach (string s in source)
				if (!String.IsNullOrEmpty(s))
					result.Add(new Item { Value = s });
			return result;
		}
	}
	public class AutoMapperBugTest
	{
		[Fact]
		public void ShouldMapOneToTwo()
		{
            var config = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
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
