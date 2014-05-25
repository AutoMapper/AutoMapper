using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Mappers
{
    public class GenerateEquivilentExpressionsBasedOnEntityFrameworkPrimaryKeys<TDatabaseContext> : IGenerateEquivilentExpressions
        where TDatabaseContext : IObjectContextAdapter, new()
    {
        private readonly TDatabaseContext _context = new TDatabaseContext();
        private readonly MethodInfo _createObjectSetMethodInfo = typeof (ObjectContext).GetMethod("CreateObjectSet", Type.EmptyTypes);

        readonly IDictionary<Type,IDictionary<Type,IDictionary<PropertyInfo,PropertyInfo>>> _sourceToDestPropMaps = new Dictionary<Type, IDictionary<Type, IDictionary<PropertyInfo, PropertyInfo>>>();

        public bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType)
        {
            UpdateIfDontExistMappings(sourceType,destinationType);
            return GetPropertyMatches(sourceType, destinationType).Any();
        }

        public IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType)
        {
            var propertyRules = GetPropertyMatches(sourceType, destinationType).Select(ToPropertyComparerFunction).ToArray();
            var a = new GenerateEquivilentExpressionsBasedOnProperties(propertyRules);
            return a.GeneratEquivilentExpression(sourceType, destinationType);
        }

        private static Func<PropertyInfo, PropertyInfo, bool> ToPropertyComparerFunction(KeyValuePair<PropertyInfo, PropertyInfo> kp)
        {
            return (s, d) => s == kp.Key && d == kp.Value;
        }

        private void UpdateIfDontExistMappings(Type sourceType, Type destinationType)
        {
            UpdateDictionaryHolder(sourceType, destinationType);
            var properyMappings = _sourceToDestPropMaps[sourceType][destinationType];
            if (properyMappings == null)
            {
                var a = _createObjectSetMethodInfo.MakeGenericMethod(destinationType);
                try
                {
                    dynamic objectSet = a.Invoke(_context.ObjectContext, null);
                    IEnumerable<EdmMember> keyMembers = objectSet.EntitySet.ElementType.KeyMembers;
                    var keyProperties = keyMembers.ToDictionary(m => sourceType.GetProperty(m.Name),
                        m => destinationType.GetProperty(m.Name));
                    if (keyProperties.Values.Any(v => v == null))
                        _sourceToDestPropMaps[sourceType][destinationType] = new Dictionary<PropertyInfo, PropertyInfo>();
                    else
                        _sourceToDestPropMaps[sourceType][destinationType] = keyProperties;
                }
                catch (Exception)
                {
                    _sourceToDestPropMaps[sourceType][destinationType] = new Dictionary<PropertyInfo, PropertyInfo>();
                }
            }

        }

        private void UpdateDictionaryHolder(Type sourceType, Type destinationType)
        {
            if (!_sourceToDestPropMaps.ContainsKey(sourceType))
                _sourceToDestPropMaps.Add(sourceType, new Dictionary<Type, IDictionary<PropertyInfo, PropertyInfo>>());
            if (!_sourceToDestPropMaps[sourceType].ContainsKey(destinationType))
                _sourceToDestPropMaps[sourceType].Add(destinationType, null);
        }

        private IDictionary<PropertyInfo, PropertyInfo> GetPropertyMatches(Type sourceType, Type destinationType)
        {
            return _sourceToDestPropMaps[sourceType][destinationType];
        }
    }
}