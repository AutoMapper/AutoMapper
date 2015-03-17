using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XpressionMapper.Extensions;

namespace XpressionMapper.ArgumentMappers
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
                Expression exp = ((LambdaExpression)((UnaryExpression)this.argument).Operand).Body;
                Expression ex = this.ExpressionVisitor.Visit(exp);

                Type parameterType = exp.GetParameterType();
                LambdaExpression mapped = Expression.Lambda(ex, this.ExpressionVisitor.InfoDictionary[parameterType].NewParameter);

                return Expression.Quote(mapped);
            }
        }
    }
}
