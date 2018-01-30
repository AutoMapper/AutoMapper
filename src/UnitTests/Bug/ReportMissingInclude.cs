using Xunit;
using Shouldly;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class ReportMissingInclude
    {
        [Fact]
        public void ShouldDiscoverMissingMappingsInIncludedType()
        {
            new Action(()=>new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<object, BaseType>().Include<object, ChildType>();
            })).ShouldThrowException<InvalidOperationException>(ex=>ex.Message.ShouldBe("You cannot include a type map into itself."));
        }

        public class BaseType { }

        public class ChildType : BaseType
        {
            public string Value { get; set; }
        }
    }
}