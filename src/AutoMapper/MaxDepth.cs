using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;
using static AutoMapper.Internal.ExpressionFactory;

namespace AutoMapper
{
    public static class MaxDepthExtensions
    {
        /// <summary>
        /// For self-referential types, limit recurse depth.
        /// Enables PreserveReferences.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="depth">Number of levels to limit to</param>
        /// <returns>Itself</returns>
        public static IMappingExpression<TSource, TDestination> MaxDepth<TSource, TDestination>(this IMappingExpression<TSource, TDestination> self, int depth)
        {
            self.TypeMapActions.Add(tm => tm.MaxDepth = depth);

            return self.PreserveReferences();
        }

        /// <summary>
        /// Preserve object identity. Useful for circular references.
        /// </summary>
        /// <param name="self"></param>
        /// <returns>Itself</returns>
        public static IMappingExpression<TSource, TDestination> PreserveReferences<TSource, TDestination>(this IMappingExpression<TSource, TDestination> self)
        {
            self.TypeMapActions.Add(tm => tm.PreserveReferences = true);

            return self;
        }
        
        /// <summary>
        /// Preserve object identity. Useful for circular references.
        /// </summary>
        /// <param name="self"></param>
        /// <returns>Itself</returns>
        public static IMappingExpression PreserveReferences(this IMappingExpression self)
        {
            self.TypeMapActions.Add(tm => tm.PreserveReferences = true);

            return self;
        }
    }
}

namespace AutoMapper.Execution
{
    internal static class MaxDepthExtensions
    {
        private static readonly Expression<Func<ResolutionContext, int>> GetTypeDepthInfo =
            ctxt => ctxt.GetTypeDepth(default(TypePair));

        private static readonly Expression<Action<ResolutionContext>> IncTypeDepthInfo =
            ctxt => ctxt.IncrementTypeDepth(default(TypePair));

        private static readonly Expression<Action<ResolutionContext>> DecTypeDepthInfo =
            ctxt => ctxt.DecrementTypeDepth(default(TypePair));

        private static readonly Expression<Func<IRuntimeMapper, ResolutionContext>> CreateContext =
            mapper => new ResolutionContext(mapper.DefaultContext.Options, mapper);

        private static readonly MethodInfo CacheDestinationMethodInfo =
            typeof(ResolutionContext).GetDeclaredMethod(nameof(ResolutionContext.CacheDestination));

        private static readonly MethodInfo GetDestinationMethodInfo =
            typeof(ResolutionContext).GetDeclaredMethod(nameof(ResolutionContext.GetDestination));

        internal static void CheckForCycles(this TypeMapPlanBuilder planBuilder, Stack<TypeMap> typeMapsPath)
        {
            if (planBuilder.TypeMap.PreserveReferences)
            {
                return;
            }
            if (typeMapsPath == null)
            {
                typeMapsPath = new Stack<TypeMap>();
            }
            typeMapsPath.Push(planBuilder.TypeMap);
            var propertyTypeMaps =
            (from propertyTypeMap in
                (from pm in planBuilder.TypeMap.GetPropertyMaps() where pm.CanResolveValue() select planBuilder.ResolvePropertyTypeMap(pm))
                where propertyTypeMap != null && !propertyTypeMap.PreserveReferences
                select propertyTypeMap).Distinct();
            foreach (var propertyTypeMap in propertyTypeMaps)
            {
                if (typeMapsPath.Contains(propertyTypeMap))
                {
                    Debug.WriteLine($"Setting PreserveReferences: {planBuilder.TypeMap.SourceType} - {planBuilder.TypeMap.DestinationType} => {propertyTypeMap.SourceType} - {propertyTypeMap.DestinationType}");
                    propertyTypeMap.PreserveReferences = true;
                }
                else
                {
                    propertyTypeMap.Seal(planBuilder.ConfigurationProvider, typeMapsPath);
                }
            }
            typeMapsPath.Pop();
        }

        private static TypeMap ResolvePropertyTypeMap(this TypeMapPlanBuilder planBuilder, AutoMapper.PropertyMap propertyMap)
        {
            if (propertyMap.SourceType == null)
            {
                return null;
            }
            var types = new TypePair(propertyMap.SourceType, propertyMap.DestinationPropertyType);
            var typeMap = planBuilder.ConfigurationProvider.ResolveTypeMap(types);
            if (typeMap == null && planBuilder.ConfigurationProvider.FindMapper(types) is IObjectMapperInfo mapper)
            {
                typeMap = planBuilder.ConfigurationProvider.ResolveTypeMap(mapper.GetAssociatedTypes(types));
            }
            return typeMap;
        }

        internal static Expression GetCache(this Expression expression, TypeMapPlanBuilder planBuilder)
        {
            if (!planBuilder.TypeMap.PreserveReferences)
                return expression;

            var dest = Variable(typeof(object), "dest");
            var set = Call(planBuilder.Context, CacheDestinationMethodInfo, planBuilder.Source, Constant(planBuilder.Destination.Type), planBuilder.Destination);
            var setCache = IfThen(NotEqual(planBuilder.Source, Constant(null)), set);

            return Block(new[] { dest }, Assign(dest, expression), setCache, dest);
        }

        internal static Expression AssignCache(this Expression expression, TypeMapPlanBuilder planBuilder)
        {
            if (!planBuilder.TypeMap.PreserveReferences)
                return expression;

            var cache = Variable(planBuilder.TypeMap.DestinationTypeToUse, "cachedDestination");
            var assignCache =
                Assign(cache,
                    ToType(Call(planBuilder.Context, GetDestinationMethodInfo, planBuilder.Source, Constant(planBuilder.Destination.Type)), planBuilder.Destination.Type));
            var condition = Expression.Condition(
                AndAlso(NotEqual(planBuilder.Source, Constant(null)), NotEqual(assignCache, Constant(null))),
                cache,
                expression);

            return Block(new[] { cache }, condition);
        }

        internal static Expression MaxDepthCheck(this Expression mapperFunc, TypeMap typeMap, Expression context)
        {
            return typeMap.MaxDepth > 0
                ? Expression.Condition(
                    LessThanOrEqual(
                        Call(context, ((MethodCallExpression)GetTypeDepthInfo.Body).Method, Constant(typeMap.Types)),
                        Constant(typeMap.MaxDepth)
                    ),
                    mapperFunc,
                    Default(typeMap.DestinationTypeToUse))
                : mapperFunc;
        }

        internal static Expression MaxDepthIncrement(this TypeMap typeMap, Expression Context)
        {
            return typeMap.MaxDepth > 0
                ? Call(Context, ((MethodCallExpression)IncTypeDepthInfo.Body).Method, Constant(typeMap.Types))
                : null;
        }

        internal static Expression MaxDepthDecrement(this TypeMap typeMap, Expression Context)
        {
            return typeMap.MaxDepth > 0
                ? Call(Context, ((MethodCallExpression)DecTypeDepthInfo.Body).Method, Constant(typeMap.Types))
                : null;
        }

        internal static ConditionalExpression CheckContext(this TypeMap typeMap, Expression context)
        {
            if (typeMap.MaxDepth > 0 || typeMap.PreserveReferences)
            {
                var mapper = Property(context, nameof(ResolutionContext.Mapper));
                return IfThen(Property(context, nameof(ResolutionContext.IsDefault)), Assign(context, Invoke(CreateContext, mapper)));
            }
            return null;
        }
    }
}

namespace AutoMapper.QueryableExtensions
{
    internal static class MaxDepthExtensions
    {
        internal static bool ExceedsMaxDepth(this TypeMap typeMap, ExpressionRequest request, IDictionary<ExpressionRequest, int> typePairCount)
        {
            return typeMap.MaxDepth > 0 && request.GetDepth(typePairCount) >= typeMap.MaxDepth;
        }

        private static int GetDepth(this ExpressionRequest request, IDictionary<ExpressionRequest, int> typePairCount)
        {
            if (typePairCount.TryGetValue(request, out int visitCount))
            {
                visitCount = visitCount + 1;
            }
            typePairCount[request] = visitCount;
            return visitCount;
        }
    }
}