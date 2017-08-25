using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper
{
    public static class AfterMapExtensions
    {
        /// <summary>
        /// Execute a custom function to the source and/or destination types after member mapping
        /// </summary>
        /// <param name="mappingExpression"></param>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        public static IMappingExpression<TSource, TDestination> AfterMap<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Action<TSource, TDestination> afterFunction)
        {
            mappingExpression.TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => afterFunction(src, dest);

                tm.AddAfterMapAction(expr);
            });

            return mappingExpression;
        }

        /// <summary>
        /// Execute a custom function to the source and/or destination types after member mapping
        /// </summary>
        /// <param name="mappingExpression"></param>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        public static IMappingExpression<TSource, TDestination> AfterMap<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Action<TSource, TDestination, ResolutionContext> afterFunction)
        {
            mappingExpression.TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => afterFunction(src, dest, ctxt);

                tm.AddAfterMapAction(expr);
            });

            return mappingExpression;
        }

        /// <summary>
        /// Execute a custom mapping action after member mapping
        /// </summary>
        /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns>Itself</returns>
        public static IMappingExpression<TSource, TDestination> AfterMap<TMappingAction, TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression)
            where TMappingAction : IMappingAction<TSource, TDestination>
        {
            void AfterFunction(TSource src, TDestination dest, ResolutionContext ctxt)
                => ((TMappingAction)ctxt.Options.ServiceCtor(typeof(TMappingAction))).Process(src, dest);

            return mappingExpression.AfterMap(AfterFunction);
        }
    }
}

namespace AutoMapper.Execution
{
    internal static class AfterMapExtensions
    {
        internal static IEnumerable<Expression> GetAfterExpressions(this TypeMapPlanBuilder planBuilder, TypeMap typeMap)
            => typeMap.AfterMapActions.Select(_ => _.ReplaceParameters(planBuilder.Source, planBuilder.Destination, planBuilder.Context)).ToList();
    }
}