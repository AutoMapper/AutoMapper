using Xunit;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    namespace DestinationCtorCalledTwice
    {
        public class Bug : AutoMapperSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }
            public class Destination
            {
                private static int _callCount = 0;
                
                public Destination()
                {
                    _callCount++;
                }

                public int Value { get; set; }
                public static int CallCount { get { return _callCount; } }

                public static void Reset()
                {
                    _callCount = 0;
                }
            }

            public Bug()
            {
                Destination.Reset();
            }

            [Fact]
            public void Should_call_ctor_once()
            {
                var source = new Source {Value = 5};
                var dest = new Destination {Value = 7};

                Mapper.Map(source, dest, opt => opt.CreateMissingTypeMaps = true);

                Destination.CallCount.ShouldEqual(1);
            }
        }
    }
}