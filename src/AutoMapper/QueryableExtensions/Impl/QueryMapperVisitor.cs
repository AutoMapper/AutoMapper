using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class QueryMapperVisitor : ExpressionVisitor
    {
        private readonly IQueryable _destQuery;
        private readonly ParameterExpression _instanceParameter;
        private readonly Type _sourceType;
        private readonly Type _destinationType;
        private readonly Stack<object> _tree = new Stack<object>();
        private readonly Stack<object> _newTree = new Stack<object>();
        private readonly MemberAccessQueryMapperVisitor _memberVisitor;


        internal QueryMapperVisitor(Type sourceType, Type destinationType, IQueryable destQuery, IConfigurationProvider config)
        {
            _sourceType = sourceType;
            _destinationType = destinationType;
            _destQuery = destQuery;
            _instanceParameter = Expression.Parameter(destinationType, "dto");
            _memberVisitor = new MemberAccessQueryMapperVisitor(this, config);
        }

        public static IQueryable<TDestination> Map<TSource, TDestination>(IQueryable<TSource> sourceQuery, IQueryable<TDestination> destQuery, IConfigurationProvider config)
        {
            var visitor = new QueryMapperVisitor(typeof(TSource), typeof(TDestination), destQuery, config);
            var expr = visitor.Visit(sourceQuery.Expression);

            var newDestQuery = destQuery.Provider.CreateQuery<TDestination>(expr);
            return newDestQuery;
        }

        public override Expression Visit(Expression node)
        {
            _tree.Push(node);
            // OData Client DataServiceQuery initial expression node type
            if (node != null && (int)node.NodeType == 10000)
            {
                return node;
            }
            var newNode = base.Visit(node);
            _newTree.Push(newNode);
            return newNode;
        }

        protected override Expression VisitParameter(ParameterExpression node) => _instanceParameter;

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // It is data source of queryable object instance
            if (node.Value is IQueryable query && query.ElementType == _sourceType)
                return _destQuery.Expression;
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            // Convert Right expression value to left expr type
            // It is needed when PropertyMap is changing type of property
            if (left.Type != right.Type && right.NodeType == ExpressionType.Constant)
            {
                var value = Convert.ChangeType(((ConstantExpression)right).Value, left.Type, CultureInfo.CurrentCulture);

                right = Expression.Constant(value, left.Type);
            }

            return Expression.MakeBinary(node.NodeType, left, right);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var newBody = Visit(node.Body);
            var newParams = node.Parameters.Select(p => (ParameterExpression)Visit(p));

            var delegateType = ChangeLambdaArgTypeFormSourceToDest(node.Type, newBody.Type);

            var newLambda = Expression.Lambda(delegateType, newBody, newParams);
            return newLambda;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "OrderBy" || node.Method.Name == "OrderByDescending" ||
                    node.Method.Name == "ThenBy" || node.Method.Name == "ThenByDescending")
            {
                return VisitOrderBy(node);
            }

            var args = node.Arguments.Select(Visit).ToList();
            var newObject = Visit(node.Object);
            var method = ChangeMethodArgTypeFormSourceToDest(node.Method);

            var newMethodCall = Expression.Call(newObject, method, args);
            return newMethodCall;
        }

        private Expression VisitOrderBy(MethodCallExpression node)
        {
            var query = node.Arguments[0];
            var orderByExpr = node.Arguments[1];

            var newQuery = Visit(query);
            var newOrderByExpr = Visit(orderByExpr);
            var newObject = Visit(node.Object);



            var genericMethod = node.Method.GetGenericMethodDefinition();
            var methodArgs = node.Method.GetGenericArguments();
            methodArgs[0] = methodArgs[0].ReplaceItemType(_sourceType, _destinationType);

            // for typical orderby expression, a unaryexpression is used that contains a 
            // func which in turn defines the type of the field that has to be used for ordering/sorting
            if (newOrderByExpr is UnaryExpression unary && unary.Operand.Type.IsGenericType())
            {
                methodArgs[1] = methodArgs[1].ReplaceItemType(typeof(string), unary.Operand.Type.GetGenericArguments().Last());
            }
            else
            {
                methodArgs[1] = methodArgs[1].ReplaceItemType(typeof(string), typeof(int));
            }
            var orderByMethod = genericMethod.MakeGenericMethod(methodArgs);

            return Expression.Call(newObject, orderByMethod, newQuery, newOrderByExpr);
        }

        protected override Expression VisitMember(MemberExpression node) => _memberVisitor.Visit(node);

        private MethodInfo ChangeMethodArgTypeFormSourceToDest(MethodInfo mi)
        {
            if (!mi.IsGenericMethod)
                return mi;
            var genericMethod = mi.GetGenericMethodDefinition();
            var methodArgs = mi.GetGenericArguments();
            methodArgs = methodArgs.Select(t => t.ReplaceItemType(_sourceType, _destinationType)).ToArray();
            return genericMethod.MakeGenericMethod(methodArgs);

        }

        private Type ChangeLambdaArgTypeFormSourceToDest(Type lambdaType, Type returnType)
        {
            if (lambdaType.IsGenericType())
            {
                var genArgs = lambdaType.GetTypeInfo().GenericTypeArguments;
                var newGenArgs = genArgs.Select(t => t.ReplaceItemType(_sourceType, _destinationType)).ToArray();
                var genericTypeDef = lambdaType.GetGenericTypeDefinition();
                if (genericTypeDef.FullName.StartsWith("System.Func"))
                {
                    newGenArgs[newGenArgs.Length - 1] = returnType;
                }
                return genericTypeDef.MakeGenericType(newGenArgs);
            }
            return lambdaType;
        }
    }
}
