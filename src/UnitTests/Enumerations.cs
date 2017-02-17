using System;
using AutoMapper.UnitTests;
using Should;
using Xunit;

namespace AutoMapper.Tests
{
    public class DefaultEnumValueToString : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public ConsoleColor Color { get; set; }
        }

        class Destination
        {
            public string Color { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Color.ShouldEqual("Black");
        }
    }

    public class StringToNullableEnum : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public string Color { get; set; }
        }

        class Destination
        {
            public ConsoleColor? Color { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { Color = "Red" });
        }

        [Fact]
        public void Should_map_with_underlying_type()
        {
            _destination.Color.ShouldEqual(ConsoleColor.Red);
        }
    }

    public class NullableEnumToString : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public ConsoleColor? Color { get; set; }
        }

        class Destination
        {
            public string Color { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<Enum, string>().ConvertUsing((Enum src) => "Test");
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { Color = ConsoleColor.Black });
        }

        [Fact]
        public void Should_map_with_underlying_type()
        {
            _destination.Color.ShouldEqual("Test");
        }
    }

    public class EnumMappingFixture
    {
        public EnumMappingFixture()
        {
            Cleanup();
        }

        public void Cleanup()
        {
            
        }

        [Fact]
        public void ShouldMapSharedEnum()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDto>());

            var order = new Order
                {
                    Status = Status.InProgress
                };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<Order, OrderDto>(order);

            dto.Status.ShouldEqual(Status.InProgress);
        }

        [Fact]
        public void ShouldMapToUnderlyingType() {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDtoInt>());

            var order = new Order {
                Status = Status.InProgress
            };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<Order, OrderDtoInt>(order);

            dto.Status.ShouldEqual(1);
        }

        [Fact]
        public void ShouldMapToStringType() {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDtoString>());

            var order = new Order {
                Status = Status.InProgress
            };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<Order, OrderDtoString>(order);

            dto.Status.ShouldEqual("InProgress");
        }

        [Fact]
        public void ShouldMapFromUnderlyingType() {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<OrderDtoInt, Order>());

            var order = new OrderDtoInt {
                Status = 1
            };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<OrderDtoInt, Order>(order);

            dto.Status.ShouldEqual(Status.InProgress);
        }

        [Fact]
        public void ShouldMapFromStringType() {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<OrderDtoString, Order>());

            var order = new OrderDtoString {
                Status = "InProgress"
            };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<OrderDtoString, Order>(order);

            dto.Status.ShouldEqual(Status.InProgress);
        }
        
        [Fact]
        public void ShouldMapEnumByMatchingNames()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDtoWithOwnStatus>());

            var order = new Order
                {
                    Status = Status.InProgress
                };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<Order, OrderDtoWithOwnStatus>(order);

            dto.Status.ShouldEqual(StatusForDto.InProgress);
        }

        [Fact]
        public void ShouldMapEnumByMatchingValues()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDtoWithOwnStatus>());

            var order = new Order
                {
                    Status = Status.InProgress
                };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<Order, OrderDtoWithOwnStatus>(order);

            dto.Status.ShouldEqual(StatusForDto.InProgress);
        }

        [Fact]
        public void ShouldMapSharedNullableEnum() 
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<OrderWithNullableStatus, OrderDtoWithNullableStatus>());

            var order = new OrderWithNullableStatus {
                Status = Status.InProgress
            };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<OrderWithNullableStatus, OrderDtoWithNullableStatus>(order);

            dto.Status.ShouldEqual(Status.InProgress);
        }

        [Fact]
        public void ShouldMapNullableEnumByMatchingValues() 
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<OrderWithNullableStatus, OrderDtoWithOwnNullableStatus>());

            var order = new OrderWithNullableStatus {
                Status = Status.InProgress
            };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<OrderWithNullableStatus, OrderDtoWithOwnNullableStatus>(order);

            dto.Status.ShouldEqual(StatusForDto.InProgress);
        }

        [Fact]
        public void ShouldMapNullableEnumToNullWhenSourceEnumIsNullAndDestinationWasNotNull() 
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullDestinationValues = true;
                cfg.CreateMap<OrderWithNullableStatus, OrderDtoWithOwnNullableStatus>();
            });

            var dto = new OrderDtoWithOwnNullableStatus()
            {
                Status = StatusForDto.Complete
            };

            var order = new OrderWithNullableStatus
            {
                Status = null
            };

            var mapper = config.CreateMapper();
            mapper.Map(order, dto);

            dto.Status.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapNullableEnumToNullWhenSourceEnumIsNull() 
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<OrderWithNullableStatus, OrderDtoWithOwnNullableStatus>());

            var order = new OrderWithNullableStatus {
                Status = null
            };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<OrderWithNullableStatus, OrderDtoWithOwnNullableStatus>(order);

            dto.Status.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapEnumUsingCustomResolver()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDtoWithOwnStatus>()
                .ForMember(dto => dto.Status, options => options.ResolveUsing<DtoStatusValueResolver>()));

            var order = new Order
                {
                    Status = Status.InProgress
                };

            var mapper = config.CreateMapper();
            var mappedDto = mapper.Map<Order, OrderDtoWithOwnStatus>(order);

            mappedDto.Status.ShouldEqual(StatusForDto.InProgress);
        }

        [Fact]
        public void ShouldMapEnumUsingGenericEnumResolver()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDtoWithOwnStatus>()
                .ForMember(dto => dto.Status, options => options.ResolveUsing<EnumValueResolver<Status, StatusForDto>, Status>(m => m.Status)));

            var order = new Order
                {
                    Status = Status.InProgress
                };

            var mapper = config.CreateMapper();
            var mappedDto = mapper.Map<Order, OrderDtoWithOwnStatus>(order);

            mappedDto.Status.ShouldEqual(StatusForDto.InProgress);
        }

        [Fact]
        public void ShouldMapEnumWithInvalidValue()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDtoWithOwnStatus>());

            var order = new Order
            {
                Status = 0
            };

            var mapper = config.CreateMapper();
            var dto = mapper.Map<Order, OrderDtoWithOwnStatus>(order);

            var expected = (StatusForDto)0;

            dto.Status.ShouldEqual(expected);
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

        public class OrderDtoInt {
            public int Status { get; set; }
        }

        public class OrderDtoString {
            public string Status { get; set; }
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

        public class DtoStatusValueResolver : IValueResolver<Order, object, StatusForDto>
        {
            public StatusForDto Resolve(Order source, object d, StatusForDto dest, ResolutionContext context)
            {
                return context.Mapper.Map<StatusForDto>(source.Status);
            }
        }

        public class EnumValueResolver<TInputEnum, TOutputEnum> : IMemberValueResolver<object, object, TInputEnum, TOutputEnum>
        {
            public TOutputEnum Resolve(object s, object d, TInputEnum source, TOutputEnum dest, ResolutionContext context)
            {
                return ((TOutputEnum)Enum.Parse(typeof(TOutputEnum), Enum.GetName(typeof(TInputEnum), source), false));
            }
        }
    }
    public class When_mapping_from_a_null_object_with_an_enum : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullDestinationValues = false;
            cfg.CreateMap<SourceClass, DestinationClass>();
        });

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

        [Fact]
        public void Should_set_the_target_enum_to_the_default_value()
        {
            SourceClass sourceClass = null;
            var dest = Mapper.Map<SourceClass, DestinationClass>(sourceClass);
            dest.Values.ShouldEqual(default(EnumValues));
        }
    }

    public class When_mapping_to_a_nullable_flags_enum : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceClass, DestinationClass>();
        });

        [Flags]
        public enum EnumValues
        {
            One, Two = 2, Three = 4
        }

        public class SourceClass
        {
            public EnumValues Values { get; set; }
        }

        public class DestinationClass
        {
            public EnumValues? Values { get; set; }
        }

        [Fact]
        public void Should_set_the_target_enum_to_the_default_value()
        {
            var values = EnumValues.Two | EnumValues.Three;
            var dest = Mapper.Map<SourceClass, DestinationClass>(new SourceClass { Values = values });
            dest.Values.ShouldEqual(values);
        }
    }

    public class When_mapping_from_a_null_object_with_a_nullable_enum : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullDestinationValues = false;
            cfg.CreateMap<SourceClass, DestinationClass>();
        });

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

        [Fact]
        public void Should_set_the_target_enum_to_the_default_value()
        {
            SourceClass sourceClass = null;
            var dest = Mapper.Map<SourceClass, DestinationClass>(sourceClass);
            dest.Values.ShouldEqual(default(EnumValues));
        }
    }
    public class When_mapping_from_a_null_object_with_a_nullable_enum_as_string : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceClass, DestinationClass>();
        });

        public enum EnumValues
        {
            One, Two, Three
        }

        public class DestinationClass
        {
            public EnumValues Values1 { get; set; }
            public EnumValues? Values2 { get; set; }
            public EnumValues Values3 { get; set; }
        }

        public class SourceClass
        {
            public string Values1 { get; set; }
            public string Values2 { get; set; }
            public string Values3 { get; set; }
        }

        [Fact]
        public void Should_set_the_target_enum_to_the_default_value()
        {
            var sourceClass = new SourceClass();
            var dest = Mapper.Map<SourceClass, DestinationClass>(sourceClass);
            dest.Values1.ShouldEqual(default(EnumValues));
        }

        [Fact]
        public void Should_set_the_target_nullable_to_null()
        {
            var sourceClass = new SourceClass();
            var dest = Mapper.Map<SourceClass, DestinationClass>(sourceClass);
            dest.Values2.ShouldBeNull();
        }

        [Fact]
        public void Should_set_the_target_empty_to_null()
        {
            var sourceClass = new SourceClass
            {
                Values3 = ""
            };
            var dest = Mapper.Map<SourceClass, DestinationClass>(sourceClass);
            dest.Values3.ShouldEqual(default(EnumValues));
        }
    }


    public class When_mapping_a_flags_enum : NonValidatingSpecBase
    {
        private DestinationFlags _result;

        [Flags]
        private enum SourceFlags
        {
            None = 0,
            One = 1,
            Two = 2,
            Four = 4,
            Eight = 8
        }

        [Flags]
        private enum DestinationFlags
        {
            None = 0,
            One = 1,
            Two = 2,
            Four = 4,
            Eight = 8
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        protected override void Because_of()
        {
            _result = Mapper.Map<SourceFlags, DestinationFlags>(SourceFlags.One | SourceFlags.Four | SourceFlags.Eight);
        }

        [Fact]
        public void Should_include_all_source_enum_values()
        {
            _result.ShouldEqual(DestinationFlags.One | DestinationFlags.Four | DestinationFlags.Eight);
        }
    }

}
