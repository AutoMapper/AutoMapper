using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.Execution;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;
namespace AutoMapper.Internal.Mappers
{
    using static Expression;
    using static ExpressionBuilder;
    public class FromDynamicMapper : IObjectMapper
    {
        private static readonly MethodInfo MapMethodInfo = typeof(FromDynamicMapper).GetStaticMethod(nameof(Map));
        private static object Map(object source, object destination, Type destinationType, ResolutionContext context, ProfileMap profileMap)
        {
            destination ??= ObjectFactory.CreateInstance(destinationType);
            var destinationTypeDetails = profileMap.CreateTypeDetails(destinationType);
            foreach (var member in destinationTypeDetails.WriteAccessors)
            {
                object sourceMemberValue;
                try
                {
                    sourceMemberValue = GetDynamically(member.Name, source);
                }
                catch (RuntimeBinderException)
                {
                    continue;
                }
                var destinationMemberValue = context.MapMember(member, sourceMemberValue, destination);
                member.SetMemberValue(destination, destinationMemberValue);
            }
            return destination;
        }
        private static object GetDynamically(string memberName, object target)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, null,
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, target);
        }
        public bool IsMatch(TypePair context) => context.SourceType.IsDynamic() && !context.DestinationType.IsDynamic();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            Call(MapMethodInfo, sourceExpression, destExpression.ToObject(), Constant(destExpression.Type), ContextParameter, Constant(profileMap));
    }
}