using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Should;

namespace AutoMapper.UnitTests.Projection
{
    namespace PrimitiveArrays
    {
        using QueryableExtensions;

        public class PrimitiveArraysExpressionTest
        {
            [Fact]
            public void Should_not_fail()
            {
                Mapper.CreateMap<Source, Destination>();

                typeof(NullReferenceException).ShouldNotBeThrownBy(() => Mapper.Engine.CreateMapExpression<Source, Destination>());
            }

            [Fact]
            public void Should_map_values()
            {
                Mapper.CreateMap<Source, Destination>();

                var sources = new List<Source>
                {
                    new Source
                    {
                        Value1 = new byte[] {1, 2, 3},
                        Value2 = new[] {4, 5, 6}
                    }
                };

                var expr = sources.AsQueryable().ProjectTo<Destination>();

                var result = expr.ToList();

                result[0].Value1.ShouldEqual(sources[0].Value1);
                result[0].Value2.ShouldEqual(sources[0].Value2);
            }
        }
        public class Source
        {
            public byte[] Value1 { get; set; }
            public int[] Value2 { get; set; }
        }

        public class Destination
        {
            public byte[] Value1 { get; set; }
            public int[] Value2 { get; set; }
        }
    }
}