namespace AutoMapper.UnitTests.Bug
{
    namespace EnumConditionsBug
    {
        [Flags]
        public enum Enum1 { One, Two };
        [Flags]
        public enum Enum2 { Three, Four }

        public class EnumTestSource
        {
            public IEnumerable<Enum1> Prop1 { get; set; }
            public IEnumerable<Enum2> Prop2 { get; set; }
        }

        public class EnumTestDest
        {
            public Enum1 Prop1 { get; set; }
            public Enum2 Prop2 { get; set; }
        }
        public class EnumMapperTest
        {
            [Fact]
            public void Mapper_respects_condition()
            {
                var _c1Called = false;
                var _c2Called = false;
                var config = new MapperConfiguration(cfg => cfg.CreateMap<EnumTestSource, EnumTestDest>()
                    .ForMember(m => m.Prop1, o =>
                    {
                        o.Condition((_, srcProp, destProp) => { _c1Called = true; return srcProp != null; });
                        o.MapFrom((f, _) => f.Prop1?.Aggregate((current, next) => current | next));
                    })
                    .ForMember(m => m.Prop2, o =>
                    {
                        o.Condition((_, srcProp, destProp) => { _c2Called = true; return srcProp != null; });
                        o.MapFrom((f, _) => f.Prop2?.Aggregate((current, next) => current | next));
                    }));
                var src = new EnumTestSource { Prop1 = new[] { Enum1.One }, Prop2 = null };
                var dest = config.CreateMapper().Map<EnumTestDest>(src); // will throw
                _c1Called.ShouldBeTrue();
                _c2Called.ShouldBeTrue();
            }
        }
    }
}
