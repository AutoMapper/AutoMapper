using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
	public class NullableBytesAndEnums : AutoMapperSpecBase
	{
		private Destination _destination;

		public class Source
		{
			public byte? Value { get; set; }
		}

		public enum Foo : byte
		{
			Blarg = 1,
			Splorg = 2
		}

		public class Destination
		{
			public Foo? Value { get; set; }
		}

		protected override void Establish_context()
		{
			Mapper.Initialize(cfg =>
			{
				cfg.CreateMap<Source, Destination>();
			});
		}

		protected override void Because_of()
		{
			_destination = Mapper.Map<Source, Destination>(new Source {Value = 2});
		}

		[Fact]
		public void Should_map_the_byte_to_the_enum_with_the_same_value()
		{
			_destination.Value.ShouldEqual(Foo.Splorg);
		}
	}

#if NET40
    public class NullableLong : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Destination
        {
            public long? Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Value = 2 });
        }

        [Fact]
        public void Should_map_the_byte_to_the_enum_with_the_same_value()
        {
            _destination.Value.ShouldEqual(2);
        }
    }
#endif
}
