using Should;
using NUnit.Framework;
#if !SILVERLIGHT
using Rhino.Mocks;
#endif

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

    public abstract class SpecBaseBase
    {
        public virtual void MainSetup()
        {
            Establish_context();
            Because_of();
        }

        public virtual void MainTeardown()
        {
            Cleanup();
        }

        protected virtual void Establish_context()
        {
        }

        protected virtual void Because_of()
        {
        }

        protected virtual void Cleanup()
        {
        }

#if !SILVERLIGHT
        protected TType CreateDependency<TType>()
            where TType : class
        {
            return MockRepository.GenerateMock<TType>();
        }

        protected TType CreateStub<TType>() where TType : class
        {
            return MockRepository.GenerateStub<TType>();
        }
#endif
    }

    [TestFixture]
    public abstract class SpecBase : SpecBaseBase
    {
        [TestFixtureSetUp]
        public override void MainSetup()
        {
            base.MainSetup();
        }

        [TestFixtureTearDown]
        public override void MainTeardown()
        {
            base.MainTeardown();
        }
    }

}

