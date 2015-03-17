using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AWSample.AutoMappings;
using AWSample.Domain.Person;
using AWSample.EF.POCO.Person;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XpressionMapper.Extensions;
using XpressionMapper.Structures;

namespace AWSample.UnitTests
{
    [TestClass]
    public class ExpressionMappingTests
    {
        [TestInitialize()]
        public void Initialize()
        {
            SetupAutoMapper();
        }

        #region Tests
        [TestMethod]
        public void Map_projection()
        {
            //Arrange
            Expression<Func<BusinessEntityContactModel, bool>> selection = s => s != null && s.Person.FullName.StartsWith("A");
            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                selection.CreateMapperInfo<BusinessEntityContactModel, BusinessEntityContact>("p")
            }.ToDictionary(i => i.SourceType);

            //Act
            Expression<Func<BusinessEntityContact, bool>> selectionMapped = selection.MapExpression<BusinessEntityContact, bool>(infoDictionary);

            //Assert
            Assert.IsNotNull(selectionMapped);
        }

        [TestMethod]
        public void Map_orderBy_thenBy_expression()
        {
            //Arrange
            Expression<Func<IQueryable<PersonModel>, IQueryable<PersonModel>>> exp = q => q.OrderBy(s => s.BusinessEntityID).ThenBy(s => s.FullName);
            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                exp.CreateMapperInfo<IQueryable<PersonModel>, IQueryable<Person>>("q"),//mapping for outer expression must come first
                exp.CreateMapperInfo<PersonModel, Person>("p")
            }.ToDictionary(i => i.SourceType);

            //Act
            Expression<Func<IQueryable<Person>, IQueryable<Person>>> expMapped = exp.MapExpression<IQueryable<Person>, IQueryable<Person>>(infoDictionary);

            //Assert
            Assert.IsNotNull(expMapped);
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
