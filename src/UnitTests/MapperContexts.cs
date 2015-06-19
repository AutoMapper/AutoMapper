namespace AutoMapper.UnitTests
{
    using Internal;
    using Should;
    using Xunit;

    // ReSharper disable UseNullPropagation
    namespace MapperContexts
    {
        /// <summary>
        /// As long as the previously existing unit and integration tests are working successfully,
        /// and are verifying valid scenarios, then we should be able to ascertain separation between
        /// context instances themselves. That should be sufficient to demonstrate contexts are indeed
        /// separate. That's not counting any expression oddities that might arise, but we will not
        /// concern ourselves with situations like that.
        /// </summary>
        public class Mapper_context_factory : AutoMapperSpecBase
        {
            /// <summary>
            /// We will use a factory throughout the unit test.
            /// </summary>
            private IMapperContextFactory _factory;

            /// <summary>
            /// We will monitor a previous context instance.
            /// </summary>
            private IMapperContext _previousContext;

            /// <summary>
            /// We will monitor a current context instance.
            /// </summary>
            private IMapperContext _currentContext;

            /// <summary>
            /// Prepares the factory.
            /// </summary>
            protected override void Establish_context()
            {
                base.Establish_context();

                // Will use this throughout the spec.
                _factory = PlatformAdapter.Resolve<IMapperContextFactory>();
                // This fact is confirmed later on.
            }

            /// <summary>
            /// This is the simple because of preparing the spec.
            /// </summary>
            protected override void Because_of()
            {
                base.Because_of();

                // Just verify we started from a known state.
                _currentContext.ShouldBeNull();
                _previousContext.ShouldBeNull();
            }

            public override void Should_have_valid_configuration()
            {
                base.Should_have_valid_configuration();

                _factory
                    .ShouldNotBeNull()
                    .ShouldBeInstanceOf<MapperContextFactory>();
            }

            /// <summary>
            /// Verifies that factories should create different instances.
            /// </summary>
            [Fact]
            public void Should_create_different_context_instances()
            {
                // First iteration from Null (non-initialized, cold) state.
                _currentContext.ShouldBeNull();

                // Second iteration from non-Null (initialized, warm) state.
                while ((_previousContext = _currentContext) == null)
                {
                    _currentContext = _factory.CreateMapperContext();

                    Verify_simple_spec(_previousContext);
                    Verify_simple_spec(_currentContext);

                    Verify_compares_with(_currentContext, _previousContext);
                }

                // For sanity.
                _previousContext.ShouldNotBeNull();
            }

            /// <summary>
            /// Verifies that the context is sufficiently built up within and throughout itself.
            /// </summary>
            /// <param name="mapperContext"></param>
            private static void Verify_simple_spec(IMapperContext mapperContext)
            {
                if (mapperContext == null) return;

                mapperContext.AssertConfigurationIsValid();

                // Do not lose sight of the static instance, even here:
                mapperContext.ShouldNotBeSameAs(Mapper.Context);

                mapperContext.Engine.ShouldMeet(engine =>
                {
                    engine.ShouldNotBeNull();
                    engine.ConfigurationProvider.ShouldNotBeNull();
                    engine.ExpressionCache.ShouldNotBeNull();
                    engine.Runner.ShouldNotBeNull();
                });

                mapperContext.Configuration.ShouldMeet(cfg =>
                {
                    cfg.ObjectMappers.ShouldNotBeNull();
                });

                mapperContext.ConfigurationProvider.ShouldMeet(provider =>
                {
                    provider.ServiceCtor.ShouldNotBeNull();
                });

                mapperContext.ObjectMappers.ShouldMeet(mappers =>
                {
                    mappers.ShouldNotBeNull();
                });

                mapperContext.Runner.ShouldMeet(runner =>
                {
                    runner.ShouldNotBeNull();
                });

                //TODO: may want to focus these interfaces to minimize the exposure...
                mapperContext.ShouldMeet(context =>
                {
                    context.Engine.ConfigurationProvider.ShouldBeSameAs(context.ConfigurationProvider);
                    context.Engine.Runner.ShouldBeSameAs(context.Runner);
                    context.Configuration.ObjectMappers.ShouldBeSameAs(context.ObjectMappers);
                });
            }

            /// <summary>
            /// We are already checking for internal consistency via <see cref="Verify_simple_spec"/>.
            /// This is sufficient to ensure that there is separation between contexts.
            /// </summary>
            /// <param name="currentContext"></param>
            /// <param name="previousContext"></param>
            private static void Verify_compares_with(IMapperContext currentContext, IMapperContext previousContext)
            {
                currentContext.ShouldNotBeSameAs(previousContext);

                // Nothing further to compare in this instance.
                if (previousContext == null) return;

                // By logical connection will not need to drill into the instances any further.
                currentContext.ShouldCompareWith(previousContext, (a, b) =>
                {
                    a.ShouldNotBeSameAs(b);

                    // These are the root instances.
                    a.ConfigurationProvider.ShouldNotBeSameAs(b.ConfigurationProvider);
                    a.Configuration.ShouldNotBeSameAs(b.Configuration);
                    a.ObjectMappers.ShouldNotBeSameAs(b.ObjectMappers);
                    a.Engine.ShouldNotBeSameAs(b.Engine);
                    a.Runner.ShouldNotBeSameAs(b.Runner);
                });
            }

            /// <summary>
            /// Cleans up after the tests have run.
            /// </summary>
            protected override void Cleanup()
            {
                // Need to dance around a couple of facts with slightly different states involved.
                if (_currentContext != null) _currentContext.Reset();

                // May have one instance and not the other depending on the facts being verified.
                if (_previousContext != null) _previousContext.Reset();

                //TODO: might consider whether Disposable is a good thing to want to run...
                _currentContext = null;
                _previousContext = null;

                _factory = null;

                base.Cleanup();
            }
        }
    }
}