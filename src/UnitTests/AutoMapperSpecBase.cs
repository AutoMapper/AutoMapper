using System;
using Xunit;

namespace AutoMapper.UnitTests
{
    using QueryableExtensions;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public abstract class AutoMapperSpecBase : NonValidatingSpecBase
    {
        [Fact]
        public void Should_have_valid_configuration()
        {
            Configuration.AssertConfigurationIsValid();
        }

    }

    public abstract class NonValidatingSpecBase : SpecBase
    {
        private IMapper mapper;

        protected abstract MapperConfiguration Configuration { get; }
        protected IConfigurationProvider ConfigProvider => Configuration;

        protected IMapper Mapper => mapper ??= Configuration.CreateMapper();

        protected TDestination Map<TDestination>(object source) => Mapper.Map<TDestination>(source);

        protected IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, object parameters = null, params Expression<Func<TDestination, object>>[] membersToExpand) => 
            Mapper.ProjectTo(source, parameters, membersToExpand);

        protected IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand) =>
            Mapper.ProjectTo<TDestination>(source, parameters, membersToExpand);
    }

    public abstract class SpecBaseBase
    {
        protected virtual void MainSetup()
        {
            Establish_context();
            Because_of();
        }

        protected virtual void MainTeardown()
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
    }
    public abstract class SpecBase : SpecBaseBase, IDisposable
    {
        protected SpecBase()
        {
            Establish_context();
            Because_of();
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
        public static void Assert(IQueryable queryable, int count) => Assert(queryable.Expression, count);
        public static void Assert(Expression expression, int count)
        {
            var firstOrDefault = new FirstOrDefaultCounter();
            firstOrDefault.Visit(expression);
            firstOrDefault.Count.ShouldBe(count);
        }
    }
}