using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Should;
using AutoMapper.QueryableExtensions;
using Xunit;

namespace AutoMapper.UnitTests.Query
{
    public class SourceInjectedQuery : AutoMapperSpecBase
    {
        readonly Source[] _source = new[]
                    {
                        new Source {SrcValue = 5, InttoStringValue = 5},
                        new Source {SrcValue = 4, InttoStringValue = 4},
                        new Source {SrcValue = 7, InttoStringValue = 7}
                    };

        public class Source
        {
            public int SrcValue { get; set; }
            public string StringValue { get; set; }
            public int InttoStringValue { get; set; }
            public string[] Strings { get; set; }
        }

        public class Destination
        {
            public int DestValue { get; set; }
            public string StringValue { get; set; }
            public string InttoStringValue { get; set; }
            public string[] Strings { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Destination, Source>()
                .ForMember(s => s.SrcValue, opt => opt.MapFrom(d => d.DestValue))
                .ReverseMap()
                .ForMember(d => d.DestValue, opt => opt.MapFrom(s => s.SrcValue));
        });

        [Fact]
        public void Shoud_support_const_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
              .UseAsDataSource(Mapper).For<Destination>()
              .Where(s => s.DestValue > 6);

            result.Count().ShouldEqual(1);
            result.Any(s => s.DestValue > 6).ShouldBeTrue();
        }

        [Fact]
        public void Shoud_use_destination_elementType()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource(Mapper).For<Destination>();

            result.ElementType.ShouldEqual(typeof(Destination));

            result = result.Where(s => s.DestValue > 3);
            result.ElementType.ShouldEqual(typeof(Destination));
        }

        [Fact]
        public void Shoud_support_single_item_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource(Mapper).For<Destination>();

            result.First(s => s.DestValue > 6).ShouldBeType<Destination>();
        }

        [Fact]
        public void Shoud_support_IEnumerable_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
              .UseAsDataSource(Mapper).For<Destination>()
              .Where(s => s.DestValue > 6);

            List<Destination> list = result.ToList();
        }

        [Fact]
        public void Shoud_convert_source_item_to_destination()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource(Mapper).For<Destination>();

            var destItem = result.First(s => s.DestValue == 7);
            var sourceItem = _source.First(s => s.SrcValue == 7);

            destItem.DestValue.ShouldEqual(sourceItem.SrcValue);
        }

        [Fact]
        public void Shoud_support_order_by_statement_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
              .UseAsDataSource(Mapper).For<Destination>()
              .OrderByDescending(s => s.DestValue);

            result.First().DestValue.ShouldEqual(_source.Max(s => s.SrcValue));
        }

        [Fact]
        public void Shoud_support_any_stupid_thing_you_can_throw_at_it()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource(Mapper).For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => s.DestValue);

            result.First().ShouldEqual(_source.Max(s => s.SrcValue));
        }

        [Fact]
        public void Shoud_support_string_return_type()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource(Mapper).For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => s.StringValue);

            result.First().ShouldEqual(null);
        }
        [Fact]
        public void Shoud_support_enumerable_return_type()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource(Mapper).For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => s.Strings);

            result.First().Count().ShouldEqual(0);
        }


        [Fact]
        public void Shoud_support_enumerable_return_type_with_result()
        {
            var source = new[]
                    {
                        new Source {SrcValue = 5, Strings = new [] {"lala5", "lili5"}},
                        new Source {SrcValue = 4, Strings = new [] {"lala4", "lili4"}},
                        new Source {SrcValue = 7, Strings = new [] {"lala7", "lili7"}}
                    };

            var result = source.AsQueryable()
              .UseAsDataSource(Configuration).For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 5).Take(2)
              .OrderByDescending(s => s.DestValue).Select(s => s.Strings);

            var item = result.First();
            item.Length.ShouldEqual(2);
            item.All(x => x.EndsWith("7")).ShouldEqual(true);
        }

        [Fact]
        public void Shoud_support_any_stupid_thing_you_can_throw_at_it_with_annonumus_types()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource(Mapper).For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => new { A = s.DestValue });

            result.First().A.ShouldEqual(_source.Max(s => s.SrcValue));
        }
        readonly User[] _source2 = new[]
                    {
                        new User { UserId = 2, Account = new Account(){ Id = 4,Things = {new Thing(){Bar = "Bar"}, new Thing(){ Bar ="Bar 2"}}}},
                        new User { UserId = 1, Account = new Account(){ Id = 3,Things = {new Thing(){Bar = "Bar 3"}, new Thing(){ Bar ="Bar 4"}}}},
                    };
        
        [Fact]
        public void Map_select_method()
        {
            var mapper = SetupAutoMapper();
            var result = _source2.AsQueryable()
              .UseAsDataSource(mapper).For<UserModel>().OrderBy(s => s.Id).ThenBy(s => s.FullName).Select(s => (object)s.AccountModel.ThingModels.Select(b => b.BarModel));

            (result.First() as IEnumerable<string>).Last().ShouldEqual("Bar 4");
        }

        [Fact]
        public void Map_orderBy_thenBy_expression()
        {
            var mapper = SetupAutoMapper();
            var result = _source2.AsQueryable()
              .UseAsDataSource(mapper).For<UserModel>().Select(s => (object)s.AccountModel.ThingModels);

            (result.First() as IEnumerable<Thing>).Last().Bar.ShouldEqual("Bar 2");
        }

        [Fact]
        public void Shoud_convert_source_item_to_destination_toList()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource(Configuration).For<Destination>();

            var destItem = result.ToList().First(s => s.DestValue == 7);
            var sourceItem = _source.First(s => s.SrcValue == 7);

            destItem.DestValue.ShouldEqual(sourceItem.SrcValue);
        }

        [Fact]
        public void Shoud_support_order_by_statement_result_toList()
        {
            IQueryable<Destination> result = _source.AsQueryable()
              .UseAsDataSource(Configuration).For<Destination>()
              .OrderByDescending(s => s.DestValue);

            result.ToList().First().DestValue.ShouldEqual(_source.Max(s => s.SrcValue));
        }

        [Fact]
        public void Shoud_support_any_stupid_thing_you_can_throw_at_it_toList()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource(Configuration).For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => s.DestValue);

            var list = result.ToList();
            list.First().ShouldEqual(_source.Max(s => s.SrcValue));
        }

        [Fact]
        public void Shoud_support_string_return_type_toList()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource(Configuration).For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => s.StringValue);

            result.ToList().First().ShouldEqual(null);
        }

        [Fact]
        public void Shoud_support_enumerable_return_type_toList()
        {
            foreach (var src in _source)
            {
                src.Strings = null;
            }

            var result = _source.AsQueryable()
              .UseAsDataSource(Configuration).For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => s.Strings);

            var list = result.ToList();
            list.First().ShouldBeNull(); // must be null as source values are null as well and Destination does not create empty array in constructor
        }

        [Fact]
        public void Shoud_support_enumerable_return_type_with_result_toList()
        {
            var source = new[]
                    {
                        new Source {SrcValue = 5, Strings = new [] {"lala5", "lili5"}},
                        new Source {SrcValue = 4, Strings = new [] {"lala4", "lili4"}},
                        new Source {SrcValue = 7, Strings = new [] {"lala7", "lili7"}}
                    };

            var result = source.AsQueryable()
              .UseAsDataSource(Configuration).For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 5).Take(2)
              .OrderByDescending(s => s.DestValue).Select(s => s.Strings);

            var list = result.ToList();
            list.Count.ShouldEqual(2);
            list[0].ShouldNotBeNull();
            list[0].Length.ShouldEqual(2);
            list[0].All(x => x.EndsWith("7")).ShouldEqual(true);

            list[1].ShouldNotBeNull();
            list[1].Length.ShouldEqual(2);
            list[1].All(x => x.EndsWith("5")).ShouldEqual(true);
        }

        [Fact]
        public void Shoud_support_any_stupid_thing_you_can_throw_at_it_with_annonumus_types_toList()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource(Configuration).For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => new { A = s.DestValue });

            result.ToList().First().A.ShouldEqual(_source.Max(s => s.SrcValue));
        }

        [Fact]
        public void Map_select_method_toList()
        {
            var mapper = SetupAutoMapper();
            var result = _source2.AsQueryable()
              .UseAsDataSource(mapper).For<UserModel>().OrderBy(s => s.Id).ThenBy(s => s.FullName).Select(s => (object)s.AccountModel.ThingModels.Select(b => b.BarModel));

            (result.ToList().First() as IEnumerable<string>).Last().ShouldEqual("Bar 4");
        }

        [Fact]
        public void Map_orderBy_thenBy_expression_toList()
        {
            var mapper = SetupAutoMapper();
            var result = _source2.AsQueryable()
              .UseAsDataSource(mapper).For<UserModel>().Select(s => (object)s.AccountModel.ThingModels);

            (result.ToList().First() as IEnumerable<Thing>).Last().Bar.ShouldEqual("Bar 2");
        }

        [Fact]
        public void CanMapCyclicObjectGraph()
        {
            // Arrange
            var mapper = SetupAutoMapper();
            var master = new Master()
            {
                Name = "Harry Marry",
                Id = Guid.NewGuid(),
            };
            var detail = new Detail()
            {
                Id = Guid.NewGuid(),
                Name = "Test Order",
                Master = master,
            };
            master.Details.Add(detail);

            // Act
            var dto = mapper.Map<DetailCyclicDto>(detail);

            // Assert
            AssertValidDtoGraph(detail, master, dto);
        }

        [Fact]
        public void CanMapCaclicExpressionGraph()
        {
            // Arrange
            var mapper = SetupAutoMapper();
            var master = new Master()
            {
                Name = "Harry Marry",
                Id = Guid.NewGuid(),
            };
            var detail = new Detail()
            {
                Id = Guid.NewGuid(),
                Name = "Test Order",
                Master = master,
            };
            master.Details.Add(detail);
            var detailQuery = new List<Detail> { detail }.AsQueryable();

            // Act
            var detailDtoQuery = detailQuery.UseAsDataSource(mapper)
                .For<DetailCyclicDto>();

            // Assert
            var dto = detailDtoQuery.Single();

            AssertValidDtoGraph(detail, master, dto);
        }

        [Fact]
        public void CanMapCaclicExpressionGraph_WithPropertyFilter()
        {
            // Arrange
            var mapper = SetupAutoMapper();
            var master = new Master()
            {
                Name = "Harry Marry",
                Id = Guid.NewGuid(),
            };
            var detail = new Detail()
            {
                Id = Guid.NewGuid(),
                Name = "Test Order",
                Master = master,
            };
            master.Details.Add(detail);
            var detailQuery = new List<Detail> { detail }.AsQueryable();

            // Act
            var detailDtoQuery = detailQuery.UseAsDataSource(mapper)
                .For<DetailCyclicDto>()
                .Where(d => d.Name.EndsWith("rder"));

            // Assert
            var dto = detailDtoQuery.Single();

            AssertValidDtoGraph(detail, master, dto);
        }

        [Fact]
        public void CanMapCaclicExpressionGraph_WithPropertyPathEqualityFilter_Single()
        {
            // Arrange
            var mapper = SetupAutoMapper();
            var master = new Master()
            {
                Name = "Harry Marry",
                Id = Guid.NewGuid(),
            };
            var detail = new Detail()
            {
                Id = Guid.NewGuid(),
                Name = "Test Order",
                Master = master,
            };
            master.Details.Add(detail);
            var detailQuery = new List<Detail> { detail }.AsQueryable();

            // Act
            var detailDtoQuery = detailQuery.UseAsDataSource(mapper)
                .For<DetailCyclicDto>()
                .Where(d => d.Master.Name == "Harry Marry");

            // Assert
            var dto = detailDtoQuery.Single();

            AssertValidDtoGraph(detail, master, dto);
        }

        [Fact]
        [Description("Fix for issue #882")]
        public void Should_support_propertypath_expressons_with_equally_named_properties()
        {
            // Arrange
            var mapper = SetupAutoMapper();

            var master = new Master { Id = Guid.NewGuid(), Name = "Harry Marry" };
            var detail = new Detail { Id = Guid.NewGuid(), Master = master, Name = "Some detail" };
            master.Details.Add(detail);
            var source = new List<Detail> { detail };

            // Act
            var detailDtoQuery = source.AsQueryable().UseAsDataSource(mapper)
                .For<DetailDto>()
                .Where(d => d.Master.Name == "Harry Marry");

            // Assert
            detailDtoQuery.ToList().Count().ShouldEqual(1);
        }

        private void AssertValidDtoGraph(Detail detail, Master master, DetailCyclicDto dto)
        {
            dto.ShouldNotBeNull();
            detail.Id.ShouldEqual(dto.Id);

            detail.Master.ShouldNotBeNull();
            master.Details.ShouldNotBeEmpty();
            detail.Master.Id.ShouldEqual(master.Id);
            
            dto.Master.Details.Single().Id.ShouldEqual(dto.Id, "Dto was not added to inner collection");
            //dto.GetHashCode().ShouldEqual(dto.Master.Details.Single().GetHashCode()); // "Underlying provider always creates two distinct instances"
        }

        [Fact]
        public void SupportsParmeterization()
        {
            // Arrange
            int value = 0;

            Expression<Func<SourceWithParams, int>> sourceMember = src => value + 5;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<SourceWithParams, DestWithParams>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember)));
            var mapper = new Mapper(config);

            var source = new[]
            {
                new SourceWithParams()
            }.AsQueryable();

            // Act
            var result = source.UseAsDataSource(mapper).For<DestWithParams>(new Dictionary<string, object> { { "value", 10 } }).ToArray();
            
            // Assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Single().Value.ShouldEqual(15);
        }

        [Fact]
        public void SupportsParmeterization_DoesNotCacheParameter()
        {
            // Arrange
            int value = 0;

            Expression<Func<SourceWithParams, int>> sourceMember = src => value + 5;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<SourceWithParams, DestWithParams>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember)));
            var mapper = new Mapper(config);

            var source = new[]
            {
                new SourceWithParams()
            }.AsQueryable();

            // Act
            var result1 = source.UseAsDataSource(mapper).For<DestWithParams>(new Dictionary<string, object> { { "value", 10 } }).ToArray();
            var result = source.UseAsDataSource(mapper).For<DestWithParams>(new Dictionary<string, object> { { "value", 15 } }).ToArray();

            // Assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Single().Value.ShouldEqual(20);
        }


        [Fact]
        public void SupportsParameterizationInQuery()
        {
            // Arrange
            var sources = new List<ResourceWithPermissions>()
            {
                new ResourceWithPermissions
                {
                    Title = "Resource 01",
                    Permissions = new List<Permission>
                    {
                        new Permission {PermissionName = "Edit", UserId = 22},
                        new Permission {PermissionName = "Read", UserId = 4}
                    }
                },
                new ResourceWithPermissions
                {
                    Title = "Resource 02",
                    Permissions = new List<Permission>
                    {
                        new Permission {PermissionName = "Edit", UserId = 4}
                    }
                },
                new ResourceWithPermissions
                {
                    Title = "Resource 03",
                    Permissions = new List<Permission>()
                }
            };

            // note:
            // we need to create the mapping and the query in two distinct scopes (therefore action and func)
            // so that we can declare a parameter (userId) that is independent of mapping and query definition.
            // that way, in the query, we have a parameter which needs to be replaced.
            // (in SourceInjectedQueryProvider.ConvertDestinationExpressionToSourceExpression)
            var config = new MapperConfiguration(cfg =>
            {
                // parameter defined in mapping part
                long userId = default(long);
                cfg.CreateMap<ResourceWithPermissions, ResourceDto>()
                    .ForMember(t => t.HasEditPermission,
                        o => o.MapFrom(s => s.Permissions.Any(p => p.PermissionName == "Edit" && p.UserId == userId)));
                cfg.CreateMap<ResourceDto, ResourceWithPermissions>()
                    .ForMember(t => t.Permissions, o => o.Ignore());
            });
            var mapper = new Mapper(config);

            var factoryFunc = new Func<IList<ResourceWithPermissions>, IQueryable<ResourceWithPermissions>>((list) =>
            {
                // same parameter defined in query part
                long userId = default(long);
                return list.AsQueryable().Where(r => r.Permissions.Any(p => p.UserId == userId));
            });

            var query = factoryFunc(sources)
                .UseAsDataSource(mapper)
                .For<ResourceDto>(new {userId = 4});
                
            // Act
            var results = query.ToList();

            // Assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();

            results.Count.ShouldEqual(2);
            results[0].HasEditPermission.ShouldBeFalse();
            results[1].HasEditPermission.ShouldBeTrue();
        }


        [Fact]
        public void Shoud_convert_type_changes()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource(Configuration).For<Destination>();

            var destItem = result.First(s => s.InttoStringValue == "7");
            var sourceItem = _source.First(s => s.InttoStringValue == 7);

            destItem.DestValue.ShouldEqual(sourceItem.SrcValue);
        }

        private static IMapper SetupAutoMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.UserId))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.LoggedOn, opt => opt.MapFrom(s => s.IsLoggedOn ? "Y" : "N"))
                .ForMember(d => d.IsOverEighty, opt => opt.MapFrom(s => s.Age > 80))
                .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.Account == null ? string.Empty : string.Concat(s.Account.FirstName, " ", s.Account.LastName)))
                .ForMember(d => d.AgeInYears, opt => opt.MapFrom(s => s.Age))
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.Active))
                .ForMember(d => d.AccountModel, opt => opt.MapFrom(s => s.Account));

                cfg.CreateMap<UserModel, User>()
                    .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.Id))
                    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName))
                    .ForMember(d => d.IsLoggedOn, opt => opt.MapFrom(s => s.LoggedOn.ToUpper() == "Y"))
                    .ForMember(d => d.Age, opt => opt.MapFrom(s => s.AgeInYears))
                    .ForMember(d => d.Active, opt => opt.MapFrom(s => s.IsActive))
                    .ForMember(d => d.Account, opt => opt.MapFrom(s => s.AccountModel));

                cfg.CreateMap<Account, AccountModel>()
                    .ForMember(d => d.Bal, opt => opt.MapFrom(s => s.Balance))
                    .ForMember(d => d.DateCreated, opt => opt.MapFrom(s => s.CreateDate))
                    .ForMember(d => d.ComboName, opt => opt.MapFrom(s => string.Concat(s.FirstName, " ", s.LastName)))
                    .ForMember(d => d.ThingModels, opt => opt.MapFrom(s => s.Things));

                cfg.CreateMap<AccountModel, Account>()
                    .ForMember(d => d.Balance, opt => opt.MapFrom(s => s.Bal))
                    .ForMember(d => d.Things, opt => opt.MapFrom(s => s.ThingModels));

                cfg.CreateMap<Thing, ThingModel>()
                    .ForMember(d => d.FooModel, opt => opt.MapFrom(s => s.Foo))
                    .ForMember(d => d.BarModel, opt => opt.MapFrom(s => s.Bar));

                cfg.CreateMap<ThingModel, Thing>()
                    .ForMember(d => d.Foo, opt => opt.MapFrom(s => s.FooModel))
                    .ForMember(d => d.Bar, opt => opt.MapFrom(s => s.BarModel));

                cfg.CreateMap<Master, MasterDto>().ReverseMap();
                cfg.CreateMap<Detail, DetailDto>().ReverseMap();

                // dtos with cyclic references that would cause a stackoverflow exception upon projection mapping
                cfg.CreateMap<Master, MasterCyclicDto>().PreserveReferences();
                cfg.CreateMap<MasterCyclicDto, Master>().PreserveReferences();
                cfg.CreateMap<Detail, DetailCyclicDto>().PreserveReferences();
                cfg.CreateMap<DetailCyclicDto, Detail>().PreserveReferences();
            });


           return config.CreateMapper();
        }

    }

    public class Account
    {
        public Account()
        {
            Things = new List<Thing>();
        }
        public int Id { get; set; }
        public double Balance { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreateDate { get; set; }
        public ICollection<Thing> Things { get; set; }
    }

    public class AccountModel
    {
        public AccountModel()
        {
            ThingModels = new List<ThingModel>();
        }
        public int Id { get; set; }
        public double Bal { get; set; }
        public string ComboName { get; set; }
        public DateTime DateCreated { get; set; }
        public ICollection<ThingModel> ThingModels { get; set; }
    }

    public class Thing
    {
        public int Foo { get; set; }
        public string Bar { get; set; }
    }

    public class ThingModel
    {
        public int FooModel { get; set; }
        public string BarModel { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool IsLoggedOn { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }
        public Account Account { get; set; }
    }

    public class UserModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string AccountName { get; set; }
        public bool IsOverEighty { get; set; }
        public string LoggedOn { get; set; }
        public int AgeInYears { get; set; }
        public bool IsActive { get; set; }
        public AccountModel AccountModel { get; set; }
    }

    public class Master
    {
        public Master()
        {
            Details = new List<Detail>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Detail> Details { get; set; }
    }

    public class Detail
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Master Master { get; set; }
    }

    public class MasterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class DetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public MasterDto Master { get; set; }
    }

    
    public class MasterCyclicDto
    {
        public MasterCyclicDto()
        {
            Details = new List<DetailCyclicDto>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<DetailCyclicDto> Details { get; set; }
    }

    public class DetailCyclicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public MasterCyclicDto Master { get; set; }
    }

    public class SourceWithParams
    {
    }

    public class DestWithParams
    {
        public int Value { get; set; }
    }

    public class ResourceWithPermissions
    {
        public string Title { get; set; }
        public ICollection<Permission> Permissions { get; set; }
    }

    public class Permission
    {
        public Permission()
        {
        }

        public Permission(string permissionName, long userId)
        {
            PermissionName = permissionName;
            UserId = userId;
        }

        public long UserId { get; set; }
        public string PermissionName { get; set; }
    }

    public class ResourceDto
    {
        public string Title { get; set; }
        public bool HasEditPermission { get; set; }
    }
}


