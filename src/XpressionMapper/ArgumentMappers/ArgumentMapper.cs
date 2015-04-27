using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace XpressionMapper.ArgumentMappers
{
    internal abstract class ArgumentMapper
    {
        protected ArgumentMapper(XpressionMapperVisitor expressionVisitor, Expression argument)
        {
            this.expressionVisitor = expressionVisitor;
            this.argument = argument;
        }

        #region Variables
        private XpressionMapperVisitor expressionVisitor;
        #endregion Variables

        #region Properties
        protected Expression argument;
        protected virtual XpressionMapperVisitor ExpressionVisitor { get { return this.expressionVisitor; } }
        public abstract Expression MappedArgumentExpression { get; }
        #endregion Properties

        #region Methods
        public static ArgumentMapper Create(XpressionMapperVisitor expressionVisitor, Expression argument)
        {
            switch (argument.NodeType)
            {
                case ExpressionType.Lambda:
                    return new LambdaArgumentMapper(expressionVisitor, argument);
                case ExpressionType.Quote:
                    return new QuoteArgumentMapper(expressionVisitor, argument);
                default:
                    return new DefaultArgumentMapper(expressionVisitor, argument);
            }
        }
        #endregion Methods
    }
}
