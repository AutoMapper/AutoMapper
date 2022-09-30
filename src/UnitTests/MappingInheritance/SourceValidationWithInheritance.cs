namespace AutoMapper.UnitTests;

public class SourceValidationWithInheritance : AutoMapperSpecBase
{
    public abstract class FormElement2
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
    }

    public abstract class FieldControl2 : FormElement2
    {
        public string Label { get; set; }
        public string Trailer { get; set; }
        public bool Misspelled { get; set; }
    }

    public abstract class TextFieldControl2 : FieldControl2
    {
        public string DefaultValue { get; set; }
        public int Size { get; set; }
    }

    public class TextBoxControl2 : TextFieldControl2
    {
        public int Rows { get; set; }
    }

    public abstract class FormControlBaseDTO2
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
    }

    public class FormElementDTO2 : FormControlBaseDTO2
    {
        public int ElementType { get; set; }
        public string Label { get; set; }
        public string Trailer { get; set; }

        public string DefaultValue { get; set; }
        public int Size { get; set; }
        public int Rows { get; set; }

        public bool Prepopulate { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<FormElement2, FormElementDTO2>(MemberList.Source)
            .Include<FieldControl2, FormElementDTO2>();

        cfg.CreateMap<FieldControl2, FormElementDTO2>(MemberList.Source)
            .ForMember(dto => dto.Prepopulate, opt => opt.MapFrom(src => src.Misspelled))
            .Include<TextBoxControl2, FormElementDTO2>();

        cfg.CreateMap<TextBoxControl2, FormElementDTO2>(MemberList.Source)
            .ForMember(dto => dto.ElementType, opt => opt.MapFrom(src => 0));
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class SourceValidationWithIgnore: AutoMapperSpecBase
{
    public abstract class FormElement2
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
    }

    public abstract class FieldControl2 : FormElement2
    {
        public string Label { get; set; }
        public string Trailer { get; set; }
        public bool Misspelled { get; set; }
    }

    public abstract class TextFieldControl2 : FieldControl2
    {
        public string DefaultValue { get; set; }
        public int Size { get; set; }
    }

    public class TextBoxControl2 : TextFieldControl2
    {
        public int Rows { get; set; }
    }

    public abstract class FormControlBaseDTO2
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
    }

    public class FormElementDTO2 : FormControlBaseDTO2
    {
        public int ElementType { get; set; }
        public string Label { get; set; }
        public string Trailer { get; set; }

        public string DefaultValue { get; set; }
        public int Size { get; set; }
        public int Rows { get; set; }

        public bool Prepopulate { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<FormElement2, FormElementDTO2>(MemberList.Source)
            .Include<FieldControl2, FormElementDTO2>();

        cfg.CreateMap<FieldControl2, FormElementDTO2>(MemberList.Source)
            .ForSourceMember(src => src.Misspelled, o=>o.DoNotValidate())
            .Include<TextBoxControl2, FormElementDTO2>();

        cfg.CreateMap<TextBoxControl2, FormElementDTO2>(MemberList.Source)
            .ForMember(dto => dto.ElementType, opt => opt.MapFrom(src => 0));
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}