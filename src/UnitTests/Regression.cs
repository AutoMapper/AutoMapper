namespace AutoMapper.UnitTests
{
    namespace Regression
    {
        public class TestDomainItem : ITestDomainItem
        {
            public Guid ItemId { get; set; }
        }

        public interface ITestDomainItem
        {
            Guid ItemId { get; }
        }

        public class TestDtoItem
        {
            public Guid Id { get; set; }
        }
        public class automapper_fails_to_map_custom_mappings_when_mapping_collections_for_an_interface : AutoMapperSpecBase
        {
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<ITestDomainItem, TestDtoItem>()
                    .ForMember(d => d.Id, s => s.MapFrom(x => x.ItemId));
            });


            [Fact]
            public void should_map_the_id_property()
            {
                var domainItems = new List<ITestDomainItem>
                {
                    new TestDomainItem {ItemId = Guid.NewGuid()},
                    new TestDomainItem {ItemId = Guid.NewGuid()}
                };

                var dtos = Mapper.Map<IEnumerable<ITestDomainItem>, TestDtoItem[]>(domainItems);

                domainItems[0].ItemId.ShouldBe(dtos[0].Id);
            }
        }

        public class Chris_bennages_nullable_datetime_issue : AutoMapperSpecBase
        {
            private Destination _result;

            public class Source
            {
                public DateTime? SomeDate { get; set; }
            }

            public class Destination
            {
                public MyCustomDate SomeDate { get; set; }
            }

            public class MyCustomDate
            {
                public int Day { get; set; }
                public int Month { get; set; }
                public int Year { get; set; }

                public MyCustomDate(int day, int month, int year)
                {
                    Day = day;
                    Month = month;
                    Year = year;
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
                cfg.CreateMap<DateTime?, MyCustomDate>()
                    .ConvertUsing(
                        src => src.HasValue ? new MyCustomDate(src.Value.Day, src.Value.Month, src.Value.Year) : null);
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<Source, Destination>(new Source { SomeDate = new DateTime(2005, 12, 1) });
            }

            [Fact]
            public void Should_map_a_date_with_a_value()
            {
                _result.SomeDate.Day.ShouldBe(1);
            }

            [Fact]
            public void Should_map_null_to_null()
            {
                var destination = Mapper.Map<Source, Destination>(new Source());
                destination.SomeDate.ShouldBeNull();
            }
        }

        public class TestEnumerable : AutoMapperSpecBase
        {
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<Person, PersonModel>();
            });

            [Fact]
            public void MapsEnumerableTypes()
            {
                Person[] personArr = new[] {new Person() {Name = "Name"}};
                People people = new People(personArr);
                
                var pmc = Mapper.Map<People, List<PersonModel>>(people);
                
                pmc.ShouldNotBeNull();
                (pmc.Count == 1).ShouldBeTrue();
            }

            public class People : IEnumerable
            {
                private readonly Person[] people;
                public People(Person[] people)
                {
                    this.people = people;
                }
                public IEnumerator GetEnumerator()
                {
                    foreach (var person in people)
                    {
                        yield return person;
                    }
                }
            }

            public class Person
            {
                public string Name { get; set; }
            }

            public class PersonModel
            {
                public string Name { get; set; }
            }
        }

    }
}