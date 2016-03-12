using System.Collections.Generic;

namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class MemberResolverExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap, IValueResolver valueResolver)
        {
            return ExpressionResolutionResult(expressionResolutionResult, propertyMap.CustomExpression);
        }

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, LambdaExpression lambdaExpression)
        {
            var oldParameter = lambdaExpression.Parameters.Single();
            var newParameter = expressionResolutionResult.ResolutionExpression;
            var converter = new ParameterConversionVisitor(newParameter, oldParameter);

            Expression currentChild = converter.Visit(lambdaExpression.Body);
            Type currentChildType = currentChild.Type;

            return new ExpressionResolutionResult(currentChild, currentChildType);
        }

        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap, IValueResolver valueResolver)
        {
            return ExpressionResolutionResult(expressionResolutionResult, null);
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            IValueResolver valueResolver)
        {
            return valueResolver is IMemberResolver;
        }
    }

    internal class ParameterConversionVisitor : ExpressionVisitor
    {
        private readonly Expression newParameter;
        private readonly ParameterExpression oldParameter;

        public ParameterConversionVisitor(Expression newParameter, ParameterExpression oldParameter)
        {
            this.newParameter = newParameter;
            this.oldParameter = oldParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // replace all old param references with new ones
            return node == oldParameter ? newParameter : node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != oldParameter) // if instance is not old parameter - do nothing
                return base.VisitMember(node);

            var newObj = Visit(node.Expression);
            MemberInfo newMember = null;
            if (newParameter.Type.GetTypeInfo().IsInterface)
            {
                // Type.GetMember() does not return inherited members when the type is an interface,
                // regardless of the binding flags used.  See http://stackoverflow.com/q/2132791/239394.
                var typesQueried = new HashSet<Type>();
                var interfaceQueue = new Queue<Type>();
                interfaceQueue.Enqueue(newParameter.Type);

                while (newMember == null && interfaceQueue.Count > 0)
                {
                    var @interface = interfaceQueue.Dequeue();
                    if (typesQueried.Add(@interface))
                    {
                        newMember = @interface.GetMember(node.Member.Name).FirstOrDefault();
                        foreach (var inherited in @interface.GetTypeInfo().ImplementedInterfaces)
                        {
                            interfaceQueue.Enqueue(inherited);
                        }
                    }
                }
                if (newMember == null)
                {
                    throw new InvalidOperationException($"No member named {node.Member.Name} was found in the interface {newParameter.Type} or its inherited interfaces");
                }
            }
            else
            {
                newMember = newParameter.Type.GetMember(node.Member.Name).First();
            }
            return Expression.MakeMemberAccess(newObj, newMember);
        }
    }
}