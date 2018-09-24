using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class ValueConverters
    {
        public class When_specifying_value_converter_for_matching_member : AutoMapperSpecBase
        {
            public class EightDigitIntToStringConverter : IValueConverter<int, string>
            {
                public string Convert(int sourceMember, ResolutionContext context)
                    => sourceMember.ToString("d8");
            }
            public class FourDigitIntToStringConverter : IValueConverter<int, string>
            {
                public string Convert(int sourceMember, ResolutionContext context)
                    => sourceMember.ToString("d4");
            }

            public class Source
            {
                public int Value1 { get; set; }
                public int Value2 { get; set; }
                public int Value3 { get; set; }
                public int Value4 { get; set; }
            }

            public class Dest
            {
                public string Value1 { get; set; }
                public string Value2 { get; set; }
                public string Value3 { get; set; }
                public string Value4 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForMember(d => d.Value1, opt => opt.ConvertUsing<EightDigitIntToStringConverter, int>())
                    .ForMember(d => d.Value2, opt => opt.ConvertUsing<EightDigitIntToStringConverter, int>())
                    .ForMember(d => d.Value3, opt => opt.ConvertUsing<FourDigitIntToStringConverter, int>())
                    .ForMember(d => d.Value4, opt => opt.ConvertUsing<FourDigitIntToStringConverter, int>());
            });

            [Fact]
            public void Should_apply_converters()
            {
                var source = new Source
                {
                    Value1 = 1,
                    Value2 = 2,
                    Value3 = 3,
                    Value4 = 4
                };

                var dest = Mapper.Map<Source, Dest>(source);

                dest.Value1.ShouldBe("00000001");
                dest.Value2.ShouldBe("00000002");
                dest.Value3.ShouldBe("0003");
                dest.Value4.ShouldBe("0004");
            }
        }
        public class When_specifying_value_converter_for_non_matching_member : AutoMapperSpecBase
        {
            public class EightDigitIntToStringConverter : IValueConverter<int, string>
            {
                public string Convert(int sourceMember, ResolutionContext context)
                    => sourceMember.ToString("d8");
            }
            public class FourDigitIntToStringConverter : IValueConverter<int, string>
            {
                public string Convert(int sourceMember, ResolutionContext context)
                    => sourceMember.ToString("d4");
            }

            public class Source
            {
                public int Value1 { get; set; }
                public int Value2 { get; set; }
                public int Value3 { get; set; }
                public int Value4 { get; set; }
            }

            public class Dest
            {
                public string ValueFoo1 { get; set; }
                public string ValueFoo2 { get; set; }
                public string ValueFoo3 { get; set; }
                public string ValueFoo4 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForMember(d => d.ValueFoo1, opt => opt.ConvertUsing<EightDigitIntToStringConverter, int>(src => src.Value1))
                    .ForMember(d => d.ValueFoo2, opt => opt.ConvertUsing<EightDigitIntToStringConverter, int>(src => src.Value2))
                    .ForMember(d => d.ValueFoo3, opt => opt.ConvertUsing<FourDigitIntToStringConverter, int>(src => src.Value3))
                    .ForMember(d => d.ValueFoo4, opt => opt.ConvertUsing<FourDigitIntToStringConverter, int>(src => src.Value4));
            });

            [Fact]
            public void Should_apply_converters()
            {
                var source = new Source
                {
                    Value1 = 1,
                    Value2 = 2,
                    Value3 = 3,
                    Value4 = 4
                };

                var dest = Mapper.Map<Source, Dest>(source);

                dest.ValueFoo1.ShouldBe("00000001");
                dest.ValueFoo2.ShouldBe("00000002");
                dest.ValueFoo3.ShouldBe("0003");
                dest.ValueFoo4.ShouldBe("0004");
            }
        }

        public class When_specifying_value_converter_instance_for_matching_member : AutoMapperSpecBase
        {
            public class EightDigitIntToStringConverter : IValueConverter<int, string>
            {
                public string Convert(int sourceMember, ResolutionContext context)
                    => sourceMember.ToString("d8");
            }
            public class FourDigitIntToStringConverter : IValueConverter<int, string>
            {
                public string Convert(int sourceMember, ResolutionContext context)
                    => sourceMember.ToString("d4");
            }

            public class Source
            {
                public int Value1 { get; set; }
                public int Value2 { get; set; }
                public int Value3 { get; set; }
                public int Value4 { get; set; }
            }

            public class Dest
            {
                public string Value1 { get; set; }
                public string Value2 { get; set; }
                public string Value3 { get; set; }
                public string Value4 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForMember(d => d.Value1, opt => opt.ConvertUsing(new EightDigitIntToStringConverter()))
                    .ForMember(d => d.Value2, opt => opt.ConvertUsing(new EightDigitIntToStringConverter()))
                    .ForMember(d => d.Value3, opt => opt.ConvertUsing(new FourDigitIntToStringConverter()))
                    .ForMember(d => d.Value4, opt => opt.ConvertUsing(new FourDigitIntToStringConverter()));
            });

            [Fact]
            public void Should_apply_converters()
            {
                var source = new Source
                {
                    Value1 = 1,
                    Value2 = 2,
                    Value3 = 3,
                    Value4 = 4
                };

                var dest = Mapper.Map<Source, Dest>(source);

                dest.Value1.ShouldBe("00000001");
                dest.Value2.ShouldBe("00000002");
                dest.Value3.ShouldBe("0003");
                dest.Value4.ShouldBe("0004");
            }
        }

        public class When_specifying_value_converter_instance_for_non_matching_member : AutoMapperSpecBase
        {
            public class EightDigitIntToStringConverter : IValueConverter<int, string>
            {
                public string Convert(int sourceMember, ResolutionContext context)
                    => sourceMember.ToString("d8");
            }
            public class FourDigitIntToStringConverter : IValueConverter<int, string>
            {
                public string Convert(int sourceMember, ResolutionContext context)
                    => sourceMember.ToString("d4");
            }

            public class Source
            {
                public int Value1 { get; set; }
                public int Value2 { get; set; }
                public int Value3 { get; set; }
                public int Value4 { get; set; }
            }

            public class Dest
            {
                public string ValueFoo1 { get; set; }
                public string ValueFoo2 { get; set; }
                public string ValueFoo3 { get; set; }
                public string ValueFoo4 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForMember(d => d.ValueFoo1, opt => opt.ConvertUsing(new EightDigitIntToStringConverter(), src => src.Value1))
                    .ForMember(d => d.ValueFoo2, opt => opt.ConvertUsing(new EightDigitIntToStringConverter(), src => src.Value2))
                    .ForMember(d => d.ValueFoo3, opt => opt.ConvertUsing(new FourDigitIntToStringConverter(), src => src.Value3))
                    .ForMember(d => d.ValueFoo4, opt => opt.ConvertUsing(new FourDigitIntToStringConverter(), src => src.Value4));
            });

            [Fact]
            public void Should_apply_converters()
            {
                var source = new Source
                {
                    Value1 = 1,
                    Value2 = 2,
                    Value3 = 3,
                    Value4 = 4
                };

                var dest = Mapper.Map<Source, Dest>(source);

                dest.ValueFoo1.ShouldBe("00000001");
                dest.ValueFoo2.ShouldBe("00000002");
                dest.ValueFoo3.ShouldBe("0003");
                dest.ValueFoo4.ShouldBe("0004");
            }
        }

    }
}