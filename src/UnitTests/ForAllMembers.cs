using System;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.ForAllMembers
{
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
            cfg.ForAllPropertyMaps(pm => pm.DestinationName.StartsWith("Other"),
                (pm, opt) => opt.MapFrom(typeof(ConditionalValueResolver), pm.SourceMember.Name));

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

            dest.SomeDate.ShouldBe(source.SomeDate);
            dest.OtherDate.ShouldBe(source.OtherDate.AddDays(1));
        }
    }
    public class When_conditionally_applying_a_resolver_per_profile : AutoMapperSpecBase
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
        class MyProfile : Profile
        {
            public MyProfile()
            {
                CreateMap<Source, Dest>();
                ForAllPropertyMaps(pm => pm.DestinationName.StartsWith("Other"), (pm, opt) => opt.MapFrom(typeof(ConditionalValueResolver), pm.SourceMember.Name));
            }
        }
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.AddProfile<MyProfile>());
        public class ConditionalValueResolver : IMemberValueResolver<object, object, DateTime, DateTime>
        {
            public DateTime Resolve(object s, object d, DateTime source, DateTime destination, ResolutionContext context) => source.AddDays(1);
        }
        [Fact]
        public void Should_use_resolver()
        {
            var source = new Source { SomeDate = new DateTime(2000, 1, 1), OtherDate = new DateTime(2000, 1, 1) };
            var dest = Mapper.Map<Source, Dest>(source);
            dest.SomeDate.ShouldBe(source.SomeDate);
            dest.OtherDate.ShouldBe(source.OtherDate.AddDays(1));
        }
    }
}