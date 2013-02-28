using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
	[TestFixture]
	class MappingInheritance
	{
		private Entity testEntity;
		private EditModel testModel;

		[SetUp]
		public void Prepare_common_staff()
		{
			Mapper.Reset();

			testEntity = new Entity
			{
				Value1 = 1,
				Value2 = 2,
			};
		}

		[Test]
		public void AutoMapper_should_map_derived_types_properly()
		{
			Mapper.CreateMap<Entity, BaseModel>()
				.ForMember(model => model.Value1, mce => mce.MapFrom(entity => entity.Value2))
				.ForMember(model => model.Value2, mce => mce.MapFrom(entity => entity.Value1))
				.Include<Entity, EditModel>();
			Mapper.CreateMap<Entity, EditModel>()
				.ForMember(model => model.Value3, mce => mce.MapFrom(entity => entity.Value1 + entity.Value2));

			testModel = Mapper.Map<Entity, EditModel>(testEntity);
		}

		[Test]
		public void AutoMapper_should_map_derived_types_properly_2()
		{
			Mapper.CreateMap<Entity, BaseModel>()
				.ForMember(model => model.Value1, mce => mce.MapFrom(entity => entity.Value2))
				.ForMember(model => model.Value2, mce => mce.MapFrom(entity => entity.Value1))
				.Include<Entity, EditModel>()
				.Include<Entity, ViewModel>();
			Mapper.CreateMap<Entity, EditModel>()
				.ForMember(model => model.Value3, mce => mce.MapFrom(entity => entity.Value1 + entity.Value2));
			Mapper.CreateMap<Entity, ViewModel>();

			testModel = Mapper.Map<Entity, EditModel>(testEntity);
		}

		[TearDown]
		public void Verify_mapping_results()
		{
			Assert.AreEqual(testEntity.Value1, testModel.Value2);
			Assert.AreEqual(testEntity.Value2, testModel.Value1);
			Assert.AreEqual(testEntity.Value1 + testEntity.Value2, testModel.Value3);
		}
	}

	class Entity
	{
		public int Value1 { get; set; }
		public int Value2 { get; set; }
	}

	class BaseModel
	{
		public int Value1 { get; set; }
		public int Value2 { get; set; }
	}

	class EditModel : BaseModel
	{
		public int Value3 { get; set; }
	}

	class ViewModel : BaseModel { }
}
