using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    using System;

    [TestFixture]
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

        // https://github.com/AutoMapper/AutoMapper/issues/154 
        [Test, Ignore("Until fixed")]
        public void Should_pick_a_ctor_which_best_matches()
        {
            Mapper.CreateMap<SourceClass, DestinationClass>();

            var source = new SourceClass();

            Mapper.Map<DestinationClass>(source);
        }
    }
}