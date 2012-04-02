using System.Collections.Generic;
using System.Collections.ObjectModel;
using Should;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace NullBehavior
	{
		public class When_mapping_a_model_with_null_items : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelDto
			{
				public ModelSubDto Sub { get; set; }
				public int SubSomething { get; set; }
				public string NullString { get; set; }
			}

			public class ModelSubDto
			{
				public int[] Items { get; set; }
			}

			public class ModelObject
			{
				public ModelSubObject Sub { get; set; }
				public string NullString { get; set; }
			}

			public class ModelSubObject
			{
				public int[] GetItems()
				{
					return new[] { 0, 1, 2, 3 };
				}

				public int Something { get; set; }
			}

			protected override void Establish_context()
			{
				var model = new ModelObject();
				model.Sub = null;

				Mapper.AllowNullDestinationValues = false;
				Mapper.CreateMap<ModelObject, ModelDto>();
				Mapper.CreateMap<ModelSubObject, ModelSubDto>();

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_populate_dto_items_with_a_value()
			{
				_result.Sub.ShouldNotBeNull();
			}

			[Test]
			public void Should_provide_empty_array_for_array_type_values()
			{
				_result.Sub.Items.ShouldNotBeNull();
			}

			[Test]
			public void Should_return_default_value_of_property_in_the_chain()
			{
				_result.SubSomething.ShouldEqual(0);
			}
	
            [Test]
			public void Default_value_for_string_should_be_empty()
			{
				_result.NullString.ShouldEqual(string.Empty);
			}
        }

		public class When_overriding_null_behavior_with_null_source_items : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelDto
			{
				public ModelSubDto Sub { get; set; }
				public int SubSomething { get; set; }
				public string NullString { get; set; }
			}

			public class ModelSubDto
			{
				public int[] Items { get; set; }
			}

			public class ModelObject
			{
				public ModelSubObject Sub { get; set; }
				public string NullString { get; set; }
			}

			public class ModelSubObject
			{
				public int[] GetItems()
				{
					return new[] { 0, 1, 2, 3 };
				}

				public int Something { get; set; }
			}

			protected override void Establish_context()
			{
				var model = new ModelObject();
				model.Sub = null;
				model.NullString = null;

				Mapper.Initialize(c => c.AllowNullDestinationValues = true);
				Mapper.CreateMap<ModelObject, ModelDto>();
				Mapper.CreateMap<ModelSubObject, ModelSubDto>();

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_map_first_level_items_as_null()
			{
				_result.NullString.ShouldBeNull();
			}

			[Test]
			public void Should_map_primitive_items_as_default()
			{
				_result.SubSomething.ShouldEqual(0);
			}

			[Test]
			public void Should_map_any_sub_mapped_items_as_null()
			{
				_result.Sub.ShouldBeNull();
			}
		}

		public class When_overriding_null_behavior_in_a_profile : AutoMapperSpecBase
		{
			private DefaultDestination _defaultResult;
			private NullDestination _nullResult;

			public class DefaultSource
			{
				public object Value { get; set; }
			}

			public class DefaultDestination
			{
				public object Value { get; set; }
			}

			public class NullSource
			{
				public object Value { get; set; }
			}

			public class NullDestination
			{
				public object Value { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateProfile("MapsNulls", p =>
					{
						p.AllowNullDestinationValues = false;
						p.CreateMap<NullSource, NullDestination>();
					});
				Mapper.CreateMap<DefaultSource, DefaultDestination>();
			}

			protected override void Because_of()
			{
				_defaultResult = Mapper.Map<DefaultSource, DefaultDestination>(new DefaultSource());
				_nullResult = Mapper.Map<NullSource, NullDestination>(new NullSource());
			}

			[Test]
			public void Should_use_default_behavior_in_default_profile()
			{
				_defaultResult.Value.ShouldBeNull();
			}

			[Test]
			public void Should_use_overridden_null_behavior_in_profile()
			{
				_nullResult.Value.ShouldNotBeNull();
			}
		}

		public class When_using_a_custom_resolver_and_the_source_value_is_null : NonValidatingSpecBase
		{
			public class NullResolver : ValueResolver<Source, string>
			{
				protected override string ResolveCore(Source source)
				{
					if (source == null)
						return "jon";
					return "fail";
				}
			}

			private Source _source;
			private Destination _dest;

			public class Source
			{
				public string MyName { get; set; }
			}

			public class Destination
			{
				public string Name { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>()
					.ForMember(dest => dest.Name, opt => opt.ResolveUsing<NullResolver>().FromMember(src => src.MyName));
				_source = new Source();
			}

			protected override void Because_of()
			{
				_dest = Mapper.Map<Source, Destination>(_source);
			}

			[Test]
			public void Should_perform_the_translation()
			{
				_dest.Name.ShouldEqual("jon");
			}
		}

	    public class When_mapping_using_a_custom_member_mapping_and_source_is_null : AutoMapperSpecBase
	    {
	        private Dest _dest;

	        public class Source
            {
                public SubSource Sub { get; set; }
            }

            public class SubSource
            {
                public int Value { get; set; }
            }

            public class Dest
            {
                public int OtherValue { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.AllowNullDestinationValues = false;
                Mapper.CreateMap<Source, Dest>()
                    .ForMember(dest => dest.OtherValue, opt => opt.MapFrom(src => src.Sub.Value));
            }

            protected override void Because_of()
            {
                _dest = Mapper.Map<Source, Dest>(new Source());
            }

	        [Test]
	        public void Should_map_to_null_on_destination_values()
	        {
	            _dest.OtherValue.ShouldEqual(0);
	        }
	    }

	    public class When_specifying_a_resolver_for_a_nullable_type : SpecBase
	    {
	        private FooViewModel _result;

	        public class NullableBoolToLabel : TypeConverter<bool?, string>
            {
                protected override string ConvertCore(bool? source)
                {
                    if (source.HasValue)
                    {
                        if (source.Value)
                            return "Yes";
                        else
                            return "No";
                    }
                    else
                        return "(n/a)";
                }
            }

            public class Foo
            {
                public bool? IsFooBarred { get; set; }
            }

            public class FooViewModel
            {
                public string IsFooBarred { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<bool?, string>().ConvertUsing<NullableBoolToLabel>();
                    cfg.CreateMap<Foo, FooViewModel>();
                });
            }

            protected override void Because_of()
            {
                var foo3 = new Foo { IsFooBarred = null };
                _result = Mapper.Map<Foo, FooViewModel>(foo3);
            }

	        [Test]
	        public void Should_allow_the_resolver_to_handle_null_values()
	        {
                _result.IsFooBarred.ShouldEqual("(n/a)");
            }
	    }

        public class When_overriding_collection_null_behavior : AutoMapperSpecBase
        {
            private Dest _dest;

            public class Source
            {
                public IEnumerable<int> Values1 { get; set; }
                public List<int> Values2 { get; set; }
                public Dictionary<string, int> Values3 { get; set; }
                public int[] Values4 { get; set; }
                public ReadOnlyCollection<int> Values5 { get; set; }
                public Collection<int> Values6 { get; set; }
            }

            public class Dest
            {
                public IEnumerable<int> Values1 { get; set; }
                public List<int> Values2 { get; set; }
                public Dictionary<string, int> Values3 { get; set; }
                public int[] Values4 { get; set; }
                public ReadOnlyCollection<int> Values5 { get; set; }
                public Collection<int> Values6 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Dest>();
                    cfg.AllowNullCollections = true;
                });
            }

            protected override void Because_of()
            {
                _dest = Mapper.Map<Source, Dest>(new Source());
            }

            [Test]
            public void Should_allow_null_ienumerables()
            {
                _dest.Values1.ShouldBeNull();
            }

            [Test]
            public void Should_allow_null_lists()
            {
                _dest.Values2.ShouldBeNull();
            }

            [Test]
            public void Should_allow_null_dictionaries()
            {
                _dest.Values3.ShouldBeNull();
            }

            [Test]
            public void Should_allow_null_arrays()
            {
                _dest.Values4.ShouldBeNull();
            }

            [Test]
            public void Should_allow_null_read_only_collections()
            {
                _dest.Values5.ShouldBeNull();
            }

            [Test]
            public void Should_allow_null_collections()
            {
                _dest.Values6.ShouldBeNull();
            }
        }
	}
}