﻿using System;
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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
            public Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps) =>
                Convert(resolvedSource, memberMap.DestinationType);
            public bool IsMatch(MemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) =>
                memberMap.SourceType.IsEnum && Enum.GetUnderlyingType(memberMap.SourceType) == memberMap.DestinationType;
        }
    }
}