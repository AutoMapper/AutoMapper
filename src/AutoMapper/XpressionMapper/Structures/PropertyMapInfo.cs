using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.XpressionMapper.Structures
{
    public class PropertyMapInfo
    {
        public PropertyMapInfo(LambdaExpression customExpression, List<MemberInfo> destinationPropertyInfos)
        {
            CustomExpression = customExpression;
            DestinationPropertyInfos = destinationPropertyInfos;
        }

        public LambdaExpression CustomExpression { get; set; }
        public List<MemberInfo> DestinationPropertyInfos { get; set; }
    }
}
