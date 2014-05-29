using System;
using System.Collections.Generic;

namespace AutoMapper.EquivilencyExpression
{
    public interface IGeneratePropertyMaps
    {
        IEnumerable<PropertyMap> GeneratePropertyMaps(Type srcType, Type destType);
    }
}