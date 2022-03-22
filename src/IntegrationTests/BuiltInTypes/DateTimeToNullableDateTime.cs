﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.UnitTests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.BuiltInTypes;

public class DateTimeToNullableDateTime : IntegrationTest<DateTimeToNullableDateTime.DatabaseInitializer>
{
    public class Parent
    {
        public int Id { get; set; }
        public int Value { get; set; }
                
    }
    public class ParentDto
    {
        public int? Value { get; set; }
        public DateTime? Date { get; set; }
    }

    private readonly DateTime _expected = new(2000, 1, 1);
    protected override MapperConfiguration CreateConfiguration() => new(cfg => 
        cfg.CreateProjection<Parent, ParentDto>().ForMember(dto => dto.Date, opt => opt.MapFrom(src => _expected)));
    public class TestContext : LocalDbContext
    {
        public DbSet<Parent> Parents { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
    {
        protected override void Seed(TestContext testContext)
        {
            testContext.Parents.Add(new Parent{ Value = 5 });
            base.Seed(testContext);
        }
    }
    [Fact]
    public void Should_not_fail()
    {
        using (var context = new TestContext())
        {
            ProjectTo<ParentDto>(context.Parents).Single().Date.ShouldBe(_expected);
        }
    }
}