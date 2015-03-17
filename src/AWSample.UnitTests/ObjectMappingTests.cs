using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AWSample.AutoMappings;
using AWSample.Domain.Person;
using AWSample.EF.POCO.Person;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AWSample.UnitTests
{
    [TestClass]
    public class ObjectMappingTests
    {
        [TestInitialize()]
        public void Initialize()
        {
            SetupAutoMapper();
        }

        #region Tests
        [TestMethod]
        public void Map_person_graph_to_person_model_graph()
        {
            //Arrange
            Person person = new Person()
            {
                BusinessEntityContacts = new List<BusinessEntityContact>
                {
                    new BusinessEntityContact()
                    {
                        BusinessEntityID = 1,
                        ContactTypeID = 2,
                        EntityState = EF.POCO.EntityStateType.Modified,
                        ModifiedDate = new DateTime(2014, 12, 22),
                        PersonID = 1,
                        rowguid = Guid.NewGuid()
                    },
                    new BusinessEntityContact()
                    {
                        BusinessEntityID = 2,
                        ContactTypeID = 3,
                        EntityState = EF.POCO.EntityStateType.Modified,
                        ModifiedDate = new DateTime(2014, 12, 12),
                        PersonID = 1,
                        rowguid = Guid.NewGuid()
                    }
                },
                BusinessEntityID = 1,
                FirstName = "Jack",
                MiddleName = "Michael",
                LastName = "Smith",
                AdditionalContactInfo = "",
                Demographics = "",
                EmailPromotion = 1,
                EntityState = EF.POCO.EntityStateType.Modified,
                ModifiedDate = new DateTime(2014, 12, 10),
                NameStyle = true,
                PersonType = "Type1",
                rowguid = Guid.NewGuid(),
                Suffix = "Jr",
                Title = "Mr"
            };

            //Act
            PersonModel mapped = Mapper.Map<PersonModel>(person);

            //Assert
            Assert.IsNotNull(mapped);
        }
        #endregion Tests

        private void SetupAutoMapper()
        {
            Type interfaceType = typeof(IAutoMapperMapping);
            List<Type> list = interfaceType.Assembly.GetTypes().Where(p => interfaceType.IsAssignableFrom(p) && !p.IsAbstract && !p.IsGenericTypeDefinition && !p.IsInterface).ToList();
            foreach (Type type in list)
            {
                System.Reflection.MethodInfo mi = type.GetMethod("Setup");
                object o = Activator.CreateInstance(type);
                mi.Invoke(o, null);
            }
        }
    }
}
