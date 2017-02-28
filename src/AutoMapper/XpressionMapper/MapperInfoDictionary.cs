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
            if (this.ContainsKey(key))
                return;

            if (typeMappings.ContainsKey(key.Type))
                this.Add(key, new MapperInfo(Expression.Parameter(typeMappings[key.Type], key.Name), key.Type, typeMappings[key.Type]));
            else
                this.Add(key, new MapperInfo(Expression.Parameter(key.Type, key.Name), key.Type, key.Type));
        }
    }
}
