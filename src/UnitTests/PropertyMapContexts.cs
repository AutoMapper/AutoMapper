using System;
using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
    public class CustomValueResolverIsNotSuppliedWithContextPropertyMap
    {

        public class Source
        {
            public string CodeValue1 { get; set; }
            public string CodeValue2 { get; set; }
        }


        public class DestinationDto
        {
            public CodeValueDto CodeValue1 { get; set; }
            public CodeValueDto CodeValue2 { get; set; }
        }


        public class CodeValueDto
        {
            public string Code { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
        }


        public class CodeValueDtoResolver : IValueResolver
        {
            public ResolutionResult Resolve(ResolutionResult source)
            {
                var propertyMap = source.Context.PropertyMap;

                propertyMap.ShouldNotBeNull();

                var codeValueTypeId = propertyMap.SourceMember.DeclaringType.Name + ":" + propertyMap.SourceMember.Name;
                return source.New(LookupCodeValue(codeValueTypeId, "" + source.Value));
            }

            private CodeValueDto LookupCodeValue(string codeValueTypeId, string code)
            {
                switch (codeValueTypeId + "==" + code)
                {
                    case "Source:CodeValue1==Value1":
                        return new CodeValueDto { Code = "" + code, Title = "lookup value for Value1==1" };
                    case "Source:CodeValue2==Value1":
                        return new CodeValueDto { Code = "" + code, Title = "lookup value for Value2==1" };
                    default:
                        throw new InvalidOperationException();
                }
            }
        }


        [Fact]
        public void CustomValueResolver_Should_Be_Supplied_With_Current_PropertyMap()
        {
            Mapper.CreateMap<Source, DestinationDto>()
              .ForMember(x => x.CodeValue1, o => o.ResolveUsing<CodeValueDtoResolver>().FromMember(y => y.CodeValue1))
              .ForMember(x => x.CodeValue2, o => o.ResolveUsing<CodeValueDtoResolver>().FromMember(y => y.CodeValue2));

            var src = new Source { CodeValue1 = "Value1", CodeValue2 = "Value1" };

            var dest = Mapper.Map<Source, DestinationDto>(src);

            dest.CodeValue1.Title.ShouldEqual("lookup value for Value1==1");
            dest.CodeValue2.Title.ShouldEqual("lookup value for Value2==1");
        }
    }
}