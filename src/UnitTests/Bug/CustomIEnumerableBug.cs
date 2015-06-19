namespace AutoMapper.UnitTests.Bug
{
    using System.Collections.Generic;
    using System.Linq;
    using Should;
    using Xunit;

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
            foreach (var s in source)
                if (!string.IsNullOrEmpty(s))
                    result.Add(new Item {Value = s});
            return result;
        }
    }

    public class AutoMapperBugTest
    {
        /// <summary>
        /// This is now very <see cref="MapperContext"/> oriented. Everything flows through its context.
        /// </summary>
        [Fact]
        public void Should_map_one_to_two()
        {
            //TODO: may want to run this through MapperContextFactory, at least PlatformAdapter ...
            var context = new MapperContext();

            context.Configuration.CreateMap<One, Two>();

            context.Configuration.CreateMap<IEnumerable<string>, IEnumerable<Item>>()
                .ConvertUsing<StringToItemConverter>();

            context.AssertConfigurationIsValid();

            var one = new One
            {
                Stuff = new List<string> {"hi", "", "mom"}
            };

            var two = context.Engine.Map<One, Two>(one);

            two.ShouldNotBeNull();
            two.Stuff.Count().ShouldEqual(2);
        }
    }
}