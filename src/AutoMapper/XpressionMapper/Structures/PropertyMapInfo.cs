using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.XpressionMapper.Structures
{
    public class PropertyMapInfo
    {
        public PropertyMapInfo(LambdaExpression CustomExpression, List<MemberInfo> DestinationPropertyInfos)
        {
            this.CustomExpression = CustomExpression;
            this.DestinationPropertyInfos = DestinationPropertyInfos;
        }

        public LambdaExpression CustomExpression { get; set; }
        public List<MemberInfo> DestinationPropertyInfos { get; set; }
    }
}
