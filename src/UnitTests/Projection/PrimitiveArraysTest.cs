namespace AutoMapper.UnitTests.Projection
{
    namespace PrimitiveArrays
    {
        public class PrimitiveArraysExpressionTest
        {
            [Fact]
            public void Should_not_fail()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateProjection<Source, Destination>();
                });

                typeof(NullReferenceException).ShouldNotBeThrownBy(() => config.Internal().ProjectionBuilder.GetMapExpression<Source, Destination>());
            }

            [Fact]
            public void Should_map_values()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateProjection<Source, Destination>();
                });

                var sources = new List<Source>
                {
                    new Source
                    {
                        Value1 = new byte[] {1, 2, 3},
                        Value2 = new[] {4, 5, 6}
                    }
                };

                var expr = sources.AsQueryable().ProjectTo<Destination>(config);

                var result = expr.ToList();

                result[0].Value1.ShouldBe(sources[0].Value1);
                result[0].Value2.ShouldBe(sources[0].Value2);
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