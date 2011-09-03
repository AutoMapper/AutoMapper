using System.Collections.Generic;
using AutoMapper;
using Should;
using NUnit.Framework;
using System.Linq;

namespace AutoMapperSamples.Mappers
{
	namespace Lists
	{
		[TestFixture]
		public class SimpleExample
		{
			public class Source
			{
				public int Value { get; set; }
			}

			public class Destination
			{
				public int Value { get; set; }
			}

			[Test]
			public void Example()
			{
				Mapper.Initialize(cfg =>
				{
					cfg.CreateMap<Source, Destination>();
				});

				var sources = new[]
					{
						new Source {Value = 5},
						new Source {Value = 6},
						new Source {Value = 7}
					};

				IEnumerable<Destination> ienumerableDest = Mapper.Map<Source[], IEnumerable<Destination>>(sources);
				ICollection<Destination> icollectionDest = Mapper.Map<Source[], ICollection<Destination>>(sources);
				IList<Destination> ilistDest = Mapper.Map<Source[], IList<Destination>>(sources);
				List<Destination> listDest = Mapper.Map<Source[], List<Destination>>(sources);

				ienumerableDest.Count().ShouldEqual(3);
				icollectionDest.Count().ShouldEqual(3);
				ilistDest.Count().ShouldEqual(3);
				listDest.Count().ShouldEqual(3);
			}
		}

		[TestFixture]
		public class PolymorphicExample
		{
			public class ParentSource
			{
				public int Value1 { get; set; }
			}

			public class ChildSource : ParentSource
			{
				public int Value2 { get; set; }
			}

			public class ParentDestination
			{
				public int Value1 { get; set; }
			}

			public class ChildDestination : ParentDestination
			{
				public int Value2 { get; set; }
			}

			[Test]
			public void Example()
			{
				Mapper.Initialize(cfg =>
				{
					cfg.CreateMap<ParentSource, ParentDestination>()
						.Include<ChildSource, ChildDestination>();
					cfg.CreateMap<ChildSource, ChildDestination>();
				});

				var sources = new[]
					{
						new ParentSource(),
						new ChildSource(),
						new ParentSource()
					};

				var destinations = Mapper.Map<ParentSource[], ParentDestination[]>(sources);

				destinations[0].ShouldBeType<ParentDestination>();
				destinations[1].ShouldBeType<ChildDestination>();
				destinations[2].ShouldBeType<ParentDestination>();
			}
		}
	}
}