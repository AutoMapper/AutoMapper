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
            public string MappedFrom { get; set; }
        }

        public class BarModel<T> : BarModelBase
        {
            public T Value { get; set; }
            public string DerivedMember { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BarBase, BarModelBase>()
                .ForMember(d=>d.Ignored, o=>o.Ignore())
                .ForMember(d=>d.MappedFrom, o=>o.MapFrom(_=>"mappedFrom"))
                .Include(typeof(Bar<>), typeof(BarModel<>));
            cfg.CreateMap<Person, PersonModel>();
            cfg.CreateMap(typeof(Bar<>), typeof(BarModel<>)).ForMember("DerivedMember", o=>o.MapFrom("Id"));
        });

        [Fact]
        public void Should_work()
        {
            var person = new Person { Name = "Jack", BarList = { new Bar<string>{ Id = 1, Value = "One" }, new Bar<string>{ Id = 2, Value = "Two" } } };

            var personMapped = Mapper.Map<PersonModel>(person);

            var barModel = (BarModel<string>)personMapped.BarList[0];
            barModel.DerivedMember.ShouldBe("1");
            barModel.MappedFrom.ShouldBe("mappedFrom");
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
            public string MappedFrom { get; set; }
        }

        public class BarModel<T> : BarModelBase
        {
            public T Value { get; set; }
            public string DerivedMember { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(BarBase), typeof(BarModelBase))
                .ForMember("Ignored", o => o.Ignore())
                .ForMember("MappedFrom", o => o.MapFrom(_=>"mappedFrom"));
            cfg.CreateMap<Person, PersonModel>();
            cfg.CreateMap(typeof(Bar<>), typeof(BarModel<>))
                .ForMember("DerivedMember", o => o.MapFrom("Id"))
                .IncludeBase(typeof(BarBase), typeof(BarModelBase));
        });

        [Fact]
        public void Should_work()
        {
            var person = new Person { Name = "Jack", BarList = { new Bar<string> { Id = 1, Value = "One" }, new Bar<string> { Id = 2, Value = "Two" } } };

            var personMapped = Mapper.Map<PersonModel>(person);

            var barModel = (BarModel<string>)personMapped.BarList[0];
            barModel.DerivedMember.ShouldBe("1");
            barModel.MappedFrom.ShouldBe("mappedFrom");
        }
    }

    public class OpenGenericsWithAs : AutoMapperSpecBase
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
            public string MappedFrom { get; set; }
        }

        public class BarModel<T> : BarModelBase
        {
            public T Value { get; set; }
            public string DerivedMember { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(BarBase), typeof(BarModelBase))
                .ForMember("Ignored", o => o.Ignore())
                .ForMember("MappedFrom", o => o.MapFrom(_=>"mappedFrom"))
                .As(typeof(BarModel<>));
            cfg.CreateMap<Person, PersonModel>();
            cfg.CreateMap(typeof(Bar<>), typeof(BarModel<>))
            .ForMember("DerivedMember", o => o.MapFrom("Id"))
                .IncludeBase(typeof(BarBase), typeof(BarModelBase));
        });

        [Fact]
        public void Should_work()
        {
            var person = new Person { Name = "Jack", BarList = { new Bar<string> { Id = 1, Value = "One" }, new Bar<string> { Id = 2, Value = "Two" } } };

            var personMapped = Mapper.Map<PersonModel>(person);

            var barModel = (BarModel<string>)personMapped.BarList[0];
            barModel.DerivedMember.ShouldBe("1");
            barModel.MappedFrom.ShouldBe("mappedFrom");
        }
    }
}