using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using NUnit.Framework;
using Shouldly;

namespace AutoMapperSamples.Mappers
{
    namespace Query
    {
        [TestFixture]
        public class SimpleExample
        {
            readonly Destination[] _destinations = new[]
                    {
                        new Destination {DestValue = 5},
                        new Destination {DestValue = 4},
                        new Destination {DestValue = 7}
                    };

            public class Source
            {
                public int SrcValue { get; set; }
            }

            public class Destination
            {
                public int DestValue { get; set; }
            }

            [Test]
            public void Select_destinations_items_with_query_by_source_items()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ForMember(d => d.DestValue, opt => opt.MapFrom(s => s.SrcValue));
                });

               
                IQueryable<Destination> detsResult = new Source[0]
                    .AsQueryable()
                    .Where(s => s.SrcValue > 6)
                    .Map(_destinations.AsQueryable(), config);

                detsResult.Count().ShouldBe(1);
                detsResult.First().GetType().ShouldBe(typeof(Destination));
            }

            [Test]
            public void Select_source_items_from_destinations()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ForMember(d => d.DestValue, opt => opt.MapFrom(s => s.SrcValue))
                        .ReverseMap(); // reverse map added
                });

                IQueryable<Source> sourceResult = new Source[0]
                    .AsQueryable()
                    .Where(s => s.SrcValue > 6)
                    .Map<Source, Destination>(_destinations.AsQueryable(), config)
                    .ProjectTo<Source>(config); // projection added

                sourceResult.Count().ShouldBe(1);
                sourceResult.First().GetType().ShouldBe(typeof(Source));
            }
        }
    }
}
