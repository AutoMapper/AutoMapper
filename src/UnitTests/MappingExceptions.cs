using System;
using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests
{
	namespace MappingExceptions
	{
        public class When_encountering_a_member_mapping_problem_during_mapping : NonValidatingSpecBase
        {
            public class Source
            {
                public string Value { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Dest>();
            }

            [Test]
            public void Should_provide_a_contextual_exception()
            {
                var source = new Source { Value = "adsf" };
                typeof(AutoMapperMappingException).ShouldBeThrownBy(() => Mapper.Map<Source, Dest>(source));
            }

            [Test]
            public void Should_have_contextual_mapping_information()
            {
                var source = new Source { Value = "adsf" };
                AutoMapperMappingException thrown = null;
                try
                {
                    Mapper.Map<Source, Dest>(source);
                }
                catch (AutoMapperMappingException ex)
                {
                    thrown = ex;
                }
                thrown.ShouldNotBeNull();
            }
        }

        [Explicit]
        public class When_encountering_a_complex_deep_error : NonValidatingSpecBase
        {
            public class Source
            {
                public SubSource Sub { get; set; }
            }

            public class SubSource
            {
                public SubSubSource OtherSub { get; set; }
            }

            public class SubSubSource
            {
                public SubSubSubSource[] Values { get; set; }
            }

            public class SubSubSubSource
            {
                public int Foo { get; set; }
            }

            public class Dest
            {
                public SubDest Sub { get; set; }
            }

            public class SubDest
            {
                public SubSubDest OtherSub { get; set; }
            }

            public class SubSubDest
            {
                public SubSubSubDest[] Values { get; set; }
            }

            public class SubSubSubDest
            {
                public int Foo { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Dest>();
                Mapper.CreateMap<SubSource, SubDest>();
                Mapper.CreateMap<SubSubSource, SubSubDest>();
                Mapper.CreateMap<SubSubSubSource, SubSubSubDest>()
                    .ForMember(dest => dest.Foo, opt => opt.ResolveUsing<MyCoolResolver>());
            }

            public class MyCoolResolver : ValueResolver<SubSubSubSource, int>
            {
                private static int Count = 0;

                protected override int ResolveCore(SubSubSubSource source)
                {
                    Count++;
                    if (Count > 3)
                        throw new Exception("Oh noes");
                    return Count;
                }
            }

            [Test]
            public void Should_provide_a_contextual_exception()
            {
                var source = new Source
                {
                    Sub = new SubSource
                    {
                        OtherSub = new SubSubSource
                        {
                            Values = new[]
                            {
                                new SubSubSubSource
                                {
                                    Foo = 5,
                                },
                                new SubSubSubSource
                                {
                                    Foo = 5,
                                },
                                new SubSubSubSource
                                {
                                    Foo = 5,
                                },
                                new SubSubSubSource
                                {
                                    Foo = 5,
                                },
                                new SubSubSubSource
                                {
                                    Foo = 5,
                                },
                                new SubSubSubSource
                                {
                                    Foo = 5,
                                },
                            },
                        }
                    }
                };
                Mapper.Map<Source, Dest>(source);
            }
        }
    }
}