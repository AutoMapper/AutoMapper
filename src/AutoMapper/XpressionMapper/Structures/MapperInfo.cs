using System;
using System.Linq.Expressions;

namespace AutoMapper.XpressionMapper.Structures
{
    public class MapperInfo
    {
        public MapperInfo()
        {

        }

        public MapperInfo(ParameterExpression newParameter, Type _sourceType, Type _destType)
        {
            NewParameter = newParameter;
            SourceType = _sourceType;
            DestType = _destType;
        }

        public Type SourceType { get; set; }
        public Type DestType { get; set; }
        public ParameterExpression NewParameter { get; set; }
    }
}
