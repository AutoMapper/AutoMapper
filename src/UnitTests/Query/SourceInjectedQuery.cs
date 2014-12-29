using System;
using System.Collections.Generic;
using System.Linq;
using Should;
using AutoMapper.QueryableExtensions;
using Xunit;

namespace AutoMapper.UnitTests.Query
{
    public class SourceInjectedQuery : AutoMapperSpecBase
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

        protected override void Establish_context()
        {
            Mapper.CreateMap<Destination, Source>()
                       .ForMember(s => s.SrcValue, opt => opt.MapFrom(d => d.DestValue))
                       .ReverseMap()
                       .ForMember(d => d.DestValue, opt => opt.MapFrom(s => s.SrcValue));
        }

        [Fact]
        public void Shoud_support_const_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
              .UseAsDataSource().For<Destination>()
              .Where(s => s.DestValue > 6);

            result.Count().ShouldEqual(1);
            result.Any(s => s.DestValue > 6).ShouldBeTrue();
        }

        [Fact]
        public void Shoud_use_destination_elementType()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource().For<Destination>();

            result.ElementType.ShouldEqual(typeof(Destination));
            
            result = result.Where(s => s.DestValue > 3);
            result.ElementType.ShouldEqual(typeof(Destination));
        }

        [Fact]
        public void Shoud_support_single_item_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource().For<Destination>();

            result.First(s => s.DestValue > 6).ShouldBeType<Destination>();
        }

        [Fact]
        public void Shoud_support_IEnumerable_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
              .UseAsDataSource().For<Destination>()
              .Where(s => s.DestValue > 6);

            List<Destination> list = result.ToList();
        }

        [Fact]
        public void Shoud_convert_source_item_to_destination()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource().For<Destination>();

            var destItem = result.First(s => s.DestValue == 7);
            var sourceItem = _source.First(s => s.SrcValue == 7);

            destItem.DestValue.ShouldEqual(sourceItem.SrcValue);
        }

        [Fact]
        public void Shoud_support_order_by_statement_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
              .UseAsDataSource().For<Destination>()
              .OrderByDescending(s => s.DestValue);

            result.First().DestValue.ShouldEqual(_source.Max(s => s.SrcValue));
        }
    }
}
