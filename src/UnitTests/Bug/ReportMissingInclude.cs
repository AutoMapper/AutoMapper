using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class ReportMissingInclude : NonValidatingSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<object, BaseType>().Include<object, ChildType>();
        });

        [Fact]
        public void ShouldDiscoverMissingMappingsInIncludedType()
        {
            new Action(Configuration.AssertConfigurationIsValid).ShouldThrow<InvalidOperationException>(ex=>ex.Message.ShouldStartWith("Missing map from Object to BaseType."));
        }

        public class BaseType { }

        public class ChildType : BaseType
        {
            public string Value { get; set; }
        }
    }
}