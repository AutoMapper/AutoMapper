using System.Collections.Generic;
using NUnit.Framework;
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

		[TestFixture]
		public class MappingTests
		{
			[Test]
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
				Assert.AreEqual(source.Name, result.Name);
				Assert.IsNotNull(result.Addresses);
				Assert.IsTrue(result.Addresses.Count == 1);
				Assert.AreEqual(source.Addresses[0].Street, result.Addresses[0].Street);

				// This is what I can't get to pass:
				result.Addresses[0].ShouldBeType<AddressTwo>();

				// Expected: instance of <AutomapperTest.AddressTwo>
				// But was:  <AutomapperTest.AddressOne>
			}
		}

	}
}