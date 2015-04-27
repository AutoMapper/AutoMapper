using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AWSample.AutoMappings;
using AWSample.Domain.Person;
using AWSample.PersonService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AWSample.IntegrationTests
{
    [TestClass]
    public class PersonServiceTests
    {
        [TestInitialize()]
        public void Initialize()
        {
            SetupAutoMapper();
        }

        #region Tests
        [TestMethod]
        public void Get_twenty_items_after_row_100_order_by_full_name()
        {
            //Arrange
            Expression<Func<IQueryable<PersonModel>, IQueryable<PersonModel>>> exp = q => q.OrderBy(s => s.FullName).Skip(100).Take(20);
            //Act
            IPersonService service = new PersonService.PersonService();
            IList<PersonModel> list = service.GetList(null, exp).ToList();

            //Assert
            Assert.IsTrue(list.Count == 20);
        }

        [TestMethod]
        public void Get_person_modify_their_business_contacts_and_save()
        {
            //Arrange
            //Act
            IPersonService service = new PersonService.PersonService();
            PersonModel person = service.GetList(item => item.BusinessEntityID == 1521, null, new List<Expression<Func<PersonModel, object>>>() { item => item.BusinessEntityContacts }).SingleOrDefault();
            foreach(BusinessEntityContactModel bcm in person.BusinessEntityContacts)
            {
                bcm.ModifiedDate = new DateTime(2014, 12, 12);
            }

            service.UpdateBusinessContacts(person);
            person = service.GetList(item => item.BusinessEntityID == 1521, null, new List<Expression<Func<PersonModel, object>>>() { item => item.BusinessEntityContacts }).SingleOrDefault();
            //Assert
            Assert.IsTrue(person.BusinessEntityContacts.ToList()[0].ModifiedDate == new DateTime(2014, 12, 12));
            
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
