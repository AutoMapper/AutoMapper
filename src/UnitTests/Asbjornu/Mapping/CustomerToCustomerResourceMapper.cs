using AutoMapper.UnitTests.Asbjornu.Models;

namespace AutoMapper.UnitTests.Asbjornu.Mapping
{
	public class CustomerToCustomerResourceMapper : MapperBase<Domain.Customer, CustomerModel>
	{
		public CustomerToCustomerResourceMapper(Profile profile)
			: base(profile)
		{
		}


		public override IMappingExpression<Domain.Customer, CustomerModel> CreateMap()
		{
			return base.CreateMap()
				.ForMember(d => d.ContactInfo, o => o.Ignore())
				.ForMember(d => d.Url, o => o.Ignore());
		}
	}
}