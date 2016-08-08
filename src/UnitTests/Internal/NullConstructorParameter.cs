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
        }

        class Parameter : ParameterInfo
        {
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());

        [Fact]
        public void Should_work()
        {
            var typeMap = Configuration.GetAllTypeMaps().Single();
            var constructorMap = new ConstructorMap(typeof(Source).GetConstructor(Type.EmptyTypes), typeMap);
            constructorMap.AddParameter(new Parameter(), new MemberInfo[0], true);
            typeMap.ConstructorMap = constructorMap;
            typeMap.ConstructorParameterMatches("");
        }
    }
}