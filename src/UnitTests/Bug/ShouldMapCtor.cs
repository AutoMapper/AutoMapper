using System.Linq;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public abstract class ShouldMapCtor
    {
        public class Internal : NonValidatingSpecBase
        {
            class Destination
            {
                internal Destination(int a, string b)
                {
                }

                public int A { get; }

                public string B { get; }

                public Destination(int a)
                {
                    
                }

                private Destination()
                {
                }
            }

            class Source
            {
                public int A { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.ShouldMapCtor = c => c.IsAssembly;
                cfg.CreateMap<Source, Destination>();
            });

            [Fact]
            public void Should_only_map_internal_ctor()
            {
                var typeMap = Configuration.GetAllTypeMaps()
                    .First(tm => tm.SourceType == typeof(Source));

                typeMap.PassesCtorValidation().ShouldBeFalse();
            }
        }

        public class Private : NonValidatingSpecBase
        {
            
            class Destination
            {
                private Destination(int a, string b)
                {
                }

                public int A { get; }

                public string B { get; }

                internal Destination(int a)
                {
                    
                }

                public Destination()
                {
                }
            }

            class Source
            {
                public int A { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.ShouldMapCtor = c => c.IsPrivate;
                cfg.CreateMap<Source, Destination>();
            });

            [Fact]
            public void Should_only_map_private_ctor()
            {
                var typeMap = Configuration.GetAllTypeMaps()
                    .First(tm => tm.SourceType == typeof(Source));

                typeMap.PassesCtorValidation().ShouldBeFalse();
            }
        }

        public class Public : NonValidatingSpecBase
        {
            
            class Destination
            {
                public Destination(int a, string b)
                {
                }

                public int A { get; }

                public string B { get; }

                internal Destination(int a)
                {
                    
                }

                private Destination()
                {
                }
            }

            class Source
            {
                public int A { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.ShouldMapCtor = c => c.IsPublic;
                cfg.CreateMap<Source, Destination>();
            });

            [Fact]
            public void Should_only_map_public_ctor()
            {
                var typeMap = Configuration.GetAllTypeMaps()
                    .First(tm => tm.SourceType == typeof(Source));

                typeMap.PassesCtorValidation().ShouldBeFalse();
            }
        }
        
        
        public class Default : AutoMapperSpecBase
        {
            
            class Destination
            {
                public Destination(int a, string b)
                {
                }

                public int A { get; }

                public string B { get; }

                private Destination()
                {
                }
            }

            class Source
            {
                public int A { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });

            [Fact]
            public void Should_map_to_a_ctor()
            {
                var typeMap = Configuration.GetAllTypeMaps()
                    .First(tm => tm.SourceType == typeof(Source));

                typeMap.PassesCtorValidation().ShouldBeTrue();
            }
        }
    }
}