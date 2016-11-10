namespace AutoMapper.UnitTests.Bug
{
    using System;
    using Rhino.Mocks;
    using Should;

    public class MapFromClosureBug : SpecBaseBase
    {
        private static readonly IDateProvider _dateProvider;

        static MapFromClosureBug()
        {
            _dateProvider = MockRepository.GenerateMock<IDateProvider>();
        }

        public interface IDateProvider
        {
            DateTime CurrentRestaurantTime(Restaurant restaurant);
        }

        public class Result
        {
            public Booking Booking { get; set; }
        }

        public class Restaurant
        {
        }

        public class Booking
        {
            public Restaurant Restaurant { get; set; }

            public int? CalculateTotal(DateTime currentTime)
            {
                return null;
            }
        }

        public class ResultDto
        {
            public BookingDto Booking { get; set; }
        }

        public class BookingDto
        {
            public int? Total { get; set; }
        }

        public void Should_map_successfully()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Result, ResultDto>();
                cfg.CreateMap<Booking, BookingDto>()
                    .ForMember(d => d.Total,
                        o => o.MapFrom(b => b.CalculateTotal(_dateProvider.CurrentRestaurantTime(b.Restaurant))));
            });

            var mapper = mapperConfiguration.CreateMapper();

            var result = new Result { Booking = new Booking() };

            // Act
            var dto = mapper.Map<ResultDto>(result);

            // Assert
            dto.ShouldNotBeNull();
        }

    }
}