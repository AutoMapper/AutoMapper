using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.XpressionMapper.Structures;

namespace AutoMapper.XpressionMapper
{
    public class MapperInfoDictionary : Dictionary<ParameterExpression, MapperInfo>
    {
        public MapperInfoDictionary(ParameterExpressionEqualityComparer comparer) : base(comparer)
        {
        }

        //const string PREFIX = "p";
        public void Add(ParameterExpression key, Dictionary<Type, Type> typeMappings)
        {
            if (ContainsKey(key))
                return;

            Add(key, typeMappings.ContainsKey(key.Type)
                    ? new MapperInfo(Expression.Parameter(typeMappings[key.Type], key.Name), key.Type,typeMappings[key.Type])
                    : new MapperInfo(Expression.Parameter(key.Type, key.Name), key.Type, key.Type));
        }
    }
}
