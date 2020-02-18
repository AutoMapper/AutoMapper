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
            cfg.Advanced.QueryableBinders.Add(new EnumToUnderlyingTypeBinder());
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
            public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) =>
                Bind(propertyMap.DestinationMember, Convert(result.ResolutionExpression, propertyMap.DestinationType));

            public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) =>
                propertyMap.SourceType.GetTypeInfo().IsEnum && Enum.GetUnderlyingType(propertyMap.SourceType) == propertyMap.DestinationType;
        }
    }

    public class QueryableResultConverters : AutoMapperSpecBase
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
            cfg.Advanced.QueryableResultConverters.Insert(0, new EnumToUnderlyingTypeResultConverter());
            cfg.CreateMap<Source, Destination>();
        });

        [Fact]
        public void Should_work_with_projections()
        {
            var destination = new[] { new Source { Color = ConsoleColor.Cyan } }.AsQueryable().ProjectTo<Destination>(Configuration).First();
            destination.Color.ShouldBe(11);
        }

        private class EnumToUnderlyingTypeResultConverter : IExpressionResultConverter
        {
            public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap) =>
                propertyMap.SourceType.GetTypeInfo().IsEnum && Enum.GetUnderlyingType(propertyMap.SourceType) == propertyMap.DestinationType;

            public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap, LetPropertyMaps letPropertyMaps) =>
                new ExpressionResolutionResult(
                    Convert(MakeMemberAccess(expressionResolutionResult.ResolutionExpression, propertyMap.SourceMember), propertyMap.DestinationType), 
                    propertyMap.DestinationType);
        }
    }
}
