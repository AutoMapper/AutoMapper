using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class ReportMissingInclude : AutoMapperSpecBase
    {
        protected override void Because_of()
        {
            Mapper.Initialize(c =>
            {
                c.CreateMap<object, BaseType>().Include<object, ChildType>();
            });
        }

        [Fact]
        public void ShouldDiscoverMissingMappingsInIncludedType()
        {
            new Action(Mapper.AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>();
        }

        public class BaseType { }

        public class ChildType : BaseType
        {
            public string Value { get; set; }
        }
    }
}