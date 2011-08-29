using System;

using AutoMapper.UnitTests.Asbjornu.Domain;
using AutoMapper.UnitTests.Asbjornu.Mapping;
using AutoMapper.UnitTests.Asbjornu.Resources;

using NUnit.Framework;

namespace AutoMapper.UnitTests.Asbjornu
{
	[TestFixture]
	public class AsbjornuTest
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			ResourceMappingProfile resourceMappingProfile = new ResourceMappingProfile();
			Mapper.Initialize(x => x.AddProfile(resourceMappingProfile));
		}


		[Test]
		public void AssertConfigurationIsValid()
		{
			Mapper.AssertConfigurationIsValid();
		}


		[Test]
		public void Map_ContactInfo_ReturnsContactInfo()
		{
			ContactInfo contactInfo = new ContactInfo
			{
				Email = String.Format("{0}@okb.no", Guid.NewGuid().ToString("N")),
				FirstName = Guid.NewGuid().ToString("N"),
				LastName = Guid.NewGuid().ToString("N"),
				Phone = "1234568",
			};

			Resources.Models.ContactInfo mappedContactInfo =
				Mapper.Map<Resources.Models.ContactInfo>(contactInfo);

			Assert.That(mappedContactInfo, Is.Not.Null, "ContactInfo");
			Assert.That(mappedContactInfo.Email, Is.EqualTo(contactInfo.Email), "ContactInfo.Email");
			Assert.That(mappedContactInfo.FirstName, Is.EqualTo(contactInfo.FirstName), "ContactInfo.FirstName");
			Assert.That(mappedContactInfo.LastName, Is.EqualTo(contactInfo.LastName), "ContactInfo.LastName");
			Assert.That(
				mappedContactInfo.Phone,
				Is.EqualTo(contactInfo.Phone),
				"ContactInfo.MobilePhoneNumber");
		}


		[Test]
		public void Map_Customer_ReturnsCustomerResource()
		{
			Customer customer = new Customer
			{
				ContactInfo = new ContactInfo
				{
					Email = String.Format("{0}@okb.no", Guid.NewGuid().ToString("N")),
					FirstName = Guid.NewGuid().ToString("N"),
					LastName = Guid.NewGuid().ToString("N"),
					Phone = "1234568",
				}
			};

			CustomerResource customerResource = Mapper.Map<CustomerResource>(customer);

			Assert.That(customerResource, Is.Not.Null, "CustomerResource");
			Assert.That(customerResource.ContactInfo, Is.Not.Null, "CustomerResource.ContactInfo");
			Assert.That(
				customerResource.ContactInfo.Email,
				Is.EqualTo(customer.ContactInfo.Email),
				"CustomerResource.ContactInfo.Email");

			Assert.That(
				customerResource.ContactInfo.FirstName,
				Is.EqualTo(customer.ContactInfo.FirstName),
				"CustomerResource.ContactInfo.FirstName");

			Assert.That(
				customerResource.ContactInfo.LastName,
				Is.EqualTo(customer.ContactInfo.LastName),
				"CustomerResource.ContactInfo.LastName");

			Assert.That(
				customerResource.ContactInfo.Phone,
				Is.EqualTo(customer.ContactInfo.Phone),
				"CustomerResource.ContactInfo.MobilePhoneNumber");
		}
	}
}