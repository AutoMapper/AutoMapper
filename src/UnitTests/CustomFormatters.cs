using System;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace CustomFormatters
	{
		public class When_applying_global_formatting_rules : AutoMapperSpecBase
		{
			private ModelDto _modelDto;

			private class HardEncoder : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return string.Format("Hard {0}", context.SourceValue);
				}
			}

			private class SoftEncoder : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return string.Format("{0} {1} Soft", context.SourceValue, context.MemberName);
				}
			}

			private class RokkenEncoder : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return string.Format("{0} Rokken", context.SourceValue);
				}
			}
			
			private class ModelDto
			{
				public string Value { get; set; }
			}

			private class ModelObject
			{
				public int Value { get; set; }
			}

			protected override void Establish_context()
			{
				AutoMapper.AddFormatter<HardEncoder>();
				AutoMapper.AddFormatter(new SoftEncoder());
				AutoMapper.AddFormatter(typeof(RokkenEncoder));
				AutoMapper.AddFormatExpression(context => context.SourceValue + " Medium");

				AutoMapper.CreateMap<ModelObject, ModelDto>();

				var modelObject = new ModelObject { Value = 14 };

				_modelDto = AutoMapper.Map<ModelObject, ModelDto>(modelObject);
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

			private class ModelDto
			{
				public string StartDate { get; set; }
				public string OtherValue { get; set; }
			}

			private class ModelObject
			{
				public DateTime StartDate { get; set; }
				public int OtherValue { get; set; }
			}

			private class ShortDateFormatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return ((DateTime)context.SourceValue).ToString("MM/dd/yyyy");
				}
			}

			protected override void Establish_context()
			{
				AutoMapper.ForSourceType<DateTime>().AddFormatter<ShortDateFormatter>();
				AutoMapper.ForSourceType<int>().AddFormatExpression(context => ((int)context.SourceValue + 1).ToString());

				AutoMapper.CreateMap<ModelObject, ModelDto>();

				var model = new ModelObject { StartDate = new DateTime(2004, 12, 25), OtherValue = 43 };

				_result = AutoMapper.Map<ModelObject, ModelDto>(model);
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
		}

		public class When_applying_type_specific_and_general_global_formatting_rules : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelDto
			{
				public string OtherValue { get; set; }
			}

			private class ModelObject
			{
				public int OtherValue { get; set; }
			}

			protected override void Establish_context()
			{
				AutoMapper.AddFormatExpression(context => string.Format("{0} Value", context.SourceValue));
				AutoMapper.ForSourceType<int>().AddFormatExpression(context => ((int)context.SourceValue + 1).ToString());

				AutoMapper.CreateMap<ModelObject, ModelDto>();

				var model = new ModelObject { OtherValue = 43 };

				_result = AutoMapper.Map<ModelObject, ModelDto>(model);
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

			private class CrazyEncoder : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return "Crazy!!!";
				}
			}

			private class ModelDto
			{
				public string Value { get; set; }
			}

			private class ModelObject
			{
				public int Value { get; set; }
			}

			protected override void Establish_context()
			{
				AutoMapper.AddFormatter<CrazyEncoder>();

				AutoMapper.Reset();

				AutoMapper.CreateMap<ModelObject, ModelDto>();

				var modelObject = new ModelObject { Value = 14 };

				_modelDto = AutoMapper.Map<ModelObject, ModelDto>(modelObject);
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

			private class ModelObject
			{
				public int ValueOne { get; set; }
				public int ValueTwo { get; set; }
			}

			private class ModelDto
			{
				public string ValueOne { get; set; }
				public string ValueTwo { get; set; }
			}

			private class SampleFormatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return "Value " + context.SourceValue;
				}
			}

			protected override void Establish_context()
			{
				AutoMapper.ForSourceType<int>().AddFormatter<SampleFormatter>();

				AutoMapper
					.CreateMap<ModelObject, ModelDto>()
					.ForMember(d => d.ValueTwo, opt => opt.SkipFormatter<SampleFormatter>());

				var model = new ModelObject { ValueOne = 24, ValueTwo = 42 };

				_result = AutoMapper.Map<ModelObject, ModelDto>(model);
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

			private class ModelObject
			{
				public int ValueOne { get; set; }
			}

			private class ModelDto
			{
				public string ValueOne { get; set; }
			}

			private class SampleFormatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return "Value " + context.SourceValue;
				}
			}

			protected override void Establish_context()
			{
				AutoMapper.AddFormatter<SampleFormatter>();
				AutoMapper.ForSourceType<int>().SkipFormatter<SampleFormatter>();

				AutoMapper.CreateMap<ModelObject, ModelDto>();

				var model = new ModelObject { ValueOne = 24 };

				_result = AutoMapper.Map<ModelObject, ModelDto>(model);
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

			private class ModelObject
			{
				public int ValueOne { get; set; }
			}

			private class ModelDto
			{
				public string ValueOne { get; set; }
			}

			private class SampleFormatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return "Value " + context.SourceValue;
				}
			}

			protected override void Establish_context()
			{
				AutoMapper
					.CreateMap<ModelObject, ModelDto>()
					.ForMember(dto => dto.ValueOne, opt => opt.AddFormatter<SampleFormatter>());

				var model = new ModelObject { ValueOne = 24 };

				_result = AutoMapper.Map<ModelObject, ModelDto>(model);
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

			private class ModelObject
			{
				public string ValueOne { get; set; }
			}

			private class ModelDto
			{
				public string ValueOne { get; set; }
			}

			protected override void Establish_context()
			{
				AutoMapper
					.CreateMap<ModelObject, ModelDto>()
					.ForMember(dto => dto.ValueOne, opt => opt.FormatNullValueAs("I am null"));
				
				var model = new ModelObject { ValueOne = null };

				_result = AutoMapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_replace_the_null_value_with_the_substitute()
			{
				_result.ValueOne.ShouldEqual("I am null");
			}
		}
	
	}

}