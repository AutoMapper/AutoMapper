using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class OpenGenericsWithInclude : AutoMapperSpecBase
    {
        public class Person
        {
            public string Name { get; set; }
            public List<BarBase> BarList { get; set; } = new List<BarBase>();
        }

        public class PersonModel
        {
            public string Name { get; set; }
            public List<BarModelBase> BarList { get; set; }
        }

        abstract public class BarBase
        {
            public int Id { get; set; }
        }

        public class Bar<T> : BarBase
        {
            public T Value { get; set; }
        }

        abstract public class BarModelBase
        {
            public int Id { get; set; }
            public string Ignored { get; set; }
        }

        public class BarModel<T> : BarModelBase
        {
            public T Value { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BarBase, BarModelBase>().ForMember(d=>d.Ignored, o=>o.Ignore()).Include(typeof(Bar<>), typeof(BarModel<>));
            cfg.CreateMap<Person, PersonModel>();
            cfg.CreateMap(typeof(Bar<>), typeof(BarModel<>));
        });

        [Fact]
        public void Should_work()
        {
            var person = new Person { Name = "Jack", BarList = { new Bar<string>{ Id = 1, Value = "One" }, new Bar<string>{ Id = 2, Value = "Two" } } };

            var personMapped = Mapper.Map<PersonModel>(person);

            ((BarModel<string>)personMapped.BarList[0]).Value.ShouldBe("One");
        }
    }

    public class OpenGenericsWithIncludeBase : AutoMapperSpecBase
    {
        public class Person
        {
            public string Name { get; set; }
            public List<BarBase> BarList { get; set; } = new List<BarBase>();
        }

        public class PersonModel
        {
            public string Name { get; set; }
            public List<BarModelBase> BarList { get; set; }
        }

        abstract public class BarBase
        {
            public int Id { get; set; }
        }

        public class Bar<T> : BarBase
        {
            public T Value { get; set; }
        }

        abstract public class BarModelBase
        {
            public int Id { get; set; }
            public string Ignored { get; set; }
        }

        public class BarModel<T> : BarModelBase
        {
            public T Value { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(BarBase), typeof(BarModelBase)).ForMember("Ignored", o=>o.Ignore());
            cfg.CreateMap<Person, PersonModel>();
            cfg.CreateMap(typeof(Bar<>), typeof(BarModel<>)).IncludeBase(typeof(BarBase), typeof(BarModelBase));
        });

        [Fact]
        public void Should_work()
        {
            var person = new Person { Name = "Jack", BarList = { new Bar<string> { Id = 1, Value = "One" }, new Bar<string> { Id = 2, Value = "Two" } } };

            var personMapped = Mapper.Map<PersonModel>(person);

            ((BarModel<string>)personMapped.BarList[0]).Value.ShouldBe("One");
        }
    }
}