namespace AutoMapper.UnitTests.Bug;

public class SequenceContainsNoElementsTest : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Person, PersonModel>();
    });

    [Fact]
    public void should_not_throw_InvalidOperationException()
    {
        Person[] personArr = new Person[] { };
        People people = new People(personArr);
        var pmc = Mapper.Map<People, List<PersonModel>>(people);
        pmc.ShouldNotBeNull();
        pmc.Count.ShouldBe(0);
    }
}

public class People : IEnumerable
{
    private readonly Person[] people;
    public People(Person[] people)
    {
        this.people = people;
    }
    public IEnumerator GetEnumerator() => people.GetEnumerator();
}

public class Person
{
    public string Name { get; set; }
}

public class PersonModel
{
    public string Name { get; set; }
}
