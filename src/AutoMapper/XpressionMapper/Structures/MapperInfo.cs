using System;
using System.Linq.Expressions;

namespace AutoMapper.XpressionMapper.Structures
{
    public class MapperInfo
    {
        public MapperInfo()
        {

        }

        public MapperInfo(ParameterExpression newParameter, Type sourceType, Type destType)
        {
            NewParameter = newParameter;
            SourceType = sourceType;
            DestType = destType;
        }

        public Type SourceType { get; set; }
        public Type DestType { get; set; }
        public ParameterExpression NewParameter { get; set; }
    }
}
