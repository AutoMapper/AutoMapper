using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class NullConstructorParameter : AutoMapperSpecBase
    {
        class Source
        {
        }
        class Destination
        {
            public Destination()
            {
            }

            public Destination(int _)
            {
            }
        }

        class Parameter : ParameterInfo
        {
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());

        [Fact]
        public void Should_work()
        {
            var typeMap = Configuration.GetAllTypeMaps().Single();
            typeMap.ConstructorMap.AddParameter(new Parameter(), new MemberInfo[0], true);
            typeMap.ConstructorParameterMatches("");
        }
    }
}