using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.XpressionMapper.Structures
{
    public class PropertyMapInfo
    {
        public PropertyMapInfo(LambdaExpression CustomExpression, MemberInfo DestinationPropertyInfo)
        {
            this.CustomExpression = CustomExpression;
            this.DestinationPropertyInfo = DestinationPropertyInfo;
        }

        public LambdaExpression CustomExpression { get; set; }
        public MemberInfo DestinationPropertyInfo { get; set; }
    }
}
