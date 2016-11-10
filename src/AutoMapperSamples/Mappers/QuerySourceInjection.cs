using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using NUnit.Framework;
using Should;

namespace AutoMapperSamples.Mappers
{
    namespace QuerySourceInjection
    {
        [TestFixture]
        public class SimpleExample
        {
            readonly Source[] _source = new[]
                    {
                        new Source {SrcValue = 5},
                        new Source {SrcValue = 4},
                        new Source {SrcValue = 7}
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
            public void Select_source_items_from_destination()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Destination, Source>()
                        .ForMember(d => d.SrcValue, opt => opt.MapFrom(s => s.DestValue))
                        .ReverseMap();
                });
                var mapper = config.CreateMapper();

                IQueryable<Destination> result = _source.AsQueryable()
                    .UseAsDataSource(mapper).For<Destination>()
                    .Where(s => s.DestValue > 6);

                result.Count().ShouldEqual(1);
                result.First().GetType().ShouldEqual(typeof(Destination));
            }

            [Test]
            public void Select_source_items_from_destination_with_explicit_mapping()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Destination, Source>()
                        .ForMember(d => d.SrcValue, opt => opt.MapFrom(s => s.DestValue))
                        .ReverseMap()
                        .ForMember(d => d.DestValue, opt => opt.MapFrom(s => s.SrcValue))
                        ;
                });

                IQueryable<Destination> result = _source.AsQueryable()
                    .UseAsDataSource(Mapper.Configuration).For<Destination>()
                    .Where(s => s.DestValue > 6);

                result.Count().ShouldEqual(1);
                result.First().GetType().ShouldEqual(typeof(Destination));
            }
        }
    }
}
