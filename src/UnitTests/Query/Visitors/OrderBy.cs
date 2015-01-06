using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.QueryableExtensions.Impl.QueryMapper;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Query.Visitors
{
    public class OrderByVisitor : AutoMapperSpecBase
    {
        private MethodInfo _orderByMethodInfo;
        private QueryMapperVisitor _rootVisitor;
        private MethodCallExpression _orderByExpression;
        private MethodCallExpression _convertedOrderByExpression;

        private class Source
        {
            public string Name { get; set; }
        }

        private class Dest
        {
            public Dest(int id)
            {
                Id = id;
            }

            public int Id { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Name))
                .ReverseMap()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Id));


            // Build expression: sourceQuery.OrderBy<Source, string>(s => s.Name)
            var sourceQuery = Expression.Constant(new Source[0].AsQueryable());
            Expression<Func<Source, string>> orderByNameStatement = s => s.Name;

            _rootVisitor = new QueryMapperVisitor(typeof(Source), typeof(Dest), new Dest[0].AsQueryable(),
                Mapper.Engine);
            var typeArguments = new Type[] { typeof(Source), typeof(string) };

            _orderByExpression = Expression.Call(typeof(Queryable), "OrderBy", typeArguments,
                sourceQuery, orderByNameStatement);
        }

        protected override void Because_of()
        {
            var orderByConverter = new OrderByConverter(_rootVisitor);
            _convertedOrderByExpression = orderByConverter.Convert(_orderByExpression);
        }

        [Fact]
        public void Should_replace_order_method_generic_arguments()
        {
            _convertedOrderByExpression.Method.GetGenericArguments()[0].ShouldEqual(typeof(Dest));
            _convertedOrderByExpression.Method.GetGenericArguments()[1].ShouldEqual(typeof(int));
        }
    }
}
