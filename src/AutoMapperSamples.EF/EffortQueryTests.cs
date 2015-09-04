using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using NUnit.Framework;

namespace AutoMapperSamples.EF
{
    [TestFixture]
    public class EffortQueryTests
    {
        static EffortQueryTests()
        {
            Effort.Provider.EffortProviderConfiguration.RegisterProvider();
        }

        [Test]
        public void Effort_FilterByDto()
        {
            using (var context = new TestContext(Effort.DbConnectionFactory.CreateTransient()))
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<OrderDto, Order>()
                        .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName))
                        .ReverseMap(); // reverse map added
                });

                IQueryable<OrderDto> sourceResult = new OrderDto[0]
                    .AsQueryable()
                    .Where(s => s.FullName.EndsWith("Bestellung"))
                    .Map<OrderDto, Order>(context.OrderSet)
                    .ProjectTo<OrderDto>(); // projection added

                var dtos = sourceResult.ToList();

                Assert.AreEqual(2, dtos.Count);
            }
        }

        [Test]
        public void Effort_FilterByMappedQuery()
        {
            using (var context = new TestContext(Effort.DbConnectionFactory.CreateTransient()))
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<OrderDto, Order>()
                        .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName))
                        .ReverseMap(); // reverse map added
                });

                // works but requires filters (Where, ...) to be specified before call to "Map"
                // however, we'd like to apply filters to the resulting IQueryable "sourceResult".
                // that does not work though.
                IQueryable<OrderDto> sourceResult = new OrderDto[0]
                    .AsQueryable()
                    .Where(s => s.FullName.EndsWith("Bestellung"))
                    .Map<OrderDto, Order>(context.OrderSet)
                    .ProjectTo<OrderDto>(); // projection added
                var dtos = sourceResult.ToList();
                Assert.AreEqual(2, dtos.Count);

                // this is what we try to achieve but it does not work
                // as the mapping is done right away by "Map<>" and "ProjectTo<>"
                // and the .Where() filter does not at that time
                // so it is not mapped and results in a
                // "System.NotSupportedException : The specified type member 'FullName' is not supported 
                // ...in LINQ to Entities. Only initializers, entity members, and entity navigation properties 
                // ...are supported."
                try
                {
                    IQueryable<OrderDto> sourceResult2 = new OrderDto[0]
                        .AsQueryable()
                        .Map<OrderDto, Order>(context.OrderSet)
                        .ProjectTo<OrderDto>(); // projection added
                    var dtos2 = sourceResult
                        .Where(s => s.FullName.EndsWith("Bestellung"))
                        .ToList();

                    Assert.Fail("NotSupportedException was expected");
                }
                catch (NotSupportedException)
                {
                }

                // this is our solution:
                // in this case, filter is applied to the "DtoQuery" on "ToList" => 
                // so it is completely translated to a DB query
                // the MappedQueryProvider internally applies the "Map<>" and "ProjectTo<>" calls
                // when the IQueryable<OrderDto> (MappedQueryable<TSource,TDestination>) is enumerated
                // this applying filters to the "lazilyMappedQuery" actually works - yay! :)
                IQueryable<OrderDto> lazilyMappedQuery = MappedQueryProvider<OrderDto, Order>.Map<OrderDto>(context.OrderSet,
                    Mapper.Engine);
                var dtos3 = lazilyMappedQuery
                    .Where(d => d.FullName.EndsWith("Bestellung")).ToList();

                Assert.AreEqual(2, dtos3.Count);
            }
        }

        [Test]
        public void Effort_FilterByMappedQuery_InnerQueryOrderedAndFiltered()
        {
            using (var context = new TestContext(Effort.DbConnectionFactory.CreateTransient()))
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<OrderDto, Order>()
                        .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName))
                        .ReverseMap(); // reverse map added
                });

                var orders = context.OrderSet.Where(o => o.Price > 85D).OrderBy(o => o.Price);

                IQueryable<OrderDto> lazilyMappedQuery = MappedQueryProvider<OrderDto, Order>.Map<OrderDto>(orders,
                    Mapper.Engine, (x) => {Assert.Fail(x.Message);});
                var dtos3 = lazilyMappedQuery
                    .Where(d => d.FullName.EndsWith("Bestellung")).ToList();

                Assert.AreEqual(1, dtos3.Count);
            }
        }


        [Test]
        public void Effort_OrderByDto_FullName()
        {
            using (var context = new TestContext(Effort.DbConnectionFactory.CreateTransient()))
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<OrderDto, Order>()
                        .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName));
                    cfg.CreateMap<Order, OrderDto>()
                        .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name));
                });

                var orders = context.OrderSet;
                
                IQueryable<OrderDto> lazilyMappedQuery = MappedQueryProvider<OrderDto, Order>.Map<OrderDto>(orders,
                    Mapper.Engine, (x) => { Assert.Fail(x.Message); });
                var dtos3 = lazilyMappedQuery
                    .OrderBy(dto => dto.FullName).Skip(2).ToList();

                Assert.AreEqual(1, dtos3.Count);
                Assert.AreEqual("Zalando Bestellung", dtos3.Single().FullName);
            }
        }


        [Test]
        public void Effort_OrderByDto_Price()
        {
            using (var context = new TestContext(Effort.DbConnectionFactory.CreateTransient()))
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<OrderDto, Order>()
                        .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName));
                    cfg.CreateMap<Order, OrderDto>()
                        .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name));
                });

                var orders = context.OrderSet;
                
                IQueryable<OrderDto> lazilyMappedQuery = MappedQueryProvider<OrderDto, Order>.Map<OrderDto>(orders,
                    Mapper.Engine, (x) => { Assert.Fail(x.Message); });
                var dtos3 = lazilyMappedQuery
                    .OrderBy(dto => dto.Price).Skip(2).ToList();

                Assert.AreEqual(1, dtos3.Count);
                Assert.AreEqual("Zalando Bestellung", dtos3.Single().FullName);
            }
        }


        [Test]
        public void Effort_FilterByMappedQuery_SkipAndTake()
        {
            using (var context = new TestContext(Effort.DbConnectionFactory.CreateTransient()))
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<OrderDto, Order>()
                        .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName));
                    cfg.CreateMap<Order, OrderDto>()
                        .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name));
                });

                var orders = context.OrderSet.OrderBy(o => o.Name);
                
                IQueryable<OrderDto> lazilyMappedQuery = MappedQueryProvider<OrderDto, Order>.Map<OrderDto>(orders,
                    Mapper.Engine, (x) => { Assert.Fail(x.Message); });
                var dtos3 = lazilyMappedQuery
                    .Skip(1).Take(1).ToList();

                Assert.AreEqual(1, dtos3.Count);
                Assert.AreEqual("Universalversand", dtos3.Single().FullName);
            }
        }

    }
}
