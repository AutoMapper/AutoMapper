using System;
using System.Linq.Expressions;
using AutoMapper.XpressionMapper.Extensions;

namespace AutoMapper.XpressionMapper
{
    internal class PrependParentNameVisitor : ExpressionVisitor
    {
        public PrependParentNameVisitor(Type CurrentParameterType, string ParentFullName, ParameterExpression NewParameter)
        {
            this.CurrentParameterType = CurrentParameterType;
            this.ParentFullName = ParentFullName;
            this.NewParameter = NewParameter;
        }

        #region Properties
        public Type CurrentParameterType { get; set; }
        public string ParentFullName { get; set; }
        public ParameterExpression NewParameter { get; set; } 
        #endregion Properties

        #region Methods
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.Constant)
                return base.VisitMember(node);

            string sourcePath = null;

            ParameterExpression parameterExpression = node.GetParameterExpression();
            Type sType = parameterExpression == null ? null : parameterExpression.Type;
            if (sType != null && sType == this.CurrentParameterType && node.IsMemberExpression())
            {
                sourcePath = node.GetPropertyFullName();
            }
            else
            {
                return base.VisitMember(node);
            }

            string fullName = string.IsNullOrEmpty(this.ParentFullName)
                            ? sourcePath
                            : string.Concat(this.ParentFullName, ".", sourcePath);

            MemberExpression me = this.NewParameter.BuildExpression(fullName);

            return me;
        }
        #endregion Methods
    }
}
