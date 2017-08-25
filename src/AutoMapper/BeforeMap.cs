using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper
{
    public static class BeforeMapExtensions
    {
        /// <summary>
        /// Execute a custom function to the source and/or destination types before member mapping
        /// </summary>
        /// <param name="mappingExpression"></param>
        /// <param name="beforeFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        public static IMappingExpression<TSource, TDestination> BeforeMap<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Action<TSource, TDestination> beforeFunction)
        {
            mappingExpression.TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => beforeFunction(src, dest);

                tm.AddBeforeMapAction(expr);
            });

            return mappingExpression;
        }

        /// <summary>
        /// Execute a custom function to the source and/or destination types before member mapping
        /// </summary>
        /// <param name="mappingExpression"></param>
        /// <param name="beforeFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        public static IMappingExpression<TSource, TDestination> BeforeMap<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Action<TSource, TDestination, ResolutionContext> beforeFunction)
        {
            mappingExpression.TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => beforeFunction(src, dest, ctxt);

                tm.AddBeforeMapAction(expr);
            });

            return mappingExpression;
        }

        /// <summary>
        /// Execute a custom mapping action before member mapping
        /// </summary>
        /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns>Itself</returns>
        public static IMappingExpression<TSource, TDestination> BeforeMap<TMappingAction, TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression)
            where TMappingAction : IMappingAction<TSource, TDestination>
        {
            void BeforeFunction(TSource src, TDestination dest, ResolutionContext ctxt)
                => ((TMappingAction)ctxt.Options.ServiceCtor(typeof(TMappingAction))).Process(src, dest);

            return mappingExpression.BeforeMap(BeforeFunction);
        }
    }
}

namespace AutoMapper.Execution
{
    internal static class BeforeMapExtensions
    {
        internal static IEnumerable<Expression> GetBeforeExpressions(this TypeMapPlanBuilder planBuilder)
            => planBuilder.TypeMap.BeforeMapActions.Select(_ => _.ReplaceParameters(planBuilder.Source, planBuilder.Destination, planBuilder.Context)).ToList();
    }
}