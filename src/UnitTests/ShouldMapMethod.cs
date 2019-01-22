using Xunit;
using Shouldly;
using System;

namespace AutoMapper.UnitTests
{
    public class ShouldMapMethod : NonValidatingSpecBase
    {
        public int SomeValue = 2354;
        public int AnotherValue = 6798;

        private Destination _destination;

        class Source
        {
            private int _someValue;
            private int _anotherValue;

            public Source(int someValue, int anotherValue) 
            {
                _someValue = someValue;
                anotherValue = _anotherValue;
            }

            public int SomeNumber() 
            {
                return _someValue;
            }

            public int AnotherNumber() {
                return _anotherValue;
            }
        }

        class Destination
        {
            public int SomeNumber { get; set; }
            public int AnotherNumber { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.ShouldMapMethod = (m => m.Name != nameof(Source.AnotherNumber));
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source(SomeValue, AnotherValue));
        }

        [Fact]
        public void Should_report_unmapped_property()
        {
            new Action(() => Configuration.AssertConfigurationIsValid())
                .ShouldThrowException<AutoMapperConfigurationException>(ex => 
                {
                    ex.Errors.ShouldNotBeNull();
                    ex.Errors.ShouldNotBeEmpty();
                    ex.Errors[0].UnmappedPropertyNames.ShouldNotBeNull();
                    ex.Errors[0].UnmappedPropertyNames.ShouldNotBeEmpty();
                    ex.Errors[0].UnmappedPropertyNames[0].ShouldBe(nameof(Destination.AnotherNumber));
                });
        }

        [Fact]
        public void Should_not_map_another_number_method() 
        {
            _destination.SomeNumber.ShouldBe(SomeValue);
            _destination.AnotherNumber.ShouldNotBe(AnotherValue);
        }
    }
}