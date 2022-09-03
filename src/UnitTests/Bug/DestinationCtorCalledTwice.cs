namespace AutoMapper.UnitTests.Bug
{
    namespace DestinationCtorCalledTwice
    {
        public class Bug
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

                var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());
                config.CreateMapper().Map(source, dest);

                Destination.CallCount.ShouldBe(1);
            }
        }
    }
}