using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper.EquivilencyExpression
{
    public interface IGeneratePropertyMatches
    {
        IDictionary<PropertyInfo, PropertyInfo> GeneratePropertyMatches(Type srcType, Type destType);
    }
}