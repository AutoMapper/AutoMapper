using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
    
    public static class ConstructorMapExtensions
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

        // This should be in Execution but it's used for a test
        public static bool ConstructorParameterMatches(this TypeMap typeMap, string destinationPropertyName)
        {
            return typeMap.ConstructorMap?.CtorParams.Any(c => !c.DefaultValue && string.Equals(c.Parameter.Name, destinationPropertyName, StringComparison.OrdinalIgnoreCase)) == true;
        }
    }
}

namespace AutoMapper.Execution
{
    using static Expression;
    using static Internal.ExpressionFactory;

    internal static class ConstructorMapExtensions
    {
        internal static Expression CreateDestinationFunc(this TypeMapPlanBuilder planBuilder, out bool constructorMapping)
        {
            var newDestFunc = ToType(planBuilder.CreateNewDestinationFunc(out constructorMapping), planBuilder.TypeMap.DestinationTypeToUse);

            var getDest = planBuilder.TypeMap.DestinationTypeToUse.IsValueType()
                ? newDestFunc
                : Coalesce(planBuilder.InitialDestination, newDestFunc);

            Expression destinationFunc = Assign(planBuilder.Destination, getDest);

            return destinationFunc.GetCache(planBuilder);
        }

        private static Expression CreateNewDestinationFunc(this TypeMapPlanBuilder planBuilder, out bool constructorMapping)
        {
            constructorMapping = false;
            if (planBuilder.TypeMap.DestinationCtor != null)
                return planBuilder.TypeMap.DestinationCtor.ReplaceParameters(planBuilder.Source, planBuilder.Context);

            if (planBuilder.TypeMap.ConstructDestinationUsingServiceLocator)
                return planBuilder.TypeMap.DestinationTypeToUse.CreateInstance(planBuilder.Context);

            if (planBuilder.TypeMap.ConstructorMap?.CanResolve == true)
            {
                constructorMapping = true;
                return planBuilder.TypeMap.ConstructorMap.CreateNewDestinationExpression(planBuilder);
            }
#if NET45 || NET40
            if (planBuilder.TypeMap.DestinationTypeToUse.IsInterface())
            {
                var ctor = Call(null,
                    typeof(DelegateFactory).GetDeclaredMethod(nameof(DelegateFactory.CreateCtor), new[] { typeof(Type) }),
                    Call(null,
                        typeof(ProxyGenerator).GetDeclaredMethod(nameof(ProxyGenerator.GetProxyType)),
                        Constant(planBuilder.TypeMap.DestinationTypeToUse)));
                // We're invoking a delegate here to make it have the right accessibility
                return Invoke(ctor);
            }
#endif
            return DelegateFactory.GenerateConstructorExpression(planBuilder.TypeMap.DestinationTypeToUse);
        }

        private static Expression CreateNewDestinationExpression(this ConstructorMap constructorMap, TypeMapPlanBuilder planBuilder)
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

namespace AutoMapper.QueryableExtensions
{
    internal static class ConstructorMapExtensions
    {
        private static readonly IExpressionResultConverter[] ExpressionResultConverters =
        {
            new MemberResolverExpressionResultConverter(),
            new MemberGetterExpressionResultConverter()
        };

        internal static LambdaExpression DestinationConstructorExpression(this TypeMap typeMap, Expression instanceParameter)
        {
            return typeMap.ConstructExpression ??
                   Lambda(typeMap.ConstructorMap?.CanResolve == true
                       ? typeMap.ConstructorMap.NewExpression(instanceParameter)
                       : New(typeMap.DestinationTypeToUse));
        }

        private static Expression NewExpression(this ConstructorMap constructorMap, Expression instanceParameter)
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

        internal static ExpressionResolutionResult ResolveExpression(this Expression instanceParameter, Type currentType, PropertyMap propertyMap, LetPropertyMaps letPropertyMaps)
        {
            var result = new ExpressionResolutionResult(instanceParameter, currentType);
            var matchingExpressionConverter =
                ExpressionResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, propertyMap));

            return matchingExpressionConverter?.GetExpressionResolutionResult(result, propertyMap, letPropertyMaps)
                     ?? throw new Exception("Can't resolve this to Queryable Expression");
        }
    }

    internal class MemberResolverExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap, LetPropertyMaps letPropertyMaps)
        {
            Expression subQueryMarker;
            if ((subQueryMarker = letPropertyMaps.GetSubQueryMarker()) != null)
            {
                return new ExpressionResolutionResult(subQueryMarker, subQueryMarker.Type);
            }
            return ExpressionResolutionResult(expressionResolutionResult, propertyMap.CustomExpression);
        }

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, LambdaExpression lambdaExpression)
        {
            var currentChild = lambdaExpression.ReplaceParameters(expressionResolutionResult.ResolutionExpression);
            var currentChildType = currentChild.Type;

            return new ExpressionResolutionResult(currentChild, currentChildType);
        }

        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap) => ExpressionResolutionResult(expressionResolutionResult, null);

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            PropertyMap propertyMap) => propertyMap.CustomExpression != null;

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap) => false;
    }

    internal class MemberGetterExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap, LetPropertyMaps letPropertyMaps)
            => ExpressionResolutionResult(expressionResolutionResult, propertyMap.SourceMembers);

        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap)
            => ExpressionResolutionResult(expressionResolutionResult, propertyMap.SourceMembers);

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, IEnumerable<MemberInfo> sourceMembers)
            => sourceMembers.Aggregate(expressionResolutionResult, ExpressionResolutionResult);

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, MemberInfo getter)
        {
            var member = MakeMemberAccess(expressionResolutionResult.ResolutionExpression, getter);
            return new ExpressionResolutionResult(member, member.Type);
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap)
            => propertyMap.SourceMembers.Any();

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, ConstructorParameterMap propertyMap)
            => propertyMap.SourceMembers.Any();
    }
}