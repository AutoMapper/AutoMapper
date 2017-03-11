using System.Linq.Expressions;
using AutoMapper.XpressionMapper.Extensions;

namespace AutoMapper.XpressionMapper.ArgumentMappers
{
    internal class LambdaArgumentMapper : ArgumentMapper
    {
        public LambdaArgumentMapper(XpressionMapperVisitor expressionVisitor, Expression argument)
            : base(expressionVisitor, argument)
        {
        }

        public override Expression MappedArgumentExpression
        {
            get
            {
                var lambdaExpression = (LambdaExpression)Argument;
                var ex = ExpressionVisitor.Visit(lambdaExpression.Body);

                var mapped = Expression.Lambda(ex, lambdaExpression.GetDestinationParameterExpressions(ExpressionVisitor.InfoDictionary, ExpressionVisitor.TypeMappings));
                return mapped;
            }
        }
    }
}
