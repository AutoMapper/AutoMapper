using AutoMapper.QueryableExtensions;
using AutoMapper.XpressionMapper.Extensions;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class XpressionMapperStructsTests
    {
        [Fact]
        public void Can_map_value_types_constants_with_instance_methods()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<GarageModel, Garage>()
                    .ReverseMap()
                        .ForMember(d => d.Color, opt => opt.MapFrom(s => s.Truck.Color));
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            List<Garage> source = new List<Garage> {
                new Garage { Truck = default }
            };

            //Act
            var output1 = source.AsQueryable().GetItems<GarageModel, Garage>(mapper, q => q.Truck.Equals(default(TruckModel)));
            var output2 = source.AsQueryable().Query<GarageModel, Garage, GarageModel, Garage>(mapper, q => q.First());
            output1.First().Truck.ShouldBe(default);
            output2.Truck.ShouldBe(default);
        }

        [Fact]
        public void Can_convert_return_type()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<GarageModel, Garage>()
                    .ReverseMap()
                        .ForMember(d => d.Color, opt => opt.MapFrom(s => s.Truck.Color));
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            List<Garage> source = new List<Garage> {
                new Garage { Truck = new Truck { Color = "blue", Year = 2000 } }
            };

            //Act
            var output1 = source.AsQueryable().GetItems<GarageModel, Garage>(mapper, q => q.Truck.Year == 2000).Select(g => g.Truck);
            var output2 = source.AsQueryable().Query<GarageModel, Garage, TruckModel, Truck>(mapper, q => q.Select(g => g.Truck).First());

            output1.First().Year.ShouldBe(2000);
            output2.Year.ShouldBe(2000);
        }
    }

    public struct Garage
    {
        public Truck Truck { get; set; }
    }

    public struct GarageModel
    {
        public string Color { get; set; }
        public TruckModel Truck { get; set; }
    }

    public struct Truck
    {
        public string Color { get; set; }
        public int Year { get; set; }
    }

    public struct TruckModel
    {
        public string Color { get; set; }
        public int Year { get; set; }
    }

    static class Extensions
    {
        internal static ICollection<TModel> GetItems<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
        Expression<Func<TModel, bool>> filter = null,
        Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null)
        {
            //Map the expressions
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> queryableFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();

            if (filter != null)
                query = query.Where(f);

            //Call the store
            ICollection<TData> list = queryableFunc != null ? queryableFunc(query).ToList() : query.ToList();

            //Map and return the data
            return mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(list).ToList();
        }


        internal static TModelResult Query<TModel, TData, TModelResult, TDataResult>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<IQueryable<TModel>, TModelResult>> queryFunc = null)
        {
            //Map the expressions
            Func<IQueryable<TData>, TDataResult> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, TDataResult>>>(queryFunc)?.Compile();

            //execute the query
            return mapper.Map<TDataResult, TModelResult>(mappedQueryFunc(query));
        }
    }
}
