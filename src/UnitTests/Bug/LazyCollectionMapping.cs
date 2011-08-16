﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Should;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
	[TestFixture]
	public class LazyCollectionMapping
	{
		[SetUp]
		public void SetUp()
		{
			Mapper.Reset();
		}

		public class OneTimeEnumerator<T> : IEnumerable<T>
		{
			private readonly IEnumerable<T> inner;

			public OneTimeEnumerator(IEnumerable<T> inner)
			{
				this.inner = inner;
			}

			private bool isEnumerated;

			public IEnumerator<T> GetEnumerator()
			{
				if (isEnumerated)
					throw new NotSupportedException();
				isEnumerated = true;
				return inner.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public class Source
		{
			public IEnumerable<string> Collection { get; set; }
		}

		public class Destination
		{
			public IEnumerable<string> Collection { get; set; }
		}

		[Test]
		public void OneTimeEnumerator_should_throw_exception_if_enumerating_twice()
		{
			IEnumerable<string> enumerable = Create(new[] {"one", "two", "three"});
			
			enumerable.Count().ShouldEqual(3);

			typeof (NotSupportedException).ShouldBeThrownBy(() => enumerable.Count());
		}
		
		[Test]
		public void Should_not_enumerate_twice()
		{
			Mapper.CreateMap<Source, Destination>();

			var source = new Source {Collection = Create(new[] {"one", "two", "three"})};
			var enumerable = Mapper.Map(source, new Destination());

			enumerable.Collection.Count().ShouldEqual(3);
		}

		public static IEnumerable<T> Create<T>(IEnumerable<T> inner)
		{
			return new OneTimeEnumerator<T>(inner);
		}
	}
}