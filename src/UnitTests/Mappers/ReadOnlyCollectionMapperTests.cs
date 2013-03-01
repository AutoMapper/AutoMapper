using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Mappers
{
	public class ReadOnlyCollectionMapperTests
	{
		SourceAsEnumerable _sourceAsEnumerable;
        public ReadOnlyCollectionMapperTests()
        {
            SetUp();
        }
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

		[Fact]
		public void should_map_to_ReadOnlyCollection_when_destination_properties_are_null()
		{
			var destination = Mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>(_sourceAsEnumerable);

			destination.ShouldNotBeNull();
			_sourceAsEnumerable.ValueInt.Count().ShouldEqual(destination.ValueInt.Count());
			foreach (var item in _sourceAsEnumerable.ValueInt)
			{
				destination.ValueInt.Contains(item).ShouldBeTrue();
			}

			_sourceAsEnumerable.ValueString.Count().ShouldEqual(destination.ValueString.Count());
			foreach (var item in _sourceAsEnumerable.ValueString)
			{
				destination.ValueString.Contains(item).ShouldBeTrue();
			}

			_sourceAsEnumerable.ValueUser.Count().ShouldEqual(destination.ValueUser.Count());
			for (int i = 0; i < _sourceAsEnumerable.ValueUser.Count(); i++)
			{
				_sourceAsEnumerable.ValueUser.ElementAt(i).Name.ShouldEqual(destination.ValueUser.ElementAt(i).Name);
			}

			_sourceAsEnumerable.ValueIUser.Count().ShouldEqual(destination.ValueIUser.Count());
			for (int i = 0; i < _sourceAsEnumerable.ValueIUser.Count(); i++)
			{
				_sourceAsEnumerable.ValueIUser.ElementAt(i).Name.ShouldEqual(destination.ValueIUser.ElementAt(i).Name);
				_sourceAsEnumerable.ValueIUser.ElementAt(i).Age.ShouldEqual(destination.ValueIUser.ElementAt(i).Age);
			}

		}

		[Fact]
		public void should_replace_ReadOnlyCollection_when_destination_properties_are_not_null()
		{
			var destination = Mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNotNull>(_sourceAsEnumerable);

			destination.ShouldNotBeNull();
			_sourceAsEnumerable.ValueInt.Count().ShouldEqual(destination.ValueInt.Count());
			foreach (var item in _sourceAsEnumerable.ValueInt)
			{
				destination.ValueInt.Contains(item).ShouldBeTrue();
			}

			_sourceAsEnumerable.ValueString.Count().ShouldEqual(destination.ValueString.Count());
			foreach (var item in _sourceAsEnumerable.ValueString)
			{
				destination.ValueString.Contains(item).ShouldBeTrue();
			}

			for (int i = 0; i < _sourceAsEnumerable.ValueUser.Count(); i++)
			{
				_sourceAsEnumerable.ValueUser.ElementAt(i).Name.ShouldEqual(destination.ValueUser.ElementAt(i).Name);
			}

			_sourceAsEnumerable.ValueIUser.Count().ShouldEqual(destination.ValueIUser.Count());
			for (int i = 0; i < _sourceAsEnumerable.ValueIUser.Count(); i++)
			{
				_sourceAsEnumerable.ValueIUser.ElementAt(i).Name.ShouldEqual(destination.ValueIUser.ElementAt(i).Name);
				_sourceAsEnumerable.ValueIUser.ElementAt(i).Age.ShouldEqual(destination.ValueIUser.ElementAt(i).Age);
			}
		}

		[Fact]
		public void should_set_ReadOnlyCollection_underlying_all_IReadOnlyList()
		{
			var destination = Mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>(_sourceAsEnumerable);

			destination.ShouldNotBeNull();
			destination.ValueIUser.ShouldBeType<ReadOnlyCollection<IUser>>();
		}

		[Fact]
		public void should_set_ReadOnlyCollection_underlying_all_IReadOnlyCollection()
		{
			var destination = Mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>(_sourceAsEnumerable);

			destination.ShouldNotBeNull();
			destination.ValueUser.ShouldBeType<ReadOnlyCollection<UserDestination>>();
		}


		public class SourceAsEnumerable
		{
			public IEnumerable<int> ValueInt { get; set; }
			public IEnumerable<string> ValueString { get; set; }
			public IEnumerable<UserSource> ValueUser { get; set; }
			public IEnumerable<IUser> ValueIUser { get; set; }
		}

	    public class DestinationAsReadOnlyCollectionNull
		{
			public ReadOnlyCollection<int> ValueInt { get; set; }
			public ReadOnlyCollection<string> ValueString { get; set; }
			public ReadOnlyCollection<UserDestination> ValueUser { get; set; }
			public ReadOnlyCollection<IUser> ValueIUser { get; set; }
		}

	    public class DestinationAsReadOnlyCollectionNotNull
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

	    public interface IUser
		{
			string Name { get; set; }
			int Age { get; set; }
		}

	    public class UserSource : IUser
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

	    public class UserDestination : IUser
		{
			public string Name { get; set; }
			int IUser.Age { get; set; }
		}
	}
}