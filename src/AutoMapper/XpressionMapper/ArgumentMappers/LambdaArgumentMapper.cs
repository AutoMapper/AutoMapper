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
                LambdaExpression lambdaExpression = (LambdaExpression)this.argument;
                Expression ex = this.ExpressionVisitor.Visit(lambdaExpression.Body);

                LambdaExpression mapped = Expression.Lambda(ex, lambdaExpression.GetDestinationParameterExpressions(this.ExpressionVisitor.InfoDictionary, this.ExpressionVisitor.TypeMappings));
                return mapped;
            }
        }
    }
}
