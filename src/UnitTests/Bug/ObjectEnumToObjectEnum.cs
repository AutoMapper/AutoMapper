using System;
using Should;
using AutoMapper.Mappers;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class ObjectEnumToObjectEnum : AutoMapperSpecBase
    {
        MappingEngine _mapper;
        Target _target;

        public enum SourceEnumValue
        {
            Donkey,
            Mule
        }

        public enum TargetEnumValue
        {
            Donkey,
            Mule
        }

        public class Source
        {
            public object Value { get; set; }
        }

        public class Target
        {
            public object Value { get; set; }
        }

        protected override void Establish_context()
        {
            var configuration = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            _mapper = new MappingEngine(configuration);
            var parentMapping = configuration.CreateMap<Source, Target>();
            parentMapping.ForMember(dest => dest.Value, opt => opt.MapFrom(s => (TargetEnumValue)s.Value));
        }

        protected override void Because_of()
        {
            _target = _mapper.Map<Target>(new Source { Value = SourceEnumValue.Mule });
        }

        [Fact]
        public void Should_be_enum()
        {
            _target.Value.ShouldBeType<TargetEnumValue>();
        }
    }
}