using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Mappers;
using AutoMapper.Configuration.Conventions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class ConvensionTest
    {
        public class Client
        {
            public int ID { get; set; }
            public string Value { get; set; }
            public string Transval { get; set; }
        }

        public class ClientDto
        {
            public int ID { get; set; }
            public string ValueTransfer { get; set; }
            public string val { get; set; }
        }

        [Fact]
        public void Fact()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateProfile("New Profile", profile =>
                {
                    profile.AddMemberConfiguration().AddName<PrePostfixName>(
                            _ => _.AddStrings(p => p.DestinationPostfixes, "Transfer")
                                .AddStrings(p => p.Postfixes, "Transfer")
                                .AddStrings(p => p.DestinationPrefixes, "Trans")
                                .AddStrings(p => p.Prefixes, "Trans"));
                    profile.AddConditionalObjectMapper().Where((s, d) => s.Name.Contains(d.Name) || d.Name.Contains(s.Name));
                });
            });

            var mapper = config.CreateMapper();
            var a2 = mapper.Map<ClientDto>(new Client() { Value= "Test", Transval = "test"});
            a2.ValueTransfer.ShouldEqual("Test");
            a2.val.ShouldEqual("test");

            var a = mapper.Map<Client>(new ClientDto() { ValueTransfer = "TestTransfer", val = "testTransfer"});
            a.Value.ShouldEqual("TestTransfer");
            a.Transval.ShouldEqual("testTransfer");

            var clients = mapper.Map<Client[]>(new[] { new ClientDto() });
            Expression<Func<Client, bool>> expr = c => c.ID < 5;
            var clientExp = mapper.Map<Expression<Func<ClientDto,bool>>>(expr);
        }

        public class ConventionProfile : Profile
        {
            protected override void Configure()
            {
                AddMemberConfiguration().AddName<PrePostfixName>(
                        _ => _.AddStrings(p => p.DestinationPostfixes, "Transfer")
                            .AddStrings(p => p.Postfixes, "Transfer")
                            .AddStrings(p => p.DestinationPrefixes, "Trans")
                            .AddStrings(p => p.Prefixes, "Trans"));
                AddConditionalObjectMapper().Where((s, d) => s.Name.Contains(d.Name) || d.Name.Contains(s.Name));
            }
        }

        public class ToDTO : Profile
        {
            protected override void Configure()
            {
                AddMemberConfiguration().AddName<PrePostfixName>(
                        _ => _.AddStrings(p => p.Postfixes, "Transfer")
                            .AddStrings(p => p.DestinationPrefixes, "Trans")).NameMapper.GetMembers.AddCondition(_ => _ is PropertyInfo);
                AddConditionalObjectMapper().Where((s, d) => s.Name == d.Name + "Dto");
            }
        }
        public class FromDTO : Profile
        {
            protected override void Configure()
            {
                AddMemberConfiguration().AddName<PrePostfixName>(
                        _ => _.AddStrings(p => p.DestinationPostfixes, "Transfer")
                            .AddStrings(p => p.Prefixes, "Trans")).NameMapper.GetMembers.AddCondition(_ => _ is PropertyInfo);
                AddConditionalObjectMapper().Where((s, d) => d.Name == s.Name + "Dto");
            }
        }
        public void Fact2()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ConventionProfile>();
            });

            var mapper = config.CreateMapper();
            var a2 = mapper.Map<ClientDto>(new Client() { Value = "Test", Transval = "test" });
            a2.ValueTransfer.ShouldEqual("Test");
            a2.val.ShouldEqual("test");

            var a = mapper.Map<Client>(new ClientDto() { ValueTransfer = "TestTransfer", val = "testTransfer" });
            a.Value.ShouldEqual("TestTransfer");
            a.Transval.ShouldEqual("testTransfer");

            var clients = mapper.Map<Client[]>(new[] { new ClientDto() });
            Expression<Func<Client, bool>> expr = c => c.ID < 5;
            var clientExp = mapper.Map<Expression<Func<ClientDto, bool>>>(expr);
        }

        [Fact]
        public void Fact3()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ToDTO>();
                cfg.AddProfile<FromDTO>();
            });

            var mapper = config.CreateMapper();
            var a2 = mapper.Map<ClientDto>(new Client() { Value = "Test", Transval = "test" });
            a2.ValueTransfer.ShouldEqual("Test");
            a2.val.ShouldEqual("test");

            var a = mapper.Map<Client>(new ClientDto() { ValueTransfer = "TestTransfer", val = "testTransfer" });
            a.Value.ShouldEqual("TestTransfer");
            a.Transval.ShouldEqual("testTransfer");

            var clients = mapper.Map<Client[]>(new[] { new ClientDto() });
            Expression<Func<Client, bool>> expr = c => c.ID < 5;
            var clientExp = mapper.Map<Expression<Func<ClientDto, bool>>>(expr);
        }

        [Fact]
        public void Should_Work_Without_Explicitly_Mapping_Before_Hand()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ToDTO>();
                cfg.AddProfile<FromDTO>();
            });

            Expression<Func<Client, bool>> expr = c => c.ID < 5;
            var clientExp = config.CreateMapper().Map<Expression<Func<ClientDto, bool>>>(expr);
        }
    }
}