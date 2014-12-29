using System;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Query
{
    public class IntTypeConverter : TypeConverter<string, int>
    {
        protected override int ConvertCore(string source)
        {
            if (source == null)
                throw new AggregateException("null string value cannot convert to non-nullable return type.");
            else
                return Int32.Parse(source);
        }
    }

    public class PropertyConvert : AutoMapperSpecBase
    {
        private Dest[] _destList;

        class Source
        {
            public string Name { get; set; }
        }

        class Dest
        {
            public Dest(int id)
            {
                Id = id;
            }
            public int Id { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<string, int>().ConvertUsing<IntTypeConverter>();
            Mapper.CreateMap<Source, Dest>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Name))
                .ReverseMap()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Id));

        }

        protected override void Because_of()
        {
            _destList = new[]
            {
                new Dest(1), 
                new Dest(2),
                new Dest(3), 
                new Dest(4),
                new Dest(5),
                new Dest(6), 
            };
        }

        [Fact]
        public void Should_convert_binary_operations()
        {
            var query = new Source[0].AsQueryable()
                .UseAsDataSource().For<Source>()
                .Where(s => s.Name == "2")
                .Map<Source, Dest>(_destList.AsQueryable());

            query.Count().ShouldEqual(1);
            query.First().Id.ShouldEqual(2);
        }

        [Fact]
        public void Should_convert_lambda_return_value()
        {
            var query = new Source[0].AsQueryable()
                .UseAsDataSource().For<Source>()
                .OrderByDescending(s => s.Name)
                .Map<Source, Dest>(_destList.AsQueryable());

            query.Count().ShouldEqual(6);
            query.First().Id.ShouldEqual(6);
        }
    }

}
