using System;
using AutoMapper.QueryableExtensions;
using System.Linq;
using Xunit;
using Shouldly;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.QueryableExtensions.Impl;
using AutoMapper.Internal;

namespace AutoMapper.UnitTests.Projection
{
    using static Expression;

    public class ProjectionMappers : AutoMapperSpecBase
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
            cfg.Internal().ProjectionMappers.Add(new EnumToUnderlyingTypeProjectionMapper());
            cfg.CreateProjection<Source, Destination>();
        });

        [Fact]
        public void Should_work_with_projections()
        {
            var destination = new[] { new Source { Color = ConsoleColor.Cyan } }.AsQueryable().ProjectTo<Destination>(Configuration).First();
            destination.Color.ShouldBe(11);
        }
        private class EnumToUnderlyingTypeProjectionMapper : IProjectionMapper
        {
            public Expression Project(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps) =>
                Convert(resolvedSource, memberMap.DestinationType);
            public bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) =>
                memberMap.SourceType.IsEnum && Enum.GetUnderlyingType(memberMap.SourceType) == memberMap.DestinationType;
        }
    }
}