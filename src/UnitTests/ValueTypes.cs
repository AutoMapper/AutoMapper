using System;
using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
    namespace ValueTypes
    {
        public class When_destination_type_is_a_value_type : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public int Value1 { get; set; }
                public string Value2 { get; set; }
            }

            public struct Destination
            {
                public int Value1 { get; set; }
                public string Value2;
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();

            });

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source {Value1 = 4, Value2 = "hello"});
            }

            [Fact]
            public void Should_map_property_value()
            {
                _destination.Value1.ShouldEqual(4);
            }

            [Fact]
            public void Should_map_field_value()
            {
                _destination.Value2.ShouldEqual("hello");
            }
        }

        public class When_source_struct_config_has_custom_mappings : AutoMapperSpecBase
        {
            public struct matrixDigiInStruct1
            {
                public ushort CNCinfo;
                public ushort Reg1;
                public ushort Reg2;
            }
            public class DigiIn1
            {
                public ushort CncInfo { get; set; }
                public ushort Reg1 { get; set; }
                public ushort Reg2 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(
                cfg => cfg.CreateMap<matrixDigiInStruct1, DigiIn1>()
                    .ForMember(d => d.CncInfo, x => x.MapFrom(s => s.CNCinfo)));

            [Fact]
            public void Should_map_correctly()
            {
                var source = new matrixDigiInStruct1
                {
                    CNCinfo = 5,
                    Reg1 = 6,
                    Reg2 = 7
                };
                var dest = Mapper.Map<matrixDigiInStruct1, DigiIn1>(source);

                dest.CncInfo.ShouldEqual(source.CNCinfo);
                dest.Reg1.ShouldEqual(source.Reg1);
                dest.Reg2.ShouldEqual(source.Reg2);
            }
        }


        public class When_destination_type_is_a_nullable_value_type : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public string Value1 { get; set; }
                public string Value2 { get; set; }
            }

            public struct Destination
            {
                public int Value1 { get; set; }
                public int? Value2 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<string, int>().ConvertUsing((string s) => Convert.ToInt32(s));
                cfg.CreateMap<string, int?>().ConvertUsing((string s) => (int?) Convert.ToInt32(s));
                cfg.CreateMap<Source, Destination>();
            });

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source {Value1 = "10", Value2 = "20"});
            }

            [Fact]
            public void Should_use_map_registered_for_underlying_type()
            {
                _destination.Value2.ShouldEqual(20);
            }

            [Fact]
            public void Should_still_map_value_type()
            {
                _destination.Value1.ShouldEqual(10);
            }


        }
    }
}
