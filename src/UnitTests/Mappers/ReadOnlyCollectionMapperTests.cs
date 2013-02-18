using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Mappers
{
	[TestFixture]
	public class ReadOnlyCollectionMapperTests
	{
		SourceAsEnumerable _sourceAsEnumerable;
		[SetUp]
		public void SetUp()
		{
			_sourceAsEnumerable = new SourceAsEnumerable()
			{
				ValueInt = new List<int>() { 1, 2, 3 },
				ValueString = new List<string>() { "a", "b", "c" },
				ValueIUser = new List<IUser>() { new UserSource("z", 21) },
				ValueUser = new List<UserSource>() { new UserSource("y", 20), new UserSource("x", 19) },
			};
			Mapper.CreateMap<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>();
			Mapper.CreateMap<SourceAsEnumerable, DestinationAsReadOnlyCollectionNotNull>();
			Mapper.CreateMap<UserSource, UserDestination>();
		}

		[Test]
		public void should_map_to_ReadOnlyCollection_when_destination_properties_are_null()
		{
			var destination = Mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>(_sourceAsEnumerable);

			Assert.IsNotNull(destination);
			Assert.AreNotEqual(0, _sourceAsEnumerable.ValueInt);
			Assert.AreEqual(_sourceAsEnumerable.ValueInt.Count(), destination.ValueInt.Count());
			foreach (var item in _sourceAsEnumerable.ValueInt)
			{
				Assert.IsTrue(destination.ValueInt.Contains(item));
			}

			Assert.AreNotEqual(0, _sourceAsEnumerable.ValueString);
			Assert.AreEqual(_sourceAsEnumerable.ValueString.Count(), destination.ValueString.Count());
			foreach (var item in _sourceAsEnumerable.ValueString)
			{
				Assert.IsTrue(destination.ValueString.Contains(item));
			}

			Assert.AreNotEqual(0, _sourceAsEnumerable.ValueUser);
			Assert.AreEqual(_sourceAsEnumerable.ValueUser.Count(), destination.ValueUser.Count());
			for (int i = 0; i < _sourceAsEnumerable.ValueUser.Count(); i++)
			{
				Assert.AreEqual(_sourceAsEnumerable.ValueUser.ElementAt(i).Name, destination.ValueUser.ElementAt(i).Name);
			}

			Assert.AreNotEqual(0, _sourceAsEnumerable.ValueIUser);
			Assert.AreEqual(_sourceAsEnumerable.ValueIUser.Count(), destination.ValueIUser.Count());
			for (int i = 0; i < _sourceAsEnumerable.ValueIUser.Count(); i++)
			{
				Assert.AreEqual(_sourceAsEnumerable.ValueIUser.ElementAt(i).Name, destination.ValueIUser.ElementAt(i).Name);
				Assert.AreEqual(_sourceAsEnumerable.ValueIUser.ElementAt(i).Age, destination.ValueIUser.ElementAt(i).Age);
			}

		}

		[Test]
		public void should_replace_ReadOnlyCollection_when_destination_properties_are_not_null()
		{
			var destination = Mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNotNull>(_sourceAsEnumerable);

			Assert.IsNotNull(destination);
			Assert.AreNotEqual(0, _sourceAsEnumerable.ValueInt);
			Assert.AreEqual(_sourceAsEnumerable.ValueInt.Count(), destination.ValueInt.Count());
			foreach (var item in _sourceAsEnumerable.ValueInt)
			{
				Assert.IsTrue(destination.ValueInt.Contains(item));
			}

			Assert.AreNotEqual(0, _sourceAsEnumerable.ValueString);
			Assert.AreEqual(_sourceAsEnumerable.ValueString.Count(), destination.ValueString.Count());
			foreach (var item in _sourceAsEnumerable.ValueString)
			{
				Assert.IsTrue(destination.ValueString.Contains(item));
			}

			Assert.AreNotEqual(0, _sourceAsEnumerable.ValueUser);
			Assert.AreEqual(_sourceAsEnumerable.ValueUser.Count(), destination.ValueUser.Count());
			for (int i = 0; i < _sourceAsEnumerable.ValueUser.Count(); i++)
			{
				Assert.AreEqual(_sourceAsEnumerable.ValueUser.ElementAt(i).Name, destination.ValueUser.ElementAt(i).Name);
			}

			Assert.AreNotEqual(0, _sourceAsEnumerable.ValueIUser);
			Assert.AreEqual(_sourceAsEnumerable.ValueIUser.Count(), destination.ValueIUser.Count());
			for (int i = 0; i < _sourceAsEnumerable.ValueIUser.Count(); i++)
			{
				Assert.AreEqual(_sourceAsEnumerable.ValueIUser.ElementAt(i).Name, destination.ValueIUser.ElementAt(i).Name);
				Assert.AreEqual(_sourceAsEnumerable.ValueIUser.ElementAt(i).Age, destination.ValueIUser.ElementAt(i).Age);
			}
		}

		[Test]
		public void should_set_ReadOnlyCollection_underlying_all_IReadOnlyList()
		{
			var destination = Mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>(_sourceAsEnumerable);

			Assert.IsNotNull(destination);
			Assert.IsInstanceOfType(typeof(ReadOnlyCollection<IUser>), destination.ValueIUser);
		}

		[Test]
		public void should_set_ReadOnlyCollection_underlying_all_IReadOnlyCollection()
		{
			var destination = Mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>(_sourceAsEnumerable);

			Assert.IsNotNull(destination);
			Assert.IsInstanceOfType(typeof(ReadOnlyCollection<UserDestination>), destination.ValueUser);
		}


		private class SourceAsEnumerable
		{
			public IEnumerable<int> ValueInt { get; set; }
			public IEnumerable<string> ValueString { get; set; }
			public IEnumerable<UserSource> ValueUser { get; set; }
			public IEnumerable<IUser> ValueIUser { get; set; }
		}

		private class DestinationAsReadOnlyCollectionNull
		{
			public ReadOnlyCollection<int> ValueInt { get; set; }
			public ReadOnlyCollection<string> ValueString { get; set; }
			public ReadOnlyCollection<UserDestination> ValueUser { get; set; }
			public ReadOnlyCollection<IUser> ValueIUser { get; set; }
		}
		private class DestinationAsReadOnlyCollectionNotNull
		{
			public DestinationAsReadOnlyCollectionNotNull()
			{
				ValueInt = new ReadOnlyCollection<int>(new List<int>());
				ValueString = new ReadOnlyCollection<string>(new List<string>());
				ValueUser = new ReadOnlyCollection<UserDestination>(new List<UserDestination>());
				ValueIUser = new ReadOnlyCollection<IUser>(new List<IUser>());
			}

			public ReadOnlyCollection<int> ValueInt { get; set; }
			public ReadOnlyCollection<string> ValueString { get; set; }
			public ReadOnlyCollection<UserDestination> ValueUser { get; set; }
			public ReadOnlyCollection<IUser> ValueIUser { get; set; }
		}

		private interface IUser
		{
			string Name { get; set; }
			int Age { get; set; }
		}

		private class UserSource : IUser
		{
			public UserSource()
			{

			}

			public UserSource(string name, int age)
			{
				Name = name;
				((IUser)this).Age = age;
			}

			public string Name { get; set; }
			int IUser.Age { get; set; }
		}

		private class UserDestination : IUser
		{
			public string Name { get; set; }
			int IUser.Age { get; set; }
		}
	}
}