using System;
using Should;
using Xunit;
using AutoMapper;

namespace AutoMapper.UnitTests.Bug
{
    public class GenericTypeConverter : AutoMapperSpecBase
    {
        Destination<int> _destination;
        OtherDestination<int> _otherDestination;

        public class Source<T>
        {
            public T Value { get; set; }
        }

        public class Destination<T>
        {
            public T Value { get; set; }
        }

        public class OtherSource<T>
        {
            public T Value { get; set; }
        }

        public class OtherDestination<T>
        {
            public T Value { get; set; }
        }

        public class Converter<T> : ITypeConverter<Source<T>, Destination<T>>, ITypeConverter<OtherSource<T>, OtherDestination<T>>
        {
            public static Destination<T> SomeDestination = new Destination<T>();
            public static OtherDestination<T> SomeOtherDestination = new OtherDestination<T>();

            public Destination<T> Convert(ResolutionContext context)
            {
                return SomeDestination;
            }

            OtherDestination<T> ITypeConverter<OtherSource<T>, OtherDestination<T>>.Convert(ResolutionContext context)
            {
                return SomeOtherDestination;
            }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap(typeof(Source<>), typeof(Destination<>)).ConvertUsing(typeof(Converter<>));
            Mapper.CreateMap(typeof(OtherSource<>), typeof(OtherDestination<>)).ConvertUsing(typeof(Converter<>));
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination<int>>(new Source<int>());
            _otherDestination = Mapper.Map<OtherDestination<int>>(new OtherSource<int>());
        }

        [Fact]
        public void Should_use_generic_type_converter()
        {
            _destination.ShouldBeSameAs(Converter<int>.SomeDestination);
            _otherDestination.ShouldBeSameAs(Converter<int>.SomeOtherDestination);
        }
    }
}