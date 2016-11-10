using AutoMapper;
using NUnit.Framework;
using Should;

namespace AutoMapperSamples.Configuration
{
    namespace Profiles
    {
        [TestFixture]
        public class SimpleExample
        {
            public class Order
            {
                public decimal Amount { get; set; }
            }

            public class OrderListViewModel
            {
                public string Amount { get; set; }
            }

            public class OrderEditViewModel
            {
                public string Amount { get; set; }
            }

            public class ViewModelProfile : Profile
            {
                protected override void Configure()
                {
                    CreateMap<Order, OrderListViewModel>();

                    CreateMap<decimal, string>().ConvertUsing(value => value.ToString("c"));
                }
            }


            [Test]
            public void Example()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<ViewModelProfile>();
                    cfg.CreateMap<Order, OrderEditViewModel>();
                });

                var order = new Order {Amount = 50m};

                var listViewModel = config.CreateMapper().Map<Order, OrderListViewModel>(order);
                var editViewModel = config.CreateMapper().Map<Order, OrderEditViewModel>(order);

                listViewModel.Amount.ShouldEqual(order.Amount.ToString("c"));
                editViewModel.Amount.ShouldEqual("50");
            }
        }
    }
}