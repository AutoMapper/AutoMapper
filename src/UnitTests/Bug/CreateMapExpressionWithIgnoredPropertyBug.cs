using System.Collections.Generic;
using System.Linq;
using AutoMapper.QueryableExtensions;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class CreateMapExpressionWithIgnoredPropertyBug : NonValidatingSpecBase
    {
        [Test]
        public void ShouldNotMapPropertyWhenItIsIgnored()
        {
            Mapper.CreateMap<Person, Person>()
                .ForMember(x => x.Name, x => x.Ignore());

            IQueryable<Person> collection = (new List<Person> { new Person { Name = "Person1" } }).AsQueryable();

            List<Person> result = collection.Project().To<Person>().ToList();

            result.ForEach(x => Assert.IsNull(x.Name));
        }

        private class Person
        {
            public string Name { get; set; }
        }
    }
}