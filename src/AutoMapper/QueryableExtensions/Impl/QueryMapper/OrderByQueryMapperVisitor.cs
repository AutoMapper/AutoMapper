using System;
using System.Linq.Expressions;
using AutoMapper.Impl;

namespace AutoMapper.QueryableExtensions.Impl.QueryMapper
{
    public class OrderByConverter
    {
        private QueryMapperVisitor _rootVisitor;

        public OrderByConverter(QueryMapperVisitor rootVisitor)
        {
            _rootVisitor = rootVisitor;
        }

        public bool IsMatch(MethodCallExpression orderByExpression)
        {
            var methodName = orderByExpression.Method.Name;
            return methodName == "OrderBy" || methodName == "OrderByDescending" ||
                   methodName == "ThenBy" || methodName == "ThenByDescending";
        }

        public void CheckIsMatch(MethodCallExpression orderByExpression)
        {
            if (!IsMatch(orderByExpression))
            {
                throw new InvalidOperationException("Current method call expression is not OrderBy statement");
            }
        }

        public MethodCallExpression Convert(MethodCallExpression orderByExpression)
        {
            CheckIsMatch(orderByExpression);

            var query = orderByExpression.Arguments[0];
            var orderByExpr = orderByExpression.Arguments[1];

            var newQuery = _rootVisitor.Visit(query);
            var newOrderByExpr = _rootVisitor.Visit(orderByExpr);
            var newObject = _rootVisitor.Visit(orderByExpression.Object);

            var visitor = new CatchLambdaReturnType();
            visitor.Visit(newOrderByExpr);
            visitor.CheckIsCatched();

            var genericMethod = orderByExpression.Method.GetGenericMethodDefinition();
            var methodArgs = orderByExpression.Method.GetGenericArguments();
            methodArgs[0] = methodArgs[0] = _rootVisitor.DestinationParameterType;
            methodArgs[1] = methodArgs[1] = visitor.ReturnType;
            var orderByMethod = genericMethod.MakeGenericMethod(methodArgs);

            return Expression.Call(newObject, orderByMethod, newQuery, newOrderByExpr);
        }
    }

    public class CatchLambdaReturnType : ExpressionVisitor
    {
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            ReturnType = node.ReturnType;
            return node;
        }

        public bool IsCatched { get { return ReturnType != null; } }

        public Type ReturnType { get; set; }

        public void CheckIsCatched()
        {
            if (!IsCatched)
            {
                throw new Exception("Expression does not contain lambda statement");
            }
        }
    }

    public class OrderByQueryMapperVisitor : ExpressionVisitor
    {
        private readonly ExpressionVisitor _rootVisitor;

        public OrderByQueryMapperVisitor(ExpressionVisitor rootVisitor)
        {
            _rootVisitor = rootVisitor;
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }
    }
}