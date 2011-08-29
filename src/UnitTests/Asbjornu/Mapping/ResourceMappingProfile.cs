using System;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.Asbjornu.Mapping
{
	public class ResourceMappingProfile : Profile
	{
		private readonly IList<IMapper> mappers;


		public ResourceMappingProfile()
		{
			this.mappers = new List<IMapper>
			{
				new ModelsContactInfoToModelsContactInfoMapper(this),
				new RelationsContactInfoToModelsContactInfoMapper(this),
				new CustomerToCustomerResourceMapper(this),
			};
		}


		public override string ProfileName
		{
			get { return GetType().Name; }
		}
		

		protected internal override void Configure()
		{
			if (this.mappers.Count == 0)
				throw new InvalidOperationException("No mappers found to configure.");

			foreach (IMapper mapper in this.mappers)
				mapper.Configure();
		}
	}
}