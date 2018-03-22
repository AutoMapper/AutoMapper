using Xunit;
using Shouldly;
using System;
using System.Linq;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper.UnitTests
{
    namespace MappingOrder
    {
        public class When_specifying_a_mapping_order_for_base_members : AutoMapperSpecBase
        {
            Destination _destination;

            class Source
            {
                public string One { get; set; }

                public string Two { get; set; }
            }

            class SourceChild : Source
            {
            }

            class Destination
            {
                // must be defined before property "One" to fail
                public string Two { get; set; }

                private string one;

                public string One
                {
                    get
                    {
                        return this.one;
                    }
                    set
                    {
                        this.one = value;
                        this.Two = value;
                    }
                }
            }

            protected override void Because_of()
            {
                _destination = Mapper.Map<Destination>(new SourceChild { One = "first", Two = "second" });
            }

            [Fact]
            public void Should_inherit_the_mapping_order()
            {
                _destination.Two.ShouldBe("first");
            }

            protected override MapperConfiguration Configuration
            {
                get
                {
                    return new MapperConfiguration(cfg =>
                    {
                        cfg.CreateMap<Source, Destination>()
                         .Include<SourceChild, Destination>()
                         .ForMember(dest => dest.One, opt => opt.SetMappingOrder(600))
                         .ForMember(dest => dest.Two, opt => opt.SetMappingOrder(-500));

                        cfg.CreateMap<SourceChild, Destination>();
                    });
                }
            }
        }

        public class When_specifying_a_mapping_order_for_source_members : AutoMapperSpecBase
        {
            private Destination _result;

            public class Source
            {
                private int _startValue;

                public Source(int startValue)
                {
                    _startValue = startValue;
                }

                public int GetValue1()
                {
                    _startValue += 10;
                    return _startValue;
                }

                public int GetValue2()
                {
                    _startValue += 5;
                    return _startValue;
                }
            }

            public class Destination
            {
                public int Value1 { get; set; }
                public int Value2 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>()
                    .ForMember(src => src.Value1, opt => opt.SetMappingOrder(2))
                    .ForMember(src => src.Value2, opt => opt.SetMappingOrder(1));
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<Source, Destination>(new Source(10));
            }

            [Fact]
            public void Should_perform_the_mapping_in_the_order_specified()
            {
                _result.Value2.ShouldBe(15);
                _result.Value1.ShouldBe(25);
            }
        }

        public class When_Not_Specifying_Mapping_Order : AutoMapperSpecBase
        {
            private PropertyMap[] _propertyMaps;

            public class Destination
            {
                public string A { get; set; }
                public string E { get; set; }
                public string D { get; set; }
                public string C { get; set; }
                public string B { get; set; }
                public string F { get; set; }
            }

            public class Source
            {
                public string A { get; set; }
                public string B { get; set; }
                public string C { get; set; }
                public string D { get; set; }
                public string E { get; set; }
                public string F { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });

            protected override void Because_of()
            {
                _propertyMaps = Configuration.FindTypeMapFor<Source, Destination>().GetPropertyMaps();
            }

            [Fact]
            public void Should_map_six_properties()
            {
                Assert.Equal(6, _propertyMaps.Length);
            }

            [Fact]
            public void Should_use_not_have_any_configured_mapping_orders()
            {
                Assert.All(_propertyMaps, pm => pm.MappingOrder = null);
            }


            [Fact]
            public void Should_perform_mapping_ordered_by_name()
            {
                Assert.Equal(6, _propertyMaps.Length);

                Assert.Equal("A", _propertyMaps.Skip(0).Select(p => p.DestinationProperty.Name).First());
                Assert.Equal("B", _propertyMaps.Skip(1).Select(p => p.DestinationProperty.Name).First());
                Assert.Equal("C", _propertyMaps.Skip(2).Select(p => p.DestinationProperty.Name).First());
                Assert.Equal("D", _propertyMaps.Skip(3).Select(p => p.DestinationProperty.Name).First());
                Assert.Equal("E", _propertyMaps.Skip(4).Select(p => p.DestinationProperty.Name).First());
                Assert.Equal("F", _propertyMaps.Skip(5).Select(p => p.DestinationProperty.Name).First());
            }
        }

        public class When_Partial_Specifying_Mapping_Order : AutoMapperSpecBase
        {
            private PropertyMap[] _propertyMaps;

            public class Destination
            {
                public string A { get; set; }
                public string E { get; set; }
                public string D { get; set; }
                public string C { get; set; }
                public string B { get; set; }
                public string F { get; set; }
            }

            public class Source
            {
                public string A { get; set; }
                public string B { get; set; }
                public string C { get; set; }
                public string D { get; set; }
                public string E { get; set; }
                public string F { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
                                                                                                    {
                                                                                                        cfg.CreateMap<Source, Destination>()
                                                                                                           .ForMember(dest => dest.E, opt => opt.SetMappingOrder(1))
                                                                                                           .ForMember(dest => dest.B, opt => opt.SetMappingOrder(2))
                                                                                                           .ForMember(dest => dest.D, opt => opt.SetMappingOrder(2));
                                                                                                    });

            protected override void Because_of()
            {
                _propertyMaps = Configuration.FindTypeMapFor<Source, Destination>().GetPropertyMaps();
            }

            [Fact]
            public void Should_map_six_properties()
            {
                Assert.Equal(6, _propertyMaps.Length);
            }

            [Fact]
            public void Should_use_configured_mappingOrders()
            {
                Assert.Equal(1, _propertyMaps.Skip(0).Select(p => p.MappingOrder).First());
                Assert.Equal(2, _propertyMaps.Skip(1).Select(p => p.MappingOrder).First());
                Assert.Equal(2, _propertyMaps.Skip(2).Select(p => p.MappingOrder).First());
                Assert.Null(_propertyMaps.Skip(3).Select(p => p.MappingOrder).First());
                Assert.Null(_propertyMaps.Skip(4).Select(p => p.MappingOrder).First());
                Assert.Null(_propertyMaps.Skip(5).Select(p => p.MappingOrder).First());
            }

            [Fact]
            public void Should_perform_sorting_on_mapping_order_then_name()
            {
                Assert.Equal("E", _propertyMaps.Skip(0).Select(p => p.DestinationProperty.Name).First());
                Assert.Equal("B", _propertyMaps.Skip(1).Select(p => p.DestinationProperty.Name).First());
                Assert.Equal("D", _propertyMaps.Skip(2).Select(p => p.DestinationProperty.Name).First());
                Assert.Equal("A", _propertyMaps.Skip(3).Select(p => p.DestinationProperty.Name).First());
                Assert.Equal("C", _propertyMaps.Skip(4).Select(p => p.DestinationProperty.Name).First());
                Assert.Equal("F", _propertyMaps.Skip(5).Select(p => p.DestinationProperty.Name).First());
            }
        }

    }
}