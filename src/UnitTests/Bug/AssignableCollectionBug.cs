using System.Collections.Generic;
using Xunit;
using Should;

namespace AutoMapper.UnitTests.Bug
{
	namespace AssignableCollectionBug
	{
		public interface IAddress
		{
			string Street { get; set; }
		}

        public interface IPerson
		{
			string Name { get; set; }
			IList<IAddress> Addresses { get; set; }
		}

		// To keep things as simple as possible, implementations are exactly the same.
        public class PersonOne : IPerson
		{
			#region Implementation of IPerson

			public string Name { get; set; }
			public IList<IAddress> Addresses { get; set; }

			#endregion
		}

        public class PersonTwo : IPerson
		{
			public string Name { get; set; }
			public IList<IAddress> Addresses { get; set; }
		}

        public class AddressOne : IAddress
		{
			#region Implementation of IAddress

			public string Street { get; set; }

			#endregion
		}

        public class AddressTwo : IAddress
		{
			#region Implementation of IAddress

			public string Street { get; set; }

			#endregion
		}
		public class MappingTests
		{
			[Fact(Skip = "This sounds like really bad behavior to support, at least this way.")]
			public void CanMapPersonOneToPersonTwo()
			{
				IList<IAddress> adrList = new List<IAddress> { new AddressOne { Street = "Street One" } };
				PersonOne source = new PersonOne { Name = "A Name", Addresses = adrList };

				// I thought these mappings would be enough. I tried various others, without success.
				Mapper.CreateMap<PersonOne, PersonTwo>();
				Mapper.CreateMap<AddressOne, AddressTwo>();
				Mapper.CreateMap<AddressOne, IAddress>().ConvertUsing(Mapper.Map<AddressOne, AddressTwo>);
				Mapper.AssertConfigurationIsValid();
				var result = Mapper.Map<PersonOne, PersonTwo>(source);

				// These are ok.
				source.Name.ShouldEqual(result.Name);
				result.Addresses.ShouldNotBeNull();
				(result.Addresses.Count == 1).ShouldBeTrue();
				source.Addresses[0].Street.ShouldEqual(result.Addresses[0].Street);

				// This is what I can't get to pass:
				result.Addresses[0].ShouldBeType<AddressTwo>();

				// Expected: instance of <AutomapperTest.AddressTwo>
				// But was:  <AutomapperTest.AddressOne>
			}
		}

	}

    namespace ByteArrayBug
    {
        public class When_mapping_byte_arrays : AutoMapperSpecBase
        {
            private Picture _source;
            private PictureDto _dest;

            public class Picture
            {
                public int Id { get; set; }
                public string Description { get; set; }
                public byte[] ImageData { get; set; }
            }

            public class PictureDto
            {
                public string Description { get; set; }
                public byte[] ImageData { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg => cfg.CreateMap<Picture, PictureDto>());
            }

            protected override void Because_of()
            {
                _source = new Picture {ImageData = new byte[100]};
                _dest = Mapper.Map<Picture, PictureDto>(_source);
            }

            [Fact]
            public void Should_copy_array()
            {
                _dest.ImageData.ShouldBeSameAs(_source.ImageData);
            }
        }
    }

    namespace AssignableLists
    {
        public class AutoMapperTests
        {
            [Fact]
            public void ListShouldNotMapAsReference()
            {
                // arrange
                Mapper.Reset();
                Mapper.CreateMap<A, B>();
                var source = new A { Images = new List<string>() };

                // act
                var destination = Mapper.Map<B>(source);
                destination.Images.Add("test");

                // assert
                destination.Images.Count.ShouldEqual(1);
                source.Images.Count.ShouldEqual(0); // in 3.1.0 source.Images.Count is 1
            }
        }

        public class A
        {
            public IList<string> Images { get; set; }
        }

        public class B
        {
            public IList<string> Images { get; set; }
        }
    }
}
