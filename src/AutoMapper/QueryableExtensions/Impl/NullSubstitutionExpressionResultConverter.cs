namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq.Expressions;
    using Internal;

    public class NullSubstitutionExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap, IValueResolver valueResolver)
        {
            Expression currentChild = expressionResolutionResult.ResolutionExpression;
            Type currentChildType = expressionResolutionResult.Type;
            var nullSubstitute = propertyMap.NullSubstitute;

            var newParameter = expressionResolutionResult.ResolutionExpression;
            var converter = new NullSubstitutionConversionVisitor(newParameter, nullSubstitute);

            currentChild = converter.Visit(currentChild);
            currentChildType = currentChildType.GetTypeOfNullable();

            return new ExpressionResolutionResult(currentChild, currentChildType);
        }

        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap, IValueResolver valueResolver)
        {
            throw new NotImplementedException();
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            IValueResolver valueResolver)
        {
            return valueResolver is NullReplacementMethod && expressionResolutionResult.Type.IsNullableType();
        }

        private class NullSubstitutionConversionVisitor : ExpressionVisitor
        {
            private readonly Expression newParameter;
            private readonly object _nullSubstitute;

            public NullSubstitutionConversionVisitor(Expression newParameter, object nullSubstitute)
            {
                this.newParameter = newParameter;
                _nullSubstitute = nullSubstitute;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node == newParameter)
                {
                    var equalsNull = Expression.Property(newParameter, "HasValue");
                    var nullConst = Expression.Condition(equalsNull, Expression.Property(newParameter, "Value"),
                        Expression.Constant(_nullSubstitute), node.Type.GetTypeOfNullable());
                    return nullConst;
                }
                return node;
            }
        }
    }
}