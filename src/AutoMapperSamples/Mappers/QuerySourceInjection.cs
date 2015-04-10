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
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Destination, Source>()
                        .ForMember(d => d.SrcValue, opt => opt.MapFrom(s => s.DestValue))
                        .ReverseMap();
                });

                IQueryable<Destination> result = _source.AsQueryable()
                    .UseAsDataSource().For<Destination>()
                    .Where(s => s.DestValue > 6);

                result.Count().ShouldEqual(1);
                result.First().GetType().ShouldEqual(typeof(Destination));
            }
        }
    }
}
