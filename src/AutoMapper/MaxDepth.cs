using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using static System.Linq.Expressions.Expression;

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

        private static readonly Expression<Func<ResolutionContext, int>> GetTypeDepthInfo =
            ctxt => ctxt.GetTypeDepth(default(TypePair));

        private static readonly Expression<Action<ResolutionContext>> IncTypeDepthInfo =
            ctxt => ctxt.IncrementTypeDepth(default(TypePair));

        private static readonly Expression<Action<ResolutionContext>> DecTypeDepthInfo =
            ctxt => ctxt.DecrementTypeDepth(default(TypePair));

        private static readonly Expression<Func<IRuntimeMapper, ResolutionContext>> CreateContext =
            mapper => new ResolutionContext(mapper.DefaultContext.Options, mapper);

        internal static Expression MaxDepthCheck(this Expression mapperFunc, TypeMap typeMap, Expression context)
        {
            return typeMap.MaxDepth > 0
                ? Condition(
                    LessThanOrEqual(
                        Call(context, ((MethodCallExpression) GetTypeDepthInfo.Body).Method, Constant(typeMap.Types)),
                        Constant(typeMap.MaxDepth)
                    ),
                    mapperFunc,
                    Default(typeMap.DestinationTypeToUse))
                : mapperFunc;
        }
        
        internal static Expression MaxDepthIncrement(this TypeMap typeMap, Expression Context)
        {
            return typeMap.MaxDepth > 0
                ? Call(Context, ((MethodCallExpression) IncTypeDepthInfo.Body).Method, Constant(typeMap.Types))
                : null;
        }
        
        internal static Expression MaxDepthDecrement(this TypeMap typeMap, Expression Context)
        {
            return typeMap.MaxDepth > 0
                ? Call(Context, ((MethodCallExpression) DecTypeDepthInfo.Body).Method, Constant(typeMap.Types))
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