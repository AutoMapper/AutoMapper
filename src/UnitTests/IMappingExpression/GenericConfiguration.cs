using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.UnitTests.IMappingExpression
{
    public class GenericConfiguration
    {
        class Source
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
        class Destination
        {
            public string Name { get; set; }
            public InnerDestination InnerDestination { get; set; }
        }
        class InnerDestination
        {
            public int Value { get; set; }
        }

        [Fact]
        public void MapperField()
        {
            var source = new Source() { Name = "Foo", Value = 100 };
            var mapping = new GenericTypeMap<Source, Destination>().Map(source);

            Assert.Equal(source.Value, mapping.InnerDestination.Value);
        }
    }
    public class GenericTypeMap<TSource, TDestination>
    {
        class FieldMapper
        {
            public string Source { get; set; }
            public string Destination { get; set; }
        }

        private MapperConfiguration Configuration => new MapperConfiguration(x =>
        {
            var map = x.CreateMap<TSource, TDestination>();

            var dbMapperProperties = new List<FieldMapper>
                {
                    new FieldMapper{ Source = "Value", Destination = "InnerDestination.Value" },
                };

            foreach (var propertyMap in dbMapperProperties)
            {
                var peSource = Expression.Parameter(typeof(TSource), "src");
                var sourceExpression = Expression.Property(peSource, propertyMap.Source);
                var sourceMapFromExpression =
                     Expression.Lambda<Func<TSource, object>>(
                         Expression.Convert(sourceExpression, typeof(object)), new ParameterExpression[] { peSource });

                var peDestination = Expression.Parameter(typeof(TDestination), "dest");

                Expression destinationExpression = peDestination;
                foreach (var navigatinoPropertieDestination in propertyMap.Destination.Split('.'))
                    destinationExpression = Expression.Property(destinationExpression, navigatinoPropertieDestination);

                Expression<Func<TDestination, object>> destinationMapFromExpression =
                       Expression.Lambda<Func<TDestination, object>>
                           (Expression.Convert(destinationExpression, typeof(object)), new ParameterExpression[] { peDestination });


                map.ForPath(destinationMapFromExpression, a => a.MapFrom(sourceMapFromExpression));
            }
        });

        public TDestination Map(TSource source) =>
            Configuration.CreateMapper().Map<TDestination>(source);
    }

}
