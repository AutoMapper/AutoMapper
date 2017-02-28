using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.Configuration;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace AutoMapper.Mappers
{
    public class FromDynamicMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context, Func<TDestination> ifNull)
        {
            if (destination == null)
            {
                destination = ifNull();
            }
            object boxedDestination = destination;
            var sourceTypeDetails = context.ConfigurationProvider.Configuration.CreateTypeDetails(typeof(TSource));
            var destinationTypeDetails = context.ConfigurationProvider.Configuration.CreateTypeDetails(typeof(TDestination));
            foreach (var destinationMember in destinationTypeDetails.PublicWriteAccessors)
            {
                object sourceMemberValue;
                object destinationMemberValue;

                var sourceMember = sourceTypeDetails.PublicReadAccessors.SingleOrDefault(a => a.Name == destinationMember.Name);
                if (sourceMember != null)
                {

                    try
                    {
                        sourceMemberValue = GetDynamically(sourceMember, source);
                    }
                    catch (RuntimeBinderException)
                    {
                        continue;
                    }

                    var destinationType = destinationMember.GetMemberType();
                    var sourceType = sourceMember.GetMemberType();
                    if (sourceType.IsCollectionType() && destinationType.IsCollectionType()
                        && sourceType.GetGenericArguments()[0] != destinationType.GetGenericArguments()[0])
                    {
                        destinationMemberValue = context.MapMember(destinationMember, sourceMemberValue);
                        destinationMember.SetMemberValue(boxedDestination, destinationMemberValue);
                        continue;
                    }

                }
                try
                {
                    sourceMemberValue = GetDynamically(destinationMember, source);
                }
                catch (RuntimeBinderException)
                {
                    continue;
                }
                destinationMemberValue = context.MapMember(destinationMember, sourceMemberValue, boxedDestination);
                destinationMember.SetMemberValue(boxedDestination, destinationMemberValue);
            }
            return (TDestination)boxedDestination;
        }

        private static object GetDynamically(MemberInfo member, object target)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, member.Name, ToDynamicMapper.GetMemberType(member),
                                                            new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, target);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(FromDynamicMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return context.SourceType.IsDynamic() && !context.DestinationType.IsDynamic();
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, destExpression, contextExpression, Expression.Constant(CollectionMapperExtensions.Constructor(destExpression.Type)));
        }
    }

    public class ToDynamicMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context, Func<TDestination> ifNull)
        {
            if (destination == null)
            {
                destination = ifNull();
            }
            var sourceTypeDetails = context.ConfigurationProvider.Configuration.CreateTypeDetails(typeof(TSource));
            var destinationTypeDetails = context.ConfigurationProvider.Configuration.CreateTypeDetails(typeof(TDestination));
            foreach (var sourceMember in sourceTypeDetails.PublicReadAccessors)
            {
                object sourceMemberValue;
                try
                {
                    sourceMemberValue = sourceMember.GetMemberValue(source);
                }
                catch (RuntimeBinderException)
                {
                    continue;
                }

                var destinationMember = destinationTypeDetails.PublicWriteAccessors.SingleOrDefault(a => a.Name == sourceMember.Name);
                object destinationMemberValue;
                if (destinationMember != null)
                {
                    var destinationType = destinationMember.GetMemberType();
                    var sourceType = sourceMember.GetMemberType();
                    if (sourceType.IsCollectionType() && destinationType.IsCollectionType()
                        && sourceType.GetGenericArguments()[0] != destinationType.GetGenericArguments()[0])
                    {
                        destinationMemberValue = context.MapMember(destinationMember, sourceMemberValue);
                        SetDynamically(destinationMember, destination, destinationMemberValue);
                        continue;
                    }
                }
                destinationMemberValue = context.MapMember(sourceMember, sourceMemberValue);
                SetDynamically(sourceMember, destination, destinationMemberValue);
            }
            return destination;
        }

        public static Type GetMemberType(MemberInfo member)
        {
            var memberType = member.GetMemberType();
            return memberType.IsArray ? typeof(object) : memberType;
        }

        private static void SetDynamically(MemberInfo member, object target, object value)
        {
            var binder = Binder.SetMember(CSharpBinderFlags.None, member.Name, GetMemberType(member),
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

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, destExpression, contextExpression, Expression.Constant(CollectionMapperExtensions.Constructor(destExpression.Type)));
        }
    }
}
