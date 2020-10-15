using System;
using AutoMapper.QueryableExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.QueryableExtensions.Impl;
using AutoMapper.Internal;

namespace AutoMapper.UnitTests.Projection
{
    using static Expression;

    public class QueryableBinders : AutoMapperSpecBase
    {
        class Source
        {
            public ConsoleColor Color { get; set; }
        }

        class Destination
        {
            public int Color { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.Internal().QueryableBinders.Add(new EnumToUnderlyingTypeBinder());
            cfg.CreateMap<Source, Destination>();
        });

        [Fact]
        public void Should_work_with_projections()
        {
            var destination = new[] { new Source { Color = ConsoleColor.Cyan } }.AsQueryable().ProjectTo<Destination>(Configuration).First();
            destination.Color.ShouldBe(11);
        }

        private class EnumToUnderlyingTypeBinder : IExpressionBinder
        {
            public Expression Build(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ExpressionRequest request, ExpressionResolutionResult resolvedSource, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) =>
                Convert(resolvedSource.ResolutionExpression, memberMap.DestinationType);

            public bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, ExpressionResolutionResult resolvedSource) =>
                memberMap.SourceType.GetTypeInfo().IsEnum && Enum.GetUnderlyingType(memberMap.SourceType) == memberMap.DestinationType;
        }
    }
}