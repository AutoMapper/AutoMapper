namespace AutoMapper.UnitTests.Mappers
{
    namespace ReadOnlyCollections
    {
        public class When_mapping_to_interface_readonly_collection : AutoMapperSpecBase
        {
            public class Source
            {
                public IReadOnlyCollection<int> Values { get; set; }
            }

            public class Destination
            {
                public IReadOnlyCollection<int> Values { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(config =>
            {
                config.CreateMap<Source, Destination>();
            });

            [Fact]
            public void Should_map_readonly_values()
            {
                var source = new Source
                {
                    Values = new List<int>
                    {
                        1,
                        2,
                        3,
                        4,
                    }
                };

                var dest = Mapper.Map<Destination>(source);

                dest.Values.Count.ShouldBe(4);
            }
        }

        public class When_mapping_to_concrete_readonly_collection : AutoMapperSpecBase
        {
            public class Source
            {
                public ReadOnlyCollection<int> Values { get; set; }
            }

            public class Destination
            {
                public ReadOnlyCollection<int> Values { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(config =>
            {
                config.CreateMap<Source, Destination>();
            });

            [Fact]
            public void Should_map_readonly_values()
            {
                var source = new Source
                {
                    Values = new ReadOnlyCollection<int>(new List<int>
                    {
                        1,
                        2,
                        3,
                        4,
                    })
                };

                var dest = Mapper.Map<Destination>(source);

                dest.Values.Count.ShouldBe(4);
            }
        }

        public class When_mapping_to_interface_readonly_list : AutoMapperSpecBase
        {
            public class Source
            {
                public IReadOnlyList<int> Values { get; set; }
            }

            public class Destination
            {
                public IReadOnlyList<int> Values { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(config =>
            {
                config.CreateMap<Source, Destination>();
            });

            [Fact]
            public void Should_map_readonly_values()
            {
                var source = new Source
                {
                    Values = new List<int>
                    {
                        1,
                        2,
                        3,
                        4,
                    }
                };

                var dest = Mapper.Map<Destination>(source);

                dest.Values.Count.ShouldBe(4);
            }
        }

        public class ReadOnlyCollectionMapperTests
        {
            readonly SourceAsEnumerable _sourceAsEnumerable;
            private readonly IMapper _mapper;

            public ReadOnlyCollectionMapperTests()
            {
                _sourceAsEnumerable = new SourceAsEnumerable()
                {
                    ValueInt = new List<int>() {1, 2, 3},
                    ValueString = new List<string>() {"a", "b", "c"},
                    ValueIUser = new List<IUser>() {new UserSource("z", 21)},
                    ValueUser = new List<UserSource>() {new UserSource("y", 20), new UserSource("x", 19)},
                };
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>();
                    cfg.CreateMap<SourceAsEnumerable, DestinationAsReadOnlyCollectionNotNull>();
                    cfg.CreateMap<UserSource, UserDestination>();
                });
                _mapper = config.CreateMapper();
            }

            [Fact]
            public void should_map_to_ReadOnlyCollection_when_destination_properties_are_null()
            {
                var destination =
                    _mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>(_sourceAsEnumerable);

                destination.ShouldNotBeNull();
                _sourceAsEnumerable.ValueInt.Count().ShouldBe(destination.ValueInt.Count());
                foreach (var item in _sourceAsEnumerable.ValueInt)
                {
                    destination.ValueInt.Contains(item).ShouldBeTrue();
                }

                _sourceAsEnumerable.ValueString.Count().ShouldBe(destination.ValueString.Count());
                foreach (var item in _sourceAsEnumerable.ValueString)
                {
                    destination.ValueString.Contains(item).ShouldBeTrue();
                }

                _sourceAsEnumerable.ValueUser.Count().ShouldBe(destination.ValueUser.Count());
                for (int i = 0; i < _sourceAsEnumerable.ValueUser.Count(); i++)
                {
                    _sourceAsEnumerable.ValueUser.ElementAt(i).Name.ShouldBe(destination.ValueUser.ElementAt(i).Name);
                }

                _sourceAsEnumerable.ValueIUser.Count().ShouldBe(destination.ValueIUser.Count());
                for (int i = 0; i < _sourceAsEnumerable.ValueIUser.Count(); i++)
                {
                    _sourceAsEnumerable.ValueIUser.ElementAt(i).Name.ShouldBe(destination.ValueIUser.ElementAt(i).Name);
                    _sourceAsEnumerable.ValueIUser.ElementAt(i).Age.ShouldBe(destination.ValueIUser.ElementAt(i).Age);
                }

            }

            [Fact]
            public void should_replace_ReadOnlyCollection_when_destination_properties_are_not_null()
            {
                var destination =
                    _mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNotNull>(_sourceAsEnumerable);

                destination.ShouldNotBeNull();
                _sourceAsEnumerable.ValueInt.Count().ShouldBe(destination.ValueInt.Count());
                foreach (var item in _sourceAsEnumerable.ValueInt)
                {
                    destination.ValueInt.Contains(item).ShouldBeTrue();
                }

                _sourceAsEnumerable.ValueString.Count().ShouldBe(destination.ValueString.Count());
                foreach (var item in _sourceAsEnumerable.ValueString)
                {
                    destination.ValueString.Contains(item).ShouldBeTrue();
                }

                for (int i = 0; i < _sourceAsEnumerable.ValueUser.Count(); i++)
                {
                    _sourceAsEnumerable.ValueUser.ElementAt(i).Name.ShouldBe(destination.ValueUser.ElementAt(i).Name);
                }

                _sourceAsEnumerable.ValueIUser.Count().ShouldBe(destination.ValueIUser.Count());
                for (int i = 0; i < _sourceAsEnumerable.ValueIUser.Count(); i++)
                {
                    _sourceAsEnumerable.ValueIUser.ElementAt(i).Name.ShouldBe(destination.ValueIUser.ElementAt(i).Name);
                    _sourceAsEnumerable.ValueIUser.ElementAt(i).Age.ShouldBe(destination.ValueIUser.ElementAt(i).Age);
                }
            }

            [Fact]
            public void should_set_ReadOnlyCollection_underlying_all_IReadOnlyList()
            {
                var destination =
                    _mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>(_sourceAsEnumerable);

                destination.ShouldNotBeNull();
                destination.ValueIUser.ShouldBeOfType<ReadOnlyCollection<IUser>>();
            }

            [Fact]
            public void should_set_ReadOnlyCollection_underlying_all_IReadOnlyCollection()
            {
                var destination =
                    _mapper.Map<SourceAsEnumerable, DestinationAsReadOnlyCollectionNull>(_sourceAsEnumerable);

                destination.ShouldNotBeNull();
                destination.ValueUser.ShouldBeOfType<ReadOnlyCollection<UserDestination>>();
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
                    ((IUser) this).Age = age;
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
}