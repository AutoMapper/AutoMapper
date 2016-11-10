using Xunit;
using Should;
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
            })).ShouldThrow<InvalidOperationException>(ex=>ex.Message.ShouldStartWith("Missing map from Object to ChildType."));
        }

        public class BaseType { }

        public class ChildType : BaseType
        {
            public string Value { get; set; }
        }
    }
}