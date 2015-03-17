using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XpressionMapper.Structures
{
    internal class PropertyMapInfo
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
