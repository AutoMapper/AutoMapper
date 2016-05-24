namespace AutoMapper.UnitTests
{
    using Configuration;
    using Should;
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
        public SeparateConfiguration()
        {
            var expr = new MapperConfigurationExpression();

            expr.CreateMap<Source, Dest>();

            var configuration = new MapperConfiguration(expr);

            Configuration = configuration;
        }

        protected override MapperConfiguration Configuration { get; }

        [Fact]
        public void Should_use_passed_in_configuration()
        {
            var source = new Source {Value = 5};
            var dest = Mapper.Map<Source, Dest>(source);

            dest.Value.ShouldEqual(source.Value);
        }
    }
}