using System.Linq;
using Xunit;
using Shouldly;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Collections.Generic;
using System;

namespace AutoMapper.UnitTests.Projection
{
    public class ProjectionWithSubQueryTests : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderModel>()
                       .ForMember(dst => dst.OrderSubModel, opt => opt.MapFrom(src => src.MostImportantOrderLine != null ? src : null))
                       .ForMember(dst => dst.MostImportantOrderLine, opt =>
                       {
                           opt.MapFrom(src => src.OrderLines.FirstOrDefault(x => x.OrderLineNumber == src.MostImportantOrderLine));
                       });

            cfg.CreateMap<Order, OrderSubModel>()
                .ForMember(dst => dst.OrderId, opt => opt.MapFrom(src => src.Id));

            cfg.CreateMap<OrderLine, OrderLineModel>()
                .ForMember(dst => dst.OrderLineId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Description, opt => opt.MapFrom(src => src.Description));
        });

        [Fact]
        public void Should_not_throw_when_executing()
        {
            var orders = new[]
            {
                new Order
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    MostImportantOrderLine = 1,
                    OrderLines = new List<OrderLine>
                    {
                        new OrderLine
                        {
                            Id = Guid.Parse("00000000-0000-0000-0000-000000000101"),
                            OrderId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                            OrderLineNumber = 1,
                            Description = "ChessSet"
                        }
                    }
                }
            }.AsQueryable();

            var projection = orders.ProjectTo<OrderModel>(Configuration);
            var orderModel = projection.First();

            orderModel.OrderSubModel.OrderId.ShouldBe(Guid.Parse("00000000-0000-0000-0000-000000000001"));
            orderModel.MostImportantOrderLine.OrderLineId.ShouldBe(Guid.Parse("00000000-0000-0000-0000-000000000101"));
            orderModel.MostImportantOrderLine.Description.ShouldBe("ChessSet");
        }

        private class Order
        {
            public Guid Id { get; set; }
            public Guid CustomerId { get; set; }

            public int? MostImportantOrderLine { get; set; }

            public ICollection<OrderLine> OrderLines { get; set; } = null!;
        }
        private class OrderLine
        {
            public Guid Id { get; set; }
            public Guid OrderId { get; set; }

            public int OrderLineNumber { get; set; }
            public string Description { get; set; } = null!;

            public Order Order { get; set; } = null!;
        }
        private class OrderModel
        {
            public OrderSubModel OrderSubModel { get; set; }
            public OrderLineModel MostImportantOrderLine { get; set; }
        }

        private class OrderSubModel
        {
            public Guid? OrderId { get; set; }
        }

        private class OrderLineModel
        {
            public Guid OrderLineId { get; set; }
            public string Description { get; set; } = string.Empty;
        }
    }
}