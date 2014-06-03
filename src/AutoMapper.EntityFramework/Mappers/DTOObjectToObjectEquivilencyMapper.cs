using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.EquivilencyExpression;

namespace AutoMapper.Mappers
{
    public class DTOToEFObjectEquivilencyMapper<TDatabase> : DTOObjectToObjectEquivilencyMapper
        where TDatabase : IObjectContextAdapter, new()
    {
        public DTOToEFObjectEquivilencyMapper()
            : base(new GenerateEntityFrameworkPrimaryKeyPropertyMaps<TDatabase>())
        {
        }
    }

    public abstract class DTOObjectToObjectEquivilencyMapper : ExpressionMapperBase
    {
        private readonly IGeneratePropertyMaps _generatePropertyMaps;

        protected DTOObjectToObjectEquivilencyMapper(IGeneratePropertyMaps generatePropertyMaps)
        {
            _generatePropertyMaps = generatePropertyMaps;
        }

        public override bool IsMatch(ResolutionContext context)
        {
            base.IsMatch(context);

            var allreadyCached = AllreadyCached(context);
            if (allreadyCached.HasValue)
                return allreadyCached.Value;

            var destExpressArgType = GetPredicateExpressionArgumentType(context.DestinationType);

            var mapper = Mapper.FindTypeMapFor(context.SourceType, destExpressArgType);
            if (mapper == null)
                return SetMapToBadValue(context);

            SetExpression(context, destExpressArgType);
            return true;
        }

        private void SetExpression(ResolutionContext context, Type objType)
        {
            var propertyMappings = _generatePropertyMaps.GeneratePropertyMaps(context.SourceType, objType);
            MapperCache[context.SourceType][context.DestinationType] =
                new GenerateObjectEquivilentExpressionFromPropertyMaps(propertyMappings).GeneratEquivilentExpression(
                    context.SourceValue, objType);
        }
    }

    public class DTOExpressionToExpressionMapper : ExpressionMapperBase
    {
        public override bool IsMatch(ResolutionContext context)
        {
            base.IsMatch(context);

            var allreadyCached = AllreadyCached(context);
            if (allreadyCached.HasValue)
                return allreadyCached.Value;

            var srcExpressArgType = GetPredicateExpressionArgumentType(context.SourceType);
            var destExpressArgType = GetPredicateExpressionArgumentType(context.DestinationType);

            var mapper = Mapper.FindTypeMapFor(srcExpressArgType, destExpressArgType);
            if (mapper == null)
                return SetMapToBadValue(context);

            SetExpression(context, srcExpressArgType, destExpressArgType);
            return true;
        }

        private void SetExpression(ResolutionContext context, Type srcType, Type destType)
        {
            var mapper = Mapper.FindTypeMapFor(srcType, destType);
            var propertyMappings = mapper.GetPropertyMaps();
            MapperCache[context.SourceType][context.DestinationType] =
                new GenerateEquivilentExpressionFromEquivilentExpressionUsingPropertyMaps(propertyMappings).GeneratEquivilentExpression(context.SourceValue, destType);
        }
    }

    public interface IGenerateEquivilentExpression
    {
        Expression GeneratEquivilentExpression(object value, Type destinationType);
    }

    public class GenerateEquivilentExpressionFromEquivilentExpressionUsingPropertyMaps : IGenerateEquivilentExpression
    {
        private readonly IEnumerable<PropertyMap> _propertyMaps;

        public GenerateEquivilentExpressionFromEquivilentExpressionUsingPropertyMaps(IEnumerable<PropertyMap> propertyMaps)
        {
            _propertyMaps = propertyMaps;
        }

        public Expression GeneratEquivilentExpression(object value, Type destinationType)
        {
            return CreateEquivilentExpression(value, destinationType);
        }

        private Expression CreateEquivilentExpression(object value, Type destType)
        {
            var express = value as LambdaExpression;
            var destExpr = Expression.Parameter(destType, express.Parameters[0].Name);

            var result = new CustomExpressionVisitor(destExpr, _propertyMaps).Visit(express.Body);

            return Expression.Lambda(result, destExpr);
        }
    }

    public class CustomExpressionVisitor : ExpressionVisitor
    {
        readonly ParameterExpression _parameter;
        private readonly IEnumerable<PropertyMap> _propertyMaps;

        public CustomExpressionVisitor(ParameterExpression parameter, IEnumerable<PropertyMap> propertyMaps)
        {
            _parameter = parameter;
            _propertyMaps = propertyMaps;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.MemberType == MemberTypes.Property)
            {
                var memberName = node.Member.Name;
                var matchPM = _propertyMaps.FirstOrDefault(pm => pm.SourceMember == node.Member);
                if (matchPM == null)
                    throw new Exception("No matching PropertyMap");
                var memberExpression = Expression.Property(Visit(node.Expression), matchPM.DestinationProperty.MemberInfo as PropertyInfo);
                return memberExpression;
            }
            return base.VisitMember(node);
        }
    }

    public class GenerateObjectEquivilentExpressionFromPropertyMaps : IGenerateEquivilentExpression
    {
        private readonly IEnumerable<PropertyMap> _propertyMaps;

        public GenerateObjectEquivilentExpressionFromPropertyMaps(IEnumerable<PropertyMap> propertyMaps)
        {
            _propertyMaps = propertyMaps;
        }

        public Expression GeneratEquivilentExpression(object value, Type destinationType)
        {
            return CreateEquivilentExpression(value, destinationType);
        }

        private Expression CreateEquivilentExpression(object value, Type destType)
        {
            var destExpr = Expression.Parameter(destType, "dest");

            var equalExpr = _propertyMaps.Select(pm => SourcePropertyEqualsDestinationConstant(pm, value, destExpr)).ToList();
            if (!equalExpr.Any())
                return null;
            var finalExpression = equalExpr.Skip(1).Aggregate(equalExpr.First(), Expression.And);

            return Expression.Lambda(finalExpression, destExpr);
        }

        private BinaryExpression SourcePropertyEqualsDestinationConstant(PropertyMap propertyMap, object value, Expression destExpr)
        {
            var srcPropExpr = Expression.Constant((propertyMap.SourceMember as PropertyInfo).GetValue(value, null));
            var destPropExpr = Expression.Property(destExpr, propertyMap.DestinationProperty.MemberInfo as PropertyInfo);
            return Expression.Equal(srcPropExpr, destPropExpr);
        }
    }

    public abstract class ExpressionMapperBase : IObjectMapper
    {
        protected readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Expression>> MapperCache = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, Expression>>();

        private readonly Expression _badExpression = Expression.Empty();

        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            return MapperCache[context.SourceType][context.DestinationType];
        }

        public virtual bool IsMatch(ResolutionContext context)
        {
            UpdateCache(context);
            return false;
        }

        protected bool? AllreadyCached(ResolutionContext context)
        {
            var expression = MapperCache[context.SourceType][context.DestinationType];
            if (expression == _badExpression)
                return false;
            if (expression == null)
                return null;
            return true;
        }

        protected static Type GetPredicateExpressionArgumentType(Type type)
        {
            var isExpression = typeof(Expression).IsAssignableFrom(type);
            if (!isExpression)
                return null;

            var expressionOf = type.GetGenericArguments().First();
            var isFunction = expressionOf.GetGenericTypeDefinition() == typeof (Func<,>);
            if (!isFunction)
                return null;

            var isPredicate = expressionOf.GetGenericArguments()[1] == typeof (bool);
            if (!isPredicate)
                return null;

            var objType = expressionOf.GetGenericArguments().First();
            return objType;
        }

        private void UpdateCache(ResolutionContext context)
        {
            MapperCache.AddOrUpdate(context.SourceType, new ConcurrentDictionary<Type, Expression>(), (type, dict) => dict);
            MapperCache[context.SourceType].AddOrUpdate(context.DestinationType, (Expression)null, (type, obj) => obj);
        }

        protected bool SetMapToBadValue(ResolutionContext context)
        {
            MapperCache[context.SourceType][context.DestinationType] = _badExpression;
            return false;
        }
    }
}