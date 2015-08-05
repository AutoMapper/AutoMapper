namespace AutoMapper
{
    using System;
    using System.Linq.Expressions;

    public interface ICtorParamConfigurationExpression<TSource>
    {
        void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember);
    }
}