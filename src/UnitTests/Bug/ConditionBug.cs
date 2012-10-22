using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    namespace ConditionBug
    {
        [TestFixture]
        public class Example : AutoMapperSpecBase
        {
            public class SubSource
            {
                public string SubValue { get; set; }
            }

            public class Source
            {
                public Source() { Value = new List<SubSource>(); }
                public List<SubSource> Value { get; set; }
            }

            public class Destination
            {
                public string Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg => cfg.CreateMap<Source, Destination>()
                                             .ForMember(dest => dest.Value, opt =>
                                             {
                                                 opt.Condition(src => src.Value.Count > 1);
                                                 opt.ResolveUsing(src => src.Value[1].SubValue);
                                             }));
            }

            [Test]
            public void Should_skip_the_mapping_when_the_condition_is_false()
            {
                var src = new Source();
                src.Value.Add(new SubSource { SubValue = "x" });
                var destination = Mapper.Map<Source, Destination>(src);

                destination.Value.ShouldBeNull();
            }

            [Test]
            public void Should_execute_the_mapping_when_the_condition_is_true()
            {
                var src = new Source();
                src.Value.Add(new SubSource { SubValue = "x" });
                src.Value.Add(new SubSource { SubValue = "x" });
                var destination = Mapper.Map<Source, Destination>(src);

                destination.Value.ShouldEqual("x");
            }
        }
    }
}
