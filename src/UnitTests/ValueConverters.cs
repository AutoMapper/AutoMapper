namespace AutoMapper.UnitTests;

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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
    public class When_specifying_value_converter_with_no_source : AutoMapperSpecBase
    {
        public class EightDigitIntToStringConverter : IValueConverter<int, string>
        {
            public string Convert(int sourceMember, ResolutionContext context) => sourceMember.ToString("d8");
        }
        public class Source
        {
        }
        public class Dest
        {
            public string ValueFoo1 { get; set; }
        }
        protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Dest>()
                .ForMember(d => d.ValueFoo1, opt => opt.ConvertUsing<EightDigitIntToStringConverter, int>()));
        [Fact]
        public void Should_report_error() => new Action(()=>Map<Dest>(new Source())).ShouldThrow<AutoMapperMappingException>().InnerException.Message.ShouldBe(
            "Cannot find a source member to pass to the value converter of type AutoMapper.UnitTests.ValueConverters+When_specifying_value_converter_with_no_source+EightDigitIntToStringConverter. Configure a source member to map from.");
    }

    public class When_specifying_value_converter_for_string_based_matching_member : AutoMapperSpecBase
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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember("Value1", opt => opt.ConvertUsing<EightDigitIntToStringConverter, int>())
                .ForMember("Value2", opt => opt.ConvertUsing<EightDigitIntToStringConverter, int>())
                .ForMember("Value3", opt => opt.ConvertUsing<FourDigitIntToStringConverter, int>())
                .ForMember("Value4", opt => opt.ConvertUsing<FourDigitIntToStringConverter, int>());
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

    public class When_specifying_value_converter_for_string_based_non_matching_member : AutoMapperSpecBase
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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember("ValueFoo1", opt => opt.ConvertUsing<EightDigitIntToStringConverter, int>("Value1"))
                .ForMember("ValueFoo2", opt => opt.ConvertUsing<EightDigitIntToStringConverter, int>("Value2"))
                .ForMember("ValueFoo3", opt => opt.ConvertUsing<FourDigitIntToStringConverter, int>("Value3"))
                .ForMember("ValueFoo4", opt => opt.ConvertUsing<FourDigitIntToStringConverter, int>("Value4"));
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

    public class When_specifying_value_converter_for_type_and_string_based_matching_member : AutoMapperSpecBase
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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap(typeof(Source), typeof(Dest))
                .ForMember("Value1", opt => opt.ConvertUsing(typeof(EightDigitIntToStringConverter)))
                .ForMember("Value2", opt => opt.ConvertUsing(typeof(EightDigitIntToStringConverter)))
                .ForMember("Value3", opt => opt.ConvertUsing(typeof(FourDigitIntToStringConverter)))
                .ForMember("Value4", opt => opt.ConvertUsing(typeof(FourDigitIntToStringConverter)));
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

    public class When_specifying_value_converter_for_type_and_string_based_non_matching_member : AutoMapperSpecBase
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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap(typeof(Source), typeof(Dest))
                .ForMember("ValueFoo1", opt => opt.ConvertUsing(typeof(EightDigitIntToStringConverter), "Value1"))
                .ForMember("ValueFoo2", opt => opt.ConvertUsing(typeof(EightDigitIntToStringConverter), "Value2"))
                .ForMember("ValueFoo3", opt => opt.ConvertUsing(typeof(FourDigitIntToStringConverter), "Value3"))
                .ForMember("ValueFoo4", opt => opt.ConvertUsing(typeof(FourDigitIntToStringConverter), "Value4"));
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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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

    public class When_specifying_value_converter_instance_for_string_based_matching_member : AutoMapperSpecBase
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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember("Value1", opt => opt.ConvertUsing(new EightDigitIntToStringConverter()))
                .ForMember("Value2", opt => opt.ConvertUsing(new EightDigitIntToStringConverter()))
                .ForMember("Value3", opt => opt.ConvertUsing(new FourDigitIntToStringConverter()))
                .ForMember("Value4", opt => opt.ConvertUsing(new FourDigitIntToStringConverter()));
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

    public class When_specifying_value_converter_instance_for_string_based_non_matching_member : AutoMapperSpecBase
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

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember("ValueFoo1", opt => opt.ConvertUsing(new EightDigitIntToStringConverter(), "Value1"))
                .ForMember("ValueFoo2", opt => opt.ConvertUsing(new EightDigitIntToStringConverter(), "Value2"))
                .ForMember("ValueFoo3", opt => opt.ConvertUsing(new FourDigitIntToStringConverter(), "Value3"))
                .ForMember("ValueFoo4", opt => opt.ConvertUsing(new FourDigitIntToStringConverter(), "Value4"));
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

    public class When_specifying_value_converter_for_all_members : AutoMapperSpecBase
    {
        public class EightDigitIntToStringConverter : IValueConverter<int, string>
        {
            public string Convert(int sourceMember, ResolutionContext context)
                => sourceMember.ToString("d8");
        }

        public class Source
        {
            public int Value { get; set; }
        }

        public class OtherSource
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public string Value { get; set; }
        }

        public class OtherDest
        {
            public string Value { get; set; }
        }

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
            cfg.CreateMap<OtherSource, OtherDest>();
            cfg.ForAllPropertyMaps
                (pm => pm.SourceType == typeof(int) && pm.DestinationType == typeof(string), 
                (pm, opt) => opt.ConvertUsing(new EightDigitIntToStringConverter()));
        });

        [Fact]
        public void Should_apply_converters()
        {
            var source = new Source
            {
                Value = 1,
            };
            var otherSource = new OtherSource
            {
                Value = 2,
            };

            var dest = Mapper.Map<Source, Dest>(source);
            var otherDest = Mapper.Map<OtherSource, OtherDest>(otherSource);

            dest.Value.ShouldBe("00000001");
            otherDest.Value.ShouldBe("00000002");
        }
    }

}