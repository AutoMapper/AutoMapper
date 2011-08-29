using AutoMapper.UnitTests.Asbjornu.Domain.Relations;

namespace AutoMapper.UnitTests.Asbjornu.Domain.Customers
{
	public class Customer
	{
		public int Id { get; set; }

		public virtual ContactInfo ContactInfo { get; set; }
	}
}