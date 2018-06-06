using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AutoMapper.UnitTests.Projection
{
    public class SubQueryExpressionMapFromMethod
    {
        [Fact]
        public void Should_not_fail()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ParentReference, ParentReference>();
                cfg.CreateMap<BaseObject, BaseModel>()
                    .ForMember(a => a.CalculatedValue,
                        e => e.MapFrom(f => Calculator.Calculate(f))); // <-- here it crashes
                cfg.CreateMap<MyObject, Model>().IncludeBase<BaseObject, BaseModel>()
                    .ForMember(a => a.Name, e => e.MapFrom(f => f.Name));
                cfg.CreateMap<ChildObject, BaseModelWithParent>().IncludeBase<BaseObject, BaseModel>()
                    .ForMember(a => a.Parent,
                        e => e.MapFrom(f =>
                            f.ParentReferences.FirstOrDefault())); // <-- this line results in the exception
            });
            typeof(ArgumentException).ShouldNotBeThrownBy(() =>
                configuration.ExpressionBuilder.GetMapExpression(typeof(MyObject), typeof(Model),
                    new Dictionary<string, object>(), new MemberInfo[0]));
        }

        public class ParentReference
        {

        }

        public class ParentReferenceModel
        {

        }

        public class ChildObject : BaseObject
        {
            public List<ParentReference> ParentReferences { get; set; } = new List<ParentReference>();
        }

        public class MyObject : BaseObject
        {
            public string Name { get; set; }
            public List<ChildObject> Children { get; set; } = new List<ChildObject>();

        }

        public class Model : BaseModel
        {
            public string Name { get; set; }
            public List<BaseModelWithParent> Children { get; set; } = new List<BaseModelWithParent>();
        }

        public class BaseModelWithParent : BaseModel
        {
            public ParentReferenceModel Parent { get; set; }
        }


        public abstract class BaseObject
        {

        }

        public abstract class BaseModel
        {
            public string CalculatedValue { get; set; }
        }

        public static class Calculator
        {
            public static string Calculate(BaseObject input)
            {
                return "Hello World";
            }
        }
    }
}
