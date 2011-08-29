using AutoMapper.UnitTests.Asbjornu.Resources;

namespace AutoMapper.UnitTests.Asbjornu.Mapping.Customer
{
	public class CustomerToCustomerResourceMapper : MapperBase<Domain.Customers.Customer, CustomerResource>
	{
		public CustomerToCustomerResourceMapper(Profile profile)
			: base(profile)
		{
		}


		public override IMappingExpression<Domain.Customers.Customer, CustomerResource> CreateMap()
		{
			return base.CreateMap()
				.ForMember(d => d.ContactInfo, o => o.Ignore())
				.ForMember(d => d.Url, o => o.Ignore());
		}
	}
}