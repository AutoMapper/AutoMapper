using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class When_using_ConfigurationExpression_Advanced : AutoMapperSpecBase
    {
        private static bool _sealed;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PlaceHolder, PlaceHolder>();
            cfg.Advanced.BeforeSeal = _ => SetSealed(_);
        });

        private static bool SetSealed(IConfigurationProvider _)
        {
            return _sealed = _.FindTypeMapFor(typeof(PlaceHolder), typeof(PlaceHolder)) != null;
        }

        [Fact]
        public void BeforeSeal_should_be_called_before_Seal()
        {
            _sealed.ShouldBeFalse();
            // Prove that sealed actualy seals
            SetSealed(Configuration);
            _sealed.ShouldBeTrue();
        }

        public class PlaceHolder { }
    }
}