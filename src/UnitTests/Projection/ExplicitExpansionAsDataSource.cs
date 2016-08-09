namespace AutoMapper.UnitTests.Projection
{
    using System.Linq;
    using QueryableExtensions;
    using Should;
    using Xunit;

    public class ExplicitExpansionAsDataSource : AutoMapperSpecBase
    {
        private Dest[] _dests;

        public class Source
        {
            public ChildSource Child1 { get; set; }
            public ChildSource Child2 { get; set; }
            public ChildSource Child3 { get; set; }
            public ChildSource Child4 { get; set; }
        }

        public class ChildSource
        {
            public GrandChildSource GrandChild { get; set; }
        }

        public class GrandChildSource
        {
            
        }

        public class Dest
        {
            public ChildDest Child1 { get; set; }
            public ChildDest Child2 { get; set; }
            public ChildDest Child3 { get; set; }
            public ChildDest Child4 { get; set; }
        }

        public class ChildDest
        {
            public GrandChildDest GrandChild { get; set; }
        }

        public class GrandChildDest
        {
            
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {

            cfg.CreateMap<Source, Dest>()
                .ForMember(m => m.Child1, opt => opt.ExplicitExpansion())
                .ForMember(m => m.Child2, opt => opt.ExplicitExpansion())
                .ForMember(m => m.Child4, opt => opt.ExplicitExpansion())
                ;
            cfg.CreateMap<ChildSource, ChildDest>()
                .ForMember(m => m.GrandChild, opt => opt.ExplicitExpansion());

            cfg.CreateMap<GrandChildSource, GrandChildDest>();
        });

        protected override void Because_of()
        {
            var sourceList = new[]
            {
                new Source
                {
                    Child1 = new ChildSource(),
                    Child2 = new ChildSource(),
                    Child3 = new ChildSource(),
                    Child4 = new ChildSource()
                    {
                        GrandChild = new GrandChildSource()
                    }
                }
            };

            _dests = sourceList.AsQueryable().UseAsDataSource(Configuration).For<Dest>(d => d.Child2, d => d.Child4, d => d.Child4.GrandChild).ToArray();
        }

        [Fact]
        public void Should_leave_non_expanded_item_null()
        {
            _dests[0].Child1.ShouldBeNull();
        }

        [Fact]
        public void Should_expand_explicitly_expanded_item()
        {
            _dests[0].Child2.ShouldNotBeNull();
        }

        [Fact]
        public void Should_default_to_expand()
        {
            _dests[0].Child3.ShouldNotBeNull();
        }
        
        [Fact]
        public void Should_expand_full_path()
        {
            _dests[0].Child4.ShouldNotBeNull();
            _dests[0].Child4.GrandChild.ShouldNotBeNull();
        }

    }


    public class ExplicitExpansion_WithElementOperator_AsDataSource : AutoMapperSpecBase
    {
        private Dest _dest;

        public class Source
        {
            public ChildSource Child1 { get; set; }
            public ChildSource Child2 { get; set; }
            public ChildSource Child3 { get; set; }
            public ChildSource Child4 { get; set; }
        }

        public class ChildSource
        {
            public GrandChildSource GrandChild { get; set; }
        }

        public class GrandChildSource
        {

        }

        public class Dest
        {
            public ChildDest Child1 { get; set; }
            public ChildDest Child2 { get; set; }
            public ChildDest Child3 { get; set; }
            public ChildDest Child4 { get; set; }
        }

        public class ChildDest
        {
            public GrandChildDest GrandChild { get; set; }
        }

        public class GrandChildDest
        {

        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(m => m.Child1, opt => opt.ExplicitExpansion())
                .ForMember(m => m.Child2, opt => opt.ExplicitExpansion())
                .ForMember(m => m.Child4, opt => opt.ExplicitExpansion())
                ;
            cfg.CreateMap<ChildSource, ChildDest>()
                .ForMember(m => m.GrandChild, opt => opt.ExplicitExpansion());

            cfg.CreateMap<GrandChildSource, GrandChildDest>();
        });

        protected override void Because_of()
        {
            var sourceList = new[]
            {
                new Source
                {
                    Child1 = new ChildSource(),
                    Child2 = new ChildSource(),
                    Child3 = new ChildSource(),
                    Child4 = new ChildSource()
                    {
                        GrandChild = new GrandChildSource()
                    }
                }
            };

            _dest = sourceList.AsQueryable().UseAsDataSource(Configuration).For<Dest>(d => d.Child2, d => d.Child4, d => d.Child4.GrandChild).FirstOrDefault();
        }

        [Fact]
        public void Should_leave_non_expanded_item_null()
        {
            _dest.Child1.ShouldBeNull();
        }

        [Fact]
        public void Should_expand_explicitly_expanded_item()
        {
            _dest.Child2.ShouldNotBeNull();
        }

        [Fact]
        public void Should_default_to_expand()
        {
            _dest.Child3.ShouldNotBeNull();
        }

        [Fact]
        public void Should_expand_full_path()
        {
            _dest.Child4.ShouldNotBeNull();
            _dest.Child4.GrandChild.ShouldNotBeNull();
        }
    }
}