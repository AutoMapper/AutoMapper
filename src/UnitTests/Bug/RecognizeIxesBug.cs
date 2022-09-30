namespace AutoMapper.UnitTests.Bug
{
    namespace RecognizeIxesBug
    {
        public class IxesTest : AutoMapperSpecBase
        {
            private Stuff _source;
            private StuffView _dest;

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.RecognizeDestinationPostfixes("CodeKey", "Key");
                cfg.CreateMap<Stuff, StuffView>();
            });

            protected override void Because_of()
            {
                _source = new Stuff
                {
                    Id = 4,
                    Name = "Foo",
                    RankCode = "Bar"
                };
                _dest = Mapper.Map<Stuff, StuffView>(_source);
            }

            [Fact]
            public void Should_recognize_a_full_prefix()
            {
                _dest.IdCodeKey.ShouldBe(_source.Id);
            }

            [Fact]
            public void Should_recognize_a_partial_prefix()
            {
                _dest.NameKey.ShouldBe(_source.Name);
            }

            [Fact]
            public void Should_recognize_a_partial_match_prefix()
            {
                _dest.RankCodeKey.ShouldBe(_source.RankCode);
            }

            public class Stuff
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public string RankCode { get; set; }
            }

            public class StuffView
            {
                public int IdCodeKey { get; set; }
                public string NameKey { get; set; }
                public string RankCodeKey { get; set; }
            }
        }
    }
}