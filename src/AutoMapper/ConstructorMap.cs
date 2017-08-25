using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Execution;
using AutoMapper.QueryableExtensions.Impl;
using static System.Linq.Expressions.Expression;
using static AutoMapper.Execution.ExpressionBuilder;

namespace AutoMapper
{
    public class ConstructorMap
    {
        private readonly IList<ConstructorParameterMap> _ctorParams = new List<ConstructorParameterMap>();

        public ConstructorInfo Ctor { get; }
        public TypeMap TypeMap { get; }
        internal IEnumerable<ConstructorParameterMap> CtorParams => _ctorParams;

        public ConstructorMap(ConstructorInfo ctor, TypeMap typeMap)
        {
            Ctor = ctor;
            TypeMap = typeMap;
        }

        public bool CanResolve => CtorParams.All(param => param.CanResolve);
        
        public void AddParameter(ParameterInfo parameter, MemberInfo[] resolvers, bool canResolve)
        {
            _ctorParams.Add(new ConstructorParameterMap(parameter, resolvers, canResolve));
        }
    }
    
    public static class Extensions
    {
        /// <summary>
        /// Supply a custom instantiation expression for the destination type for LINQ projection
        /// </summary>0
        /// <param name="mappingExpression"></param>
        /// <param name="ctor">Callback to create the destination type given the source object</param>
        /// <returns>Itself</returns>
        public static IMappingExpression ConstructProjectionUsing(this IMappingExpression mappingExpression, LambdaExpression ctor)
        {
            mappingExpression.TypeMapActions.Add(tm => tm.ConstructExpression = ctor);

            return mappingExpression;
        }

        /// <summary>
        /// Supply a custom instantiation expression for the destination type for LINQ projection
        /// </summary>
        /// <param name="mappingExpression"></param>
        /// <param name="ctor">Callback to create the destination type given the source object</param>
        /// <returns>Itself</returns>
        public static IMappingExpression<TSource, TDestination> ConstructProjectionUsing<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TSource, TDestination>> ctor)
        {
            mappingExpression.TypeMapActions.Add(tm =>
            {
                tm.ConstructExpression = ctor;

                var ctxtParam = Parameter(typeof(ResolutionContext), "ctxt");
                var srcParam = Parameter(typeof(TSource), "src");

                var body = ctor.ReplaceParameters(srcParam);

                tm.DestinationCtor = Lambda(body, srcParam, ctxtParam);
            });

            return mappingExpression;
        }
    }
}

namespace AutoMapper.Map.ConstructorMap
{
    internal static class Extensions
    {
        internal static Expression CreateNewDestinationExpression(this global::AutoMapper.ConstructorMap constructorMap, TypeMapPlanBuilder planBuilder)
        {
            if (!constructorMap.CanResolve)
                return null;

            var ctorArgs = constructorMap.CtorParams.Select(_ => CreateConstructorParameterExpression(_, planBuilder));

            ctorArgs =
                ctorArgs.Zip(constructorMap.Ctor.GetParameters(),
                        (exp, pi) => exp.Type == pi.ParameterType ? exp : Convert(exp, pi.ParameterType))
                    .ToArray();
            var newExpr = New(constructorMap.Ctor, ctorArgs);
            return newExpr;
        }

        private static Expression CreateConstructorParameterExpression(this ConstructorParameterMap ctorParamMap, TypeMapPlanBuilder planBuilder)
        {
            var valueResolverExpression = planBuilder.ResolveSource(ctorParamMap);
            var sourceType = valueResolverExpression.Type;
            var resolvedValue = Variable(sourceType, "resolvedValue");
            return Block(new[] { resolvedValue },
                Assign(resolvedValue, valueResolverExpression),
                MapExpression(planBuilder.ConfigurationProvider, planBuilder.TypeMap.Profile,
                    new TypePair(sourceType, ctorParamMap.DestinationType), resolvedValue, planBuilder.Context, null, null));
        }

        private static Expression ResolveSource(this TypeMapPlanBuilder planBuilder, ConstructorParameterMap ctorParamMap)
        {
            if (ctorParamMap.CustomExpression != null)
                return ctorParamMap.CustomExpression.ConvertReplaceParameters(planBuilder.Source)
                    .IfNotNull(ctorParamMap.DestinationType);
            if (ctorParamMap.CustomValueResolver != null)
                return ctorParamMap.CustomValueResolver.ConvertReplaceParameters(planBuilder.Source, planBuilder.Context);
            if (ctorParamMap.Parameter.IsOptional)
            {
                ctorParamMap.DefaultValue = true;
                return Constant(ctorParamMap.Parameter.GetDefaultValue(), ctorParamMap.Parameter.ParameterType);
            }
            return ctorParamMap.SourceMembers.Aggregate(
                    (Expression)planBuilder.Source,
                    (inner, getter) => getter is MethodInfo
                        ? Call(getter.IsStatic() ? null : inner, (MethodInfo)getter)
                        : (Expression)MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                )
                .IfNotNull(ctorParamMap.DestinationType);
        }
    }
}

namespace AutoMapper.QueryableExtensions.ConstructorMap
{
    internal static class Extensions
    {
        private static readonly IExpressionResultConverter[] ExpressionResultConverters =
        {
            new MemberResolverExpressionResultConverter(),
            new MemberGetterExpressionResultConverter()
        };

        internal static LambdaExpression DestinationConstructorExpression(this TypeMap typeMap, Expression instanceParameter)
        {
            return typeMap.ConstructExpression ?? Lambda(typeMap.NewExpression(instanceParameter));
        }

        private static Expression NewExpression(this TypeMap typeMap, Expression instanceParameter)
        {
            return typeMap.ConstructorMap?.CanResolve == true
                ? typeMap.ConstructorMap.NewExpression(instanceParameter)
                : New(typeMap.DestinationTypeToUse);
        }

        private static Expression NewExpression(this global::AutoMapper.ConstructorMap constructorMap, Expression instanceParameter)
        {
            var parameters = constructorMap.CtorParams.Select(map =>
            {
                var result = new ExpressionResolutionResult(instanceParameter, constructorMap.Ctor.DeclaringType);

                var matchingExpressionConverter =
                    ExpressionResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, map));

                result = matchingExpressionConverter?.GetExpressionResolutionResult(result, map)
                         ?? throw new Exception("Can't resolve this to Queryable Expression");

                return result;
            });
            return New(constructorMap.Ctor, parameters.Select(p => p.ResolutionExpression));
        }
    }
}