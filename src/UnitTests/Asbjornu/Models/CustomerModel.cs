using System;

namespace AutoMapper.UnitTests.Asbjornu.Models
{
	public class CustomerModel
	{
		public CustomerModel()
		{
		}


		public CustomerModel(ContactInfo contactInfo)
		{
			if (contactInfo == null)
				throw new ArgumentNullException("contactInfo");

			ContactInfo = contactInfo;
		}

		public ContactInfo ContactInfo { get; set; }

		public int Id { get; set; }
	}
}