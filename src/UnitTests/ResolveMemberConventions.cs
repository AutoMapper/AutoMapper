using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Execution;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class ResolveMemberConventions
    {
        public class ReverseSourceToDestinationNameMapperAttributesMember : ISourceToDestinationNameMapper
        {
            private static readonly IDictionary<TypeDetails, Dictionary<MemberInfo, IEnumerable<MapFromAttribute>>> Cache = new ConcurrentDictionary<TypeDetails, Dictionary<MemberInfo, IEnumerable<MapFromAttribute>>>();

            public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
            {
                var destTypeDetails = new TypeDetails(destType);
                if (!Cache.ContainsKey(destTypeDetails))
                    Cache.Add(destTypeDetails, getTypeInfoMembers.GetMemberInfos(destTypeDetails).ToDictionary(mi => mi, mi => mi.GetCustomAttributes(typeof(MapFromAttribute), true).OfType<MapFromAttribute>()));

                var matchingMapTo = Cache[destTypeDetails].SelectMany(kp => kp.Value.Where(_ => string.Compare(kp.Key.Name, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0)).FirstOrDefault();
                return matchingMapTo == null
                    ? null
                    : getTypeInfoMembers.GetMemberInfos(typeInfo)
                        .FirstOrDefault(mi => mi.Name == matchingMapTo.MatchingName);
            }
        }

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class MapFromAttribute : Attribute
        {
            public string MatchingName { get; private set; }

            public MapFromAttribute(string matchingName)
            {
                MatchingName = matchingName;
            }
        }

        public class From
        {
            public int Int { get; set; }
        }

        public class To
        {
            [MapFrom("Int")]
            public string String { get; set; }
        }

        [Fact]
        public void Should_Pass_Over_Destination_Type_In_GetMatchingMemberInfo()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddMemberConfiguration().AddName<ReverseSourceToDestinationNameMapperAttributesMember>();
                cfg.CreateMissingTypeMaps = true;
            }).CreateMapper();
            var from = new From {Int = 5};
            mapper.Map<To>(from).String.ShouldEqual("5");

            var query = new[] { from }.AsQueryable().ProjectTo<To>(mapper.ConfigurationProvider).First();
            query.String.ShouldEqual("5");
        }

        public class CultureInfoLocilization : IChildMemberConfiguration
        {
            public bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType,
                Type destType, Type destMemberType,
                string nameToSearch, LinkedList<IValueResolver> resolvers, IMemberConfiguration parent)
            {
                var memberType = sourceType.PublicReadAccessors.FirstOrDefault(mi => IsEnumerable(mi, nameToSearch));
                if (memberType == null)
                    return false;
                var itemType = memberType.GetMemberType().GetGenericElementType();
                var cultureInfoMember = itemType.GetProperties().First(p => p.GetMemberType() == typeof (CultureInfo));
                var searchProperty = itemType.GetProperty(nameToSearch);
                var firstMethodInfo =
                    typeof (Enumerable).GetMethods()
                        .First(m => m.Name == "First" && m.GetParameters().Count() > 1)
                        .MakeGenericMethod(itemType);
                var sourceParam = Expression.Parameter(sourceType.Type, "s");
                Expression expression = Expression.PropertyOrField(sourceParam, memberType.Name);
                var lambdaParam = Expression.Parameter(itemType, "e");
                var callFirstExpression = Expression.Call(firstMethodInfo, expression,
                    Expression.Lambda(
                        Expression.Equal(Expression.PropertyOrField(lambdaParam, cultureInfoMember.Name),
                            Expression.Constant(CultureInfo.CurrentCulture)), lambdaParam));
                var searchPropertyOfFirstExpression = Expression.PropertyOrField(callFirstExpression,
                    searchProperty.Name);
                var lambdaExpression = Expression.Lambda(searchPropertyOfFirstExpression, sourceParam);
                var delegateResolverType = typeof (DelegateBasedResolver<,>).MakeGenericType(sourceType.Type,
                    searchProperty.GetMemberType());
                resolvers.AddLast(
                    Activator.CreateInstance(delegateResolverType, lambdaExpression) as IValueResolver);
                return true;
            }

            private static bool IsEnumerable(MemberInfo mi, string name)
            {
                var type = mi.GetMemberType();
                var itemType = type.GetGenericElementType();
                return type.IsEnumerableType() && itemType.GetProperty(name) != null &&
                       itemType.GetProperties().Any(p => p.GetMemberType() == typeof (CultureInfo));
            }
        }

        public class Question
        {
            public ICollection<QuestionLocalization> Localizations { get; } = new List<QuestionLocalization>();
        }
        public class QuestionLocalization
        {
            public string Text { get; set; }
            public CultureInfo CultureInfo { get; set; }
        }

        private class QuestionDTO
        {
            public string Text { get; set; }
        }

        [Fact]
        public void Should_Pass_Over_Destination_Member_Type_In_MapDestinationPropertyToSource()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddMemberConfiguration().AddMember<CultureInfoLocilization>();
                cfg.CreateMissingTypeMaps = true;
            }).CreateMapper();

            var question = new Question
            {
                Localizations = { new QuestionLocalization { CultureInfo = CultureInfo.CurrentCulture, Text = "Current" } }
            };
            mapper.Map<QuestionDTO>(question).Text.ShouldEqual("Current");

            var query = new [] {question}.AsQueryable().ProjectTo<QuestionDTO>(mapper.ConfigurationProvider).First();
            query.Text.ShouldEqual("Current");
        }
    }
}