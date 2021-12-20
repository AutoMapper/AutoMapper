using System;
using Xunit;
using AutoMapper.Internal;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Configuration;

namespace AutoMapper.UnitTests
{
    /// <summary>
    /// Ignore this member for validation and skip during mapping
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreMapAttribute : Attribute
    {
    }
    static class Utils
    {
        public static TypeMap FindTypeMapFor<TSource, TDestination>(this IConfigurationProvider configurationProvider) => configurationProvider.Internal().FindTypeMapFor<TSource, TDestination>();
        public static IReadOnlyCollection<TypeMap> GetAllTypeMaps(this IConfigurationProvider configurationProvider) => configurationProvider.Internal().GetAllTypeMaps();
        public static TypeMap ResolveTypeMap(this IConfigurationProvider configurationProvider, Type sourceType, Type destinationType) => configurationProvider.Internal().ResolveTypeMap(sourceType, destinationType);
        public static void ForAllMaps(this IMapperConfigurationExpression configurationProvider, Action<TypeMap, IMappingExpression> configuration) => configurationProvider.Internal().ForAllMaps(configuration);
        public static void ForAllPropertyMaps(this IMapperConfigurationExpression configurationProvider, Func<PropertyMap, bool> condition, Action<PropertyMap, IMemberConfigurationExpression> memberOptions) => 
            configurationProvider.Internal().ForAllPropertyMaps(condition, memberOptions);
        public static void AddIgnoreMapAttribute(this IMapperConfigurationExpression configuration)
        {
            configuration.ForAllMaps((typeMap, mapExpression) => mapExpression.ForAllMembers(memberOptions =>
            {
                if (memberOptions.DestinationMember.Has<IgnoreMapAttribute>())
                {
                    memberOptions.Ignore();
                }
            }));
            configuration.ForAllPropertyMaps(propertyMap => propertyMap.SourceMember?.Has<IgnoreMapAttribute>() == true, 
                (_, memberOptions) => memberOptions.Ignore());
        }
    }

    public abstract class AutoMapperSpecBase : NonValidatingSpecBase
    {
        protected override void OnConfig(MapperConfiguration mapperConfiguration) => mapperConfiguration.AssertConfigurationIsValid();
    }

    public abstract class NonValidatingSpecBase : SpecBase
    {
        private IMapper _mapper;
        protected abstract MapperConfiguration CreateConfiguration();
        protected IGlobalConfiguration Configuration => Mapper.ConfigurationProvider.Internal();
        protected IMapper Mapper => _mapper ??= CreateMapper();
        IMapper CreateMapper()
        {
            var config = CreateConfiguration();
            OnConfig(config);
            return config.CreateMapper();
        }
        protected virtual void OnConfig(MapperConfiguration mapperConfiguration) { }
        protected TDestination Map<TDestination>(object source) => Mapper.Map<TDestination>(source);
        protected TypeMap FindTypeMapFor<TSource, TDestination>() => Configuration.FindTypeMapFor<TSource, TDestination>();
        protected void AssertConfigurationIsValid() => Configuration.AssertConfigurationIsValid();
        protected void AssertConfigurationIsValid<TSource, TDestination>() => Configuration.AssertConfigurationIsValid(Configuration.FindTypeMapFor<TSource, TDestination>());
        protected void AssertConfigurationIsValid(Type sourceType, Type destinationType) => Configuration.AssertConfigurationIsValid(Configuration.FindTypeMapFor(sourceType, destinationType));
        public void AssertConfigurationIsValid(string profileName) => Configuration.AssertConfigurationIsValid(profileName);
        public void AssertConfigurationIsValid<TProfile>() where TProfile : Profile, new() => Configuration.AssertConfigurationIsValid<TProfile>();
        protected IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, object parameters = null, params Expression<Func<TDestination, object>>[] membersToExpand) => 
            Mapper.ProjectTo(source, parameters, membersToExpand);
        protected IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand) =>
            Mapper.ProjectTo<TDestination>(source, parameters, membersToExpand);
        public IEnumerable<ProfileMap> GetProfiles() => Configuration.Profiles;
    }

    public abstract class SpecBaseBase
    {
        protected virtual void MainSetup()
        {
            Establish_context();
            Because_of();
        }

        protected virtual void MainTeardown()
        {
            Cleanup();
        }

        protected virtual void Establish_context()
        {
        }

        protected virtual void Because_of()
        {
        }

        protected virtual void Cleanup()
        {
        }
    }
    public abstract class SpecBase : SpecBaseBase, IDisposable
    {
        protected SpecBase()
        {
            Establish_context();
            Because_of();
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
        public static void Assert(IQueryable queryable, int count) => Assert(queryable.Expression, count);
        public static void Assert(Expression expression, int count)
        {
            var firstOrDefault = new FirstOrDefaultCounter();
            firstOrDefault.Visit(expression);
            firstOrDefault.Count.ShouldBe(count);
        }
    }
}