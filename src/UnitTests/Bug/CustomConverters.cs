namespace AutoMapper.UnitTests.Bug;

public class NullableTypeConverter : AutoMapperSpecBase
{
    Destination _destination;

    class Source
    {
        public DateTimeOffset? Date { get; set; }
    }

    class Destination
    {
        public DateTime? Date { get; set; }
    }

    public class NullableDateTimeOffsetConverter : ITypeConverter<DateTimeOffset?, DateTime?>
    {
        public DateTime? Convert(DateTimeOffset? source, DateTime? destination, ResolutionContext context)
        {
            if(source.HasValue)
            {
                return source.Value.DateTime;
            }
            return default(DateTime?);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<Source, Destination>();
        c.CreateMap<DateTimeOffset?, DateTime?>().ConvertUsing<NullableDateTimeOffsetConverter>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source { Date = DateTimeOffset.MaxValue });
    }

    [Fact]
    public void Should_use_the_converter()
    {
        _destination.Date.ShouldBe(DateTime.MaxValue);
    }
}

public class CustomConverters : AutoMapperSpecBase
{
    public class NullableIntToBoolConverter : ITypeConverter<int?, bool>
    {
        public bool Convert(int? source, bool destination, ResolutionContext context)
        {
            if(source == null)
                return false;

            return source == 1;
        }
    }

    public class BoolToNullableIntConverter : ITypeConverter<bool, int?>
    {
        public int? Convert(bool source, int? destination, ResolutionContext context)
        {
            return source ? 1 : 0;
        }
    }

    public class IntToBoolConverter : ITypeConverter<int, bool>
    {
        public bool Convert(int source, bool destination, ResolutionContext context)
        {
            return source == 1;
        }
    }

    public class BoolToIntConverter : ITypeConverter<bool, int>
    {
        public int Convert(bool source, int destination, ResolutionContext context)
        {
            return source ? 1 : 0;
        }
    }

    private class IntEntity
    {
        public int IntProperty { get; set; }

        public IntEntity() { }

        public IntEntity(int value)
        {
            IntProperty = value;
        }
    }

    private class BoolModel
    {
        public bool IntProperty { get; set; }

        public BoolModel() { }

        public BoolModel(bool value)
        {
            IntProperty = value;
        }
    }

    private class NullableIntEntity
    {
        public int? IntProperty { get; set; }

        public NullableIntEntity() { }

        public NullableIntEntity(int? value)
        {
            IntProperty = value;
        }
    }


    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<int, bool>().ConvertUsing<IntToBoolConverter>();
        cfg.CreateMap<bool, int>().ConvertUsing<BoolToIntConverter>();
        cfg.CreateMap<int?, bool>().ConvertUsing<NullableIntToBoolConverter>();
        cfg.CreateMap<bool, int?>().ConvertUsing<BoolToNullableIntConverter>();
        cfg.CreateMap<IntEntity, BoolModel>().ReverseMap();
        cfg.CreateMap<NullableIntEntity, BoolModel>().ReverseMap();
    });

    [Fact]
    public void CheckConverters()
    {
        Mapper.Map<bool>(1).ShouldBeTrue();
        Mapper.Map<bool>(0).ShouldBeFalse();
        Mapper.Map<int?, bool>(0).ShouldBeFalse();
        Mapper.Map<int?, bool>(1).ShouldBeTrue();
        Mapper.Map<int>(true).ShouldBe(1);
        Mapper.Map<int>(false).ShouldBe(0);
        Mapper.Map<int?>(true).ShouldBe(1);
        Mapper.Map<int?>(false).ShouldBe(0);
        Mapper.Map<int?, bool>(null).ShouldBeFalse();
    }
}