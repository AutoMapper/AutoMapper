using System;
using System.Globalization;
using Should;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace CustomFormatters
	{
		public class When_applying_global_formatting_rules : AutoMapperSpecBase
		{
			private ModelDto _modelDto;

			public class HardEncoder : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return string.Format("Hard {0}", context.SourceValue);
				}
			}

			public class SoftEncoder : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return string.Format("{0} {1} Soft", context.SourceValue, context.MemberName);
				}
			}

			public class RokkenEncoder : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return string.Format("{0} Rokken", context.SourceValue);
				}
			}
			
			public class ModelDto
			{
				public string Value { get; set; }
			}

			public class ModelObject
			{
				public int Value { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.AddFormatter<HardEncoder>();
				Mapper.AddFormatter(new SoftEncoder());
				Mapper.AddFormatter(typeof(RokkenEncoder));
				Mapper.AddFormatExpression(context => context.SourceValue + " Medium");

				Mapper.CreateMap<ModelObject, ModelDto>();

				var modelObject = new ModelObject { Value = 14 };

				_modelDto = Mapper.Map<ModelObject, ModelDto>(modelObject);
			}

			[Test]
			public void It_formats_the_values_in_the_order_declared()
			{
				_modelDto.Value.ShouldEqual("Hard 14 Value Soft Rokken Medium");
			}
		}

		public class When_applying_type_specific_global_formatting_rules : AutoMapperSpecBase
		{
			private ModelDto _result;
		    private string _intResult;

		    public class ModelDto
			{
				public string StartDate { get; set; }
				public string OtherValue { get; set; }
			}

			public class ModelObject
			{
				public DateTime StartDate { get; set; }
				public int OtherValue { get; set; }
			}

			public class ShortDateFormatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return ((DateTime)context.SourceValue).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
				}
			}

			protected override void Establish_context()
			{
                Mapper.Initialize(cfg =>
                {
                    cfg.ForSourceType<DateTime>().AddFormatter<ShortDateFormatter>();
                    cfg.ForSourceType<int>().AddFormatExpression(context => ((int)context.SourceValue + 1).ToString());
                    
                    cfg.CreateMap<ModelObject, ModelDto>();
                });

				var model = new ModelObject { StartDate = new DateTime(2004, 12, 25), OtherValue = 43 };

				_result = Mapper.Map<ModelObject, ModelDto>(model);

			    _intResult = Mapper.Map<int, string>(44);
			}

			[Test]
			public void Should_format_using_concrete_formatter_class()
			{
				_result.StartDate.ShouldEqual("12/25/2004");
			}

			[Test]
			public void Should_format_using_custom_expression_formatter()
			{
				_result.OtherValue.ShouldEqual("44");
			}

		    [Test]
		    public void Should_use_formatter_outside_of_type_map()
		    {
		        _intResult.ShouldEqual("45");
		    }
		}

		public class When_applying_type_specific_and_general_global_formatting_rules : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelDto
			{
				public string OtherValue { get; set; }
			}

			public class ModelObject
			{
				public int OtherValue { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.AddFormatExpression(context => string.Format("{0} Value", context.SourceValue));
				Mapper.ForSourceType<int>().AddFormatExpression(context => ((int)context.SourceValue + 1).ToString());

				Mapper.CreateMap<ModelObject, ModelDto>();

				var model = new ModelObject { OtherValue = 43 };

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_apply_the_type_specific_formatting_first_then_global_formatting()
			{
				_result.OtherValue.ShouldEqual("44 Value");
			}
		}

		public class When_resetting_the_global_formatting : AutoMapperSpecBase
		{
			private ModelDto _modelDto;

			public class CrazyEncoder : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return "Crazy!!!";
				}
			}

			public class ModelDto
			{
				public string Value { get; set; }
			}

			public class ModelObject
			{
				public int Value { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.AddFormatter<CrazyEncoder>();

				Mapper.Reset();

				Mapper.CreateMap<ModelObject, ModelDto>();

				var modelObject = new ModelObject { Value = 14 };

				_modelDto = Mapper.Map<ModelObject, ModelDto>(modelObject);
			}

			[Test]
			public void Should_not_apply_the_global_formatting()
			{
				_modelDto.Value.ShouldEqual("14");
			}
		}

		public class When_skipping_a_specific_property_formatting : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelObject
			{
				public int ValueOne { get; set; }
				public int ValueTwo { get; set; }
			}

			public class ModelDto
			{
				public string ValueOne { get; set; }
				public string ValueTwo { get; set; }
			}

			public class SampleFormatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return "Value " + context.SourceValue;
				}
			}

			protected override void Establish_context()
			{
				Mapper.ForSourceType<int>().AddFormatter<SampleFormatter>();

				Mapper
					.CreateMap<ModelObject, ModelDto>()
					.ForMember(d => d.ValueTwo, opt => opt.SkipFormatter<SampleFormatter>());

				var model = new ModelObject { ValueOne = 24, ValueTwo = 42 };

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_preserve_the_existing_formatter()
			{
				_result.ValueOne.ShouldEqual("Value 24");
			}

			[Test]
			public void Should_not_format_using_the_skipped_formatter()
			{
				_result.ValueTwo.ShouldEqual("42");
			}
		}

		public class When_skipping_a_specific_type_formatting : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelObject
			{
				public int ValueOne { get; set; }
			}

			public class ModelDto
			{
				public string ValueOne { get; set; }
			}

			public class SampleFormatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return "Value " + context.SourceValue;
				}
			}

			protected override void Establish_context()
			{
				Mapper.AddFormatter<SampleFormatter>();
				Mapper.ForSourceType<int>().SkipFormatter<SampleFormatter>();

				Mapper.CreateMap<ModelObject, ModelDto>();

				var model = new ModelObject { ValueOne = 24 };

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_not_apply_the_skipped_formatting()
			{
				_result.ValueOne.ShouldEqual("24");
			}
		}

		public class When_configuring_formatting_for_a_specific_member : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelObject
			{
				public int ValueOne { get; set; }
			}

			public class ModelDto
			{
				public string ValueOne { get; set; }
			}

			public class SampleFormatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return "Value " + context.SourceValue;
				}
			}

			protected override void Establish_context()
			{
				Mapper
					.CreateMap<ModelObject, ModelDto>()
					.ForMember(dto => dto.ValueOne, opt => opt.AddFormatter<SampleFormatter>());

				var model = new ModelObject { ValueOne = 24 };

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_apply_formatting_to_that_member()
			{
				_result.ValueOne.ShouldEqual("Value 24");
			}
		}

		public class When_substituting_a_specific_value_for_nulls : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelObject
			{
				public string ValueOne { get; set; }
			}

			public class ModelDto
			{
				public string ValueOne { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper
					.CreateMap<ModelObject, ModelDto>()
					.ForMember(dto => dto.ValueOne, opt => opt.NullSubstitute("I am null"));
				
				var model = new ModelObject { ValueOne = null };

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_replace_the_null_value_with_the_substitute()
			{
				_result.ValueOne.ShouldEqual("I am null");
			}
		}
	
		public class When_using_a_custom_contruction_method_for_formatters : AutoMapperSpecBase
		{
			private Dest _result;

			public class Source { public int Value { get; set; } }
			public class Dest { public string Value { get; set; } }

			public class CustomFormatter : IValueFormatter
			{
				private int _toAdd;

				public CustomFormatter()
				{
					_toAdd = 7;
				}

				public CustomFormatter(int toAdd)
				{
					_toAdd = toAdd;
				}

				public string FormatValue(ResolutionContext context)
				{
					return (((int) context.SourceValue) + _toAdd).ToString();
				}
			}

			public class OtherCustomFormatter : IValueFormatter
			{
				private string _toAppend;

				public OtherCustomFormatter()
				{
					_toAppend = " Blarg";
				}

				public OtherCustomFormatter(string toAppend)
				{
					_toAppend = toAppend;
				}

				public string FormatValue(ResolutionContext context)
				{
					return context.SourceValue + _toAppend;
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Dest>();
				Mapper.AddFormatter<CustomFormatter>().ConstructedBy(() => new CustomFormatter(10));
				Mapper.AddFormatter(typeof(OtherCustomFormatter)).ConstructedBy(() => new OtherCustomFormatter(" Splorg"));
			}

			protected override void Because_of()
			{
				var source = new Source { Value = 10};
				_result = Mapper.Map<Source, Dest>(source);
			}

			[Test]
			public void Should_apply_the_constructor_specified()
			{
				_result.Value.ShouldEqual("20 Splorg");
			}
		}

		public class When_using_a_global_custom_construction_method_for_formatters : AutoMapperSpecBase
		{
			private Destination _result;

			public class Source
			{
				public int Value { get; set; }
			}

			public class Destination
			{
				public string Value { get; set; }
			}

			public class SomeFormatter : IValueFormatter
			{
				private readonly string _prefix = "asdf";

				public SomeFormatter() {}

				public SomeFormatter(string prefix)
				{
					_prefix = prefix;
				}

				public string FormatValue(ResolutionContext context)
				{
					return _prefix + context.SourceValue;
				}
			}

			protected override void Establish_context()
			{
				Mapper.Initialize(cfg => cfg.ConstructServicesUsing(type => new SomeFormatter("ctor'd")));
				Mapper.CreateMap<Source, Destination>()
					.ForMember(d => d.Value, opt => opt.AddFormatter<SomeFormatter>());
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Source, Destination>(new Source {Value = 5});
			}

			[Test]
			public void Should_use_the_global_construction_method_for_creation()
			{
				_result.Value.ShouldEqual("ctor'd5");
			}
		}

	}

}