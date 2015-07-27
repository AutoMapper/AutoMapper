using System;
using System.Collections.Generic;
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
                        new Source {SrcValue = 5},
                        new Source {SrcValue = 4},
                        new Source {SrcValue = 7}
                    };

        public class Source
        {
            public int SrcValue { get; set; }
            public string StringValue { get; set; }
            public string[] Strings { get; set; }
        }

        public class Destination
        {
            public int DestValue { get; set; }
            public string StringValue { get; set; }
            public string[] Strings { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Destination, Source>()
                       .ForMember(s => s.SrcValue, opt => opt.MapFrom(d => d.DestValue))
                       .ReverseMap()
                       .ForMember(d => d.DestValue, opt => opt.MapFrom(s => s.SrcValue));
        }

        [Fact]
        public void Shoud_support_const_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
              .UseAsDataSource().For<Destination>()
              .Where(s => s.DestValue > 6);

            result.Count().ShouldEqual(1);
            result.Any(s => s.DestValue > 6).ShouldBeTrue();
        }

        [Fact]
        public void Shoud_use_destination_elementType()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource().For<Destination>();

            result.ElementType.ShouldEqual(typeof(Destination));

            result = result.Where(s => s.DestValue > 3);
            result.ElementType.ShouldEqual(typeof(Destination));
        }

        [Fact]
        public void Shoud_support_single_item_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource().For<Destination>();

            result.First(s => s.DestValue > 6).ShouldBeType<Destination>();
        }

        [Fact]
        public void Shoud_support_IEnumerable_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
              .UseAsDataSource().For<Destination>()
              .Where(s => s.DestValue > 6);

            List<Destination> list = result.ToList();
        }

        [Fact]
        public void Shoud_convert_source_item_to_destination()
        {
            IQueryable<Destination> result = _source.AsQueryable()
                .UseAsDataSource().For<Destination>();

            var destItem = result.First(s => s.DestValue == 7);
            var sourceItem = _source.First(s => s.SrcValue == 7);

            destItem.DestValue.ShouldEqual(sourceItem.SrcValue);
        }

        [Fact]
        public void Shoud_support_order_by_statement_result()
        {
            IQueryable<Destination> result = _source.AsQueryable()
              .UseAsDataSource().For<Destination>()
              .OrderByDescending(s => s.DestValue);

            result.First().DestValue.ShouldEqual(_source.Max(s => s.SrcValue));
        }

        [Fact]
        public void Shoud_support_any_stupid_thing_you_can_throw_at_it()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource().For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => s.DestValue);

            result.First().ShouldEqual(_source.Max(s => s.SrcValue));
        }

        [Fact]
        public void Shoud_support_string_return_type()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource().For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => s.StringValue);

            result.First().ShouldEqual(null);
        }
        [Fact]
        public void Shoud_support_enumerable_return_type()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource().For<Destination>()
              .Where(s => true && 5.ToString() == "5" && s.DestValue.ToString() != "0")
              .OrderBy(s => s.DestValue).SkipWhile(d => d.DestValue < 7).Take(1)
              .OrderByDescending(s => s.DestValue).Select(s => s.Strings);

            result.First().Count().ShouldEqual(0);
        }

        [Fact]
        public void Shoud_support_any_stupid_thing_you_can_throw_at_it_with_annonumus_types()
        {
            var result = _source.AsQueryable()
              .UseAsDataSource().For<Destination>()
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
            SetupAutoMapper();
            var result = _source2.AsQueryable()
              .UseAsDataSource().For<UserModel>().OrderBy(s => s.Id).ThenBy(s => s.FullName).Select(s => (object)s.AccountModel.ThingModels.Select(b => b.BarModel));

            (result.First() as IEnumerable<string>).Last().ShouldEqual("Bar 4");
        }

        [Fact]
        public void Map_orderBy_thenBy_expression()
        {
            SetupAutoMapper();
            var result = _source2.AsQueryable()
              .UseAsDataSource().For<UserModel>().Select(s => (object)s.AccountModel.ThingModels);

            (result.First() as IEnumerable<Thing>).Last().Bar.ShouldEqual("Bar 2");
        }

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
                .ForMember(d => d.DateCreated, opt => opt.MapFrom(s => s.CreateDate))
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


