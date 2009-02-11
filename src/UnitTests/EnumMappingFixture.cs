using System;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.Tests
{
	[TestFixture]
	public class EnumMappingFixture
	{
		[SetUp]
		[TearDown]
		public void Cleanup()
		{
			Mapper.Reset();
		}

		[Test]
		public void ShouldMapSharedEnum()
		{
			Mapper.CreateMap<Order, OrderDto>();

			var order = new Order
				{
					Status = Status.InProgress
				};

			var dto = Mapper.Map<Order, OrderDto>(order);

			dto.Status.ShouldEqual(Status.InProgress);
		}

		
		[Test]
		public void ShouldMapEnumByMatchingNames()
		{
			Mapper.CreateMap<Order, OrderDtoWithOwnStatus>();

			var order = new Order
				{
					Status = Status.InProgress
				};

			var dto = Mapper.Map<Order, OrderDtoWithOwnStatus>(order);

			dto.Status.ShouldEqual(StatusForDto.InProgress);
		}

		[Test]
		public void ShouldMapEnumByMatchingValues()
		{
			Mapper.CreateMap<Order, OrderDtoWithOwnStatus>();

			var order = new Order
				{
					Status = Status.InProgress
				};

			var dto = Mapper.Map<Order, OrderDtoWithOwnStatus>(order);

			dto.Status.ShouldEqual(StatusForDto.InProgress);
		}

		[Test]
		public void ShouldMapSharedNullableEnum() 
		{
			Mapper.CreateMap<OrderWithNullableStatus, OrderDtoWithNullableStatus>();

			var order = new OrderWithNullableStatus {
				Status = Status.InProgress
			};

			var dto = Mapper.Map<OrderWithNullableStatus, OrderDtoWithNullableStatus>(order);

			dto.Status.ShouldEqual(Status.InProgress);
		}

		[Test]
		public void ShouldMapNullableEnumByMatchingValues() 
		{
			Mapper.CreateMap<OrderWithNullableStatus, OrderDtoWithOwnNullableStatus>();

			var order = new OrderWithNullableStatus {
				Status = Status.InProgress
			};

			var dto = Mapper.Map<OrderWithNullableStatus, OrderDtoWithOwnNullableStatus>(order);

			dto.Status.ShouldEqual(StatusForDto.InProgress);
		}

		[Test]
		public void ShouldMapNullableEnumToNullWhenSourceEnumIsNull() 
		{
			Mapper.CreateMap<OrderWithNullableStatus, OrderDtoWithOwnNullableStatus>();

			var order = new OrderWithNullableStatus {
				Status = null
			};

			var dto = Mapper.Map<OrderWithNullableStatus, OrderDtoWithOwnNullableStatus>(order);

			dto.Status.ShouldBeNull();
		}

		[Test]
		public void ShouldMapEnumUsingCustomResolver()
		{
			Mapper.CreateMap<Order, OrderDtoWithOwnStatus>()
				.ForMember(dto => dto.Status, options => options
				                                         	.ResolveUsing<DtoStatusValueResolver>());

			var order = new Order
				{
					Status = Status.InProgress
				};

			var mappedDto = Mapper.Map<Order, OrderDtoWithOwnStatus>(order);

			mappedDto.Status.ShouldEqual(StatusForDto.InProgress);
		}

		[Test]
		public void ShouldMapEnumUsingGenericEnumResolver()
		{
			Mapper.CreateMap<Order, OrderDtoWithOwnStatus>()
				.ForMember(dto => dto.Status, options => options
				                                         	.ResolveUsing<EnumValueResolver<Status, StatusForDto>>()
				                                         	.FromMember(m => m.Status));

			var order = new Order
				{
					Status = Status.InProgress
				};

			var mappedDto = Mapper.Map<Order, OrderDtoWithOwnStatus>(order);

			mappedDto.Status.ShouldEqual(StatusForDto.InProgress);
		}

		public enum Status
		{
			InProgress = 1,
			Complete = 2
		}

		public enum StatusForDto
		{
			InProgress = 1,
			Complete = 2
		}

		public class Order
		{
			public Status Status { get; set; }
		}

		public class OrderDto
		{
			public Status Status { get; set; }
		}

		public class OrderDtoWithOwnStatus
		{
			public StatusForDto Status { get; set; }
		}

		public class OrderWithNullableStatus 
		{
			public Status? Status { get; set; }
		}

		public class OrderDtoWithNullableStatus 
		{
			public Status? Status { get; set; }
		}

		public class OrderDtoWithOwnNullableStatus 
		{
			public StatusForDto? Status { get; set; }
		}

		public class DtoStatusValueResolver : IValueResolver
		{
			public ResolutionResult Resolve(ResolutionResult source)
			{
				return new ResolutionResult(((Order)source.Value).Status);
			}
		}

		public class EnumValueResolver<TInputEnum, TOutputEnum> : IValueResolver
		{
			public ResolutionResult Resolve(ResolutionResult source)
			{
				return new ResolutionResult((TOutputEnum) Enum.Parse(typeof (TOutputEnum), Enum.GetName(typeof (TInputEnum), source.Value)));
			}
		}
	}

	[TestFixture]
	public class When_mapping_from_a_null_object_with_an_enum
	{
		[SetUp]
		public void SetUp()
		{
			Mapper.CreateMap<SourceClass, DestinationClass>();
		}

		public enum EnumValues
		{
			One, Two, Three
		}

		public class DestinationClass
		{
			public EnumValues Values { get; set; }
		}

		public class SourceClass
		{
			public EnumValues Values { get; set; }
		}

		[Test]
		public void Should_set_the_target_enum_to_the_default_value()
		{
			SourceClass sourceClass = null;
			var dest = Mapper.Map<SourceClass, DestinationClass>(sourceClass);
			dest.Values.ShouldEqual(default(EnumValues));
		}
	}

	[TestFixture]
	public class When_mapping_from_a_null_object_with_an_enum_on_a_nullable_enum
	{
		[SetUp]
		public void SetUp()
		{
			Mapper.CreateMap<SourceClass, DestinationClass>();
		}

		public enum EnumValues
		{
			One, Two, Three
		}

		public class DestinationClass
		{
			public EnumValues? Values { get; set; }
		}

		public class SourceClass
		{
			public EnumValues Values { get; set; }
		}

		[Test]
		public void Should_set_the_target_enum_to_null()
		{
			SourceClass sourceClass = null;
			var dest = Mapper.Map<SourceClass, DestinationClass>(sourceClass);
			dest.Values.ShouldEqual(null);
		}
	}

	[TestFixture]
	public class When_mapping_from_a_null_object_with_a_nullable_enum
	{
		[SetUp]
		public void SetUp()
		{
			Mapper.CreateMap<SourceClass, DestinationClass>();
		}

		public enum EnumValues
		{
			One, Two, Three
		}

		public class DestinationClass
		{
			public EnumValues Values { get; set; }
		}

		public class SourceClass
		{
			public EnumValues? Values { get; set; }
		}

		[Test]
		public void Should_set_the_target_enum_to_the_default_value()
		{
			SourceClass sourceClass = null;
			var dest = Mapper.Map<SourceClass, DestinationClass>(sourceClass);
			dest.Values.ShouldEqual(default(EnumValues));
		}
	}
}
