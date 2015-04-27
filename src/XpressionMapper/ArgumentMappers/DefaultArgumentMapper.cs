using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace XpressionMapper.ArgumentMappers
{
    internal class DefaultArgumentMapper : ArgumentMapper
    {
        public DefaultArgumentMapper(XpressionMapperVisitor expressionVisitor, Expression argument)
            : base(expressionVisitor, argument)
        {
        }

        public override Expression MappedArgumentExpression
        {
            get
            {
                Expression ex = this.ExpressionVisitor.Visit(this.argument);
                return ex;
            }
        }
    }
}
