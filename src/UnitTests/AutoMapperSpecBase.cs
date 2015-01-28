using System;
using Should;
using Xunit;
#if !SILVERLIGHT && !NETFX_CORE
using Rhino.Mocks;
#endif

namespace AutoMapper.UnitTests
{
    public class AutoMapperSpecBase : NonValidatingSpecBase
    {
        [Fact]
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

#if !SILVERLIGHT && !NETFX_CORE
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
    public abstract class SpecBase : SpecBaseBase, IDisposable
    {
        protected SpecBase()
        {
            MainSetup();
        }

        public void Dispose()
        {
            MainTeardown();
        }
    }

}

