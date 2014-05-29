using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using AutoMapper.EquivilencyExpression;

namespace AutoMapper.Mappers
{
    public class GenerateEntityFrameworkPrimaryKeyPropertyMaps<TDatabaseContext> : IGeneratePropertyMaps
        where TDatabaseContext : IObjectContextAdapter, new()
    {
        private readonly TDatabaseContext _context = new TDatabaseContext();
        private readonly MethodInfo _createObjectSetMethodInfo = typeof(ObjectContext).GetMethod("CreateObjectSet", Type.EmptyTypes);

        public IEnumerable<PropertyMap> GeneratePropertyMaps(Type srcType, Type destType)
        {
            var mapper = Mapper.FindTypeMapFor(srcType, destType);
            var propertyMaps = mapper.GetPropertyMaps();
            var createObjectSetMethod = _createObjectSetMethodInfo.MakeGenericMethod(destType);
            dynamic objectSet = createObjectSetMethod.Invoke(_context.ObjectContext, null);

            IEnumerable<EdmMember> keyMembers = objectSet.EntitySet.ElementType.KeyMembers;
            var primaryKeyPropertyMatches = keyMembers.Select(m => propertyMaps.FirstOrDefault(p => p.DestinationProperty.Name == m.Name));

            return primaryKeyPropertyMatches;
        }
    }
}