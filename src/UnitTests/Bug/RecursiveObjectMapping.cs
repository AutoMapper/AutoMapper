namespace AutoMapper.UnitTests
{
    using System.Linq;
    using Shouldly;
    using Xunit;
    using QueryableExtensions;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AutoMapper.Mappers.Internal;
    using AutoMapper.Configuration.Internal;

    public class RecursiveObjectMapping : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(c =>
        {
            var recursiveMapper = new RecursiceObjectMapper();
            c.Mappers.Insert(0, recursiveMapper);
            c.Advanced.BeforeSeal(cfg => recursiveMapper.Configuration = cfg);
        });

        [Fact]
        public void Can_resolve_specific_typemap()
        {
            new Action(() => Mapper.ConfigurationProvider.ResolveTypeMap(typeof(System), typeof(SystemViewModel)))
                .ShouldThrow<NotImplementedException>()
                .Message.ShouldBe(nameof(RecursiceObjectMapper));
        }

        [Fact]
        public void Can_map_with_recursivly_objectmapper()
        {
            var system = new System
            {
                Name = "My First System",
                Contacts = new List<Contact>
                {
                    new Contact
                    {
                        Name = "John",
                        Emails = new List<Email>()
                        {
                            new Email
                            {
                                 Address = "john@doe.com"
                            }
                        }
                    }
                }
            };

            new Action(() => Mapper.Map<SystemViewModel>(system))
                .ShouldThrow<NotImplementedException>()
                .Message.ShouldBe(nameof(RecursiceObjectMapper));
        }

        public class RecursiceObjectMapper : IObjectMapper
        {
            public IConfigurationProvider Configuration { get; set; }

            public bool IsMatch(TypePair context)
            {
                if (PrimitiveHelper.IsEnumerableType(context.SourceType)
                   && PrimitiveHelper.IsCollectionType(context.DestinationType))
                {
                    var realType = new TypePair(ElementTypeHelper.GetElementType(context.SourceType), ElementTypeHelper.GetElementType(context.DestinationType));
                    var typeMap = Configuration.ResolveTypeMap(realType);
                    if (typeMap != null && realType == new TypePair(typeof(Contact), typeof(ContactViewModel)))
                    {
                        return true;
                    }
                }

                return false;
            }

            public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            {
                throw new NotImplementedException(nameof(RecursiceObjectMapper));
            }
        }

        public class System
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ICollection<Contact> Contacts { get; set; }
        }

        public class Contact
        {
            public int Id { get; set; }
            public int SystemId { get; set; }
            public string Name { get; set; }

            public System System { get; set; }

            public ICollection<Email> Emails { get; set; }
        }

        public class Email
        {
            public int Id { get; set; }

            public int ContactId { get; set; }
            public string Address { get; set; }

            public Contact Contact { get; set; }
        }

        public class SystemViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ICollection<ContactViewModel> Contacts { get; set; }
        }

        public class ContactViewModel
        {
            public int Id { get; set; }
            public int SystemId { get; set; }
            public string Name { get; set; }

            public SystemViewModel System { get; set; }

            public ICollection<EmailViewModel> Emails { get; set; }
        }

        public class EmailViewModel
        {
            public int Id { get; set; }
            public int ContactId { get; set; }
            public string Address { get; set; }

            public ContactViewModel Contact { get; set; }
        }
    }
}
