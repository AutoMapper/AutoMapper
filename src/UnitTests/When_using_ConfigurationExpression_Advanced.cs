using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class When_using_ConfigurationExpression_Advanced : AutoMapperSpecBase
    {
        // Nullable so can see a false state
        private static bool? _sealed;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PlaceHolder, PlaceHolder>();
            cfg.Advanced.BeforeSeal(SetSealed);
        });

        private static void SetSealed(IConfigurationProvider cfg)
        {
            // If sealed _typeMapCache is filled and should be able to Resolve tye TypeMap
            // If not sealed _typeMapCache is empty and should return null
            _sealed = cfg.FindTypeMapFor(typeof(PlaceHolder), typeof(PlaceHolder)) != null;
        }

        [Fact]
        public void BeforeSeal_should_be_called_before_Seal()
        {
            _sealed.ShouldEqual(false);

            // Prove that sealed actualy seals
            SetSealed(Configuration);
            _sealed.ShouldEqual(true);
        }

        public class PlaceHolder { }
    }
}