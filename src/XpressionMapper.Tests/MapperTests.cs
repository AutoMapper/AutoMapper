using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using XpressionMapper.Extensions;
using XpressionMapper.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XpressionMapper.Tests
{
    [TestClass]
    public class MapperTests
    {
        [TestInitialize()]
        public void Initialize()
        {
            SetupAutoMapper();
        }

        #region Tests
        [TestMethod]
        public void Map_object_type_change()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s.LoggedOn == "Y";
            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                selection.CreateMapperInfo<UserModel, User>("p")
            }.ToDictionary(i => i.SourceType);

            //Act
            Expression<Func<User, bool>> selectionMapped = selection.MapExpression<User, bool>(infoDictionary);

            //Assert
            Assert.IsNotNull(selectionMapped);
        }

        [TestMethod]
        public void Map_object_type_change_2()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s.IsOverEighty;
            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                selection.CreateMapperInfo<UserModel, User>("p")
            }.ToDictionary(i => i.SourceType);

            //Act
            Expression<Func<User, bool>> selectionMapped = selection.MapExpression<User, bool>(infoDictionary);

            //Assert
            Assert.IsNotNull(selectionMapped);
        }

        [TestMethod]
        public void Map_object_including_child_and_grandchild()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s != null && s.AccountModel != null && s.AccountModel.Bal == 555.20;
            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                selection.CreateMapperInfo<UserModel, User>("p")
            }.ToDictionary(i => i.SourceType);

            //Act
            Expression<Func<User, bool>> selectionMapped = selection.MapExpression<User, bool>(infoDictionary);

            //Assert
            Assert.IsNotNull(selectionMapped);
        }

        [TestMethod]
        public void Map_project_truncated_time()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s != null && s.AccountModel != null && s.AccountModel.DateCreated == DateTime.Now;
            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                selection.CreateMapperInfo<UserModel, User>("p")
            }.ToDictionary(i => i.SourceType);

            //Act
            Expression<Func<User, bool>> selectionMapped = selection.MapExpression<User, bool>(infoDictionary);

            //Assert
            Assert.IsNotNull(selectionMapped);
        }

        [TestMethod]
        public void Map_projection()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s != null && s.AccountModel.ComboName.StartsWith("A");
            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                selection.CreateMapperInfo<UserModel, User>("p")
            }.ToDictionary(i => i.SourceType);

            //Act
            Expression<Func<User, bool>> selectionMapped = selection.MapExpression<User, bool>(infoDictionary);

            //Assert
            Assert.IsNotNull(selectionMapped);
        }

        [TestMethod]
        public void Map__flattened_property()
        {
            //Arrange
            int age = 21;
            Expression<Func<UserModel, bool>> selection = s => ((s != null ? s.AccountName : null) ?? "").ToLower().StartsWith("A".ToLower()) && ((s.AgeInYears == age) && s.IsActive);
            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                selection.CreateMapperInfo<UserModel, User>("p")
            }.ToDictionary(i => i.SourceType);

            //Act
            Expression<Func<User, bool>> selectionMapped = selection.MapExpression<User, bool>(infoDictionary);

            //Assert
            Assert.IsNotNull(selectionMapped);
        }

        [TestMethod]
        public void Map_select_method()
        {
            //Arrange
            Expression<Func<UserModel, object>> selection = s => s.AccountModel.ThingModels.Select(x => x.BarModel);
            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                selection.CreateMapperInfo<UserModel, User>("p"),//mapping for outer expression must come first
                selection.CreateMapperInfo<ThingModel, Thing>("q")
            }.ToDictionary(i => i.SourceType);

            //Act
            Expression<Func<User, object>> selectionMapped = selection.MapExpression<User, object>(infoDictionary);

            //Assert
            Assert.IsNotNull(selectionMapped);
        }

        [TestMethod]
        public void Map_orderBy_thenBy_expression()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IQueryable<UserModel>>> exp = q => q.OrderBy(s => s.Id).ThenBy(s => s.FullName);
            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                exp.CreateMapperInfo<IQueryable<UserModel>, IQueryable<User>>("q"),//mapping for outer expression must come first
                exp.CreateMapperInfo<UserModel, User>("p")
            }.ToDictionary(i => i.SourceType);

            //Act
            Expression<Func<IQueryable<User>, IQueryable<User>>> expMapped = exp.MapExpression<IQueryable<User>, IQueryable<User>>(infoDictionary);

            //Assert
            Assert.IsNotNull(expMapped);
        }
        #endregion Tests

        private static void SetupAutoMapper()
        {
            Mapper.CreateMap<User, UserModel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.UserId))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.LoggedOn, opt => opt.MapFrom(s => s.IsLoggedOn ? "Y" : "N"))
            .ForMember(d => d.IsOverEighty, opt => opt.MapFrom(s => s.Age > 80))
            .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.Account == null ? string.Empty : string.Concat(s.Account.FirstName, " ", s.Account.LastName)))
            .ForMember(d => d.AgeInYears, opt => opt.MapFrom(s => s.Age))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.Active))
            .ForMember(d => d.AccountModel, opt => opt.MapFrom(s => s.Account));

            Mapper.CreateMap<UserModel, User>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName))
                .ForMember(d => d.IsLoggedOn, opt => opt.MapFrom(s => s.LoggedOn.ToUpper() == "Y"))
                .ForMember(d => d.Age, opt => opt.MapFrom(s => s.AgeInYears))
                .ForMember(d => d.Active, opt => opt.MapFrom(s => s.IsActive))
                .ForMember(d => d.Account, opt => opt.MapFrom(s => s.AccountModel));

            Mapper.CreateMap<Account, AccountModel>()
                .ForMember(d => d.Bal, opt => opt.MapFrom(s => s.Balance))
                .ForMember(d => d.DateCreated, opt => opt.MapFrom(s => Helpers.TruncateTime(s.CreateDate).Value))
                .ForMember(d => d.ComboName, opt => opt.MapFrom(s => string.Concat(s.FirstName, " ", s.LastName)))
                .ForMember(d => d.ThingModels, opt => opt.MapFrom(s => s.Things));

            Mapper.CreateMap<AccountModel, Account>()
                .ForMember(d => d.Balance, opt => opt.MapFrom(s => s.Bal))
                .ForMember(d => d.Things, opt => opt.MapFrom(s => s.ThingModels));

            Mapper.CreateMap<Thing, ThingModel>()
                .ForMember(d => d.FooModel, opt => opt.MapFrom(s => s.Foo))
                .ForMember(d => d.BarModel, opt => opt.MapFrom(s => s.Bar));

            Mapper.CreateMap<ThingModel, Thing>()
                .ForMember(d => d.Foo, opt => opt.MapFrom(s => s.FooModel))
                .ForMember(d => d.Bar, opt => opt.MapFrom(s => s.BarModel));

            //Mapper.CreateMap<IEnumerable<Thing>, IEnumerable<ThingModel>>();
            //Mapper.CreateMap<IEnumerable<ThingModel>, IEnumerable<Thing>>();
            //Mapper.CreateMap<IEnumerable<User>, IEnumerable<UserModel>>();
            //Mapper.CreateMap<IEnumerable<UserModel>, IEnumerable<User>>();
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
}
