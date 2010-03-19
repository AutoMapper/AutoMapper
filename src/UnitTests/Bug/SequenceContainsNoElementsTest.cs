using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using NUnit.Framework;
#if !SILVERLIGHT
using NUnit.Framework.SyntaxHelpers;
#endif

namespace AutoMapper.UnitTests.Bug
{
	[TestFixture]
	public class SequenceContainsNoElementsTest : AutoMapperSpecBase
	{
		[SetUp]
		public void SetUp()
		{
			Mapper.CreateMap<Person, PersonModel>();
		}

		[Test]
		public void should_not_throw_InvalidOperationException()
		{
			Person[] personArr = new Person[] { };
			People people = new People(personArr);
			var pmc = Mapper.Map<People, List<PersonModel>>(people);
			Assert.That(pmc, Is.Not.Null);
			Assert.That(pmc.Count, Is.EqualTo(0));
		}
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
