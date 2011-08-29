using AutoMapper.UnitTests.Asbjornu.Domain;
using AutoMapper.UnitTests.Asbjornu.Models;

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
			CreateMap<Customer, CustomerModel>()
				.ForMember(d => d.ContactInfo, o => o.Ignore())
				.ForMember(d => d.Url, o => o.Ignore());

			CreateMap<Models.ContactInfo, Domain.ContactInfo>()
				.ConstructUsing(x => new Domain.ContactInfo());

			CreateMap<Domain.ContactInfo, Models.ContactInfo>();
		}
	}
}