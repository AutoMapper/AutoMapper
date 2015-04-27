using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XpressionMapper.Extensions;

namespace XpressionMapper.ArgumentMappers
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
                Expression exp = ((LambdaExpression)this.argument).Body;
                Expression ex = this.ExpressionVisitor.Visit(exp);

                Type parameterType = exp.GetParameterType();
                LambdaExpression mapped = Expression.Lambda(ex, this.ExpressionVisitor.InfoDictionary[parameterType].NewParameter);
                return mapped;
            }
        }
    }
}
