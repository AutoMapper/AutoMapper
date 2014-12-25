using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AutoMapper.Impl;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class QueryMapperVisitor : ExpressionVisitor
    {
        private readonly IQueryable _destQuery;
        private readonly ParameterExpression _instanceParameter;
        private readonly IMappingEngine _mappingEngine;
        private Type _sourceType;
        private Type _destinationType;

        protected QueryMapperVisitor(Type sourceType, Type destinationType, IQueryable destQuery, 
            IMappingEngine mappingEngine)
        {
            _sourceType = sourceType;
            _destinationType = destinationType;
            _destQuery = destQuery;
            _mappingEngine = mappingEngine;
            _instanceParameter = Expression.Parameter(destinationType, "dto");
        }

        public static IQueryable<TDestination> Map<TSource, TDestination>(IQueryable<TSource> sourceQuery,
            IQueryable<TDestination> destQuery, IMappingEngine map)
        {
            var visitor = new QueryMapperVisitor(typeof(TSource), typeof(TDestination), destQuery, map);
            var expr = visitor.Visit(sourceQuery.Expression);

            var newDestQuery = destQuery.Provider.CreateQuery<TDestination>(expr);
            return newDestQuery;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _instanceParameter;
        }

        public override Expression Visit(Expression node)
        {
            // OData Client DataServiceQuery initial expression node type
            if (node != null && (int)node.NodeType == 10000)
            {
                return node;
            }
            var newNode = base.Visit(node);
            return newNode;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var query = node.Value as IQueryable;
            // It is data source of queryable object instance
            if (query != null && query.ElementType == _sourceType)
                return _destQuery.Expression;
            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var delegateType = ChangeLambdaArgTypeFormSourceToDest<T>();
            var newLambda = Expression.Lambda(delegateType, this.Visit(node.Body),
                node.Parameters.Select(p => (ParameterExpression)Visit(p)));
            return newLambda;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = changeMethodArgTypeFormSourceToDest(node.Method);
            var args = node.Arguments.Select(Visit);
            var target = Visit(node.Object);
            var newMethodCall = Expression.Call(target, method, args);
            return newMethodCall;
        }

        private MethodInfo changeMethodArgTypeFormSourceToDest(MethodInfo mi)
        {
            if (!mi.IsGenericMethod)
                return mi;
            var genericMethod = mi.GetGenericMethodDefinition();
            var methodArgs = mi.GetGenericArguments();
            methodArgs = ReplaceSourceTypeToDestTypeByMaping(methodArgs);
            return genericMethod.MakeGenericMethod(methodArgs);

        }

        private Type ChangeLambdaArgTypeFormSourceToDest<TLambdaType>()
        {
            Type type = typeof(TLambdaType);
            if (type.IsGenericType)
            {
                var genArgs = type.GetGenericArguments();
                var newGenArgs = ReplaceSourceTypeToDestTypeByMaping(genArgs);
                return type.GetGenericTypeDefinition().MakeGenericType(newGenArgs);
            }
            return type;
        }

        private Type[] ReplaceSourceTypeToDestTypeByMaping(params Type[] genArgs)
        {
            var newGenArgs = new Type[genArgs.Length];
            for (int i = 0; i < genArgs.Length; i++)
            {
                var genArg = genArgs[i];
                if (genArg.IsGenericType)
                {
                    var genSubArgs = genArg.GetGenericArguments();
                    var newGenSubArgs = ReplaceSourceTypeToDestTypeByMaping(genSubArgs);
                    newGenArgs[i] = genArg.GetGenericTypeDefinition().MakeGenericType(newGenSubArgs);
                    continue;
                }
                newGenArgs[i] = genArg == _sourceType ? _destinationType : genArg;
            }
            return newGenArgs;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Expression parentExpr = Visit(node.Expression);

            var typeMap = _mappingEngine.ConfigurationProvider.FindTypeMapFor(node.Member.ReflectedType, parentExpr.Type);
            var propertyMap = typeMap.GetPropertyMaps()
                   .FirstOrDefault(pm => pm.CanResolveValue() && pm.SourceMember.Name == node.Member.Name);

            return Expression.MakeMemberAccess(parentExpr, propertyMap.DestinationProperty.MemberInfo);
        }


    }
}
