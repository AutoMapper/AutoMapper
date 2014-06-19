using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Impl;

namespace AutoMapper.EquivilencyExpression
{
    internal class EquivilentExpression : IEquivilentExpression
    {
        internal static IEquivilentExpression BadValue { get; private set; }

        static EquivilentExpression()
        {
            BadValue = new EquivilentExpression();
        }

        public bool IsEquivlent(object source, object destination)
        {
            throw new NotImplementedException();
        }
    }

    internal class EquivilentExpression<TSource,TDestination> : IEquivilentExpression , IToSingleSourceEquivalentExpression
        where TSource : class 
        where TDestination : class
    {
        private readonly Expression<Func<TSource, TDestination, bool>> _equivilentExpression;

        public EquivilentExpression(Expression<Func<TSource,TDestination,bool>> equivilentExpression)
        {
            _equivilentExpression = equivilentExpression;
        }

        public bool IsEquivlent(object source, object destination)
        {
            if(!(source is TSource))
                throw new EquivilentExpressionNotOfTypeException(source.GetType(), typeof(TSource));
            if (!(destination is TDestination))
                throw new EquivilentExpressionNotOfTypeException(destination.GetType(), typeof(TDestination));
            return _equivilentExpression.Compile()(source as TSource, destination as TDestination);
        }

        public Expression ToSingleSourceExpression(object source)
        {
            if (source == null)
                throw new Exception("Invalid somehow");
            return GetSourceOnlyExpresison(source as TSource);
        }

        internal Expression GetSourceOnlyExpresison<TSource>(TSource source)
        {
            var expression = new ParametersToConstantVisitor<TSource>(source).Visit(_equivilentExpression) as LambdaExpression;
            return Expression.Lambda(expression.Body, _equivilentExpression.Parameters[1]);
        }

        private class EquivilentExpressionNotOfTypeException : Exception
        {
            private readonly Type _objectType;
            private readonly Type _expectedType;

            public EquivilentExpressionNotOfTypeException(Type objectType, Type expectedType)
            {
                _objectType = objectType;
                _expectedType = expectedType;
            }

            public override string Message
            {
                get { return string.Format("{0} does not equal or inherit from {1}", _objectType.Name, _expectedType.Name); }
            }
        }
    }

    internal class ParametersToConstantVisitor<T> : ExpressionVisitor
    {
        private readonly T _value;

        public ParametersToConstantVisitor(T value)
        {
            _value = value;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is PropertyInfo && node.Member.DeclaringType == typeof(T))
            {
                var memberExpression = Expression.Constant(node.Member.ToMemberGetter().GetValue(_value));
                return memberExpression;
            }
            return base.VisitMember(node);
        }
    }
}