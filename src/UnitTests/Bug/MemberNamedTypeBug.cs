using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    using System;
    using Should;

    public class CorrectCtorIsPickedOnDestinationType : AutoMapperSpecBase
    {
        public class SourceClass { }

        public class DestinationClass
        {
            public DestinationClass() { }

            // Since the name of the parameter is 'type', Automapper.TypeMapFactory chooses SourceClass.GetType()
            // to fulfill the dependency, causing an InvalidCastException during Mapper.Map()
            public DestinationClass(Int32 type)
            {
                Type = type;
            }

            public Int32 Type { get; private set; }
        }
        
        [Fact]
        public void Should_pick_a_ctor_which_best_matches()
        {
            Mapper.CreateMap<SourceClass, DestinationClass>();

            var source = new SourceClass();

            Mapper.Map<DestinationClass>(source);
        }
    }
    public class MemberNamedTypeWrong : AutoMapperSpecBase
    {
        public class SourceClass
        {
            public string Type { get; set; }
        }

        public class DestinationClass
        {
            public string Type { get; set; }
        }

        [Fact]
        public void Should_map_correctly()
        {
            Mapper.CreateMap<SourceClass, DestinationClass>();

            var source = new SourceClass
            {
                Type = "Hello"
            };

            var result = Mapper.Map<DestinationClass>(source);
            result.Type.ShouldEqual(source.Type);
        }
    }
}