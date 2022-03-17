﻿namespace AutoMapper.UnitTests
{
    using Configuration;
    using Shouldly;
    using Xunit;

    public class SeparateConfiguration : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }
        public class Dest
        {
            public int Value { get; set; }
        }
        protected override MapperConfiguration CreateConfiguration()
        {
            var expr = new MapperConfigurationExpression();

            expr.CreateMap<Source, Dest>();

            return new MapperConfiguration(expr);
        }

        [Fact]
        public void Should_use_passed_in_configuration()
        {
            var source = new Source {Value = 5};
            var dest = Mapper.Map<Source, Dest>(source);

            dest.Value.ShouldBe(source.Value);
        }
    }
}