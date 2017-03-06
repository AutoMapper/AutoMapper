using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.Execution;

namespace AutoMapper.Mappers
{
    using Microsoft.CSharp.RuntimeBinder;
    using static Expression;
    using static ExpressionExtensions;

    public class FromDynamicMapper : IObjectMapper
    {
        private static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context, ProfileMap profileMap)
        {
            object boxedDestination = destination;
            var destinationTypeDetails = profileMap.CreateTypeDetails(typeof(TDestination));
            foreach (var member in destinationTypeDetails.PublicWriteAccessors)
            {
                object sourceMemberValue;
                try
                {
                    sourceMemberValue = GetDynamically(member, source);
                }
                catch (RuntimeBinderException)
                {
                    continue;
                }
                var destinationMemberValue = context.MapMember(member, sourceMemberValue, boxedDestination);
                member.SetMemberValue(boxedDestination, destinationMemberValue);
            }
            return (TDestination) boxedDestination;
        }

        private static object GetDynamically(MemberInfo member, object target)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, member.Name, ToDynamicMapper.GetMemberType(member),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, target);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(FromDynamicMapper).GetDeclaredMethod(nameof(Map));

        public bool IsMatch(TypePair context)
        {
            return context.SourceType.IsDynamic() && !context.DestinationType.IsDynamic();
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), 
                sourceExpression, 
                ToType(Coalesce(ToObject(destExpression), DelegateFactory.GenerateConstructorExpression(destExpression.Type)), destExpression.Type), 
                contextExpression,
                Constant(profileMap));
        }
    }
}