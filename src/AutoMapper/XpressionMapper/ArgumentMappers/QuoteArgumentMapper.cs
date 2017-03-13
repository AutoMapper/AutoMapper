using System.Linq.Expressions;
using AutoMapper.XpressionMapper.Extensions;

namespace AutoMapper.XpressionMapper.ArgumentMappers
{
    internal class QuoteArgumentMapper : ArgumentMapper
    {
        public QuoteArgumentMapper(XpressionMapperVisitor expressionVisitor, Expression argument)
            : base(expressionVisitor, argument)
        {
        }

        public override Expression MappedArgumentExpression
        {
            get
            {
                var lambdaExpression = (LambdaExpression)((UnaryExpression)Argument).Operand;
                var ex = ExpressionVisitor.Visit(lambdaExpression.Body);

                var mapped = Expression.Lambda(ex, lambdaExpression.GetDestinationParameterExpressions(ExpressionVisitor.InfoDictionary, ExpressionVisitor.TypeMappings));
                return Expression.Quote(mapped);
            }
        }
    }
}
