using Should;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
#if SILVERLIGHT
    [Ignore("Not supported in Silverlight 4")]
#endif
    public class EnumMatchingOnValue : AutoMapperSpecBase
    {
        private SecondClass _result;

        public class FirstClass
        {
            public FirstEnum EnumValue { get; set; }
        }

        public enum FirstEnum
        {
            NamedEnum = 1,
            SecondNameEnum = 2
        }

        public class SecondClass
        {
            public SecondEnum EnumValue { get; set; }
        }

        public enum SecondEnum
        {
            DifferentNamedEnum = 1,
            SecondNameEnum = 2
        }
        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<FirstClass, SecondClass>();
            });
        }

        protected override void Because_of()
        {
            var source = new FirstClass
            {
                EnumValue = FirstEnum.NamedEnum
            };
            _result = Mapper.Map<FirstClass, SecondClass>(source);
        }

        [Test]
        public void Should_match_on_the_name_even_if_values_match()
        {
            _result.EnumValue.ShouldEqual(SecondEnum.DifferentNamedEnum);
        }
    }
}