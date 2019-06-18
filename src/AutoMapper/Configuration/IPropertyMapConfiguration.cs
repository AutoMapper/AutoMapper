using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Configuration
{
    public interface IPropertyMapConfiguration
    {
        void Configure(TypeMap typeMap);
        MemberInfo DestinationMember { get; }
        LambdaExpression SourceExpression { get; }
        LambdaExpression GetDestinationExpression();
        IPropertyMapConfiguration Reverse();
    }
}