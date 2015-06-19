#if !WINDOWS_PHONE

namespace AutoMapper.UnitTests.Bug
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class CollectionMapperMapsIEnumerableToISetIncorrectly
    {
        public class TypeWithStringProperty
        {
            public string Value { get; set; }
        }

        public class SourceWithIEnumerable
        {
            public IEnumerable<TypeWithStringProperty> Stuff { get; set; }
        }

        public class TargetWithISet
        {
            public ISet<string> Stuff { get; set; }
        }

        [Fact]
        public void ShouldMapToNewISet()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<SourceWithIEnumerable, TargetWithISet>()
                    .ForMember(dest => dest.Stuff, opt => opt.MapFrom(src => src.Stuff.Select(s => s.Value)));
            });

            var source = new SourceWithIEnumerable
            {
                Stuff = new[]
                {
                    new TypeWithStringProperty {Value = "Microphone"},
                    new TypeWithStringProperty {Value = "Check"},
                    new TypeWithStringProperty {Value = "1, 2"},
                    new TypeWithStringProperty {Value = "What is this?"}
                }
            };

            var target = Mapper.Map<SourceWithIEnumerable, TargetWithISet>(source);
        }
    }
}

#endif