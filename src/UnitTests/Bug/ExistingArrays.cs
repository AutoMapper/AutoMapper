using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class ExistingArrays : AutoMapperSpecBase
    {
        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>();
        }

        [Fact]
        public void should_map_array_inside_object()
        {
            var source = new Source { Values = new[] { "1", "2" } };
            var dest = Mapper.Map<Dest>(source);
        }

        public class Source
        {
            public Source()
            {
                Values = new string[0];
            }

            public string[] Values { get; set; }
        }

        public class Dest
        {
            public Dest()
            {
                // remove this line will get it fixed. 
                Values = new string[0];
            }

            public string[] Values { get; set; }
        }
    }
}