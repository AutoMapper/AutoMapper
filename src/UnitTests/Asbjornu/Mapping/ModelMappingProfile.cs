using AutoMapper.UnitTests.Asbjornu.Models;

using ContactInfo = AutoMapper.UnitTests.Asbjornu.Models.ContactInfo;

namespace AutoMapper.UnitTests.Asbjornu.Mapping
{
	public class ModelMappingProfile : Profile
	{
		public override string ProfileName
		{
			get { return GetType().Name; }
		}
		

		protected internal override void Configure()
		{
			CreateMap<Domain.Customer, CustomerModel>();
			CreateMap<Domain.ContactInfo, ContactInfo>();
		}
	}
}