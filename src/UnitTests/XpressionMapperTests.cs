using AutoMapper.XpressionMapper.Extensions;
using Should.Core.Assertions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class XpressionMapperTests
    {
        public XpressionMapperTests()
        {
            SetupAutoMapper();
            SetupQueryableCollection();
        }

        #region Tests
        [Fact]
        public void Map_includes_list()
        {
            //Arrange
            ICollection<Expression<Func<UserModel, object>>> selections = new List<Expression<Func<UserModel, object>>>() { s => s.AccountModel.Bal, s => s.AccountName };

            //Act
            IList<Expression<Func<User, object>>> selectionsMapped = mapper.MapIncludesList<Expression<Func<User, object>>>(selections).ToList();
            List<object> accounts = Users.Select(selectionsMapped[0]).ToList();
            List<object> branches = Users.Select(selectionsMapped[1]).ToList();

            //Assert
            Assert.True(accounts.Count == 2 && branches.Count == 2);
        }

        [Fact]
        public void Map_includes_list_with_select()
        {
            //Arrange
            ICollection<Expression<Func<UserModel, object>>> selections = new List<Expression<Func<UserModel, object>>>() { s => s.AccountModel.Bal, s => s.AccountName, s => s.AccountModel.ThingModels.Select<ThingModel, object>(x => x.Color) };

            //Act
            IList<Expression<Func<User, object>>> selectionsMapped = mapper.MapIncludesList<Expression<Func<User, object>>>(selections).ToList();
            List<object> accounts = Users.Select(selectionsMapped[0]).ToList();
            List<object> branches = Users.Select(selectionsMapped[1]).ToList();
            List<object> cars = Users.Select(selectionsMapped[2]).SelectMany(o => (o as IEnumerable<object>)).ToList();

            //Assert
            Assert.True(cars.Count == 4 && accounts.Count == 2 && branches.Count == 2);
        }

        [Fact]
        public void Map_includes_with_value_types()
        {
            //Arrange
            Expression<Func<UserModel, object>> selection = s => s.AccountModel.Bal;

            //Act
            Expression<Func<User, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, object>>>(selection);
            List<object> accounts = Users.Select(selectionMapped).ToList();

            //Assert
            Assert.True(accounts.Count == 2);
        }

        [Fact]
        public void Map_includes_with_string()
        {
            //Arrange
            Expression<Func<UserModel, object>> selection = s => s.AccountName;

            //Act
            Expression<Func<User, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, object>>>(selection);
            List<object> accounts = Users.Select(selectionMapped).ToList();

            //Assert
            Assert.True(accounts.Count == 2);
        }

        [Fact]
        public void Map_includes_trim_string_nested_in_select_using_object()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<object>>> selection = s => s.AccountModel.ThingModels.Select<ThingModel, object>(x => x.Color);

            //Act
            Expression<Func<User, IEnumerable<object>>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, IEnumerable<object>>>>(selection);
            List<object> cars = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(cars.Count == 4);
        }

        [Fact]
        public void Map_includes_trim_string_nested_in_select_using_explicit_types()
        {//Probebly want to be careful about mapping strings or value types to reference types.  What it there are multiple strings in the expression?
            //Arrange
            Expression<Func<UserModel, IEnumerable<string>>> selection = s => s.AccountModel.ThingModels.Select<ThingModel, string>(x => x.Color);

            //Act
            Expression<Func<User, IEnumerable<Car>>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, IEnumerable<Car>>>>(selection);
            List<Car> cars = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(cars.Count == 4);
        }

        [Fact]
        public void Map_object_type_change()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s.LoggedOn == "Y";

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 1);
        }

        [Fact]
        public void Map_object_type_change_again()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s.IsOverEighty;

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 0);
        }

        [Fact]
        public void Map__object_including_child_and_grandchild()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s != null && s.AccountModel != null && s.AccountModel.Bal > 555.20;

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 2);
        }

        [Fact]
        public void Map_project_truncated_time()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s != null && s.AccountModel != null && s.AccountModel.DateCreated == DateTime.Now;

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 0);
        }

        [Fact]
        public void Map_projection()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s != null && s.AccountModel.Description.StartsWith("B");

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 1);
        }

        [Fact]
        public void Map__flattened_property()
        {
            //Arrange
            int age = 25;
            Expression<Func<UserModel, bool>> selection = s => ((s != null ? s.AccountName : null) ?? "").ToLower().StartsWith("P".ToLower()) && ((s.AgeInYears == age) && s.IsActive);

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 1);
        }

        [Fact]
        public void Map__select_method()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<string>>> selection = s => s.AccountModel.ThingModels.Select(x => x.BarModel).Where(b => b.EndsWith("3"));

            //Act
            Expression<Func<User, IEnumerable<string>>> selectionMapped = mapper.Map<Expression<Func<User, IEnumerable<string>>>>(selection);
            List<string> bars = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(bars.Count == 1);
        }

        [Fact]
        public void Map__select_method_projecting_to_anonymous_type()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<object>>> selection = s => s.AccountModel.ThingModels.Select(x => new { MM = x.BarModel }).Where(b => b.MM.EndsWith("3"));

            //Act
            Expression<Func<User, IEnumerable<object>>> selectionMapped = mapper.Map<Expression<Func<User, IEnumerable<object>>>>(selection);
            List<object> bars = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(bars.Count == 1);
        }

        [Fact]
        public void Map__select_method_where_parent_type_is_grandchild_type()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<int>>> selection = s => s.AccountModel.UserModels.Select(x => x.AgeInYears);

            //Act
            Expression<Func<User, IEnumerable<int>>> selectionMapped = mapper.Map<Expression<Func<User, IEnumerable<int>>>>(selection);
            List<int> bars = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(bars.Count == 2);
        }

        [Fact]
        public void Map_where_method()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<ThingModel>>> selection = s => s.AccountModel.ThingModels.Where(x => x.BarModel == s.AccountName);

            //Act
            Expression<Func<User, IEnumerable<Thing>>> selectionMapped = mapper.Map<Expression<Func<User, IEnumerable<Thing>>>>(selection);
            List<Thing> things = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(things.Count == 0);
        }

        [Fact]
        public void Map_where_multiple_arguments()
        {
            //Arrange
            Expression<Func<UserModel, AccountModel, object>> selection = (u, s) => u.FullName == s.Bal.ToString();

            //Act
            Expression<Func<User, Account, object>> selectionMapped = mapper.Map<Expression<Func<User, Account, object>>>(selection);
            object val = selectionMapped.Compile().Invoke(Users.ToList()[0], Users.ToList()[0].Account);

            //Assert
            Assert.False((bool)val);
        }

        [Fact]
        public void Map_orderBy_thenBy_expression()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IQueryable<UserModel>>> exp = q => q.OrderByDescending(s => s.Id).ThenBy(s => s.FullName);

            //Act
            Expression<Func<IQueryable<User>, IQueryable<User>>> expMapped = mapper.Map<Expression<Func<IQueryable<User>, IQueryable<User>>>>(exp);
            List<User> users = expMapped.Compile().Invoke(Users).ToList();

            //Assert
            Assert.True(users[0].UserId == 14);
        }

        [Fact]
        public void Map_orderBy_thenBy_GroupBy_expression()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IQueryable<IGrouping<int, UserModel>>>> grouped = q => q.OrderBy(s => s.Id).ThenBy(s => s.FullName).GroupBy(s => s.AgeInYears);

            //Act
            Expression<Func<IQueryable<User>, IQueryable<IGrouping<int, User>>>> expMapped = mapper.Map<Expression<Func<IQueryable<User>, IQueryable<IGrouping<int, User>>>>>(grouped);
            List<IGrouping<int, User>> users = expMapped.Compile().Invoke(Users).ToList();

            Assert.True(users[0].Count() == 2);
        }

        [Fact]
        public void Map_dynamic_return_type()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, dynamic>> exp = q => q.OrderBy(s => s.Id).ThenBy(s => s.FullName);

            //Act
            Expression<Func<IQueryable<User>, dynamic>> expMapped = mapper.Map<Expression<Func<IQueryable<User>, dynamic>>>(exp);
            List<User> users = Enumerable.ToList(expMapped.Compile().Invoke(Users));

            //Assert
            Assert.True(users[0].UserId == 11);
        }

        [Fact]
        public void Map_1821_when_mapping_expression_with_a_property_that_maps_to_a_string_type()
        {
            //Arrange
            Expression<Func<Item, object>> exp = x => x.Name;

            //Act
            Expression<Func<ItemDto, object>> expMapped = mapper.Map<Expression<Func<ItemDto, object>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Map_1893_null_exception_with_deflattening()
        {
            //Arrange
            Expression<Func<OrderLineDTO, bool>> dtoExpression = dto => dto.Item.Name == "Item #1";

            //Act
            Expression<Func<OrderLine, bool>> expression = mapper.Map<Expression<Func<OrderLine, bool>>>(dtoExpression);

            //Assert
            Assert.NotNull(expression);
        }

        [Fact]
        public void Map_parentDto_to_parent()
        {
            //Arrange
            Expression<Func<ParentDTO, bool>> exp = p => (p.DateTime.Year.ToString() != String.Empty);
            //Act
            Expression<Func<Parent, bool>> expMapped = mapper.Map<Expression<Func<Parent, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Map_parentDto_to_parent_with_index_argument()
        {
            //Arrange
            var ids = new[] { 4, 5 };
            Expression<Func<ParentDTO, bool>> exp = p => p.Children.Where((c, i) => c.ID_ > 4).Any(c => ids.Contains(c.ID_));
            //Act
            Expression<Func<Parent, bool>> expMapped = mapper.Map<Expression<Func<Parent, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Map_accountModel_to_account()
        {
            //Arrange
            Expression<Func<AccountModel, bool>> exp = p => (p.DateCreated.Year.ToString() != String.Empty);

            //Act
            Expression<Func<Account, bool>> expMapped = mapper.Map<Expression<Func<Account, bool>>>(exp);
            List<Account> accounts = Users.Select(u => u.Account).Where(expMapped).ToList();

            //Assert
            Assert.True(accounts.Count == 2);
        }

        [Fact]
        public void Map_accountModel_to_account_with_null_checks_against_value_types()
        {
            //Arrange
            Expression<Func<AccountModel, bool>> exp = f => (((f != null && f.Id != null) ? f.Id : 0) > 10)
             && (((f != null && f.DateCreated != null) ? f.DateCreated : default(DateTime)) > new DateTime(2007, 02, 17));


            //Act
            Expression<Func<Account, bool>> expMapped = mapper.MapExpression<Expression<Func<Account, bool>>>(exp);
            List<Account> accounts = Users.Select(u => u.Account).Where(expMapped).ToList();

            //Assert
            Assert.True(accounts.Count == 1);
        }

        [Fact]
        public void When_use_lambda_statement_with_typemapped_property_being_other_than_first()
        {
            //Arrange
            Expression<Func<ParentDTO, bool>> exp = p => p.Children.AnyParamReverse((c, c2) => c2.ID_ > 4);
            //Act
            Expression<Func<Parent, bool>> expMapped = mapper.Map<Expression<Func<Parent, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }
        #endregion Tests

        private static void SetupAutoMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfiles(typeof(OrganizationProfile));
            });

            mapper = config.CreateMapper();
        }

        static IMapper mapper;

        private static void SetupQueryableCollection()
        {
            Users = new User[]
            {
                new User
                {
                    Account = new Account
                    {
                        Balance = 150000,
                        Branch = new Branch { Id = 1, Name = "Head Row" },
                        CreateDate = new DateTime(2011, 2, 2),
                        Id = 12,
                        Location = new Location { City = "Leeds", Latitude = 53.8008, Longitude = -1.5491 },
                        Number = "232232232",
                        Things = new Thing[]
                        {
                            new Thing { Bar = "Bar", Car = new Car { Color = "Black", Year = 2014 } },
                            new Thing { Bar = "Bar2", Car = new Car { Color = "White", Year = 2015 } }
                        },
                        Type = "Personal",
                        Users = new User[]
                        {
                            new User
                            {
                                Active = true,
                                Age = 25,
                                FirstName = "John",
                                LastName = "Smith",
                                IsLoggedOn = true,
                                UserId = 11
                            }
                        }
                    },
                    Active = true,
                    Age = 25,
                    FirstName = "John",
                    LastName = "Smith",
                    IsLoggedOn = true,
                    UserId = 11
                },
                new User
                {
                    Account = new Account
                    {
                        Balance = 200000,
                        Branch = new Branch { Id = 1, Name = "Park Row" },
                        CreateDate = new DateTime(2012, 3, 3),
                        Id = 7,
                        Location = new Location { City = "Leeds", Latitude = 53.8008, Longitude = -1.5491 },
                        Number = "444555444",
                        Things = new Thing[]
                        {
                            new Thing { Bar = "Bar3", Car = new Car { Color = "Black", Year = 2014 } },
                            new Thing { Bar = "Bar4", Car = new Car { Color = "White", Year = 2015 } }
                        },
                        Type = "Business",
                        Users = new User[] 
                        {
                            new User
                            {
                                Active = true,
                                Age = 25,
                                FirstName = "Jack",
                                LastName = "Spratt",
                                IsLoggedOn = false,
                                UserId = 14
                            }
                        }
                    },
                    Active = true,
                    Age = 25,
                    FirstName = "Jack",
                    LastName = "Spratt",
                    IsLoggedOn = false,
                    UserId = 14
                }
            }.AsQueryable<User>();
        }

        private static IQueryable<User> Users { get; set; }
    }

    public class Account
    {
        public Account()
        {
            Things = new List<Thing>();
        }
        public int Id { get; set; }
        public double Balance { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public DateTime CreateDate { get; set; }
        public Location Location { get; set; }
        public Branch Branch { get; set; }
        public ICollection<Thing> Things { get; set; }
        public ICollection<User> Users { get; set; }
    }

    public class AccountModel
    {
        public AccountModel()
        {
            ThingModels = new List<ThingModel>();
        }
        public int Id { get; set; }
        public double Bal { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public DateTime DateCreated { get; set; }
        public ICollection<ThingModel> ThingModels { get; set; }
        public ICollection<UserModel> UserModels { get; set; }
    }

    public class Thing
    {
        public int Foo { get; set; }
        public string Bar { get; set; }
        public Car Car { get; set; }
    }

    public class ThingModel
    {
        public int FooModel { get; set; }
        public string BarModel { get; set; }
        public string Color { get; set; }
        public CarModel Car { get; set; }
    }

    public class Car
    {
        public string Color { get; set; }
        public int Year { get; set; }
    }

    public class CarModel
    {
        public string Color { get; set; }
        public int Year { get; set; }
    }

    public class Location
    {
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UserVM
    {
        public string Name { get; set; }
        public bool IsLoggedOn { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }
        public Account Account { get; set; }
    }
    public class UserM
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool IsLoggedOn { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }
        public Account Account { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
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

    public class OrderLine
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderLineDTO
    {
        public int Id { get; set; }
        public ItemDTO Item { get; set; }
        public int Quantity { get; set; }
    }

    public class ItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }
    }


    public class Item
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }
    }

    public class GrandParentDTO
    {
        public ParentDTO Parent { get; set; }
    }
    public class ParentDTO
    {
        public ICollection<ChildDTO> Children { get; set; }
        public ChildDTO Child { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class ChildDTO
    {
        public ParentDTO Parent { get; set; }
        public ChildDTO GrandChild { get; set; }
        public int ID_ { get; set; }
        public int? IDs { get; set; }
        public int? ID2 { get; set; }
    }

    public class GrandParent
    {
        public Parent Parent { get; set; }
    }

    public class Parent
    {
        public ICollection<Child> Children { get; set; }

        private Child _child;
        public Child Child
        {
            get { return _child; }
            set
            {
                _child = value;
                _child.Parent = this;
            }
        }
        public DateTime DateTime { get; set; }
    }

    public class Child
    {
        private Parent _parent;
        public Parent Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                if (GrandChild != null)
                    GrandChild.Parent = _parent;
            }
        }

        public int ID { get; set; }
        public Child GrandChild { get; set; }
        public int IDs { get; set; }
        public int? ID2 { get; set; }
    }

    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<User, UserModel>()
                    .ForMember(d => d.Id, opt => opt.MapFrom(s => s.UserId))
                    .ForMember(d => d.FullName, opt => opt.MapFrom(s => string.Concat(s.FirstName, " ", s.LastName)))
                    .ForMember(d => d.LoggedOn, opt => opt.MapFrom(s => s.IsLoggedOn ? "Y" : "N"))
                    .ForMember(d => d.IsOverEighty, opt => opt.MapFrom(s => s.Age > 80))
                    .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.Account == null ? string.Empty : string.Concat(s.Account.Branch.Name, " ", s.Account.Branch.Id.ToString())))
                    .ForMember(d => d.AgeInYears, opt => opt.MapFrom(s => s.Age))
                    .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.Active))
                    .ForMember(d => d.AccountModel, opt => opt.MapFrom(s => s.Account));

            CreateMap<UserModel, User>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FirstName, opt => opt.MapFrom(s => s.FullName))
                .ForMember(d => d.IsLoggedOn, opt => opt.MapFrom(s => s.LoggedOn.ToUpper() == "Y"))
                .ForMember(d => d.Age, opt => opt.MapFrom(s => s.AgeInYears))
                .ForMember(d => d.Active, opt => opt.MapFrom(s => s.IsActive))
                .ForMember(d => d.Account, opt => opt.MapFrom(s => s.AccountModel));

            CreateMap<Account, AccountModel>()
                .ForMember(d => d.Bal, opt => opt.MapFrom(s => s.Balance))
                .ForMember(d => d.DateCreated, opt => opt.MapFrom(s => Helpers.TruncateTime(s.CreateDate).Value))
                .ForMember(d => d.Description, opt => opt.MapFrom(s => string.Concat(s.Type, " ", s.Number)))
                //.ForMember(d => d.ComboName, opt => opt.ResolveUsing<CustomResolver>())
                .ForMember(d => d.ThingModels, opt => opt.MapFrom(s => s.Things))
                .ForMember(d => d.UserModels, opt => opt.MapFrom(s => s.Users));

            CreateMap<AccountModel, Account>()
                .ForMember(d => d.Balance, opt => opt.MapFrom(s => s.Bal))
                .ForMember(d => d.Things, opt => opt.MapFrom(s => s.ThingModels))
                .ForMember(d => d.Users, opt => opt.MapFrom(s => s.UserModels));

            CreateMap<Thing, ThingModel>()
                .ForMember(d => d.FooModel, opt => opt.MapFrom(s => s.Foo))
                .ForMember(d => d.BarModel, opt => opt.MapFrom(s => s.Bar))
                .ForMember(d => d.Color, opt => opt.MapFrom(s => s.Car.Color));

            CreateMap<ThingModel, Thing>()
                .ForMember(d => d.Foo, opt => opt.MapFrom(s => s.FooModel))
                .ForMember(d => d.Bar, opt => opt.MapFrom(s => s.BarModel));

            CreateMap<ItemDto, Item>()
                    .ForMember(dest => dest.Date, opts => opts.MapFrom(x => x.CreateDate));

            CreateMap<Item, ItemDto>()
                .ForMember(dest => dest.CreateDate, opts => opts.MapFrom(x => x.Date));

            CreateMap<OrderLine, ItemDTO>()
                .ForMember(dto => dto.Name, conf => conf.MapFrom(src => src.ItemName));
            CreateMap<OrderLine, OrderLineDTO>()
                .ForMember(dto => dto.Item, conf => conf.MapFrom(ol => ol));
            CreateMap<OrderLineDTO, OrderLine>()
                .ForMember(ol => ol.ItemName, conf => conf.MapFrom(dto => dto.Item.Name));

            CreateMap<GrandParent, GrandParentDTO>().ReverseMap();
            CreateMap<Parent, ParentDTO>()
                .ForMember(dest => dest.DateTime, opts => opts.MapFrom(x => x.DateTime))
                .ReverseMap()
                .ForMember(dest => dest.DateTime, opts => opts.MapFrom(x => x.DateTime));
            CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID_, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID_));

            CreateMissingTypeMaps = true;
        }
    }

    public static class GenericTestExtensionMethods
    {
        public static bool Any<T>(this IEnumerable<T> self, Func<T, int, bool> func)
        {
            return self.Where(func).Any();
        }

        public static bool AnyParamReverse<T>(this IEnumerable<T> self, Func<T, T, bool> func)
        {
            return self.Any(t => func(t, t));
        }

        public static bool Lambda<T>(this T self, Func<T, bool> func)
        {
            return func(self);
        }
    }

    internal static class Helpers
    {
        //[DbFunction("Edm", "TruncateTime")]
        public static DateTime? TruncateTime(DateTime? date)
        {
            return date.HasValue ? date.Value.Date : (DateTime?)null;
        }
    }
}
