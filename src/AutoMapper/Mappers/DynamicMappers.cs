using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace AutoMapper.Mappers
{
    using Execution;

    public class FromDynamicMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context, Func<TDestination> ifNull)
        {
            if (destination == null)
                destination = ifNull();
            var destinationTypeDetails = context.ConfigurationProvider.Configuration.CreateTypeDetails(typeof(TDestination));
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
                var destinationMemberValue = context.MapMember(member, sourceMemberValue, destination);
                member.SetMemberValue(destination, destinationMemberValue);
            }
            return destination;
        }

        private static object GetDynamically(MemberInfo member, object target)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, member.Name, member.GetMemberType(),
                                                            new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, target);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(FromDynamicMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return context.SourceType.IsDynamic() && !context.DestinationType.IsDynamic();
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, destExpression, contextExpression, Expression.Constant(CollectionMapperExtensions.Constructor(destExpression.Type)));
        }
    }

    public class ToDynamicMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context, Func<TDestination> ifNull)
        {
            if (destination == null)
                destination = ifNull();
            var sourceTypeDetails = context.ConfigurationProvider.Configuration.CreateTypeDetails(typeof(TSource));
            foreach (var member in sourceTypeDetails.PublicReadAccessors)
            {
                object sourceMemberValue;
                try
                {
                    sourceMemberValue = member.GetMemberValue(source);
                }
                catch (RuntimeBinderException)
                {
                    continue;
                }
                var destinationMemberValue = context.MapMember(member, sourceMemberValue);
                SetDynamically(member, destination, destinationMemberValue);
            }
            return destination;
        }

        private static void SetDynamically(MemberInfo member, object target, object value)
        {
            var binder = Binder.SetMember(CSharpBinderFlags.None, member.Name, member.GetMemberType(),
                new[]{
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });
            var callsite = CallSite<Func<CallSite, object, object, object>>.Create(binder);
            callsite.Target(callsite, target, value);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ToDynamicMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsDynamic() && !context.SourceType.IsDynamic();
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, destExpression, contextExpression, Expression.Constant(CollectionMapperExtensions.Constructor(destExpression.Type)));
        }
    }
}
