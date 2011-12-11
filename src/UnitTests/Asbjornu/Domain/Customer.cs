namespace AutoMapper.UnitTests.Asbjornu.Domain
{
	public class Customer
	{
		public int Id { get; set; }

		public virtual ContactInfo ContactInfo { get; set; }
	}
}