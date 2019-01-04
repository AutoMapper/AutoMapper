using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    public interface ICtorParamConfigurationExpression<TSource>
    {
        /// <summary>
        /// Map constructor parameter from member expression
        /// </summary>
        /// <typeparam name="TMember">Member type</typeparam>
        /// <param name="sourceMember">Member expression</param>
        void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember);

        /// <summary>
        /// Map constructor parameter from custom func that has access to <see cref="ResolutionContext"/>
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="resolver">Custom func</param>
        void MapFrom<TMember>(Func<TSource, ResolutionContext, TMember> resolver);
    }
}