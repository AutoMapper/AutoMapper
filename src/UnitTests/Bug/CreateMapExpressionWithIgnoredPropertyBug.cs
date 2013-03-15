﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class CreateMapExpressionWithIgnoredPropertyBug : NonValidatingSpecBase
    {
        [Fact]
        public void ShouldNotMapPropertyWhenItIsIgnored()
        {
            Mapper.CreateMap<Person, Person>()
                .ForMember(x => x.Name, x => x.Ignore());

            IQueryable<Person> collection = (new List<Person> { new Person { Name = "Person1" } }).AsQueryable();

            List<Person> result = collection.Project().To<Person>().ToList();

            result.ForEach(x => x.Name.ShouldBeNull());
        }

        public class Person
        {
            public string Name { get; set; }
        }
    }
}