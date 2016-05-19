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
    
    public class FromDynamicMapper : IObjectMapExpression
    {
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context)
            where TSource : DynamicObject
        {
            if (destination == null)
                destination = (TDestination) (!context.ConfigurationProvider.AllowNullDestinationValues
                    ? ObjectCreator.CreateNonNullValue(typeof (TDestination))
                    : ObjectCreator.CreateObject(typeof (TDestination)));
            var memberContext = new ResolutionContext(context);
            foreach (var member in new TypeDetails(typeof(TDestination)).PublicWriteAccessors)
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
                var destinationMemberValue = memberContext.Map(member, sourceMemberValue);
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

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.SourceType, context.DestinationType).Invoke(null, new[] { context.SourceValue, context.DestinationValue, context });
        }

        public bool IsMatch(TypePair context)
        {
            return context.SourceType.IsDynamic() && !context.DestinationType.IsDynamic();
        }

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, destExpression, contextExpression);
        }
    }

    public class ToDynamicMapper : IObjectMapExpression
    {
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context)
            where TDestination : DynamicObject
        {
            if (destination == null)
                destination = (TDestination)(!context.ConfigurationProvider.AllowNullDestinationValues
                    ? ObjectCreator.CreateNonNullValue(typeof(TDestination))
                    : ObjectCreator.CreateObject(typeof(TDestination)));
            var memberContext = new ResolutionContext(context);
            foreach (var member in new TypeDetails(typeof(TSource)).PublicWriteAccessors)
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
                var destinationMemberValue = memberContext.Map(member, sourceMemberValue);
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

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.SourceType, context.DestinationType).Invoke(null, new[] { context.SourceValue, context.DestinationValue, context });
        }

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsDynamic() && !context.SourceType.IsDynamic();
        }

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, destExpression, contextExpression);
        }
    }
}
