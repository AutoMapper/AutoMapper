namespace AutoMapper.UnitTests.Bug;

public class RecognizeDestinationPostfixes : AutoMapperSpecBase
{
    class Person
    {
        public int Age { get; set; }
        public int Age2 => 2017 - Birthday.Year;
        public DateTime Birthday { get; set; }
        public string Name { get; set; }
    }

    class PersonDto
    {
        public int AgeV { get; set; }
        public string NameV { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.RecognizeDestinationPostfixes("V");
        cfg.CreateMap<Person, PersonDto>().ForMember("AgeV", m => m.MapFrom("Age2"));
    });

    [Fact]
    public void Should_be_overriden_by_MapFrom()
    {
        var person = new Person { Birthday = new DateTime(2000, 1, 1), Name = "Shy" };
        Mapper.Map<PersonDto>(person).AgeV.ShouldBe(17);
    }
}
