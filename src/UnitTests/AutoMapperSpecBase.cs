using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
    public class AutoMapperSpecBase : NonValidatingSpecBase
	{
        [Test]
        public void Should_have_valid_configuration()
        {
            Mapper.AssertConfigurationIsValid();
        }
    }

    public class NonValidatingSpecBase : SpecBase
    {
        protected override void Cleanup()
        {
            Mapper.Reset();
        }

    }
}