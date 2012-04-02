using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    namespace RecognizeIxesBug
    {
        [TestFixture]
        public class IxesTest : AutoMapperSpecBase
        {
            private Stuff _source;
            private StuffView _dest;

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.RecognizeDestinationPostfixes("CodeKey", "Key");
                    cfg.CreateMap<Stuff, StuffView>();
                });
            }

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

            [Test]
            public void Should_recognize_a_full_prefix()
            {
                _dest.IdCodeKey.ShouldEqual(_source.Id);
            }

            [Test]
            public void Should_recognize_a_partial_prefix()
            {
                _dest.NameKey.ShouldEqual(_source.Name);
            }

            [Test]
            public void Should_recognize_a_partial_match_prefix()
            {
                _dest.RankCodeKey.ShouldEqual(_source.RankCode);
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