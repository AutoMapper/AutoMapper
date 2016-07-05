namespace AutoMapper.UnitTests
{
    namespace ForAllMembers
    {
        using System;
        using Should;
        using Xunit;

        public class When_conditionally_applying_a_resolver_globally : AutoMapperSpecBase
        {
            public class Source
            {
                public DateTime SomeDate { get; set; }
                public DateTime OtherDate { get; set; }
            }

            public class Dest
            {
                public DateTime SomeDate { get; set; }
                public DateTime OtherDate { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.ForAllPropertyMaps(pm => pm.DestinationProperty.Name.StartsWith("Other"), 
                    (pm, opt) => opt.ResolveUsing(typeof(ConditionalValueResolver), pm.SourceMember.Name));

                cfg.CreateMap<Source, Dest>();
            });

            public class ConditionalValueResolver : IMemberValueResolver<object, object, DateTime, DateTime>
            {
                public DateTime Resolve(object s, object d, DateTime source, DateTime destination, ResolutionContext context)
                {
                    return source.AddDays(1);
                }
            }

            [Fact]
            public void Should_use_resolver()
            {
                var source = new Source
                {
                    SomeDate = new DateTime(2000, 1, 1),
                    OtherDate = new DateTime(2000, 1, 1),
                };
                var dest = Mapper.Map<Source, Dest>(source);

                dest.SomeDate.ShouldEqual(source.SomeDate);
                dest.OtherDate.ShouldEqual(source.OtherDate.AddDays(1));
            }
        }
    }
}