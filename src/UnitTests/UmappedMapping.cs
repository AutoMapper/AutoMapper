using Should.Core.Assertions;

namespace AutoMapper.UnitTests
{
    using System.Linq;
    using Should;
    using Xunit;
    using QueryableExtensions;

    public class UmappedMapping
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        [Fact]
        public void Can_not_map_unmapped_type()
        {
            Mapper.Initialize(cfg =>
            {
            });

            var source = new Source {Value = 5};

            Mapper.Map<Source, Dest>(source);
        } 
    }
}