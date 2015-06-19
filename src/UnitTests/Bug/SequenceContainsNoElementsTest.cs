namespace AutoMapper.UnitTests.Bug
{
    using System.Collections.Generic;
    using System.Collections;
    using Xunit;
    using Should;

    public class SequenceContainsNoElementsTest : AutoMapperSpecBase
    {
        public SequenceContainsNoElementsTest()
        {
            SetUp();
        }

        public void SetUp()
        {
            Mapper.CreateMap<Person, PersonModel>();
        }

        [Fact]
        public void should_not_throw_InvalidOperationException()
        {
            var personArr = new Person[] {};
            var people = new People(personArr);
            var pmc = Mapper.Map<People, List<PersonModel>>(people);
            pmc.ShouldNotBeNull();
            pmc.Count.ShouldEqual(0);
        }

        /// <summary>
        /// 
        /// </summary>
        private class People : IEnumerable
        {
            /// <summary>
            /// 
            /// </summary>
            private readonly Person[] _people;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="people"></param>
            public People(Person[] people)
            {
                this._people = people;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <see cref="_people"/>
            public IEnumerator GetEnumerator()
            {
                // Careful to return at least an enumerator of the underlying People.
                return _people.GetEnumerator();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class Person
        {
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private class PersonModel
        {
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
        }
    }
}