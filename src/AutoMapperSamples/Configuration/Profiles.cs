using AutoMapper;
using NUnit.Framework;
using Shouldly;

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
                public ViewModelProfile()
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

                listViewModel.Amount.ShouldBe(order.Amount.ToString("c"));
                editViewModel.Amount.ShouldBe("50");
            }
        }
    }
}