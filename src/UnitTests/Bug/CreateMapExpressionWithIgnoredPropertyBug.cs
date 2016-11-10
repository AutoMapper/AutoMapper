using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    using QueryableExtensions;

    public class CreateMapExpressionWithIgnoredPropertyBug
    {
        [Fact]
        public void ShouldNotMapPropertyWhenItIsIgnored()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Person, Person>()
                    .ForMember(x => x.Name, x => x.Ignore());
            });

            IQueryable<Person> collection = (new List<Person> { new Person { Name = "Person1" } }).AsQueryable();

            List<Person> result = collection.ProjectTo<Person>(config).ToList();

            result.ForEach(x => x.Name.ShouldBeNull());
        }

        public class Person
        {
            public string Name { get; set; }
        }
    }
}