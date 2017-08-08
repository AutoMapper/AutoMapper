using System;
using System.Linq.Expressions;
using AutoMapper.Internal;
using AutoMapper.XpressionMapper.Extensions;

namespace AutoMapper.XpressionMapper
{
    internal class PrependParentNameVisitor : ExpressionVisitor
    {
        public PrependParentNameVisitor(Type currentParameterType, string parentFullName, ParameterExpression newParameter)
        {
            CurrentParameterType = currentParameterType;
            ParentFullName = parentFullName;
            NewParameter = newParameter;
        }

        public Type CurrentParameterType { get; }
        public string ParentFullName { get; }
        public ParameterExpression NewParameter { get; } 

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.Constant)
                return base.VisitMember(node);

            string sourcePath;

            var parameterExpression = node.GetParameterExpression();
            var sType = parameterExpression?.Type;
            if (sType != null && sType == CurrentParameterType && node.IsMemberExpression())
            {
                sourcePath = node.GetPropertyFullName();
            }
            else
            {
                return base.VisitMember(node);
            }

            var fullName = string.IsNullOrEmpty(ParentFullName)
                            ? sourcePath
                            : string.Concat(ParentFullName, ".", sourcePath);

            var me = ExpressionFactory.MemberAccesses(fullName, NewParameter);

            return me;
        }
    }
}
