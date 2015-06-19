namespace AutoMapper.UnitTests.Bug
{
    using System.Collections.Generic;
    using System.Linq;
    using Should;
    using Xunit;
    using QueryableExtensions;

    public class CreateMapExpressionWithIgnoredPropertyBug : NonValidatingSpecBase
    {
        [Fact]
        public void ShouldNotMapPropertyWhenItIsIgnored()
        {
            Mapper.CreateMap<Person, Person>()
                .ForMember(x => x.Name, x => x.Ignore());

            var collection = (new List<Person> {new Person {Name = "Person1"}}).AsQueryable();

            var result = collection.Project().To<Person>().ToList();

            result.ForEach(x => x.Name.ShouldBeNull());
        }

        public class Person
        {
            public string Name { get; set; }
        }
    }
}