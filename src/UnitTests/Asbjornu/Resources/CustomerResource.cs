using System;

using AutoMapper.UnitTests.Asbjornu.Resources.Models;


namespace AutoMapper.UnitTests.Asbjornu.Resources
{
	public class CustomerResource : ResourceBase
	{
		public CustomerResource()
		{
		}


		public CustomerResource(ContactInfo contactInfo)
		{
			if (contactInfo == null)
				throw new ArgumentNullException("contactInfo");

			ContactInfo = contactInfo;
		}

		public ContactInfo ContactInfo { get; set; }
	}
}