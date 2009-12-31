using System;
using AutoMapper;
using NUnit.Framework;

namespace AutoMapperSamples.Configuration
{
	namespace Profiles
	{
		[TestFixture]
		public class SimpleExample
		{
			public class Order
			{
				
			}

			public class OrderListViewModel
			{
				
			}

			//public class MoneyFormatter : ValueFormatter<decimal>
			//{
			//    protected override string FormatValueCore(decimal value)
			//    {
			//        return value.ToString("c");
			//    }
			//}

			public class ViewModelProfile : Profile
			{
				protected override string ProfileName
				{
					get { return "ViewModelProfile"; }
				}

				protected override void Configure()
				{
					CreateMap<Order, OrderListViewModel>();

					//ForSourceType<decimal>().AddFormatter<MoneyFormatter>();
				}
			}
		}
	}
}