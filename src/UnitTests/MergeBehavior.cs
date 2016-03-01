using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
	namespace MergeBehavior
    {
		public class When_merging_models : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelDto
			{
				public ModelSubDto Sub { get; set; }
                public string PotentialNullString { get; set; }
                public int? PotentialNullInteger { get; set; }
                public int OverridenInteger { get; set; }
            }

			public class ModelSubDto
			{
				public int[] Items { get; set; }
			}

			public class ModelObject
			{
				public ModelSubObject Sub { get; set; }
                public string PotentialNullString { get; set; }
                public int? PotentialNullInteger { get; set; }
                public int OverridenInteger { get; set; }
            }

			public class ModelSubObject
            {
                public int[] Items { get; set; }
			}

		    protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
		    {
		        cfg.AllowNullDestinationValues = false;
		        cfg.CreateMap<ModelObject, ModelDto>();
		        cfg.CreateMap<ModelSubObject, ModelSubDto>();

		    });

		    protected override void Because_of()
            {
                var model1 = new ModelObject
                {
                    PotentialNullString = null,
                    PotentialNullInteger = 1,
                    OverridenInteger = 10,
                    Sub = new ModelSubObject
                    {
                        Items = new []{ 0, 1 }
                    }
                };

                var model2 = new ModelObject
                {
                    PotentialNullString =  string.Empty,
                    PotentialNullInteger = null,
                    OverridenInteger = 11,
                    Sub = new ModelSubObject
                    {
                        Items = new[] { 1, 2 }
                    }
                };

                _result = Mapper.Merge<ModelObject, ModelDto>(model1, model2);
            }

            [Fact]
            public void Null_values_should_be_overridden()
            {
                _result.PotentialNullString.ShouldEqual(string.Empty);
            }

            [Fact]
            public void Not_null_values_should_not_be_overridden_with_nulls()
            {
                _result.PotentialNullInteger.ShouldEqual(1);
            }

            [Fact]
            public void Not_null_values_should_be_overridden_with_not_nulls()
            {
                _result.OverridenInteger.ShouldEqual(11);
            }

            [Fact]
            public void Arrays_should_be_merged()
            {
                _result.Sub.Items.ShouldBeOfLength(4);
                _result.Sub.Items.Where(i => i == 0).ShouldBeOfLength(1);
                _result.Sub.Items.Where(i => i == 1).ShouldBeOfLength(2);
                _result.Sub.Items.Where(i => i == 1).ShouldBeOfLength(1);
            }
        }
	}
}