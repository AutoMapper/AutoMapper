using System;

using AutoMapper.UnitTests.Asbjornu.Domain;
using AutoMapper.UnitTests.Asbjornu.Mapping;
using AutoMapper.UnitTests.Asbjornu.Models;

using NUnit.Framework;

using ContactInfo = AutoMapper.UnitTests.Asbjornu.Domain.ContactInfo;

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

			Models.ContactInfo mappedContactInfo =
				Mapper.Map<Models.ContactInfo>(contactInfo);

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

			CustomerModel customerModel = Mapper.Map<CustomerModel>(customer);

			Assert.That(customerModel, Is.Not.Null, "CustomerResource");
			Assert.That(customerModel.ContactInfo, Is.Not.Null, "CustomerResource.ContactInfo");
			Assert.That(
				customerModel.ContactInfo.Email,
				Is.EqualTo(customer.ContactInfo.Email),
				"CustomerResource.ContactInfo.Email");

			Assert.That(
				customerModel.ContactInfo.FirstName,
				Is.EqualTo(customer.ContactInfo.FirstName),
				"CustomerResource.ContactInfo.FirstName");

			Assert.That(
				customerModel.ContactInfo.LastName,
				Is.EqualTo(customer.ContactInfo.LastName),
				"CustomerResource.ContactInfo.LastName");

			Assert.That(
				customerModel.ContactInfo.Phone,
				Is.EqualTo(customer.ContactInfo.Phone),
				"CustomerResource.ContactInfo.MobilePhoneNumber");
		}
	}
}