using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class ReverseMapAndReplaceMemberName : AutoMapperSpecBase
    {
        const string SomeId = "someId";
        const string SomeOtherId = "someOtherId";
        private Source _source;
        private Destination _destination;

        class Source
        {
            public string AccountId { get; set; }
        }
        class Destination
        {
            public string UserId { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.ReplaceMemberName("Account", "User");
                cfg.ReplaceMemberName("User", "Account");
                cfg.CreateMap<Source, Destination>().ReverseMap();
            });
        }

        protected override void Because_of()
        {
            _source = Mapper.Map<Destination, Source>(new Destination
            {
                UserId = SomeId
            });
            _destination = Mapper.Map<Source, Destination>(new Source
            {
                AccountId = SomeOtherId
            });
        }

        [Fact]
        public void Should_work_together()
        {
            _source.AccountId.ShouldEqual(SomeId);
            _destination.UserId.ShouldEqual(SomeOtherId);
        }
    }

    public class ReverseMapAndReplaceMemberNameWithProfile : AutoMapperSpecBase
    {
        const string SomeId = "someId";
        const string SomeOtherId = "someOtherId";
        private Source _source;
        private Destination _destination;

        class Source
        {
            public string AccountId { get; set; }
        }

        class Destination
        {
            public string UserId { get; set; }
        }

        class MyProfile : Profile
        {
            protected override void Configure()
            {
                ReplaceMemberName("Account", "User");
                ReplaceMemberName("User", "Account");
                CreateMap<Source, Destination>().ReverseMap();
            }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<MyProfile>();
            });
        }

        protected override void Because_of()
        {
            _source = Mapper.Map<Destination, Source>(new Destination
            {
                UserId = SomeId
            });
            _destination = Mapper.Map<Source, Destination>(new Source
            {
                AccountId = SomeOtherId
            });
        }

        [Fact]
        public void Should_work_together()
        {
            _source.AccountId.ShouldEqual(SomeId);
            _destination.UserId.ShouldEqual(SomeOtherId);
        }
    }
}