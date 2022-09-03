namespace AutoMapper.UnitTests;

using System.Linq;
using Shouldly;
using Xunit;
using QueryableExtensions;
using System;

public class MissingMapping : AutoMapperSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(c => { });

    [Fact]
    public void Can_not_map_unmapped_type()
    {
        new Action(() => Mapper.Map<Source, Dest>(new Source())).ShouldThrow<AutoMapperMappingException>();
    } 
}